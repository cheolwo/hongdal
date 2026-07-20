using Ssalddel.Contracts.Common.Community;
using Ssalddel.WebApp.Models;

namespace Ssalddel.WebApp.Services;

public sealed class 커뮤니티화물글초안가져오기Service
{
    public bool 가져올수있음(PlatformCommunityPostResponse? post)
        => post is not null
           && CommunityBoardCatalog.Find(post.Category)?.Key == CommunityBoardKeys.Cargo;

    public bool 이미반영됨(운송의뢰작성ViewModel target, long postId)
    {
        ArgumentNullException.ThrowIfNull(target);

        var sourceMarker = BuildSourceMarker(postId);
        return ContainsSourceMarker(target.화물설명, sourceMarker)
               && ContainsSourceMarker(target.절차메모, sourceMarker);
    }

    public 커뮤니티화물글초안가져오기Result 가져오기(
        운송의뢰작성ViewModel target,
        PlatformCommunityPostResponse post)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(post);

        if (!가져올수있음(post))
        {
            throw new InvalidOperationException("화물 게시판 글만 운송 의뢰 초안으로 가져올 수 있습니다.");
        }

        var cargoTypeFilled = false;
        if (string.IsNullOrWhiteSpace(target.화물종류)
            && !string.IsNullOrWhiteSpace(post.Title))
        {
            target.화물종류 = post.Title.Trim();
            cargoTypeFilled = true;
        }

        var sourceMarker = BuildSourceMarker(post.Id);
        var descriptionAdded = !ContainsSourceMarker(target.화물설명, sourceMarker);
        if (descriptionAdded)
        {
            target.화물설명 = AppendBlock(
                target.화물설명,
                BuildPostContentBlock(post, sourceMarker));
        }

        var procedureMemoAdded = !ContainsSourceMarker(target.절차메모, sourceMarker);
        if (procedureMemoAdded)
        {
            target.절차메모 = AppendBlock(
                target.절차메모,
                $"초안 출처: {sourceMarker} (사용자가 직접 가져옴)");
        }

        return new 커뮤니티화물글초안가져오기Result(
            post.Id,
            cargoTypeFilled,
            descriptionAdded,
            procedureMemoAdded);
    }

    public static string BuildSourceMarker(long postId)
        => $"커뮤니티 화물 글 #{postId}";

    private static string BuildPostContentBlock(
        PlatformCommunityPostResponse post,
        string sourceMarker)
    {
        var lines = new List<string> { $"[{sourceMarker}]" };
        if (!string.IsNullOrWhiteSpace(post.Title))
        {
            lines.Add($"제목: {post.Title.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(post.Body))
        {
            lines.Add(post.Body.Trim());
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static bool ContainsSourceMarker(string? value, string sourceMarker)
        => !string.IsNullOrWhiteSpace(value)
           && value.Contains(sourceMarker, StringComparison.Ordinal);

    private static string AppendBlock(string? current, string block)
        => string.IsNullOrWhiteSpace(current)
            ? block
            : $"{current.TrimEnd()}{Environment.NewLine}{Environment.NewLine}{block}";
}

public sealed record 커뮤니티화물글초안가져오기Result(
    long 글Id,
    bool 화물종류채움,
    bool 화물설명추가됨,
    bool 출처메모추가됨)
{
    public bool 변경됨 => 화물종류채움 || 화물설명추가됨 || 출처메모추가됨;
}
