using System;
using System.Threading.Tasks;

namespace FormsSkiaBikeTracker.Extensions
{
    public static class TaskExtensions
    {
        public static async void Forget(this Task task, Action<Exception> errorHandler)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception e)
            {
                errorHandler?.Invoke(e);
            }
        }

        public static void ForgetAndCatch(this Task task)
        {
            task.Forget(e => Console.WriteLine($"Unhandled exception caught in ForgetAndCatch - {e.Message}"));
        }
    }
}
