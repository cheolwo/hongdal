using CommunityToolkit.Mvvm.Input;
using FDriverApp.Models;

namespace FDriverApp.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}