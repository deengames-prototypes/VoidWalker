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
    private readonly IConsole _console;

    private readonly List<Character> _party;
    private readonly char[] validInputs = ['a', 'i', 's', 'd'];

    public CharacterTurnProcessor(IGame game, IConsole console, List<Character> party, List<Monster> monsters)
    {
        ArgumentNullException.ThrowIfNull(game);
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(party);
        ArgumentNullException.ThrowIfNull(monsters);
        
        _game = game;
        _console = console;
        _party = party;
        _monsters = monsters;
    }

    internal string ProcessTurnFor(Character character)
    {
        char input = ' ';

        while (!validInputs.Contains(input))
        {
            _console.WriteLine($"{character.Name}'s turn. Pick an action:");
            _console.WriteLine($"    [{Colours.Command}]A:[/] attack");
            _console.WriteLine($"    [{Colours.Command}]S:[/] skill");
            _console.WriteLine($"    [{Colours.Command}]D:[/] defend");
            _console.WriteLine($"    [{Colours.Command}]I:[/] item");
            input = _console.ReadKey();
        }

        IEnumerable<Entity> targets; 

        switch(input)
        {
            case 'a':
                targets = [PickTargetMonster()];
                return Attack(character, targets.First() as Monster);
            case 'd':
                character.Defend();
                return $"{character.Name} defends!";
            case 's':
                // Assumes you get back a valid skill: something you have SP for.
                var skill = PickSkillFor(character);
                if (character.CurrentSkillPoints < skill.Cost)
                {
                    _console.WriteLine("You don't have enough skill points for that!");
                    // Recursion is risky, very risky ... hmm.
                    return ProcessTurnFor(character);
                }
                targets = PickTargetsFor(skill);
                // Depending on the skill, the target is an instance of Character or Monster.
                // For now, assume monster.
                return SkillApplier.Apply(character, skill, targets);
            case 'i':
                foreach (var message in new ShowInventoryCommand(_console, true).Execute(_game, _party))
                {
                    // Special case: show immediately because it requires input from the player.
                    _console.WriteLine(message);
                }
                return string.Empty;
            default:
                return string.Empty;
        }
    }

    private IEnumerable<Entity> PickTargetsFor(Skill skill)
    {
        switch (skill.Target)
        {
            case null:
            case "":
            case "Single":
            case "Enemy":
            case "Monster": // TODO: it's Enemy now, will switch
                return [PickTargetMonster()];
            case "All":
                return _monsters.Where(m => m.CurrentHealth > 0);
            case "Character":
                return [PickTargetCharacter()];
            case "Party":
                return _party;
            default:
                throw new InvalidOperationException($"TextBlade doesn't know how to pick a target of type: {skill.Target ?? "(null)"}");
        }       
    }

    /// <summary>
    /// Lets the player pick from any character, dead or alive.
    /// </summary>
    /// <returns></returns>
    private Character PickTargetCharacter()
    {
        var validTargets = _party;
        return PickFromList(validTargets);
    }

    private Entity PickTargetMonster()
    {
        var validTargets = _monsters.Where(m => m.CurrentHealth > 0);
        if (!validTargets.Any())
        {
            throw new InvalidOperationException("Character's turn when all monsters are dead.");
        }

        // Refactor: we use this "pick a valid int from this list" everywhere. DRY.
        return PickFromList(validTargets);
    }

    private T PickFromList<T>(IEnumerable<T> items)
    {
        _console.WriteLine("Pick a target:");

        for (int i = 0; i < items.Count(); i++)
        {
            var item = items.ElementAt(i);
            _console.WriteLine($"    {i + 1}: {item}");
        }

        int target;
        while (!int.TryParse(_console.ReadKey().ToString(), out target) || target == 0 || target > items.Count())
        {
            _console.WriteLine($"That's not a valid number! Enter a number from 1 to {items.Count()}: ");
        }

        return items.ElementAt(target - 1);
    }

    private Skill PickSkillFor(Character character)
    {
        _console.WriteLine("Pick a skill:");
        var skill = PickFromList(character.Skills);
        return skill;
    }

    // TODO: extract
    private string Attack(Character character, Monster targetMonster)
    {
        ArgumentNullException.ThrowIfNull(targetMonster);

        // Assume target number is legit
        var message = new StringBuilder();
        message.Append($"{character.Name} attacks {targetMonster.Name}! ");
        
        var damage = character.TotalStrength - targetMonster.Toughness;
        
        var characterWeapon = character.EquippedOn(Inv.ItemType.Weapon);
        // TODO: DRY with SkillApplier
        var effectiveMessage = "";
        if (characterWeapon?.DamageType == targetMonster.Weakness)
        {
            effectiveMessage = "[#f80]Super effective![/]";

            damage *= 2;
        }

        targetMonster.Damage(damage);
        
        var damageAmount = damage <= 0 ? "NO" : damage.ToString();
        message.Append($"[{Colours.Highlight}]{damageAmount}[/] damage! {effectiveMessage}");
        if (targetMonster.CurrentHealth <= 0)
        {
            message.Append($"{targetMonster.Name} DIES!");
        }
        
        return message.ToString();
    }
}
