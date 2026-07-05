using Hongdal.Contracts.CommonContents;
using 홍달.도메인.공통콘텐츠;

namespace Hongdal.Application.CommonContents;

internal static class 공통콘텐츠매퍼
{
    public static 홍달위젯콘텐츠Dto ToWidgetDto(this 홍달공통콘텐츠 entity)
    {
        return new 홍달위젯콘텐츠Dto
        {
            콘텐츠Id = entity.Id,
            제목 = entity.제목,
            설명 = entity.설명,
            이미지Url = entity.이미지Url,
            이동Url = entity.영상Url ?? entity.외부링크Url,
            상태문구 = "홍달과 연결됨"
        };
    }
}