using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DiceThrow : MonoBehaviour
{
    [SerializeField] private UIDie[] _dice;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClearOutline()
    {
        foreach (var die in _dice)
        {
            die.ClearOutline();
        }
    }

    public void ShowDice(List<int> numbers)
    {
        gameObject.SetActive(true);
        transform.localPosition += new Vector3(0, -200);
        transform.localScale = new Vector3(0.0f, 0.0f, 1.0f);
        StartCoroutine(ShowDiceAnimation(numbers));
   }

    IEnumerator ShowDiceAnimation(List<int> numbers)
    {
        yield return new WaitForSeconds(2);
        for(int i=0; i<_dice.Length; i++)
        {
            if (numbers[i] == -1)
            {
                continue;
            }
            _dice[i].SetSide(numbers[i]);
        }
        transform.DOScale(2, 1);
        yield return new WaitForSeconds(1.5f);
        transform.DOScale(1, 1);
        transform.DOLocalMoveY(200, 1);
    }

}
