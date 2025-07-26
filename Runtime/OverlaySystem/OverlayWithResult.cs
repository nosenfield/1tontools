using System;
using System.Threading.Tasks;

namespace OneTon.OverlaySystem
{
    public abstract class OverlayWithResult<T> : OverlayBase
    {
        private TaskCompletionSource<T> _tcs;

        /// <summary>
        /// Await the result of this overlay.
        /// </summary>
        public Task<T> WaitForResult()
        {
            if (_tcs != null)
                return _tcs.Task;

            _tcs = new TaskCompletionSource<T>();
            return _tcs.Task;
        }

        /// <summary>
        /// Call this to complete the overlay and return a result.
        /// </summary>
        protected void Resolve(T result)
        {
            if (_tcs == null || _tcs.Task.IsCompleted)
                return;

            _tcs.TrySetResult(result);
            Cleanup();
        }

        protected void Cancel()
        {
            if (_tcs == null || _tcs.Task.IsCompleted)
                return;

            _tcs.TrySetCanceled();
            Cleanup();
        }

        protected void Fail(Exception ex)
        {
            if (_tcs == null || _tcs.Task.IsCompleted)
                return;

            _tcs.TrySetException(ex);
            Cleanup();
        }
    }
}