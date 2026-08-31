using System.Reflection;
using System.Text.Json;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Xunit.Abstractions;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E3, "공통 배치 검사 추출 전후 원A의 hash·거부 순서를 고정한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3결정성검증, Boundary = "순수 호환 시험이며 Scene·E5 증거가 아니다.")]
public sealed class Simulation배치적합성호환Tests
{
    private readonly ITestOutputHelper output;
    public Simulation배치적합성호환Tests(ITestOutputHelper output) => this.output = output;

    internal static ISimulationFarmH2SurfaceReader Surface(SimulationFarmH2PlacementRequest r, string mode = "normal")
        => (ISimulationFarmH2SurfaceReader)Activator.CreateInstance(
            typeof(SimulationFarmH2부지확장Tests).GetNestedType("Surface", BindingFlags.NonPublic)!,
            new object[] { r, mode })!;

    [Theory]
    [InlineData(0, "Barn01")] [InlineData(1, "Barn01")] [InlineData(2, "Barn01")]
    [InlineData(0, "Barn02")] [InlineData(1, "Barn02")] [InlineData(2, "Barn02")]
    public void 원A_전체결과_관찰표본_hash를보존한다(int fixture, string barn)
    {
        var input = SimulationFarmH2부지확장Tests.Input(fixture, barn);
        var before = JsonSerializer.Serialize(input);
        var result = new SimulationFarmH2부지확장Service().ExpandAndValidate(input, Surface(input.ParentRequest));
        var plan = result.ValidatedPlacement!;
        var fingerprint = Simulation세계자산CanonicalHash.Hash(JsonSerializer.Serialize(plan));
        // CDF7A967 Adapter 추출 전에 기록한 전체 결과(관찰/변환/계획 hash 포함) 골든값.
        var golden = (fixture, barn) switch
        {
            (0, "Barn01") => "eedff30811188a66f58e51b913d013a6253dc97135236324520d56f853bd9b22",
            (1, "Barn01") => "f98c5d2ec3ba46d79a486efd63667139b86a7df9af5c81978b4a5e301ce8d207",
            (2, "Barn01") => "d17414ba5a24a1ccac62456f8b47d99fa92c7d10e80998ca126b587c568b997a",
            (0, "Barn02") => "64e4c9a11f7eb1b9b97eac075459a71ac0055c93b49f46dce357dc1baf562027",
            (1, "Barn02") => "24c60bd9187e3acbe37546dd93f3fdd2fedd86c24a66ae2ea68c1ab4595c9a6a",
            (2, "Barn02") => "f1d1fd41b2ac15b2e043c2333be66529d68bdefc9ae16ec65c06417ebc4cfd83",
            _ => throw new InvalidOperationException()
        };
        Assert.Equal(golden, fingerprint);
        output.WriteLine("GOLDEN|" + fixture + "|" + barn + "|" + fingerprint);
        Assert.Equal(before, JsonSerializer.Serialize(input));
        Assert.Equal(fingerprint, Simulation세계자산CanonicalHash.Hash(JsonSerializer.Serialize(
            new SimulationFarmH2PlacementAdapter().Convert(result.CandidateRequest, Surface(result.CandidateRequest)))));
    }

    [Theory]
    [InlineData("missing", "SurfaceSupportMissingOrDenied")]
    [InlineData("steep", "SlopeTooSteep")]
    [InlineData("spread", "HeightSpreadExceeded")]
    [InlineData("route-hole", "SurfaceSupportMissingOrDenied")]
    public void 복합실패는_기존최초거부를보존한다(string mode, string expected)
    {
        var input = SimulationFarmH2부지확장Tests.Input(0);
        var result = new SimulationFarmH2부지확장Service().CreateCandidate(input, Surface(input.ParentRequest));
        // 모든 표본에 지지 실패 등을 주되 기존 변환의 최초 사유를 보존한다.
        var error = Assert.Throws<ArgumentException>(() => new SimulationFarmH2PlacementAdapter()
            .Convert(result.CandidateRequest, Surface(result.CandidateRequest, mode)));
        Assert.Contains("FarmH2:" + expected, error.Message);
    }

    [Theory]
    [InlineData(0.5)] // 실제 겹침
    [InlineData(1.0)] // 경계 접촉
    [InlineData(1.54)] // 최소 간격 미달
    public void 공유원시검사_B대B와_B대A_점유거부(double secondMinX)
    {
        var helper = typeof(SimulationFarmH2PlacementAdapter).Assembly.GetType(
            "Ssalddel.Simulation.Application.Simulation배치적합성검사")!;
        var box = helper.GetNestedType("Box", BindingFlags.NonPublic)!;
        var values = Array.CreateInstance(box, 2);
        values.SetValue(Activator.CreateInstance(box, new object[] { 0d, 0d, 1d, 1d }), 0);
        values.SetValue(Activator.CreateInstance(box, new object[] { secondMinX, 0d, secondMinX + 1, 1d }), 1);
        var method = helper.GetMethod("ValidateSpacing", BindingFlags.NonPublic | BindingFlags.Static)!;
        var exception = Assert.Throws<TargetInvocationException>(() => method.Invoke(null, new object[] { values, .55 }));
        Assert.Contains("ObjectOverlapOrSpacing", exception.InnerException!.Message);
    }
}
