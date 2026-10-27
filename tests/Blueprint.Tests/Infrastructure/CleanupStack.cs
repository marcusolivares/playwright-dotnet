using System.Collections.Concurrent;

namespace Blueprint.Tests.Infrastructure;

/// <summary>
/// Clean-up steps registered while a test runs ("remove the favorite I added"). They run in reverse order after the
/// test, pass or fail, and one failing step does not stop the others.
/// </summary>
public sealed class CleanupStack
{
    private readonly ConcurrentStack<Func<Task>> _steps = new();

    public void Push(Func<Task> step) => _steps.Push(step);

    public async Task RunAsync()
    {
        var failures = new List<Exception>();

        while (_steps.TryPop(out var step))
        {
            try
            {
                await step();
            }
            catch (Exception exception)
            {
                failures.Add(exception);
            }
        }

        if (failures.Count > 0)
        {
            throw new AggregateException("Test clean-up failed; data created by this test may be left behind.", failures);
        }
    }
}
