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
            var sw = new Stopwatch();

            for (int i = 0; i < 10; i++) {
                sw.Start();
                processes.GetAll();
                sw.Stop();
                _testOutputHelper.WriteLine($"ms: {sw.ElapsedMilliseconds}");
                sw.Reset();
            }
        }
    }
}