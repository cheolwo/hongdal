using Ssalddel.Contracts.CommonContents;
using 살뜰.도메인.공통콘텐츠;

namespace Ssalddel.Application.CommonContents;

internal static class 공통콘텐츠매퍼
{
    public static 살뜰위젯콘텐츠Dto ToWidgetDto(this 살뜰공통콘텐츠 entity)
    {
        return new 살뜰위젯콘텐츠Dto
        {
            콘텐츠Id = entity.Id,
            제목 = entity.제목,
            설명 = entity.설명,
            이미지Url = entity.이미지Url,
            이동Url = entity.영상Url ?? entity.외부링크Url,
            상태문구 = "살뜰과 연결됨"
        };
    }
}