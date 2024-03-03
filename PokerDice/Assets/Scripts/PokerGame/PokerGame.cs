using UnityEngine;
using System;
using System.Collections.Generic;

public class PokerGame
{
    private GameState _state;
    private GameSettings _settings;
    private int _turn;
    private bool _won = false;


    public PokerGame(GameSettings settings, GameState state, int turn)
    {
        _settings = settings;
        _state = state;
        _turn = turn;
        _won = false;
    }

    public void Start(string[] playerNames)
    {
        GameManager.Instance.SetupGame(this, playerNames, _settings.betting);
        GameManager.Instance.DisableButtons();
        StartGame();
        if (_settings.betting)
        {
            if (_state.roundPlayers[_state.bettingTurn] == _turn)
            {
                EnableRoundButtons();
                GameManager.Instance.CheckOrCallButton.interactable = false;
                GameManager.Instance.FoldButton.interactable = false;
            }
        }
        else
        {
            GameManager.Instance.ClearPot();
            Debug.Log(_state.roundPlayers.Count + " " + _turn + " " + _state.turn);
            if (_state.roundPlayers[_state.turn] == _turn)
            {
                EnableDiceRollButtons();
            }
        }
    }

    public void StartGame()
    {
        bool below = false;
        if (_settings.betting)
        {
            for (int i = 0; i < _state.players.Count; i++)
            {
                if (_state.players[i] < _settings.minimum)
                {
                    below = true;
                    break;
                }
            }
        }
        if (_state.games == _settings.maxRounds || below)
        {
            int winner = 0;
            int score = 0;
            for (int i = 0; i < _state.players.Count; i++)
            {
                if (_state.players[i] > _state.players[winner])
                {
                    winner = i;
                    score = _state.players[i];
                }
            }
            GameManager.Instance.DisableButtons();
            GameManager.Instance.SetDiceActive(false);
            GameManager.Instance.DiceHint.SetActive(false, false);
            _won = true;
            GameManager.Instance.RunDelayedAction(() =>
            {
                GameManager.Instance.WinParticleSystem.Play();
                GameManager.Instance.WinScreen.SetWinner(winner, score);
            }, 4);
            return;
        }
        _state.rolls = 0;
        _state.diceRolls = new(_state.players.Count);
        for (int i = 0; i < _state.players.Count * 5; i++)
        {
            _state.diceRolls.Add(-1);
        }
        _state.selectedDice = new bool[5];
        Array.Fill(_state.selectedDice, true);
        ResetDice();
        _state.raise = 0;
        _state.roundPlayers = new();
        for (int i = 0; i < _state.players.Count; i++)
        {
            _state.roundPlayers.Add((i + _state.games) % _state.players.Count);
        }
        _state.games++;
        _state.bettingTurn = _settings.betting ? 0 : _state.roundPlayers.Count;
        _state.pot = _settings.betting ? 0 : 1;
        _state.turn = 0;
        UpdateTurn();

    }

