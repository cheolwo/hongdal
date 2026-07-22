using Ssalddel.Contracts.Shipper.Request;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class ShipperRequestDetailPresentationTests
{
    [Fact]
    public void 서버응답은_결제와증빙원본필드를_공용Snapshot으로보존한다()
    {
        var registeredAt = new DateTime(2026, 7, 22, 9, 30, 0, DateTimeKind.Utc);
        var source = new 화주운송의뢰응답
        {
            의뢰Id = "HD-DETAIL-1",
            의뢰상태 = "하차완료",
            결제상태 = "결제완료",
            정산상태 = "정산대기",
            배차상태 = "하차완료",
            인수증번호 = "POD-20260722-1",
            인수증등록일시 = registeredAt,
            세금계산서필요 = true,
            화물길이Mm = 1200,
            화물폭Mm = 800,
            화물높이Mm = 900,
            요약 = new 화주운송의뢰응답.요약DTO { 화물종류 = "식자재" }
        };

        var snapshot = ShipperRequestDetailSnapshot.FromContract(source);
        var proofs = ShipperRequestDetailPresentation.BuildProofs(snapshot);

        Assert.Equal("HD-DETAIL-1", snapshot.RequestId);
        Assert.Equal("식자재", snapshot.CargoType);
        Assert.Equal("1,200 × 800 × 900 mm", snapshot.CargoDimensions);
        Assert.Contains(proofs, proof => proof.Title == "인수증" && proof.State == ShipperRequestProgressState.Completed);
        Assert.Contains(proofs, proof => proof.Title == "세무 증빙 조건" && proof.State == ShipperRequestProgressState.Active);
    }

    [Fact]
    public void 진행이력은_저장된상태를_결제부터정산까지한번만분류한다()
    {
        var snapshot = new ShipperRequestDetailSnapshot
        {
            RequestId = "HD-DETAIL-2",
            PaymentStatus = "결제완료",
            DispatchStatus = "운송중",
            RequestStatus = "운송중",
            SettlementStatus = "정산대기"
        };

        var timeline = ShipperRequestDetailPresentation.BuildTimeline(snapshot);

        Assert.Equal(6, timeline.Count);
        Assert.Equal(ShipperRequestProgressState.Completed, timeline.Single(step => step.Title == "결제").State);
        Assert.Equal(ShipperRequestProgressState.Completed, timeline.Single(step => step.Title == "상차").State);
        Assert.Equal(ShipperRequestProgressState.Active, timeline.Single(step => step.Title == "하차").State);
        Assert.Equal(ShipperRequestProgressState.Active, timeline.Single(step => step.Title == "정산").State);
    }

    [Theory]
    [InlineData("상차완료", "결제대기", true)]
    [InlineData("운송중", "결제완료", false)]
    [InlineData("배차대기", "결제대기", false)]
    public void FakePG가능여부는_상차와결제확보상태를함께검사한다(
        string dispatchStatus,
        string paymentStatus,
        bool expected)
    {
        Assert.Equal(expected, ShipperRequestDetailPresentation.CanPay(dispatchStatus, paymentStatus));
    }
}
