using AutoMapper;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class ReceiptService : IReceiptService
    {
        private readonly IReceiptRepository _receiptRepository;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public ReceiptService(IReceiptRepository receiptRepository, IMapper mapper, AppDbContext context)
        {
            _receiptRepository = receiptRepository;
            _mapper = mapper;
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Receipt> CreateReceiptAsync(ReceiptDto receiptDto)
        {
            // Verify all products exist before creating the receipt
            var productIds = receiptDto.Items.Select(item => item.ProductId).ToList();
            var existingProducts = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();

            var missingProductIds = productIds.Except(existingProducts).ToList();
            if (missingProductIds.Any())
            {
                throw new InvalidOperationException($"Products with IDs {string.Join(", ", missingProductIds)} do not exist.");
            }

            // Create a new Receipt entity from the DTO
            var receipt = new Receipt
            {
                CustomerName = receiptDto.CustomerName,
                Total = receiptDto.Total,
                DiscountUnit = receiptDto.DiscountUnit,
                DiscountValue = receiptDto.DiscountValue,
                PriceAfterDiscount = receiptDto.PriceAfterDiscount,
                CashReceived = receiptDto.CashReceived,
                ChangeAmount = receiptDto.ChangeAmount,
                CreatedAt = DateTime.Now,
                Items = receiptDto.Items.Select(item => new ReceiptItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Subtotal = item.Subtotal
                }).ToList()
            };

            return await _receiptRepository.CreateReceiptAsync(receipt);
        }

        public async Task<IEnumerable<ReceiptDto>> GetAllReceiptsAsync()
        {
            var receipts = await _receiptRepository.GetAllReceiptsAsync();
            return _mapper.Map<IEnumerable<ReceiptDto>>(receipts);
        }

        public async Task<ReceiptDto?> GetReceiptByIdAsync(int id)
        {
            var receipt = await _receiptRepository.GetReceiptByIdAsync(id);
            return receipt != null ? _mapper.Map<ReceiptDto>(receipt) : null;
        }
    }
}
