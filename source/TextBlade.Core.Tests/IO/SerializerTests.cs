using Newtonsoft.Json.Linq;
using NUnit.Framework;
using TextBlade.Core.IO;
using TextBlade.Core.Locations;

namespace TextBlade.Core.Tests.IO;

[TestFixture]
public class SerializerTests
{
    [Test]
    public void Serialize_IncludesTypeInfo()
    {
        // Arrange
        var kingdom = new Location("Bob's Kingdom", "KING BOB!!!!");

        // Act
        var actual = Serializer.Serialize(kingdom);

        // Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Does.Contain("\"$type\":"));
        Assert.That(actual, Does.Contain("TextBlade.Core.Locations.Location, TextBlade.Core"));
    }

    [Test]
    public void Deserialize_CorrectlyDerializesSmallVIllage()
    {
        // Arrangement is done in the JSON files
        var filePath = Path.Join("TestData", "Locations", "VillageWithInnAndShops.json");
        var json = File.ReadAllText(filePath);
        var actual = Serializer.Deserialize<Location>(json);

        // Assert
        Assert.That(actual, Is.Not.Null);

        Assert.That(actual.Name, Is.EqualTo("King's Vale"));
        Assert.That(actual.Description, Does.Contain("small village"));

        Assert.That(actual.LinkedLocations.Count, Is.EqualTo(3));
        Assert.That(actual.LinkedLocations[0].Id, Is.EqualTo("KingsVale/Inn"));
        Assert.That(actual.LinkedLocations[0].Description, Is.EqualTo("The King's Inn"));
        Assert.That(actual.LinkedLocations[1].Id, Is.EqualTo("KingsVale/EquipmentShop"));
        Assert.That(actual.LinkedLocations[1].Description, Is.EqualTo("Bronzebeard's Wares"));
        Assert.That(actual.LinkedLocations[2].Id, Is.EqualTo("KingsVale/ItemShop"));
        Assert.That(actual.LinkedLocations[2].Description, Is.EqualTo("Potions R Us"));
    }

    [Test]
    public void DeserializeParty_CorrectlyDeserializesPartyMembers()
    {
        // Arrange is done in the JSON files
        var filePath = Path.Join("TestData", "Characters", "TwoMemberParty.json");
        var json = File.ReadAllText(filePath);
        var jArray = Serializer.Deserialize<JArray>(json);
        var actual = Serializer.DeserializeParty(jArray);

        // Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Count, Is.EqualTo(2));
        var ahmed = actual[0];
        Assert.That(ahmed.Name, Is.EqualTo("Ahmed"));
        Assert.That(ahmed.TotalHealth, Is.EqualTo(100));
        Assert.That(ahmed.CurrentHealth, Is.EqualTo(10));
        var bilal = actual[1];
        Assert.That(bilal.Name, Is.EqualTo("Bilal"));
        Assert.That(bilal.TotalHealth, Is.EqualTo(75));
        Assert.That(bilal.CurrentHealth, Is.EqualTo(66));
    }

    [Test]
    public void Deserialize_CorrectlyDeserializesTownAsLocationn_WhenTypeIsSpecified()
    {
         // Arrange is done in the JSON files
        var filePath = Path.Join("TestData", "Locations", "InnWithType.json");
        var json = File.ReadAllText(filePath);
        var actual = Serializer.Deserialize<Location>(json);

        // Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Is.TypeOf<Inn>());
        var inn = actual as Inn;
        Assert.That(inn.InnCost, Is.EqualTo(100));
    }
}
