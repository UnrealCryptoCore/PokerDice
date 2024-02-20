using TMPro;
using UnityEngine;

public class InGamePlayerGameInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;

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
        _text.text = _name + " -- " + _money + (money ? "$" : "");
    }

    void Start()
    {

    }

    void Update()
    {

    }
}
