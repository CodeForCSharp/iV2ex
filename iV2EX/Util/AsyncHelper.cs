using System;
using System.Threading;
using System.Threading.Tasks;

namespace iV2EX.Util
{
    public static class AsyncHelper
    {
        public static async Task<T> RetryAsync<T>(Func<Task<T>> factory, int maxRetries)
        {
            for (var i = 0; i < maxRetries; i++)
            {
                try
                {
                    return await factory();
                }
                catch when (i < maxRetries - 1)
                {
                }
            }
            return await factory();
        }

        public static Action<T> Debounce<T>(Action<T> action, TimeSpan interval)
        {
            var lastCts = new CancellationTokenSource();
            return arg =>
            {
                lastCts.Cancel();
                lastCts.Dispose();
                var cts = new CancellationTokenSource();
                lastCts = cts;
                var token = cts.Token;
                Task.Delay(interval, token).ContinueWith(_ =>
                {
                    if (!token.IsCancellationRequested)
                        action(arg);
                }, TaskScheduler.Default);
            };
        }
    }
}
