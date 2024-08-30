using System.Diagnostics;
using Xunit.Abstractions;
using TaskMgrProcess = Task.Manager.System.Process;

namespace Task.Manager.System.UnitTests
{
    public class When_Using_Processes
    {
        private readonly ITestOutputHelper _testOutputHelper;
        public When_Using_Processes(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Should_Return_ProcessInfos()
        {
            var processes = new TaskMgrProcess::Processes();

            for (int i = 0; i < 10; i++) {
                var timeTaken = Time(() => processes.GetAll());
                Debug.Assert(timeTaken.Milliseconds < 25);
                _testOutputHelper.WriteLine($"ms: {timeTaken.Milliseconds}");
            }
        }

        private TimeSpan Time(Action toTime)
        {
            var timer = Stopwatch.StartNew();
            toTime();
            timer.Stop();
            return timer.Elapsed;
        }
    }
}