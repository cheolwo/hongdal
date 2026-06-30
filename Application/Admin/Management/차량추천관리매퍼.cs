using Hongdal.Contracts.Admin.Management;
using Hongdal.Contracts.Shipper.Request;
using 홍달.도메인.차량;
using 홍달.도메인.운송;

namespace Hongdal.Application.Admin.Management;

internal static class 차량추천관리매퍼
{
    internal static 차량단가응답 To응답(차량단가 entity)
    {
        return new 차량단가응답
        {
            Id = entity.Id,
            차량종류 = entity.차량종류,
            기본운임 = entity.기본운임,
            Km당단가 = entity.Km당단가,
            야간할증 = entity.야간할증,
            우천할증 = entity.우천할증,
            최소운임 = entity.최소운임
        };
    }

    internal static 차량추천기준응답 To응답(차량제원 entity)
    {
        return new 차량추천기준응답
        {
            차량코드 = entity.차량코드,
            차량명 = entity.차량명,
            차급 = entity.차급,
            차체형태 = entity.차체형태,
            적재함길이Mm = entity.적재함길이Mm,
            적재함폭Mm = entity.적재함폭Mm,
            적재함높이Mm = entity.적재함높이Mm,
            최대적재중량Kg = entity.최대적재중량Kg,
            운영권장중량Kg = entity.운영권장중량Kg,
            팔레트적재개수 = entity.팔레트적재개수,
            계산CBM = CalculatePhysicalCbm(entity),
            권장최대CBM = entity.권장최대CBM,
            추천우선순위 = entity.추천우선순위,
            추천사용여부 = entity.추천사용여부
        };
    }

    internal static 차량추천시뮬레이션응답 To응답(차량추천응답 source)
    {
        return new 차량추천시뮬레이션응답
        {
            추천차량종류 = source.추천차량종류,
            추정화물부피Cbm = source.추정화물부피Cbm,
            추천사유 = source.추천사유,
            경고목록 = source.경고목록,
            후보목록 = source.후보목록
                .Select(x => new 차량추천시뮬레이션후보응답
                {
                    차량코드 = x.차량코드,
                    차량종류 = x.차량종류,
                    우선순위 = x.우선순위,
                    적재가능중량Kg = x.적재가능중량Kg,
                    적재가능부피Cbm = x.적재가능부피Cbm,
                    적재가능팔레트개수 = x.적재가능팔레트개수,
                    설명 = x.설명
                })
                .ToArray()
        };
    }

    internal static decimal? CalculatePhysicalCbm(차량제원 entity)
    {
        if (entity.적재함길이Mm <= 0 || entity.적재함폭Mm <= 0 || entity.적재함높이Mm.GetValueOrDefault() <= 0)
        {
            return null;
        }

        var cbm = (entity.적재함길이Mm / 1000m)
                  * (entity.적재함폭Mm / 1000m)
                  * (entity.적재함높이Mm!.Value / 1000m);
        return decimal.Round(cbm, 3, MidpointRounding.AwayFromZero);
    }
}
