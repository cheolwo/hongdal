using Ssalddel.Contracts.Common.PublicData;

namespace 살뜰.Services.External.PublicData;

public interface IPublicDataApiMetadataCatalog
{
    PublicDataApiMetadataResponse GetCatalog(PublicDataApiMetadataQuery query);
}
