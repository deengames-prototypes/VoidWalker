using TextBlade.Core.Commands;

namespace TextBlade.Core.Locations;

public class Dungeon : Location
{
    // Simple but complex. Can be "Iron Shield," can be listed twice to give me two, can be "100 Gold," etc.
    // Floor (e.g. B2) => list of lootz
    public Dictionary<string, List<string>> FloorLoot = new();

    // It's a list, one entry per floor.
    // Each entry is a list of monsters and other stuff on that floor.
    // I'm sure I'll put treasure and stuff in here eventually.
    private readonly List<List<string>> _floorMonsters = new();
    private int _currentFloorNumber = 0;

    public Dungeon(string name, string description, int numFloors, List<string> monsters, string boss, string locationClass = null)
    : base(name, description, locationClass)
    {
        if (numFloors <= 0)
        {
            throw new ArgumentException(nameof(numFloors));
        }

        if (!monsters.Any())
        {
            throw new ArgumentException(nameof(monsters));
        }

        // Iterate up to the second-last floor and generate monsters
        for (int i = 0; i < numFloors - 1; i++)
        {
            var currentFloorOccupants = new List<string>();
            
            // Assumes monsters are in order of difficulty, roughly.
            // Which monsters can we spawn on this floor? Up to (floor_number + 1) inclusive.
            // Always guarantee at least two monster types
            var firstMonsterIndex = Math.Min(i - 1, monsters.Count - 2);
            firstMonsterIndex = Math.Max(firstMonsterIndex, 0);
            var lastMonsterIndex = Math.Min(i + 2, monsters.Count);

            List<string> validMonsters = [];
            // Keep this at base zero, so we know the correct value of each monster.
            // If we change this to firstMonsterIndex, we'll end up thinking the first
            // entry in validMonsters is value 1, but it's real value might be 5.
            for (int j = 0; j < lastMonsterIndex; j++)
            {
                validMonsters.Add(monsters[j]);
            }
            
            // Intentionally imprecise, nor does it guarantee future floors are always harder than past floors.
            var pointsLeft = (i + 1) * 5;
            while (pointsLeft > 0)
            {
                var nextMonsterIndex = Random.Shared.Next(firstMonsterIndex, lastMonsterIndex);
                currentFloorOccupants.Add(validMonsters[nextMonsterIndex]);
                // Magic number of *2 here cuts the monster count roughly in half
                var pointCost = (nextMonsterIndex + 1) * 2;
                pointsLeft -= pointCost;
            }

            _floorMonsters.Add(currentFloorOccupants);
        }

        // Generate a boss on the last floor, with 2-3 of the strongest minion
        int numMinions = Random.Shared.Next(2, 4);
        var minion = monsters[monsters.Count - 1];
        var finalFloor = new List<string>();
        // TODO: add loot! Mega loot!
        for (int i = 0; i < numMinions; i++)
        {
            finalFloor.Add(minion);
        }
        finalFloor.Add(boss);
        _floorMonsters.Add(finalFloor);
    }

    public void OnVictory()
    {
        // Clear out all monsters
        _floorMonsters[_currentFloorNumber].Clear();
    }

    override public string? GetExtraDescription()
    {
       var currentFloorData = _floorMonsters[_currentFloorNumber];
        var monstersMessage = $"You see: {string.Join(", ", currentFloorData)}. Type f/fight to fight.";
        if (!currentFloorData.Any())
        {
            monstersMessage = "There are no monsters left.";
            if (_currentFloorNumber < _floorMonsters.Count - 1)
            {
                monstersMessage += "You see stairs leading down. Type d/down/descend to go to the next floor.";
            } 
        }

        var treasureMessage = "";
        if (FloorLoot.ContainsKey($"B{_currentFloorNumber + 1}"))
        {
            treasureMessage = "You see something shiny nearby.  ";
        }

        return $"You are on floor {_currentFloorNumber + 1}. {treasureMessage}{monstersMessage}";
    }

    override public ICommand GetCommandFor(string input)
    {
        var currentFloorData = _floorMonsters[_currentFloorNumber];
        if (input == "f" || input == "fight")
        {
            return new FightCommand(currentFloorData);
        }
        if (input == "d" || input == "down" || input == "descend" || input == ">")
        {
            if (currentFloorData.Any())
            {
                Console.WriteLine("You can't descend while monsters are around!");
            }
            else if (_currentFloorNumber == _floorMonsters.Count - 1)
            {
                Console.WriteLine("You're already at the bottom of the dungeon!");
            }
            else
            {
                // Valid descent
                _currentFloorNumber++;
            }
        }

        return new DoNothingCommand();
    }
}
