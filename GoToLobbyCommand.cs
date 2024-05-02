using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using UnityEngine;

namespace LobbySpawnpoint;

public class GoToLobbyCommand : IRocketCommand
{
    public AllowedCaller AllowedCaller => AllowedCaller.Player;
    public string Name => "lobbytp";
    public string Help => "Teleport to the set lobby position.";
    public string Syntax => "/lobbytp";
    public List<string> Aliases { get; } = [ ];
    public List<string> Permissions { get; } = [ "lobby.tp" ];
    public void Execute(IRocketPlayer caller, string[] command)
    {
        if (caller is not UnturnedPlayer player)
            return;

        player.Player.teleportToLocation(new Vector3(
                LobbySpawnpoint.Instance.Configuration.Instance.X, 
                LobbySpawnpoint.Instance.Configuration.Instance.Y, 
                LobbySpawnpoint.Instance.Configuration.Instance.Z), 
            LobbySpawnpoint.Instance.Configuration.Instance.Yaw);

        ChatManager.say(player.CSteamID, LobbySpawnpoint.Instance.Translate("spawnpoint_teleported",
            player.Player.transform.position.x.ToString("N1"),
            player.Player.transform.position.y.ToString("N1"),
            player.Player.transform.position.z.ToString("N1"),
            player.Player.transform.rotation.eulerAngles.y.ToString("N1")
        ), Color.white, true);
    }
}