using Hongdal.Contracts.Shipper.Request;

namespace Hongdal.Application.Shipper.Request;

public sealed class 화주운송의뢰일괄등록파싱결과
{
    public IReadOnlyList<화주운송의뢰일괄등록행입력> 행목록 { get; }
    public IReadOnlyList<string> 오류목록 { get; }

    public 화주운송의뢰일괄등록파싱결과(IReadOnlyList<화주운송의뢰일괄등록행입력> 행목록, IReadOnlyList<string> 오류목록)
    {
        this.행목록 = 행목록;
        this.오류목록 = 오류목록;
    }
}
