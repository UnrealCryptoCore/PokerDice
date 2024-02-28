using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WinScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetWinner(int idx, int score)
    {
        gameObject.SetActive(true);
        _text.text = GameManager.Instance.PlayerNames[idx] + " won the game!";
    }


}
