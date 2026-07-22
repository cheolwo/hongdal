using Ssalddel.Domain.Community;
using 살뜰.Data;

namespace Ssalddel.Services.Community;

internal readonly record struct CommunityPostMutationCapabilities(
    bool CanEdit,
    bool EditRequiresPassword,
    bool CanDelete,
    bool DeleteRequiresPassword);

internal static class CommunityPostMutationAccessPolicy
{
    public static CommunityPostMutationCapabilities Resolve(
        PlatformCommunityPost post,
        string? currentUserId,
        string? currentUserRole)
    {
        if (CommunityLedgerCompletionPublication.IsSystemPost(post))
        {
            return default;
        }

        var isAdministrator = string.Equals(
            currentUserRole,
            역할명.서버관리자,
            StringComparison.OrdinalIgnoreCase);
        var isAnonymousPost = string.IsNullOrWhiteSpace(post.AuthorUserId);
        var isAuthor = !isAnonymousPost
                       && !string.IsNullOrWhiteSpace(currentUserId)
                       && string.Equals(
                           post.AuthorUserId,
                           currentUserId,
                           StringComparison.Ordinal);

        return new CommunityPostMutationCapabilities(
            CanEdit: isAnonymousPost || isAuthor,
            EditRequiresPassword: isAnonymousPost || isAuthor,
            CanDelete: isAdministrator || isAnonymousPost || isAuthor,
            DeleteRequiresPassword: isAnonymousPost && !isAdministrator);
    }
}
