using Microsoft.Extensions.Options;
using Ssalddel.Services.FoodCulture;
using 살뜰.Services.External.Mfds;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.FoodCulture;

public sealed class MfdsImportedFoodIngredientCompanySourceTests
{
    [Fact]
    public async Task 수입표시의_해외제조업소는_명칭과국가가일치할때만_시설코드를보강한다()
    {
        var source = new MfdsImportedFoodIngredientCompanySource(
            new FakeLabelService(),
            new FakeFacilityService(),
            Options.Create(new 수입식품한글표시사항조회Options { ServiceKey = "test-key" }),
            Options.Create(new 해외제조업소조회Options { ServiceKey = "test-key" }),
            Options.Create(new PublicDataOptions
            {
                MfdsIngredientCompanies = new MfdsIngredientCompanyOptions
                {
                    MaxForeignFacilityLookups = 5
                }
            }));

        var result = await source.SearchAsync("참깨", 10);

        var record = Assert.Single(result.Records);
        Assert.True(result.RegistryLookupAttempted);
        Assert.False(result.RegistryLookupFailed);
        Assert.Equal("한국수입", record.ImporterName);
        Assert.Equal("GLOBAL SESAME FOODS", record.ForeignManufacturerName);
        Assert.Equal("US-FOOD-10", record.ForeignManufacturerIdentifier);
        Assert.Equal("CALIFORNIA", record.ForeignManufacturerAreaName);
        Assert.Equal("100 SESAME ROAD, CALIFORNIA", record.ForeignManufacturerAddress);
        Assert.True(record.ForeignManufacturerRegistryMatched);
    }

    private sealed class FakeLabelService : I수입식품한글표시사항조회Service
    {
        public Task<수입식품한글표시사항조회응답DTO> 조회Async(
            수입식품한글표시사항조회요청DTO 요청,
            CancellationToken 취소토큰 = default)
        {
            Assert.Equal("참깨", 요청.원재료명);
            return Task.FromResult(new 수입식품한글표시사항조회응답DTO
            {
                항목목록 =
                [
                    new 수입식품한글표시사항조회항목DTO
                    {
                        수입업체명 = "한국수입",
                        해외제조업소명 = "GLOBAL SESAME FOODS",
                        제조국명 = "미국",
                        한글제품명 = "참깨 페이스트",
                        품목명 = "기타가공품",
                        원재료명 = "참깨 100%",
                        처리일자 = "20260718"
                    }
                ]
            });
        }
    }

    private sealed class FakeFacilityService : I해외제조업소조회Service
    {
        public Task<해외제조업소조회응답> 조회Async(
            해외제조업소조회요청 요청,
            CancellationToken 취소토큰 = default)
        {
            Assert.Equal("GLOBAL SESAME FOODS", 요청.해외제조업소명);
            Assert.Equal("미국", 요청.국가명);
            return Task.FromResult(new 해외제조업소조회응답
            {
                본문 = new 해외제조업소조회본문
                {
                    아이템 = new 해외제조업소조회아이템목록
                    {
                        항목 =
                        [
                            new 해외제조업소조회항목
                            {
                                해외제조업소코드 = "US-FOOD-10",
                                해외제조업소명 = "GLOBAL SESAME FOODS",
                                해외제조업소주소 = "100 SESAME ROAD, CALIFORNIA",
                                국가명 = "미국",
                                지역명 = "CALIFORNIA"
                            }
                        ]
                    }
                }
            });
        }
    }
}
