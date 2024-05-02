using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace LobbySpawnpoint.Tests;
public class TestConvertToBinary
{
    private LobbySpawnpoint? _sp;

    [SetUp]
    public void Setup()
    {
        _sp = (LobbySpawnpoint)FormatterServices.GetUninitializedObject(typeof(LobbySpawnpoint));

        File.WriteAllLines("./online_players.dat", [
            "76561198267927009",
            "76561198267927010",
            "76561198267927011",
            "76561198267927012",
            "76561198267927013"
        ]);
    }

    [Test]
    public void TestConvert()
    {
        Assert.That(_sp, Is.Not.Null);

        PlayerSaver saver = new PlayerSaver(_sp, _ => false, Environment.CurrentDirectory);
        saver.Read();
        
        ulong[] players = saver.PreviouslyJoinedPlayers.ToArray();

        Assert.That(saver.CurrentFileIsPlainText, Is.False);

        Assert.That(players.Length, Is.EqualTo(5));
        Assert.That(players[0], Is.EqualTo(76561198267927009));
        Assert.That(players[1], Is.EqualTo(76561198267927010));
        Assert.That(players[2], Is.EqualTo(76561198267927011));
        Assert.That(players[3], Is.EqualTo(76561198267927012));
        Assert.That(players[4], Is.EqualTo(76561198267927013));

        saver.Read();

        players = saver.PreviouslyJoinedPlayers.ToArray();

        Assert.That(saver.CurrentFileIsPlainText, Is.False);

        Assert.That(players.Length, Is.EqualTo(5));
        Assert.That(players[0], Is.EqualTo(76561198267927009));
        Assert.That(players[1], Is.EqualTo(76561198267927010));
        Assert.That(players[2], Is.EqualTo(76561198267927011));
        Assert.That(players[3], Is.EqualTo(76561198267927012));
        Assert.That(players[4], Is.EqualTo(76561198267927013));
    }
}
