namespace Backend.Constants
{
    public static class PosCatalogConstants
    {
        /// <summary>
        /// POS item IDs for catalog medicines without a TenantMedicine row use MedicineId + offset.
        /// IDs below this value are TenantMedicine.Id.
        /// </summary>
        public const int CatalogIdOffset = 1_000_000;

        public static bool IsCatalogMedicinePosId(int posItemId) => posItemId >= CatalogIdOffset;

        public static int ToMedicineId(int posItemId) => posItemId - CatalogIdOffset;

        public static int ToPosItemId(int medicineId) => medicineId + CatalogIdOffset;
    }
}
