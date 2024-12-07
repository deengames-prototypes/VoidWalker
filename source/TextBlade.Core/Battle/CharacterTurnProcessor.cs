using System.Text;
using TextBlade.Core.Characters;
using TextBlade.Core.Commands.Display;
using TextBlade.Core.Game;
using TextBlade.Core.IO;

namespace TextBlade.Core.Battle;

public class CharacterTurnProcessor
{
    private readonly List<Monster> _monsters;
    private readonly IGame _game;
    private readonly List<Character> _party;

    public CharacterTurnProcessor(IGame game, List<Character> party, List<Monster> monsters)
    {
        _game = game;
        _party = party;
        _monsters = monsters;
    }

    internal string ProcessTurnFor(Character character)
    {
        Console.WriteLine($"{character.Name}'s turn. Pick an action: [a]ttack, [i]tem, [s]kill, or [d]efend");
        var input = Console.ReadKey();
        Monster target; // not going to work for healing skills

        switch(input.KeyChar)
        {
            case 'a':
            case 'A':
                target = PickTarget();
                return Attack(character, target);
            case 'd':
            case 'D':
                character.Defend();
                return $"{character.Name} defends!";
            case 's':
            case 'S':
                // Assumes you get back a valid skill: something you have SP for.
                var skill = PickSkillFor(character);
                target = PickTarget();
                // Depending on the skill, the target is an instance of Character or Monster.
                // For now, assume monster.
                return SkillApplier.Apply(character, skill, target);
            case 'i':
            case 'I':
                foreach (var message in new ShowInventoryCommand(true).Execute(_game, _party))
                {
                    // Special case: show immediately because it requires input from the player.
                    Console.WriteLine(message);
                }
                return string.Empty;
            default:
                return string.Empty;
        }
    }

    private Monster PickTarget()
    {
        var validTargets = _monsters.Where(m => m.CurrentHealth > 0);
        if (!validTargets.Any())
        {
            throw new InvalidOperationException("Character's turn when all monsters are dead.");
        }

        Console.WriteLine("Pick a target:");

        for (int i = 0; i < validTargets.Count(); i++)
        {
            var monster = validTargets.ElementAt(i);
            Console.WriteLine($"    {i+1}: {monster.Name} ({monster.CurrentHealth}/{monster.TotalHealth} health)");
        }

        var target = 0;
        while (target == 0 || target > validTargets.Count())
        {
            if (!int.TryParse(Console.ReadKey().KeyChar.ToString().Trim(), out target))
            {
                Console.WriteLine($"That's not a valid number! Enter a number from 1 to {validTargets.Count()}: ");
            }
        }

        return validTargets.ElementAt(target - 1);
    }

    private static Skill PickSkillFor(Character character)
    {
        Console.WriteLine("Pick a skill:");
        for (int i = 0; i < character.Skills.Count; i++)
        {
            Console.WriteLine($"    {i+1}: {character.SkillNames[i]}");
        }

        var target = int.Parse(Console.ReadKey().KeyChar.ToString());
        // Assume target is valid
        return character.Skills[target - 1];
    }

    private string Attack(Character character, Monster targetMonster)
    {
        // Assume target number is legit
        var message = new StringBuilder();
        message.Append($"{character.Name} attacks {targetMonster.Name}! ");
        
        var damage = character.TotalStrength - targetMonster.Toughness;
        targetMonster.Damage(damage);
        
        var damageAmount = damage <= 0 ? "NO" : damage.ToString();
        message.Append($"[{Colours.Highlight}]{damageAmount}[/] damage!");
        if (targetMonster.CurrentHealth <= 0)
        {
            message.Append($"{targetMonster.Name} DIES!");
        }
        
        return message.ToString();
    }
}