    public void ResetDice()
    {
        foreach (var die in GameManager.Instance.Dice)
        {
            die.ClearOutline();
            die.SetSelected(true);
            die.Selectable = false;
        }
        GameManager.Instance.DiceThrow.ClearOutline();
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
            if (_won)
            {
                return;
            }
            if (_settings.betting)
            {
                if (_state.bettingTurn < _state.roundPlayers.Count && _state.roundPlayers[_state.bettingTurn] == _turn)
                {
                    EnableRoundButtons();
                    GameManager.Instance.CheckOrCallButton.interactable = false;
                    GameManager.Instance.FoldButton.interactable = false;
                }
            }
            else
            {
                if (_state.bettingTurn < _state.roundPlayers.Count && _state.roundPlayers[_state.turn] == _turn)
                {
                    EnableDiceRollButtons();
                }
            }
        }
    }

    public void DiceRoll(List<int> diceRolls, bool[] lastSelection)
    {
        for (int i = 0; i < 5; i++)
        {
            _state.diceRolls[_state.roundPlayers[_state.turn] * 5 + i] = diceRolls[i];
        }
        Debug.Log(string.Join(",", _state.diceRolls));

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
        GameManager.Instance.SetDiceActive(true);
        GameManager.Instance.ThrowDice(diceRolls, lastSelection);
        GameManager.Instance.RunDelayedAction(() =>
        {
            GameManager.Instance.PlayerInfoBox.SetDiceSide(_state.roundPlayers[_state.turn], diceRolls);
        }, 2);
        if (_state.rolls == 3)
        {
            NextTurn();
        }
        if (_won)
        {
            return;
        }

        if (_settings.betting)
        {
            if (_state.roundPlayers[_state.bettingTurn] == _turn)
            {
                EnableRoundButtons();
                if (_state.rolls == 0 && _state.turn == 0)
                {
                    GameManager.Instance.FoldButton.interactable = false;
                    GameManager.Instance.CheckOrCallButton.interactable = false;
                }
            }
        }
        else
        {
            if (_state.roundPlayers[_state.turn] == _turn)
            {
                EnableDiceRollButtons();
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
        if (_won)
        {
            return;
        }
        if (_settings.betting)
        {
            if (_state.roundPlayers[_state.bettingTurn] == _turn)
            {
                EnableRoundButtons();
                if (_state.rolls == 0 && _state.turn == 0)
                {
                    GameManager.Instance.FoldButton.interactable = false;
                    GameManager.Instance.CheckOrCallButton.interactable = false;
                }
            }
        }
        else
        {
            if (_state.roundPlayers[_state.turn] == _turn)
            {
                GameManager.Instance.DiceHint.SetActive(true, true);
            }
        }
    }

    public void EnableDiceRollButtons()
    {
        UpdateDiceRollButtons();
        for (int i = 0; i < _state.selectedDice.Length; i++)
        {
            GameManager.Instance.Dice[i].Selectable = _state.rolls > 0 && _state.selectedDice[i];
            GameManager.Instance.Dice[i].ClearOutline();
        }
        GameManager.Instance.DiceThrow.ClearOutline();
    }

    public void UpdateDiceRollButtons()
    {
        if (_state.rolls == 0)
        {
            GameManager.Instance.RunDelayedAction(() =>
            {
                GameManager.Instance.DiceHint.SetActive(true, true);
            }, _settings.betting ? 0 : 2);
            GameManager.Instance.EndButton.interactable = false;
            return;
        }

        int selected = 0;
        foreach (var die in GameManager.Instance.Dice)
        {
            selected += die.Selected ? 1 : 0;
        }

        if (selected == 0)
        {
            GameManager.Instance.DiceHint.SetActive(false, true);
            GameManager.Instance.EndButton.interactable = true;
        }
        else
        {
            GameManager.Instance.DiceHint.SetActive(true, true);
            GameManager.Instance.EndButton.interactable = false;
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
                EnableRoundButtons();
            }
            return; // not all players have set their bet
        }
        if (_state.roundPlayers[_state.turn] == _turn)
        {
            EnableDiceRollButtons();
        }
    }

    private void EnableRoundButtons()
    {
        int min = Math.Max(_state.raise, _settings.minimum);
        int max = Math.Min(_settings.maximum, _state.players[_turn]);
        GameManager.Instance.EnableRoundButtons(min, max, _state.players[_turn] < min);
        GameManager.Instance.BettingSlider.value = min;
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
        ResetDice();
        if (_state.turn < _state.roundPlayers.Count)
        {
            UpdateTurn();
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

    private void UpdateTurn()
    {
        GameManager.Instance.PlayerInfoBox.SetPlayerTurn(_state.roundPlayers[_state.turn]);
    }

    public void SetWinner(int player)
    {
        TransferFromPot(player);
        GameManager.Instance.HighlightPot();
        GameManager.Instance.ParticleSystem.Play();
        GameManager.Instance.RunDelayedAction(() =>
        {
            GameManager.Instance.PlayerInfoBox.SetWinner(player);
            GameManager.Instance.DiceThrow.gameObject.SetActive(false);
            //GameManager.Instance.HideDice();
        }, 2);
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
        for (int i = 1; i < count.Length - 1; i++) // bug fix
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
            else if (count[0] > 0)
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
                if (count[i] == 2 && maxIdx != i + 1)
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
    public bool allowRaisingOnce;
    public int maxRounds;

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