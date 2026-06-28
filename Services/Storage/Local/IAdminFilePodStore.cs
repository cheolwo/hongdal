namespace 홍달.Services.Storage.Local
{
    public interface IAdminFilePodStore
    {
        AdminFilePodMetadata Add(AdminFilePodMetadata item);
        IReadOnlyList<AdminFilePodMetadata> List(string? fileType = null, string? requestId = null);
        AdminFilePodMetadata? UpdateStatus(Guid id, string status);
    }
}
