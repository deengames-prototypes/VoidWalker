using TextBlade.Core.Commands;
using TextBlade.Core.IO;

namespace TextBlade.Core.Locations;

/// <summary>
/// Your regular location. Has reachable locations (sub-locations, adjacent locations, however you concieve of it.)
/// </summary>
public class Location
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string BackgroundAudio { get; set; } = string.Empty;

    /// <summary>
    /// For custom code, this is the class name of the code-behind class for this location.
    /// </summary>
    public string? LocationClass { get; set; }

    /// <summary>
    /// Used by Game to pass along the current save data.
    /// Could be DI constructor injected, too.
    /// </summary>
    public SaveData CurrentSaveData { set; protected get; }

    public List<LocationLink> LinkedLocations { get; set; } = new();
    public string LocationId { get; internal set; } = null!; // Saved so we know our location
    
    public Location(string name, string description, string? locationClass = null)
    {
        this.Name = name;
        this.Description = description;
        this.LocationClass = locationClass;
    } 

    public virtual ICommand GetCommandFor(string input)
    {
        // Leave it up to sub-types, like inn, to handle their own input and return a command.
        return new DoNothingCommand();
    }

    public virtual string GetExtraDescription()
    {
        // Override for stuff like "You are in 2B, you see three Tiramisu Bettles"
        return string.Empty;
    }

    public virtual string GetExtraMenuOptions()
    {
        // Override for stuff like "type f/fight to fight"
        return string.Empty;
    }
}

