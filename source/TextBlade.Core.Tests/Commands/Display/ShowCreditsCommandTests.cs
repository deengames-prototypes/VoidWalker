using NUnit.Framework;
using TextBlade.Core.Commands.Display;
using TextBlade.Core.Tests.Stubs;

namespace TextBlade.Core.Tests.Commands.Display;

[TestFixture]
public class ShowCreditsCommandTests
{
    [Test]
    public void ShowCreditsCommand_ReturnsTextBladeMessage()
    {
        // Arrange
        var console = new ConsoleStub();
        var command = new ShowCreditsCommand(console);
        
        // Act
        command.Execute(null);
        var actual = console.LastMessage;

        // Assert
        Assert.That(actual, Does.Contain("TextBlade"));
        Assert.That(actual, Does.Contain("by NightBlade"));
    }
}
