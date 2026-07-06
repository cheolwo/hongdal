using Hongdal.Contracts.Common.PublicData;

namespace 홍달.Services.External.PublicData;

public interface IPublicDataApiMetadataCatalog
{
    PublicDataApiMetadataResponse GetCatalog(PublicDataApiMetadataQuery query);
}
