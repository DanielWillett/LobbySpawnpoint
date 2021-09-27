using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;

namespace LobbySpawnpoint
{
    public class LobbySetCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "lobbyset";
        public string Help => "Sets the spawn for new players to your position and rotation.";
        public string Syntax => "/lobbyset";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string> { "lobby.set" };
        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (!(caller is UnturnedPlayer player)) return;
            LobbySpawnpoint.I.Configuration.Instance.X = player.Player.transform.position.x;
            LobbySpawnpoint.I.Configuration.Instance.Y = player.Player.transform.position.y;
            LobbySpawnpoint.I.Configuration.Instance.Z = player.Player.transform.position.z;
            LobbySpawnpoint.I.Configuration.Instance.Yaw = player.Player.transform.rotation.eulerAngles.y;
            LobbySpawnpoint.I.Configuration.Instance.SetUp = true;
            ChatManager.say(LobbySpawnpoint.I.Translate("spawnpoint_set",
                player.Player.transform.position.x.ToString("N1"),
                player.Player.transform.position.y.ToString("N1"),
                player.Player.transform.position.z.ToString("N1"),
                player.Player.transform.rotation.eulerAngles.y.ToString("N1")
                ), UnityEngine.Color.white, true);
            LobbySpawnpoint.I.Configuration.Save();
        }
    }
}
