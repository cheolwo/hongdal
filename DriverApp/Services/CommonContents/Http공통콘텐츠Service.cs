using Ssalddel.Contracts.CommonContents;

namespace DriverApp.Services.CommonContents;

/// <summary>기사 앱 공통 콘텐츠를 서버 원본과 동기화합니다.</summary>
public sealed class Http공통콘텐츠Service(IDriverApiClient client) : I공통콘텐츠Service
{
    private const string BasePath = "api/v1/app/common-contents";

    public Task<살뜰위젯콘텐츠Dto?> 위젯콘텐츠조회Async(
        string 위치,
        CancellationToken cancellationToken = default)
        => client.GetAsync<살뜰위젯콘텐츠Dto>(
            $"{BasePath}/widget?역할=driver&위치={Uri.EscapeDataString(위치)}",
            "기사 공통 콘텐츠 조회",
            cancellationToken);

    public async Task<long?> 시청시작Async(
        long 콘텐츠Id,
        int 영상전체초,
        CancellationToken cancellationToken = default)
        => (await client.PostAsync<콘텐츠시청시작Request, 콘텐츠시청시작Result>(
            $"{BasePath}/{콘텐츠Id}/watch/start",
            new 콘텐츠시청시작Request { 영상전체초 = 영상전체초 },
            "공통 콘텐츠 시청 시작",
            cancellationToken))?.세션Id;

    public Task 시청진행저장Async(
        long 세션Id,
        int 현재시청초,
        CancellationToken cancellationToken = default)
        => client.PostAsync(
            $"{BasePath}/watch/{세션Id}/progress",
            new 콘텐츠시청진행Request { 현재시청초 = 현재시청초 },
            "공통 콘텐츠 시청 진행 저장",
            cancellationToken);

    public Task<콘텐츠시청완료Result?> 시청완료Async(
        long 세션Id,
        CancellationToken cancellationToken = default)
        => client.PostAsync<콘텐츠시청완료Result>(
            $"{BasePath}/watch/{세션Id}/complete",
            "공통 콘텐츠 시청 완료",
            cancellationToken);

    public async Task<살뜰위젯콘텐츠Dto?> 위젯콘텐츠동기화Async(
        string 위치,
        CancellationToken cancellationToken = default)
    {
        var content = await 위젯콘텐츠조회Async(위치, cancellationToken);
        if (content is null)
        {
            return null;
        }

#if ANDROID
        var context = global::Android.App.Application.Context;
        var preferences = context.GetSharedPreferences(
            "ssalddel_widget",
            global::Android.Content.FileCreationMode.Private);
        preferences.Edit()!
            .PutString("title", content.제목)
            .PutString("status", content.상태문구)
            .PutString("image_path", string.Empty)
            .Apply();

        var manager = global::Android.Appwidget.AppWidgetManager.GetInstance(context);
        var component = new global::Android.Content.ComponentName(
            context,
            Java.Lang.Class.FromType(typeof(global::DriverApp.살뜰위젯Provider)));
        var ids = manager.GetAppWidgetIds(component);
        if (ids.Length > 0)
        {
            var intent = new global::Android.Content.Intent(
                context,
                typeof(global::DriverApp.살뜰위젯Provider));
            intent.SetAction(global::Android.Appwidget.AppWidgetManager.ActionAppwidgetUpdate);
            intent.PutExtra(global::Android.Appwidget.AppWidgetManager.ExtraAppwidgetIds, ids);
            context.SendBroadcast(intent);
        }
#endif

        return content;
    }
}
