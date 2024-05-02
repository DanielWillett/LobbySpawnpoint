using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;

namespace LobbySpawnpoint;

public class LobbyResetCommand : IRocketCommand
{
    public AllowedCaller AllowedCaller => AllowedCaller.Player;
    public string Name => "lobbyreset";
    public string Help => "Resets the save file, forcing all players to be considered 'new' players the next time they join.";
    public string Syntax => "/lobbyreset";
    public List<string> Aliases { get; } = [ ];
    public List<string> Permissions { get; } = [ "lobby.reset" ];
    public void Execute(IRocketPlayer caller, string[] command)
    {
        if (caller is not UnturnedPlayer player)
            return;

        LobbySpawnpoint.Instance.Saver.Reset();

        ChatManager.say(player.CSteamID, LobbySpawnpoint.Instance.Translate("spawnpoint_reset"), UnityEngine.Color.white, true);
    }
}