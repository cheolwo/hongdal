using CommunityToolkit.Mvvm.Input;
using Deliver.Models;

namespace Deliver.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}