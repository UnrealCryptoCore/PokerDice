using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ButtonRollHint : MonoBehaviour
{
    [SerializeField] public GameObject[] Text;
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public void SetActive(bool b1, bool b2)
    {
        gameObject.SetActive(true);
        Text[0].SetActive(b1);
        Text[1].SetActive(b2);
    }
}
