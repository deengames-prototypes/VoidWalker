using TextBlade.Core.Commands;
using TextBlade.Core.Locations;

namespace TextBlade.ConsoleRunner.IO;

public static class InputProcessor
{
    public static ICommand PromptForAction(Location currentLocation)
    {
        Console.Write("Enter a command, or the number of your destination: ");
        var rawResponse = Console.ReadLine().Trim().ToLowerInvariant();
        
        // It's some special command that the location handles. That doesn't change location.
        var command = currentLocation.GetCommandFor(rawResponse);
        if (!(command is DoNothingCommand))
        {
            return command;
        }

        // No? Maybe it's a destination?
        int destinationOption;
        if (int.TryParse(rawResponse, out destinationOption))
        {
            // Assume it's valid
            var destination = currentLocation.LinkedLocations[destinationOption - 1];
            return new ChangeLocationCommand(destination.Id);
        }

        // Nah, nah, it's just a global command.
        // If you update this, update the help listing in ShowHelpCommand.
        switch (rawResponse)
        {
            case "credits":
            return new ShowCreditsCommand();
            case "quit":
            case "q":
                return new QuitGameCommand();
            case "help":
            case "h":
            case "p":
                return new ShowPartyStatusCommand();
            case "?":
                return new ShowHelpCommand();
        }

        return new DoNothingCommand();
    }
}
