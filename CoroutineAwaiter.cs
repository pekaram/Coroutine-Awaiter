using System.Collections;
using UnityEngine;
using System.Threading.Tasks;

public static class CoroutineAwaiter
{
    /// <summary>
    /// Wraps a coroutines in a task and awaits for it to finish or fail.
    /// Should be avoided for coroutines that might get instantiated twice at the same time.
    /// </summary>
    /// <summary>
    public static async Task ExecuteCoroutineAsync(this MonoBehaviour monoBehavior, IEnumerator coroutine)
    {
        // Execute manually until routine finishes
        while (coroutine.MoveNext())
        {
            // Only if a yield instruction pass to unity to handle it
            if (coroutine.Current.GetType().IsSubclassOf(typeof(YieldInstruction)))
            {
                await monoBehavior.ExecuteYieldInstructionAync((YieldInstruction)coroutine.Current);
            }
        }
    }

    /// <summary>
    /// Based on https://blogs.msdn.microsoft.com/appconsult/2017/09/01/unity-coroutine-tap-en-us/
    /// Translate a YieldInstruction to Task and run. It needs a Mono Behavior to locate Unity thread context.
    /// </summary>
    /// <param name="monoBehavior">The Mono Behavior that managing coroutines</param>
    /// <param name="yieldInstruction"></param>
    /// <returns>Task that can be await</returns>
    public static async Task ExecuteYieldInstructionAync(this MonoBehaviour monoBehavior, YieldInstruction yieldInstruction)
    {
        var tcs = new TaskCompletionSource<object>();
        monoBehavior
            .StartCoroutine(
                YieldForInstruction(
                    yieldInstruction,
                    tcs));
        await tcs.Task;
    }

    private static IEnumerator YieldForInstruction(YieldInstruction yieldInstruction, TaskCompletionSource<object> completion)
    {
        yield return yieldInstruction;
        completion.TrySetResult(null);
    }
}
