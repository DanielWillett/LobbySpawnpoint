using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using SDG.Unturned;
using UnityEngine;
namespace LobbySpawnpoint
{
    public class LobbySpawnpoint : RocketPlugin<Config>
    {
        public PlayerSaver Saver = new PlayerSaver();
        public static LobbySpawnpoint I;
        protected override void Load()
        {
            I = this;
            Provider.onLoginSpawning += DecideSpawn;
            U.Events.OnPlayerConnected += OnPlayerConnect;
            Saver.Read();
            base.Load();
            Rocket.Core.Logging.Logger.Log("LobbySpawnpoint Loaded.");
            Rocket.Core.Logging.Logger.Log("https://github.com/DanielWillett/LobbySpawnpoint");
        }

        private void OnPlayerConnect(Rocket.Unturned.Player.UnturnedPlayer player)
        {
            Saver?.TryAdd(player.Player.channel.owner.playerID.steamID.m_SteamID);
        }

        protected override void Unload()
        {
            Provider.onLoginSpawning -= DecideSpawn;
            base.Unload();
            I = null;
            Rocket.Core.Logging.Logger.Log("LobbySpawnpoint Unloaded.");
        }
        private void DecideSpawn(SteamPlayerID playerID, ref Vector3 point, ref float yaw, 
            ref EPlayerStance initialStance, ref bool needsNewSpawnpoint)
        {
            if (!Configuration.Instance.SetUp || Saver.HasJoined(playerID.steamID.m_SteamID)) return;
            needsNewSpawnpoint = false;
            point = new Vector3(Configuration.Instance.X, Configuration.Instance.Y, Configuration.Instance.Z);
            yaw = Configuration.Instance.Yaw;
        }

        public override TranslationList DefaultTranslations => new TranslationList
        {
            { "spawnpoint_set", "<color=#33cc33>Set lobby spawnpoint to {0}, {1}, {2} ({3}°).</color>" },
            { "spawnpoint_teleported", "<color=#33cc33>Teleported to {0}, {1}, {2} ({3}°).</color>" }
        };
    }
}
