using System.Collections.Generic;
using UnityEngine;

public class PlayerInfoBox : MonoBehaviour
{
    [SerializeField] private InGamePlayerGameInfo _playerInfo;

    private List<InGamePlayerGameInfo> _players;

    void Start()
    {
        _players = new();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddInfo(string name, int score, bool money)
    {
        var playerInfo = Instantiate(_playerInfo, transform);
        playerInfo.Init(name, score, money);
        _players.Add(playerInfo);
    }

    public void SetScore(int idx, int score, bool money)
    {
        _players[idx].UpdateScore(score, money);
    }

    public void SetDiceSide(int idx, List<int> numbers)
    {
        _players[idx].SetDiceSide(numbers);
    }

    public void SetPlayerTurn(int idx)
    {
        for (int i = 0; i < _players.Count; i++)
        {
            _players[i].SetTurn(idx == i);
        }
    }

    public void SetWinner(int idx)
    {
        _players[idx].WonRound();
    }
}
