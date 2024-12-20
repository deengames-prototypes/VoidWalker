using TextBlade.Core.Characters;
using TextBlade.Core.Game;
using TextBlade.Core.IO;

namespace TextBlade.Core.Commands.Display;

public class ShowHelpCommand : ICommand
{
    private readonly IConsole _console;

    public ShowHelpCommand(IConsole console)
    {
        _console = console;
    }

    private readonly Dictionary<string, string> _knownCommands = new()
    {
        { "help", "Shows this detailed help text"},
        { "look", "Check where you are"},
        { "inv", "Open your inventory to equip or use items"},
        { "party/status", "See your party's status"},
        { "save", "Save the game"},
        { "quit", "Quits the game" },
        { "credits", "Shows the credits" },
    };

    public bool Execute(SaveData saveData)
    {
        var helpText = new List<string>
        {
            // If you update this, update the huge case statement in InputProcessor for commands.
            $"Each location lists other locations you can visit; use [{Colours.Command}]numbers[/] to indicate where to travel.",
            $"Some locations have location-specific keys, like [{Colours.Command}]S[/] to sleep at inns, so watch out for those.",
            "The following commands are also available:"
        };

        foreach (var t in helpText)
        {
            _console.WriteLine(t);
        }

        foreach (var command in _knownCommands.Keys)
        {
            var explanation = _knownCommands[command];
            _console.WriteLine($"    [{Colours.Command}]{command}[/]: {explanation}");
        }

        return true;
    }
}
