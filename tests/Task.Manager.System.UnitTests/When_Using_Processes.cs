using TaskMgrProcess = Task.Manager.System.Process;

namespace Task.Manager.System.UnitTests
{
    public class When_Using_Processes
    {
        [Fact]
        public void Should_Return_ProcessInfos()
        {
            var processes = new TaskMgrProcess::Processes();
            var procInfos = processes.GetAll();



        }
    }
}