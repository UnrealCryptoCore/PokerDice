using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;

public class PokerGame
{
    private GameState _state;
    private GameSettings _settings;
    private int _turn;


    public PokerGame(GameSettings settings, GameState state, int turn)
    {
        _settings = settings;
        Debug.Log(_settings.betting);
        Debug.Log(_settings.money);
        _state = state;
        _turn = turn;
    }

    public void Start(string[] playerNames)
    {
        GameManager.Instance.SetupGame(this, playerNames, _settings.betting);
        GameManager.Instance.DisableButtons();
        Debug.Log("started game");
        StartGame();
        if (_settings.betting)
        {
            if (_state.roundPlayers[_state.bettingTurn] == _turn)
            {
                GameManager.Instance.BetOrRaiseButton.interactable = true;
            }
        }
        else
        {
            GameManager.Instance.ClearPot();
            if (_state.roundPlayers[_state.turn] == _turn)
            {
                EnableDiceRollButtons();
            }

        }
    }

    public void StartGame()
    {
        _state.turn = 0;
        _state.rolls = 0;
        _state.diceRolls = new(_state.players.Count);
        for (int i = 0; i < _state.players.Count * 5; i++)
        {
            _state.diceRolls.Add(-1);
        }
        _state.selectedDice = new bool[5];
        Array.Fill(_state.selectedDice, true);
        foreach (var die in GameManager.Instance.Dice)
        {
            die.ClearOutline();
            die.SetSelected(true);
            die.Selectabe = false;
        }
        _state.raise = 0;
        _state.roundPlayers = new();
        for (int i = 0; i < _state.players.Count; i++)
        {
            _state.roundPlayers.Add((i + _state.games) % _state.players.Count);
        }
        _state.games++;
        _state.bettingTurn = _settings.betting ? 0 : _state.roundPlayers.Count;
        _state.pot = _settings.betting ? 0 : 1;

    }

    public void PlayerCheckOrCall()
    {
        if (_state.rolls == 0)
        {
            TransferToPot(_state.raise);
        }
        else if (_state.rolls <= 3)
        {
            TransferToPot(_state.raise);
        }
        else
        {
            Debug.LogError("wrong number of rolls");
        }
        NextRoundBettingTurn();
    }
    public void PlayerBetOrRaise(int amount)
    {
        if (_state.rolls == 0)
        {
            _state.raise = amount;
            TransferToPot(amount);

        }
        else if (_state.rolls <= 3)
        {
            _state.raise = amount;
            TransferToPot(amount);
        }
        else
        {
            Debug.LogError("wrong number of rolls" + _state.rolls);
        }
        NextRoundBettingTurn();
    }

    public void PlayerFold()
    {
        _state.roundPlayers.RemoveAt(_state.bettingTurn);
        if (_state.roundPlayers.Count == 1)
        {
            SetWinner(_state.roundPlayers[0]);
            StartGame();
        }
    }

    public void DiceRoll(List<int> diceRolls, bool[] lastSelection)
    {
        for (int i = 0; i < 5; i++)
        {
            _state.diceRolls[_state.roundPlayers[_state.turn] * 5 + i] = diceRolls[i];
        }

        _state.rolls += 1;
        if (_settings.betting)
        {
            _state.bettingTurn = 0;
        }
        else
        {
            _state.bettingTurn = _state.roundPlayers.Count;
        }
        _state.selectedDice = lastSelection;
        foreach (var die in GameManager.Instance.Dice)
        {
            die.SetSelected(false);
        }
        GameManager.Instance.ThrowDice(diceRolls, lastSelection);
        if (_state.rolls == 3)
        {
            NextTurn();
        }

        if (_settings.betting)
        {
            if (_state.roundPlayers[_state.bettingTurn] == _turn)
            {
                GameManager.Instance.EnableRoundButtons();
            }

        }
        else
        {
            if (_state.roundPlayers[_state.turn] == _turn)
            {
                EnableDiceRollButtons();
                if (_state.rolls > 0)
                {
                    GameManager.Instance.EndButton.interactable = true;
                }
            }
        }
    }

    public void EndDiceRolls()
    {
        _state.rolls = 3;
        if (_settings.betting)
        {

            _state.bettingTurn = 0;
        }
        else
        {
            _state.bettingTurn = _state.roundPlayers.Count;
        }
        NextTurn();
        if (_settings.betting)
        {
            if (_state.roundPlayers[_state.bettingTurn] == _turn)
            {
                GameManager.Instance.EnableRoundButtons();
            }
        }
        else
        {
            if (_state.roundPlayers[_state.turn] == _turn)
            {
                GameManager.Instance.RollDiceButton.interactable = true;
            }
        }
    }

    public void EnableDiceRollButtons()
    {
        GameManager.Instance.RollDiceButton.interactable = true;
        for(int i=0; i<_state.selectedDice.Length; i++)
        {
            GameManager.Instance.Dice[i].Selectabe = _state.rolls > 0 && _state.selectedDice[i];
            GameManager.Instance.Dice[i].ClearOutline();
        }
    }

