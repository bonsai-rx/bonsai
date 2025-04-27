using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Editor
{
    static class TaskHelper
    {
        public static Task ToTask(this CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<object>();
            cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
            return tcs.Task;
        }
    }
}
