using System;
using System.Diagnostics;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Threading;
using QX.Zoo.FabricHost.Infrastructure;

namespace QX.Zoo.FabricHost
{
    public class Program
    {
        public static void Main()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            try
            {
                var task = ServiceRuntime.RegisterServiceAsync(
                    "QX.Zoo.FabricHost", 
                    ctx => new StatelessServiceFabricHost(ctx), 
                    TimeSpan.FromSeconds(300), 
                    cancellationTokenSource.Token);

                task.ContinueWith(t =>
                {
                    if (cancellationTokenSource.IsCancellationRequested)
                    {
                        cancellationTokenSource.CancelAfter(1);
                    }
                }, cancellationTokenSource.Token);

                task.GetAwaiter().GetResult();

                cancellationTokenSource.Token.WaitHandle.WaitOne();
            }
            catch (Exception e)
            {
                Trace.TraceError($"FabricHost fatal error: {e}");
            }
        }
    }
}
