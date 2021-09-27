<h1>Commands</h1>
<ul>
  <li><h3>/lobbyset</h3><h4><strong>Permission</strong>: lobby.set</h4><p>Sets the spawn for new players to your position and rotation.</p></li>
  <li><h3>/lobbytp</h3><h4><strong>Permission</strong>: lobby.tp</h4><p>Teleport to the set lobby position.</p></li>
</ul>
<h1>Translations</h1>
<ul>
  <li><h3>spawnpoint_set</h3><h4><strong>Formatting</strong>: {0} = x-position, {1} = y-position, {2} = z-position, {3} = yaw rotation</h4><p>Called after setting the spawnpoint.</p></li>
  <li><h3>spawnpoint_teleported</h3><h4><strong>Formatting</strong>: {0} = x-position, {1} = y-position, {2} = z-position, {3} = yaw rotation</h4><p>Called after teleporting to the spawnpoint.</p></li>
</ul>
<h1>Reset Previous Players List</h1>
<p>Delete the <code>online_players.dat</code> file from the plugin folder, no restart required.</p>
<h1>Read Previous Players List</h1>
<p>The player list is written in raw bytes for storage purposes. The list is only readable with a hex viewer. Every 8 bytes is a Steam64 ID. <br><em>Editing anything in that file will corrupt the list</em>.</p>
