using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace LobbySpawnpoint.Tests;

public class TestPlainTextEmpty
{
    private LobbySpawnpoint? _sp;

    [SetUp]
    public void Setup()
    {
        _sp = (LobbySpawnpoint)FormatterServices.GetUninitializedObject(typeof(LobbySpawnpoint));
        
        File.WriteAllLines("./online_players.dat", [ ]);
    }

    [Test]
    public void TestRead()
    {
        Assert.That(_sp, Is.Not.Null);

        PlayerSaver saver = new PlayerSaver(_sp, _ => true, Environment.CurrentDirectory);
        saver.Read();

        ulong[] players = saver.PreviouslyJoinedPlayers.ToArray();

        Assert.That(players.Length, Is.EqualTo(0));
    }

    [Test]
    public void TestAdd()
    {
        Assert.That(_sp, Is.Not.Null);

        PlayerSaver saver = new PlayerSaver(_sp, _ => true, Environment.CurrentDirectory);
        saver.Read();
        saver.TryAdd(76561198267927014);

        ulong[] players = saver.PreviouslyJoinedPlayers.ToArray();

        Assert.That(players.Length, Is.EqualTo(1));
        Assert.That(players[0], Is.EqualTo(76561198267927014));
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