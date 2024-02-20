using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class BtnAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private static readonly int _xOffset = 20;
    private float posX;

    void Start()
    {
        posX = transform.position.x;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOMoveX(posX - _xOffset, 0.1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOMoveX(posX + _xOffset, 0.1f);
    }
}
