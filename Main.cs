using System;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using SDG.Unturned;
using System.IO;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace LobbySpawnpoint;

public class LobbySpawnpoint : RocketPlugin<LobbySpawnpointConfiguration>
{
    private bool _isMono = Type.GetType("Mono.Runtime") != null;
    public PlayerSaver Saver { get; private set; }
    public static LobbySpawnpoint Instance { get; private set; }
    protected override void Load()
    {
        Instance = this;
        
        Provider.onLoginSpawning += DecideSpawn;
        U.Events.OnPlayerConnected += OnPlayerConnect;

        Saver = new PlayerSaver(this, Path.Combine(System.Environment.CurrentDirectory, "Plugins", Assembly.GetName().Name));
        Saver.Read();

        base.Load();
        Logger.Log("LobbySpawnpoint Loaded.");
        Logger.Log("https://github.com/DanielWillett/LobbySpawnpoint");
    }
    protected override void Unload()
    {
        Provider.onLoginSpawning -= DecideSpawn;

        base.Unload();
        Instance = null;
        Saver = null!;

        Logger.Log("LobbySpawnpoint Unloaded.");
    }
    private void OnPlayerConnect(Rocket.Unturned.Player.UnturnedPlayer player)
    {
        Saver?.TryAdd(player.Player.channel.owner.playerID.steamID.m_SteamID);
    }
    private void DecideSpawn(SteamPlayerID playerID, ref Vector3 point, ref float yaw, ref EPlayerStance initialStance, ref bool needsNewSpawnpoint)
    {
        if (!Configuration.Instance.SetUp || Saver.HasPreviouslyJoined(playerID.steamID.m_SteamID))
            return;

        needsNewSpawnpoint = false;
        point = new Vector3(Configuration.Instance.X, Configuration.Instance.Y, Configuration.Instance.Z);
        yaw = Configuration.Instance.Yaw;
    }
    public override TranslationList DefaultTranslations => new TranslationList
    {
        { "spawnpoint_set", "<color=#33cc33>Set lobby spawnpoint to {0}, {1}, {2} ({3}°).</color>" },
        { "spawnpoint_teleported", "<color=#33cc33>Teleported to {0}, {1}, {2} ({3}°).</color>" },
        { "spawnpoint_reset", "<color=#33cc33>Reset the lobby save file.</color>" }
    };
    internal void LogInfo(string warning)
    {
        if (_isMono)
            Logger.Log(warning);
        else
            Console.WriteLine(warning);
    }
    internal void LogWarning(string warning)
    {
        if (_isMono)
            Logger.LogWarning(warning);
        else
            Console.WriteLine(warning);
    }
    internal void LogError(string warning)
    {
        if (_isMono)
            Logger.LogError(warning);
        else
            Console.WriteLine(warning);
    }
}