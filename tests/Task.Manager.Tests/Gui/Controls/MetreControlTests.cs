using Moq;
using Task.Manager.Gui.Controls;
using Task.Manager.System;
using Task.Manager.Tests.Common;
using Xunit.Abstractions;

namespace Task.Manager.Tests.Gui.Controls;

public sealed class MetreControlTests
{
    private readonly ITestOutputHelper outputHelper;

    public MetreControlTests(ITestOutputHelper outputHelper) =>
        this.outputHelper = outputHelper;

    [Fact]
    public void Constructor_With_Valid_Args_Initialises_Successfully()
    {
        MetreControl ctrl = new(new Mock<ISystemTerminal>().Object); 
        Assert.NotNull(ctrl);
    }

    [Fact]
    public void Constructor_With_Null_Terminal_Throws_ArgumentNullException() =>
        Assert.Throws<ArgumentNullException>(() =>
            new MetreControl(null!));
    
    [Fact]
    public void Should_Draw_Metre()
    {
        Mock<ISystemTerminal> terminal = new();
        terminal.Setup(t => t.WindowWidth).Returns(64);
        terminal.Setup(t => t.WindowHeight).Returns(24);
        
        MetreControl ctrl = new(terminal.Object) {
            DrawStacked = false,
            Height = 1,
            Width = 20,
            MetreStyle = MetreControlStyle.Dots,
            LabelSeries1 = "k/u",
            ColourSeries1 = ConsoleColor.Green,
            PercentageSeries1 = 0.5,
            Text = "Cpu"
        };
        
        ctrl.Draw();

        terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("Cpu"))), Times.Once);
        terminal.Verify(t => t.Write(It.Is<char>(c => c == '[')), Times.Once);
        terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("Cpu"))), Times.Once);
        terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("k/u"))), Times.Once);
        terminal.Verify(t => t.Write(It.Is<char>(c => c == ']')), Times.Once);

        MockInvocationsHelper.WriteInvocations(terminal.Invocations, outputHelper);
    }

    [Fact]
    private void Should_Draw_Metre_With_Bogus_Percentage()
    {
        Mock<ISystemTerminal> terminal = new();
        terminal.Setup(t => t.WindowWidth).Returns(64);
        terminal.Setup(t => t.WindowHeight).Returns(24);
        
        MetreControl ctrl = new(terminal.Object) {
            DrawStacked = false,
            Height = 1,
            Width = 20,
            MetreStyle = MetreControlStyle.Dots,
            LabelSeries1 = "553648131.2%",
            ColourSeries1 = ConsoleColor.Green,
            PercentageSeries1 = 5536481.31,
            Text = "Gpu"
        };
        
        ctrl.Draw();

        terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("Gpu"))), Times.Once);
        terminal.Verify(t => t.Write(It.Is<char>(c => c == '[')), Times.Once);
        terminal.Verify(t => t.Write(It.Is<char>(c => c == ']')), Times.Once);

        MockInvocationsHelper.WriteInvocations(terminal.Invocations, outputHelper);
    } 
}