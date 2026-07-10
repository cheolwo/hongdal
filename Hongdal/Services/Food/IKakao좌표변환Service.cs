namespace Hongdal.Services.Food;

public interface IKakao좌표변환Service
{
    Task<(double 위도, double 경도)?> 도로명주소좌표변환Async(string 주소, CancellationToken cancellationToken = default);

    Task<Kakao주소정보?> 주소정보조회Async(string 주소, CancellationToken cancellationToken = default);

    Task<Kakao지역정보?> 좌표지역정보조회Async(decimal 위도, decimal 경도, CancellationToken cancellationToken = default);
}

public sealed record Kakao주소정보(
    string 전체주소,
    string 도로명주소,
    string Region1,
    string Region2,
    string Region3,
    decimal? 위도,
    decimal? 경도);

public sealed record Kakao지역정보(
    string Region1,
    string Region2,
    string Region3,
    string AddressName);
