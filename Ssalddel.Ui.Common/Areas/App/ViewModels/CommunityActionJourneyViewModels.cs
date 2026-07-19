using CommunityToolkit.Mvvm.ComponentModel;
using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public sealed class CommunityPostJourneyCollectionViewModel : ObservableObject
{
    public Dictionary<long, CommunityActionJourneyResponse> Items { get; } = [];

    public CommunityActionJourneyResponse? Find(long postId)
        => Items.GetValueOrDefault(postId);

    public void Set(long postId, CommunityActionJourneyResponse? journey)
    {
        if (journey is null)
        {
            Items.Remove(postId);
        }
        else
        {
            Items[postId] = journey;
        }

        OnPropertyChanged(nameof(Items));
    }
}
