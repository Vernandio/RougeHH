using UnityEngine;
using UnityEngine.EventSystems;

public class ItemHovered : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject whiteOverlay;

    private UnityEngine.UI.Image overlayImage;

    void Start()
    {
        if (whiteOverlay != null)
        {
            overlayImage = whiteOverlay.GetComponent<UnityEngine.UI.Image>();
            if (overlayImage != null)
            {
                overlayImage.raycastTarget = false;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (whiteOverlay != null)
        {
            whiteOverlay.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (whiteOverlay != null)
        {
            whiteOverlay.SetActive(false);
        }
    }
}
