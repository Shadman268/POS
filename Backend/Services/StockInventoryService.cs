using Backend.Data;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Data.Common;

namespace Backend.Services
{
    public class StockInventoryService : IStockInventoryService
    {
        private readonly AppDbContext _context;

        public StockInventoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int?> TryGetAvailableStockAsync(Tenant tenant, TenantMedicine tenantMedicine)
        {
            if (!ShouldTrackStock(tenant, tenantMedicine))
            {
                return null;
            }

            if (tenant.InventoryMode == PharmacyInventoryMode.BatchExpiry)
            {
                return await _context.MedicineBatches
                    .AsNoTracking()
                    .Where(b => b.TenantMedicineId == tenantMedicine.Id && b.IsActive)
                    .SumAsync(b => b.QuantityOnHand);
            }

            var stock = await _context.MedicineStocks
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.TenantMedicineId == tenantMedicine.Id);

            return stock?.QuantityOnHand ?? 0;
        }

        public async Task DeductForSaleAsync(Tenant tenant, ReceiptItem item, int receiptId)
        {
            if (item.TenantMedicineId == null || !await IsStockTrackedAsync(tenant, item.TenantMedicineId.Value))
            {
                return;
            }

            if (tenant.InventoryMode == PharmacyInventoryMode.BatchExpiry)
            {
                await DeductBatchStockAsync(tenant, item, receiptId);
                return;
            }

            await DeductAggregateStockAsync(tenant, item, receiptId);
        }

        public async Task RestoreForReturnAsync(Tenant tenant, ReceiptItem item, int returnReceiptId)
        {
            if (item.TenantMedicineId == null || !await IsStockTrackedAsync(tenant, item.TenantMedicineId.Value))
            {
                return;
            }

            if (tenant.InventoryMode == PharmacyInventoryMode.BatchExpiry && item.MedicineBatchId.HasValue)
            {
                await RestoreBatchStockAsync(tenant, item, returnReceiptId);
                return;
            }

            await RestoreAggregateStockAsync(tenant, item, returnReceiptId);
        }

        private async Task RestoreAggregateStockAsync(Tenant tenant, ReceiptItem item, int returnReceiptId)
        {
            var tenantMedicineId = item.TenantMedicineId!.Value;
            var quantity = item.Quantity;
            var now = DateTime.UtcNow;

            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                """
                UPDATE ms
                SET ms.QuantityOnHand = ms.QuantityOnHand + @quantity,
                    ms.UpdatedAtUtc = @now
                FROM MedicineStocks ms WITH (UPDLOCK, ROWLOCK)
                WHERE ms.TenantMedicineId = @tenantMedicineId
                  AND ms.TenantId = @tenantId
                """,
                new SqlParameter("@quantity", quantity),
                new SqlParameter("@now", now),
                new SqlParameter("@tenantMedicineId", tenantMedicineId),
                new SqlParameter("@tenantId", tenant.Id));

            if (rowsAffected == 0)
            {
                _context.MedicineStocks.Add(new MedicineStock
                {
                    TenantId = tenant.Id,
                    TenantMedicineId = tenantMedicineId,
                    QuantityOnHand = quantity,
                    UpdatedAtUtc = now
                });
            }

