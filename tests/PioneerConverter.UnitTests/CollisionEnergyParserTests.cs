using PioneerConverter.Infrastructure.Thermo;

namespace PioneerConverter.UnitTests;

public sealed class CollisionEnergyParserTests
{
    [Fact]
    public void TryParseCollisionEnergyEv_SingleValue_Parses()
    {
        bool ok = CollisionEnergyParser.TryParseCollisionEnergyEv("27.5", out float ev);

        Assert.True(ok);
        Assert.Equal(27.5f, ev);
    }

    [Fact]
    public void TryParseCollisionEnergyEv_CommaSeparated_Averages()
    {
        bool ok = CollisionEnergyParser.TryParseCollisionEnergyEv("10, 20, 40", out float ev);

        Assert.True(ok);
        Assert.Equal(70f / 3f, ev, 3);
    }

    [Fact]
    public void TryParseCollisionEnergyEv_InvalidList_Fails()
    {
        bool ok = CollisionEnergyParser.TryParseCollisionEnergyEv("abc, def", out _);

        Assert.False(ok);
    }
}
