using System;

[Serializable]
public class JoinGameRequestPacket
{
    public string gameid;
    public JoinGameRequestPacket(string gameid)
    {
        this.gameid = gameid;
    }
}
[Serializable]
public class JoinGamePacket
{
    public string gameid;
    public string[] playernames;
}

[Serializable]
public class StartGameRequestPacket
{
    public string gameid;
    public StartGameRequestPacket(string gameid)
    {
        this.gameid = gameid;
    }
}