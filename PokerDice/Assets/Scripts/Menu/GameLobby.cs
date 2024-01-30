using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameLobby : MonoBehaviour
{
    [SerializeField] private TMP_InputField _gameidText;
    [SerializeField] private TMP_Text _playerText;
    [SerializeField] private GameObject _startGameBtn;
    private string _gameid;
    private List<string> _players = new();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetGameId(string gameid) {
        _gameid = gameid;
        _gameidText.text = gameid;
    }

    public void AddPlayer(string player) {
        _players.Add(player);
        _playerText.text += "\n" + player;
    }

    public void ClearPlayers() {
        _players.Clear();
        _playerText.text = "";
    }

    public void Reset() {
        _startGameBtn.SetActive(false);
        _startGameBtn.GetComponent<Button>().interactable = false;
        ClearPlayers();
    }

    public void SetStartGameBtnActive(bool active) {
        _startGameBtn.SetActive(active);
    }

    public void SetStartGameBtnInteractable(bool b) {
        _startGameBtn.GetComponent<Button>().interactable = b;
    }
}