            _context.StockMovements.Add(new StockMovement
            {
                TenantId = tenant.Id,
                TenantMedicineId = tenantMedicineId,
                MovementType = StockMovementType.Return,
                QuantityDelta = quantity,
                ReferenceType = "Receipt",
                ReferenceId = returnReceiptId,
                CreatedAtUtc = now
            });
        }

        private async Task RestoreBatchStockAsync(Tenant tenant, ReceiptItem item, int returnReceiptId)
        {
            var tenantMedicineId = item.TenantMedicineId!.Value;
            var quantity = item.Quantity;
            var now = DateTime.UtcNow;
            var batchId = item.MedicineBatchId!.Value;

            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                """
                UPDATE mb
                SET mb.QuantityOnHand = mb.QuantityOnHand + @quantity
                FROM MedicineBatches mb WITH (UPDLOCK, ROWLOCK)
                WHERE mb.Id = @batchId
                  AND mb.TenantMedicineId = @tenantMedicineId
                  AND mb.TenantId = @tenantId
                """,
                new SqlParameter("@quantity", quantity),
                new SqlParameter("@batchId", batchId),
                new SqlParameter("@tenantMedicineId", tenantMedicineId),
                new SqlParameter("@tenantId", tenant.Id));

            if (rowsAffected == 0)
            {
                await RestoreAggregateStockAsync(tenant, item, returnReceiptId);
                return;
            }

            _context.StockMovements.Add(new StockMovement
            {
                TenantId = tenant.Id,
                TenantMedicineId = tenantMedicineId,
                MedicineBatchId = batchId,
                MovementType = StockMovementType.Return,
                QuantityDelta = quantity,
                ReferenceType = "Receipt",
                ReferenceId = returnReceiptId,
                CreatedAtUtc = now
            });

            await SyncAggregateStockAsync(tenant.Id, tenantMedicineId, now);
        }

        internal static bool ShouldTrackStock(Tenant tenant, TenantMedicine tenantMedicine)
        {
            return tenant.MaintainStock
                && tenant.InventoryMode != PharmacyInventoryMode.CatalogOnly
                && tenantMedicine.IsStockTracked;
        }

        private async Task<bool> IsStockTrackedAsync(Tenant tenant, int tenantMedicineId)
        {
            var tenantMedicine = await _context.TenantMedicines
                .AsNoTracking()
                .FirstOrDefaultAsync(tm => tm.Id == tenantMedicineId);

            return tenantMedicine != null && ShouldTrackStock(tenant, tenantMedicine);
        }

        private async Task DeductAggregateStockAsync(Tenant tenant, ReceiptItem item, int receiptId)
        {
            var tenantMedicineId = item.TenantMedicineId!.Value;
            var quantity = item.Quantity;
            var now = DateTime.UtcNow;

            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                """
                UPDATE ms
                SET ms.QuantityOnHand = ms.QuantityOnHand - @quantity,
                    ms.UpdatedAtUtc = @now
                FROM MedicineStocks ms WITH (UPDLOCK, ROWLOCK)
                INNER JOIN TenantMedicines tm WITH (UPDLOCK, ROWLOCK)
                    ON tm.Id = ms.TenantMedicineId
                WHERE ms.TenantMedicineId = @tenantMedicineId
                  AND ms.TenantId = @tenantId
                  AND tm.IsStockTracked = 1
                  AND ms.QuantityOnHand >= @quantity
                """,
                new SqlParameter("@quantity", quantity),
                new SqlParameter("@now", now),
                new SqlParameter("@tenantMedicineId", tenantMedicineId),
                new SqlParameter("@tenantId", tenant.Id));

            if (rowsAffected == 0)
            {
                throw new InvalidOperationException(
                    $"Insufficient stock for '{item.ProductName}'. Another sale may have just completed.");
            }

            _context.StockMovements.Add(new StockMovement
            {
                TenantId = tenant.Id,
                TenantMedicineId = tenantMedicineId,
                MovementType = StockMovementType.Sale,
                QuantityDelta = -quantity,
                ReferenceType = "Receipt",
                ReferenceId = receiptId,
                CreatedAtUtc = now
            });
        }

        private async Task DeductBatchStockAsync(Tenant tenant, ReceiptItem item, int receiptId)
        {
            var tenantMedicineId = item.TenantMedicineId!.Value;
            var quantity = item.Quantity;
            var now = DateTime.UtcNow;

            BatchDeductionResult batchResult;

            if (item.MedicineBatchId.HasValue)
            {
                batchResult = await DeductSpecificBatchAsync(
                    tenant.Id, tenantMedicineId, item.MedicineBatchId.Value, quantity, item.ProductName);
            }
            else
            {
                batchResult = await DeductFefoBatchAsync(
                    tenant.Id, tenantMedicineId, quantity, item.ProductName);
            }

            item.MedicineBatchId = batchResult.BatchId;
            item.BatchNumber = batchResult.BatchNumber;
            item.ExpiryDate = batchResult.ExpiryDate;

            _context.StockMovements.Add(new StockMovement
            {
                TenantId = tenant.Id,
                TenantMedicineId = tenantMedicineId,
                MedicineBatchId = batchResult.BatchId,
                MovementType = StockMovementType.Sale,
                QuantityDelta = -quantity,
                ReferenceType = "Receipt",
                ReferenceId = receiptId,
                CreatedAtUtc = now
            });

            await SyncAggregateStockAsync(tenant.Id, tenantMedicineId, now);
        }

        private async Task<BatchDeductionResult> DeductSpecificBatchAsync(
            int tenantId,
            int tenantMedicineId,
            int batchId,
            int quantity,
            string productName)
        {
            await using var command = await CreateLockedCommandAsync();
            command.CommandText = """
                UPDATE mb
                SET mb.QuantityOnHand = mb.QuantityOnHand - @quantity
                OUTPUT INSERTED.Id, INSERTED.BatchNumber, INSERTED.ExpiryDate
                FROM MedicineBatches mb WITH (UPDLOCK, ROWLOCK)
                WHERE mb.Id = @batchId
                  AND mb.TenantMedicineId = @tenantMedicineId
                  AND mb.TenantId = @tenantId
                  AND mb.IsActive = 1
                  AND mb.QuantityOnHand >= @quantity
                """;

            command.Parameters.Add(new SqlParameter("@quantity", quantity));
            command.Parameters.Add(new SqlParameter("@batchId", batchId));
            command.Parameters.Add(new SqlParameter("@tenantMedicineId", tenantMedicineId));
            command.Parameters.Add(new SqlParameter("@tenantId", tenantId));

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                throw new InvalidOperationException(
                    $"Insufficient stock for '{productName}'. Another sale may have just completed.");
            }

            return new BatchDeductionResult
            {
                BatchId = reader.GetInt32(0),
                BatchNumber = reader.GetString(1),
                ExpiryDate = reader.GetDateTime(2)
            };
        }

        private async Task<BatchDeductionResult> DeductFefoBatchAsync(
            int tenantId,
            int tenantMedicineId,
            int quantity,
            string productName)
        {
            await using var command = await CreateLockedCommandAsync();
            command.CommandText = """
                ;WITH target AS (
                    SELECT TOP 1 mb.Id
                    FROM MedicineBatches mb WITH (UPDLOCK, ROWLOCK, READPAST)
                    WHERE mb.TenantMedicineId = @tenantMedicineId
                      AND mb.TenantId = @tenantId
                      AND mb.IsActive = 1
                      AND mb.QuantityOnHand >= @quantity
                    ORDER BY mb.ExpiryDate, mb.Id
                )
                UPDATE mb
                SET mb.QuantityOnHand = mb.QuantityOnHand - @quantity
                OUTPUT INSERTED.Id, INSERTED.BatchNumber, INSERTED.ExpiryDate
                FROM MedicineBatches mb
                INNER JOIN target ON target.Id = mb.Id
                WHERE mb.QuantityOnHand >= @quantity
                """;

            command.Parameters.Add(new SqlParameter("@quantity", quantity));
            command.Parameters.Add(new SqlParameter("@tenantMedicineId", tenantMedicineId));
            command.Parameters.Add(new SqlParameter("@tenantId", tenantId));

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                throw new InvalidOperationException(
                    $"Insufficient stock for '{productName}'. Another sale may have just completed.");
            }

            return new BatchDeductionResult
            {
                BatchId = reader.GetInt32(0),
                BatchNumber = reader.GetString(1),
                ExpiryDate = reader.GetDateTime(2)
            };
        }

        private async Task<DbCommand> CreateLockedCommandAsync()
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            var command = connection.CreateCommand();
            command.Transaction = _context.Database.CurrentTransaction?.GetDbTransaction();
            return command;
        }

        private async Task SyncAggregateStockAsync(int tenantId, int tenantMedicineId, DateTime now)
        {
            await _context.Database.ExecuteSqlRawAsync(
                """
                UPDATE ms
                SET ms.QuantityOnHand = totals.TotalQty,
                    ms.UpdatedAtUtc = @now
                FROM MedicineStocks ms
                INNER JOIN (
                    SELECT @tenantMedicineId AS TenantMedicineId,
                           COALESCE(SUM(b.QuantityOnHand), 0) AS TotalQty
                    FROM MedicineBatches b
                    WHERE b.TenantMedicineId = @tenantMedicineId
                      AND b.IsActive = 1
                ) totals ON totals.TenantMedicineId = ms.TenantMedicineId
                WHERE ms.TenantMedicineId = @tenantMedicineId
                  AND ms.TenantId = @tenantId
                """,
                new SqlParameter("@tenantMedicineId", tenantMedicineId),
                new SqlParameter("@tenantId", tenantId),
                new SqlParameter("@now", now));
        }
    }
}
