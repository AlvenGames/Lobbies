Addons for the Unity Lobby Service, which will make it easier to add Lobby to your games.

Comparison with **Unity Lobby Service API**:

Addons API:
```csharp
// Set the playerId and player name.
_localLobby.Player.Id.Value = _playerId;
_localLobby.Player.DisplayName.Data.Value = playerName;

// Configure lobby
_localLobby.RelayJoinCode.Data.Value = "JoinCode";

// Configure and create lobby
await _lobbyManager.CreateAndBindLobbyAsync(lobbyName: "LobbyName", maxPlayersCount: 4, isPrivate: false, _localLobby);
```

Unity Lobby Service API:
```csharp
// Set the playerId and player name.
var player = new Player(_playerId, data: new Dictionary<string, PlayerDataObject>
{
    { "DisplayName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public) }
});

// Creating lobby
await LobbyService.Instance.CreateLobbyAsync(lobbyName: "LobbyName", maxPlayers: 4, options: new CreateLobbyOptions
{
    IsPrivate = false,
    Player = player,
    Data = new Dictionary<string, DataObject>
    {
        { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Public) }
    }
});
```

API Samples:
```csharp
[SerializeField] private LobbyManager _lobbyManager;
[SerializeField] private RelayLocalLobby _localLobby;

private async void LobbyAPI()
{
    // Begin heartbeat ping
    _localLobby.BeginTracking();

    // End heartbeat ping
    _localLobby.EndTracking();

    // Check tracking state
    if (_localLobby.IsTracking)
    {
        // ...
    }

    Lobby lobby = await _lobbyManager.CreateLobbyAsync(...);
    // Bind LocalLobby to remote Lobby for sync
    await _localLobby.BindToRemoteLobbyAsync(_lobbyManager, lobby);
}
```

LobbyManager replicates the LobbyService API, but the plus side of using it is that it monitors rate limits from https://docs.unity.com/lobby/rate-limits.html.
That way you don't have to think about those limits.

## Installation
You can add https://github.com/AlvenGames/Lobbies.git?path=Assets/Plugins/Lobbies to Package Manager
