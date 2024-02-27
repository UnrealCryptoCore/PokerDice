using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InGamePlayerGameInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private UIDie[] _dice;
    [SerializeField] private GameObject _turnMarker;
    [SerializeField] private GameObject _winMarker;

    private string _name;
    private int _money;

    public void Init(string name, int score, bool money)
    {
        _name = name;
        _money = score;
        UpdateScore(score, money);
    }

    public void UpdateScore(int score, bool money)
    {
        _money = score;
        _text.text = _name + " " + _money + (money ? "$" : "");
    }

    void Start()
    {
        SetDiceSide(null);
    }

    void Update()
    {

    }

    public void SetDiceSide(List<int> numbers)
    {
        for(int i=0; i<_dice.Length; i++)
        {
            _dice[i].SetSide(numbers != null ? numbers[i] : -1);
        }
    }

    public void SetTurn(bool b)
    {
        _turnMarker.SetActive(b);
    }

    public void WonRound()
    {
        StartCoroutine(ShowWinMarker());
    }

    IEnumerator ShowWinMarker()
    {
        _winMarker.SetActive(true);
        yield return new WaitForSeconds(2);
        _winMarker.SetActive(false);
        yield return null;
    }
}
