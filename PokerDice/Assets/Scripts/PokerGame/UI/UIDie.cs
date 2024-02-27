using UnityEngine;
using UnityEngine.UI;

public class UIDie : MonoBehaviour
{

    private Image _image;
    private UnityEngine.UI.Outline _outline;
    [SerializeField] Sprite[] _texures;
    [SerializeField] private int diceNumber = -1;

    void Awake()
    {
        _image = GetComponent<Image>();
        _outline = gameObject.GetComponent<UnityEngine.UI.Outline>();
        _outline.enabled = false;
    }
 
    void Start()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {

    }

    public void ClearOutline()
    {
        _outline.enabled = false;
    }

    public void SetSide(int side)
    {
        if (side == -1)
        {
            gameObject.SetActive(false);
            return;
        }
        else
        {
            gameObject.SetActive(true);
        }
        _image.sprite = _texures[side];
    }

    public void SelectDice()
    {
        if (diceNumber == -1)
        {
            return;
        }
        var dice = GameManager.Instance.Dice[diceNumber];
        if (!dice.Selectabe)
        {
            return;
        }
        dice.OnMouseClick();
        _outline.effectColor = dice.Selected ? dice.SelectionColor : dice.HoverColor;
    }

     
    public void HoverEnter()
    {
        if (diceNumber == -1)
        {
            return;
        }
        var dice = GameManager.Instance.Dice[diceNumber];
        if (!dice.Selectabe)
        {
            return;
        }
        if (!_outline.enabled)
        {
            _outline.enabled = true;
        }
        dice.OnMouseEnter();
    }
    public void HoverExit()
    {
        if (diceNumber == -1)
        {
            return;
        }
        var dice = GameManager.Instance.Dice[diceNumber];
        if (!dice.Selectabe)
        {
            return;
        }
         if (!dice.Selected)
        {
            _outline.enabled = false;
        }
        dice.OnMouseExit();
    }
}
