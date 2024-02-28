using System.Collections.Generic;
using UnityEngine;

public class GameClient
{
    private Client _client;
    private string _name;
    private bool _conneted = false;
    private string _gameid;
    private string _reconnect_token;
    private string[] _playerNames;

    private PokerGame _game;

    public GameClient()
    {
        _client = new();
        _client.AddPacketHandler(Packet.USER_REGISTRY_PACKET, OnUserRegistryPacket);
        _client.AddPacketHandler(Packet.USER_REGISTRY_ERROR_PACKET, OnUserRegistryErrorPacket);
        _client.AddPacketHandler(Packet.GAME_CREATE_REQUEST_PACKET, OnGameJoinPacket);
        _client.AddPacketHandler(Packet.GAME_REQUEST_ERROR_PACKET, OnGameRequestErrorPacket);
        _client.AddPacketHandler(Packet.GAME_JOIN_PACKET, OnGameJoinPacket);
        _client.AddPacketHandler(Packet.GAME_START_PACKET, OnGameStartPacket);
        _client.AddPacketHandler(Packet.GAME_BET_OR_RAISE_PACKET, OnPlayerBetOrRaise);
        _client.AddPacketHandler(Packet.GAME_CHECK_OR_CALL_PACKET, OnPlayerCheckOrCall);
        _client.AddPacketHandler(Packet.GAME_FOLD_PACKET, OnPlayerFold);
        _client.AddPacketHandler(Packet.GAME_ROLL_DICE_PACKET, OnPlayerRollDice);
        _client.AddPacketHandler(Packet.GAME_END_ROLLS_PACKET, OnPlayerEndRolls);
        _client.AddPacketHandler(Packet.GAME_CHAT_MSG_PACKET, OnChatMsg);
    }

    public void StartConnection(string ip)
    {
        _client.StartConnection(ip);
        _conneted = true;
    }

    public void RegisterName(string name)
    {
        _name = name;
        _client.SendPacket(Packet.USER_REGISTRY_PACKET, new UserRegistryData(_name));
    }


    public void RequestCreateGame(GameSettings settings)
    {
        _client.SendPacket(Packet.GAME_CREATE_REQUEST_PACKET, new RequestGameData(settings));
    }

    public void RequestJoinGame(string gameid)
    {
        _client.SendPacket(Packet.GAME_JOIN_REQUEST_PACKET, new JoinGameRequestPacket(gameid));
    }
    public void CloseConnection()
    {
        _client.CloseConnection();
    }

    public void RequestGameStart()
    {
        _client.SendPacket(Packet.GAME_START_REQUEST_PACKET, new StartGameRequestPacket(_gameid));
    }

    public void PlayerFold()
    {
        _client.SendPacket(Packet.GAME_FOLD_PACKET, new EmptyPacket());
    }

    public void PlayerCheckOrCall()
    {
        _client.SendPacket(Packet.GAME_CHECK_OR_CALL_PACKET, new EmptyPacket());
    }

    public void PlayerBetOrRaise(int amount)
    {
        _client.SendPacket(Packet.GAME_BET_OR_RAISE_PACKET, new BetOrRaisePacket(amount));
    }

    public void PlayerRollDice()
    {
        List<bool> selected = new();
        foreach (var die in GameManager.Instance.Dice)
        {
            selected.Add(die.Selected);
        }
        _client.SendPacket(Packet.GAME_ROLL_DICE_PACKET, new RollDicePacket(selected));
    }

    public void PlayerEndDiceRolls()
    {
        _client.SendPacket(Packet.GAME_END_ROLLS_PACKET, new EmptyPacket());
    }

    public void SendChatMessage(string msg)
    {
        _client.SendPacket(Packet.GAME_CHAT_MSG_PACKET, new GameChatPacket(msg));
    }

    Packet OnUserRegistryPacket(string data)
    {
        var packet = JsonUtility.FromJson<UserRegistryPacket>(data);
        _reconnect_token = packet.token;
        GameHandler.Instance.OverlayManager.AddMessage(1, "Succesfully registered user.");
        Debug.Log("register user");
        return null;
    }
    Packet OnUserRegistryErrorPacket(string data)
    {
        GameHandler.Instance.OverlayManager.AddMessage(0, "Could not register user!");
        return null;
    }

    Packet OnGameJoinPacket(string data)
    {
        var packet = JsonUtility.FromJson<JoinGamePacket>(data);
        MenuManager.Instance.OpenMenu((int)Menu.GAME_LOBBY);
        var lobby = MenuManager.Instance.Menus[(int)Menu.GAME_LOBBY].GetComponent<GameLobby>();
        lobby.Reset();
        lobby.SetGameId(packet.gameid);
        _gameid = packet.gameid;
        if (_name == packet.playernames[0])
        {
            lobby.SetStartGameBtnActive(true);
            if (packet.playernames.Length >= 2)
            {
                lobby.SetStartGameBtnInteractable(true);
            }
        }
        _playerNames = packet.playernames;
        foreach (var player in packet.playernames)
        {
            lobby.AddPlayer(player);
        }
        return null;
    }

    Packet OnGameStartPacket(string data)
    {
        var packet = JsonUtility.FromJson<GameInitPacket>(data);
        GameHandler.Instance.LoadScene("InGameScene", () =>
        {
            _game = new(packet.settings, packet.state, packet.turn);
            _game.Start(_playerNames);
        });


        return null;
    }
    Packet OnGameRequestErrorPacket(string data)
    {
        GameHandler.Instance.OverlayManager.AddMessage(0, "error: " + data);
        return null;
    }
    Packet OnPlayerEndRolls(string data)
    {
        _game.EndDiceRolls();
        return null;
    }
 
    Packet OnPlayerRollDice(string data)
    {
        var packet = JsonUtility.FromJson<DicePacket>(data);
        _game.DiceRoll(packet.dice, packet.lastSelection);
        return null;
    }
    Packet OnPlayerBetOrRaise(string data)
    {
        var packet = JsonUtility.FromJson<BetOrRaisePacket>(data);
        _game.PlayerBetOrRaise(packet.amount);
        return null;
    }
    Packet OnPlayerCheckOrCall(string data)
    {
        _game.PlayerCheckOrCall();
        return null;
    }

    Packet OnPlayerFold(string data)
    {
        _game.PlayerFold();
        return null;
    }

    Packet OnChatMsg(string data)
    {
        var packet = JsonUtility.FromJson<GameChatPacket>(data);
        GameManager.Instance.AddChatMesage(packet.sender, packet.msg);
        return null;
    }
    public bool Connected => _conneted;

    public PokerGame Game => _game;

    public bool InGame => _gameid != null;
}
