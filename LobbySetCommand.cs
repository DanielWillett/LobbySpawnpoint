using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;

namespace LobbySpawnpoint;

public class LobbySetCommand : IRocketCommand
{
    public AllowedCaller AllowedCaller => AllowedCaller.Player;
    public string Name => "lobbyset";
    public string Help => "Sets the spawn for new players to your position and rotation.";
    public string Syntax => "/lobbyset";
    public List<string> Aliases { get; } = [ ];
    public List<string> Permissions { get; } = [ "lobby.set" ];
    public void Execute(IRocketPlayer caller, string[] command)
    {
        if (caller is not UnturnedPlayer player)
            return;

        LobbySpawnpoint.Instance.Configuration.Instance.X = player.Player.transform.position.x;
        LobbySpawnpoint.Instance.Configuration.Instance.Y = player.Player.transform.position.y;
        LobbySpawnpoint.Instance.Configuration.Instance.Z = player.Player.transform.position.z;
        LobbySpawnpoint.Instance.Configuration.Instance.Yaw = player.Player.transform.rotation.eulerAngles.y;

        LobbySpawnpoint.Instance.Configuration.Instance.SetUp = true;

        ChatManager.say(player.CSteamID, LobbySpawnpoint.Instance.Translate("spawnpoint_set",
            player.Player.transform.position.x.ToString("N1"),
            player.Player.transform.position.y.ToString("N1"),
            player.Player.transform.position.z.ToString("N1"),
            player.Player.transform.rotation.eulerAngles.y.ToString("N1")
        ), UnityEngine.Color.white, true);

        LobbySpawnpoint.Instance.Configuration.Save();
    }
}