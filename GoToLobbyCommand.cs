using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using UnityEngine;

namespace LobbySpawnpoint
{
    public class GoToLobbyCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "lobbytp";
        public string Help => "Teleport to the set lobby position.";
        public string Syntax => "/lobbytp";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string> { "lobby.tp" };
        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (!(caller is UnturnedPlayer player)) return;
            player.Player.teleportToLocation(new Vector3(
                LobbySpawnpoint.I.Configuration.Instance.X, 
                LobbySpawnpoint.I.Configuration.Instance.Y, 
                LobbySpawnpoint.I.Configuration.Instance.Z), 
                LobbySpawnpoint.I.Configuration.Instance.Yaw);
            ChatManager.say(LobbySpawnpoint.I.Translate("spawnpoint_teleported",
                player.Player.transform.position.x.ToString("N1"),
                player.Player.transform.position.y.ToString("N1"),
                player.Player.transform.position.z.ToString("N1"),
                player.Player.transform.rotation.eulerAngles.y.ToString("N1")
                ), Color.white, true);
        }
    }
}
