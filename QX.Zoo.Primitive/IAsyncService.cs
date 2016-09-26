using System;
using System.Threading.Tasks;

namespace QX.Zoo.Primitive
{
    public interface IAsyncService
    {
        Task StartServiceAsync(Action<IAsyncService, Exception> asyncServiceFailureCallback = null);
        Task StopServiceAsync();
    }
}