    public GameState State => _state;

    private void TransferToPot(int amount)
    {
        TransferToPot(amount, _state.roundPlayers[_state.bettingTurn]);
    }
    private void TransferToPot(int amount, int player)
    {
        _state.pot += amount;
        _state.players[player] -= amount;
        if (_settings.betting)
        {
            GameManager.Instance.SetPotMoney(_state.pot);
        }
        GameManager.Instance.PlayerInfoBox.SetScore(player, _state.players[player], _settings.betting);
    }

    private void TransferFromPot(int player)
    {
        _state.players[player] += _state.pot;
        _state.pot = 0;
        if (_settings.betting)
        {
            GameManager.Instance.SetPotMoney(_state.pot);
        }
        GameManager.Instance.PlayerInfoBox.SetScore(player, _state.players[player], _settings.betting);

    }

    private void NextRoundBettingTurn()
    {
        _state.bettingTurn += 1;
        if (_state.bettingTurn < _state.roundPlayers.Count)
        {
            if (_settings.betting & _state.roundPlayers[_state.bettingTurn] == _turn)
            {
                GameManager.Instance.EnableRoundButtons();
            }
            return; // not all players have set their bet
        }
        if (_state.roundPlayers[_state.turn] == _turn)
        {
            EnableDiceRollButtons();
            if (_state.rolls > 0)
            {
                GameManager.Instance.EndButton.interactable = true;
            }
        }
    }

    private void NextTurn()
    {
        if (_settings.betting)
        {

            _state.bettingTurn = 0;
        }
        else
        {
            _state.bettingTurn = _state.roundPlayers.Count;
        }
        _state.turn += 1;
        _state.rolls = 0;
        Array.Fill(_state.selectedDice, true);
        foreach (var die in GameManager.Instance.Dice)
        {
            die.ClearOutline();
            die.SetSelected(true);
            die.Selectabe = false;
        }
        if (_state.turn < _state.roundPlayers.Count)
        {
            return;
        }
        _state.turn = 0;
        int[] scores = new int[_state.players.Count];
        Array.Fill(scores, -1);
        for (int i = 0; i < _state.players.Count; i++)
        {
            if (!_state.roundPlayers.Contains(i))
            {
                continue;
            }
            scores[i] = CalculateScore(_state.diceRolls.GetRange(i * 5, 5));
            Debug.Log(scores[i] + ": ");
        }
        int max = 0;
        for (int i = 1; i < scores.Length; i++)
        {
            if (scores[i] > scores[max])
            {
                max = i;
            }
        }
        SetWinner(max);
        StartGame();
    }

    public void SetWinner(int player)
    {
        TransferFromPot(player);
        Debug.Log("player " + player + " won the game");
    }

    public static int CalculateScore(List<int> dice)
    {
        int[] count = new int[6];
        Array.Fill(count, 0);
        int s = 0;
        foreach (var e in dice)
        {
            count[e]++;
            s += e;
        }
        int maxIdx = 0;
        int maxE = 0;
        for (int i = 0; i < count.Length; i++)
        {
            int e = count[i];
            if (e > maxE || (e == maxE && i > maxIdx))
            {
                maxE = e;
                maxIdx = i;
            }
        }

        maxIdx += 1;

        if (maxE == 5)
        {
            return 1_000_000_000 * maxIdx;
        }
        if (maxE == 4)
        {
            return 100_000_000 * maxIdx + s;
        }
        int idx = -1;
        for (int i = 0; i < count.Length; i++)
        {
            if (count[i] == 2)
            {
                idx = i;
                break;
            }
        }
        if (maxE == 3)
        {
            if (idx != -1)
            {
                return 10_000_000 * maxIdx + s;
            }
        }
        bool b = true;
        for (int i = 1; i < count.Length - 1; i++)
        {
            if (count[i] == 0)
            {
                b = false;
                break;
            }
        }
        if (b)
        {
            if (count[^1] > 0)
            {
                return 1_000_000 + idx + 1;
            }
            else
            {
                return 100_000 + idx + 1;
            }
        }
        if (maxE == 3)
        {
            return 10_000 * maxIdx + s;
        }
        if (maxE == 2)
        {
            int temp = -1;
            for (int i = 0; i < count.Length; i++)
            {
                if (count[i] == 2 && maxIdx != i)
                {
                    temp = i + 1;
                    break;
                }
            }
            if (temp != -1)
            {
                return 1_000 * maxIdx + 100 * temp + s;
            }
            else
            {
                return 100 * maxIdx + s;
            }
        }

        return 10 * maxIdx + s;
    }
}

[Serializable]
public class GameSettings
{
    public int money;
    public int minimum;
    public int maximum;
    public bool betting;

}

[Serializable]
public class GameState
{
    public int turn;
    public int rolls;
    public List<int> diceRolls;
    public bool[] selectedDice;
    public int pot;
    public int bettingTurn;
    public int raise;
    public List<int> roundPlayers;

    public List<int> players;

    public int games;

}