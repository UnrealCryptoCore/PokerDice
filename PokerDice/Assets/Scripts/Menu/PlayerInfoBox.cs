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
}
