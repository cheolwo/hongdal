using FDriverApp.Services;

namespace FDriverApp.Utilities
{
    public static class TaskUtilities
    {
        public static void FireAndForgetSafe(this Task task, IErrorHandler? handler = null)
        {
            ArgumentNullException.ThrowIfNull(task);
            _ = ObserveAsync(task, handler);
        }

        private static async Task ObserveAsync(Task task, IErrorHandler? handler)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                handler?.HandleError(ex);
            }
        }
    }
}
