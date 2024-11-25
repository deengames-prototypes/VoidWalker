using TextBlade.Core.Characters;
using TextBlade.Core.Game;
using TextBlade.Core.IO;

namespace TextBlade.Core.Commands;

public class SleepAtInnCommand : ICommand
{
    private readonly int _innCost = 0;

    public SleepAtInnCommand(int innCost)
    {
        _innCost = innCost;
    }

    public async IAsyncEnumerable<string> Execute(IGame game, List<Character> party)
    {
        // Check if we have enough gold. Subtract if we do!

        foreach (var character in party)
        {
            character.CurrentHealth = character.TotalHealth;
        }

        yield return $"You sleep at the inn. All party members have recovered to [{Colours.Highlight}]full health[/]!";
    }
}
