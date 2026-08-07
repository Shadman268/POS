namespace Backend.Services.Interfaces
{
    public interface ITenantContext
    {
        int TenantId { get; }
        int? BranchId { get; }
        bool IsAuthenticated { get; }
    }
}
