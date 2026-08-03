using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.PublicData;

namespace 살뜰.Services.External.PublicData;

[SsalddelCodeMetadata(
    공공데이터포털활용ApiModuleFeature.Key,
    SsalddelCodeLayer.Application,
    "활용 중 공공데이터 API의 업무 모듈과 기존 구현 연결 상태를 조회",
    ContractType = typeof(공공데이터포털활용ApiModuleResponse),
    FlowOrder = 2,
    Boundary = "외부 호출이나 활용신청 변경 없이 검증된 catalog snapshot만 조회")]
public interface I공공데이터포털활용ApiModuleCatalog
{
    공공데이터포털활용ApiModuleResponse GetCatalog(공공데이터포털활용ApiModuleQuery query);
}
