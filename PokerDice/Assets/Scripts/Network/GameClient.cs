using UnityEngine;
using UnityEngine.SceneManagement;

public class GameClient
{
    //private PDNetworkManager _client;
    private Client _client;
    private string _name;
    private bool _conneted = false;
    private string _gameid;

    public GameClient()
    {
        //_client = PDNetworkManager.Instance;
        _client = new("127.0.0.1", 12345);
        _client.AddPacketHandler(Packet.USER_REGISTRY_PACKET, OnUserRegistryPacket);
        _client.AddPacketHandler(Packet.USER_REGISTRY_ERROR_PACKET, OnUserRegistryErrorPacket);
        _client.AddPacketHandler(Packet.GAME_CREATE_REQUEST_PACKET, OnGameJoinPacket);
        _client.AddPacketHandler(Packet.GAME_JOIN_PACKET, OnGameJoinPacket);
        _client.AddPacketHandler(Packet.GAME_START_PACKET, OnGameStartPacket);
        _client.AddPacketHandler(Packet.GAME_REQUEST_ERROR_PACKET, OnGameRequestErrorPacket);
    }

    public void StartConnection()
    {
        _client.StartConnection();
        _conneted = true;
    }

    public void RegisterName(string name)
    {
        _name = name;
        _client.SendPacket(Packet.USER_REGISTRY_PACKET, new UserRegistryData(_name));
    }


    public void RequestCreateGame()
    {
        _client.SendPacket(Packet.GAME_CREATE_REQUEST_PACKET, new RequestGameData());
    }

    public void RequestJoinGame(string gameid)
    {
        _client.SendPacket(Packet.GAME_JOIN_REQUEST_PACKET, new JoinGameRequestPacket(gameid));
    }
    public void CloseConnection()
    {
        _client.CloseConnection();
    }

    public void RequestGameStart() {
        _client.SendPacket(Packet.GAME_START_REQUEST_PACKET, new StartGameRequestPacket(_gameid));
    }

    Packet OnUserRegistryPacket(string data)
    {
        GameHandler.Instance.overlayManager.AddMessage(1, "Succesfully registerd user.");
        return null; // dont do anything. we are now registerd on the server
    }
    Packet OnUserRegistryErrorPacket(string data)
    {
        GameHandler.Instance.overlayManager.AddMessage(0, "Could not register user!");
        return null;
    }

    Packet OnGameJoinPacket(string data)
    {
        var packet = JsonUtility.FromJson<JoinGamePacket>(data);
        MenuManager.Instance.OpenMenu((int)MenuManager.Menu.GAME_LOBBY);
        var lobby = MenuManager.Instance.Menus[(int)MenuManager.Menu.GAME_LOBBY].GetComponent<GameLobby>();
        lobby.Reset();
        lobby.SetGameId(packet.gameid);
        _gameid = packet.gameid;
        if (_name == packet.playernames[0]) {
            lobby.SetStartGameBtnActive(true);
            if (packet.playernames.Length >= 2) {
                lobby.SetStartGameBtnInteractable(true);
            }
        }
        foreach(var player in packet.playernames) {
            lobby.AddPlayer(player);
        }
        return null;
    }

    Packet OnGameStartPacket(string data) {
        SceneManager.LoadScene("InGameScene");
        return null;
    }

    /*Packet OnGameJoinPacket(string data)
    {
        var packet = JsonUtility.FromJson<JoinGamePacket>(data);
        MenuManager.Instance.OpenMenu((int)MenuManager.Menu.GAME_LOBBY);
        var lobby = MenuManager.Instance.Menus[(int)MenuManager.Menu.GAME_LOBBY].GetComponent<GameLobby>();
        lobby.Reset();
        lobby.ClearPlayers();
        foreach(var player in packet.playernames) {
            lobby.AddPlayer(player);
        }
        return null;
    }*/
    Packet OnGameRequestErrorPacket(string data)
    {
        GameHandler.Instance.overlayManager.AddMessage(0, "error: " + data);
        return null;
    }

    public bool Connected => _conneted;
}
