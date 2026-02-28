using System.Threading.Tasks;
using UnityEditor;

namespace CodeMode.Editor.Utilities
{
    /// <summary>
    /// Async utilities for Unity Editor that preserve main-thread execution,
    /// replacing UniTask.Yield() / UniTask.DelayFrame() with EditorApplication.delayCall-based awaiters.
    /// </summary>
    public static class EditorAsync
    {
        /// <summary>
        /// Yields execution until the next editor frame, resuming on the main thread.
        /// Equivalent to UniTask.Yield() / UniTask.DelayFrame(1) in the editor context.
        /// <para>
        /// Memory-safe: EditorApplication.delayCall is a fire-once delegate — Unity automatically
        /// clears all registered callbacks after invoking them, so no manual unsubscription is needed.
        /// </para>
        /// </summary>
        public static Task Yield()
        {
            var tcs = new TaskCompletionSource<bool>();
            // delayCall fires once then Unity clears it — no explicit -= required
            EditorApplication.delayCall += () => tcs.TrySetResult(true);
            return tcs.Task;
        }
    }
}
