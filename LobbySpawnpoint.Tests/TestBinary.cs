using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace LobbySpawnpoint.Tests;

public class TestBinary
{
    private LobbySpawnpoint? _sp;

    [SetUp]
    public void Setup()
    {
        _sp = (LobbySpawnpoint)FormatterServices.GetUninitializedObject(typeof(LobbySpawnpoint));

        using BinaryWriter writer = new BinaryWriter(
            new FileStream("./online_players.dat", FileMode.Create, FileAccess.Write, FileShare.Read),
            Encoding.ASCII,
            leaveOpen: false
        );

        writer.Write(Swap(76561198267927009));
        writer.Write(Swap(76561198267927010));
        writer.Write(Swap(76561198267927011));
        writer.Write(Swap(76561198267927012));
        writer.Write(Swap(76561198267927013));

        writer.Flush();

        return;

        static ulong Swap(ulong id)
        {
            byte[] bits = BitConverter.GetBytes(id);
            for (int i = 0; i < bits.Length / 2; ++i)
            {
                (bits[i], bits[bits.Length - i - 1]) = (bits[bits.Length - i - 1], bits[i]);
            }

            return BitConverter.ToUInt64(bits, 0);
        }
    }

    [Test]
    public void TestRead()
    {
        Assert.That(_sp, Is.Not.Null);

        PlayerSaver saver = new PlayerSaver(_sp, _ => false, Environment.CurrentDirectory);
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

        PlayerSaver saver = new PlayerSaver(_sp, _ => false, Environment.CurrentDirectory);
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

        PlayerSaver saver = new PlayerSaver(_sp, _ => false, Environment.CurrentDirectory);
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

        PlayerSaver saver = new PlayerSaver(_sp, _ => false, Environment.CurrentDirectory);
        saver.Read();

        Assert.That(saver.HasPreviouslyJoined(76561198267927009), Is.True);
    }

    [Test]
    public void TestNotExists()
    {
        Assert.That(_sp, Is.Not.Null);

        PlayerSaver saver = new PlayerSaver(_sp, _ => false, Environment.CurrentDirectory);
        saver.Read();

        Assert.That(saver.HasPreviouslyJoined(76561198267927014), Is.False);
    }
}