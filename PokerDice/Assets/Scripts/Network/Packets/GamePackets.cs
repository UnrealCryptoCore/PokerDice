using System;
using System.Collections.Generic;

[Serializable]
public class BetOrRaisePacket
{
    public int amount;
    public BetOrRaisePacket(int amount)
    {
        this.amount = amount;
    }
}

[Serializable]
public class DicePacket
{
    public List<int> dice;
    public bool[] lastSelection;
}

[Serializable]
public class GameInitPacket
{
    public int turn;
    public GameState state;
    public GameSettings settings;
}

[Serializable]
public class RollDicePacket
{
    public List<bool> selected;

    public RollDicePacket(List<bool> selected)
    {
        this.selected = selected;
    }
}

[Serializable]
public class GameChatPacket
{
    public string msg;
    public int sender;

    public GameChatPacket(string msg)
    {
        this.msg = msg;
    }
}