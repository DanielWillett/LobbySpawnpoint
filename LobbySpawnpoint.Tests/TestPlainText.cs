using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace LobbySpawnpoint.Tests;

public class TestPlainText
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
    public void TestRead()
    {
        Assert.That(_sp, Is.Not.Null);

        PlayerSaver saver = new PlayerSaver(_sp, _ => true, Environment.CurrentDirectory);
        saver.Read();

        ulong[] players = saver.PreviouslyJoinedPlayers.ToArray();

        Assert.That(players.Length, Is.EqualTo(5));
        Assert.That(players[0], Is.EqualTo(76561198267927009));
        Assert.That(players[1], Is.EqualTo(76561198267927010));
        Assert.That(players[2], Is.EqualTo(76561198267927011));
        Assert.That(players[3], Is.EqualTo(76561198267927012));
        Assert.That(players[4], Is.EqualTo(76561198267927013));
    }

    [Test]
    public void TestAdd()
    {
        Assert.That(_sp, Is.Not.Null);

        PlayerSaver saver = new PlayerSaver(_sp, _ => true, Environment.CurrentDirectory);
        saver.Read();
        saver.TryAdd(76561198267927014);

        ulong[] players = saver.PreviouslyJoinedPlayers.ToArray();

        Assert.That(players.Length, Is.EqualTo(6));
        Assert.That(players[0], Is.EqualTo(76561198267927009));
        Assert.That(players[1], Is.EqualTo(76561198267927010));
        Assert.That(players[2], Is.EqualTo(76561198267927011));
        Assert.That(players[3], Is.EqualTo(76561198267927012));
        Assert.That(players[4], Is.EqualTo(76561198267927013));
        Assert.That(players[5], Is.EqualTo(76561198267927014));
    }

    [Test]
    public void TestAddExisting()
    {
        Assert.That(_sp, Is.Not.Null);

        PlayerSaver saver = new PlayerSaver(_sp, _ => true, Environment.CurrentDirectory);
        saver.Read();
        saver.TryAdd(76561198267927009);

        ulong[] players = saver.PreviouslyJoinedPlayers.ToArray();

        Assert.That(players.Length, Is.EqualTo(5));
        Assert.That(players[0], Is.EqualTo(76561198267927009));
        Assert.That(players[1], Is.EqualTo(76561198267927010));
        Assert.That(players[2], Is.EqualTo(76561198267927011));
        Assert.That(players[3], Is.EqualTo(76561198267927012));
        Assert.That(players[4], Is.EqualTo(76561198267927013));
    }

    [Test]
    public void TestExists()
    {
        Assert.That(_sp, Is.Not.Null);

        PlayerSaver saver = new PlayerSaver(_sp, _ => true, Environment.CurrentDirectory);
        saver.Read();

        Assert.That(saver.HasPreviouslyJoined(76561198267927009), Is.True);
    }

    [Test]
    public void TestNotExists()
    {
        Assert.That(_sp, Is.Not.Null);

        PlayerSaver saver = new PlayerSaver(_sp, _ => true, Environment.CurrentDirectory);
        saver.Read();

        Assert.That(saver.HasPreviouslyJoined(76561198267927014), Is.False);
    }
}