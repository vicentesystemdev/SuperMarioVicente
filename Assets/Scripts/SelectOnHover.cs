using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Selects the UI element when the mouse hovers it, so mouse and keyboard
// share the same highlighted option.
[RequireComponent(typeof(Selectable))]
public class SelectOnHover : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GetComponent<Selectable>().IsInteractable() && EventSystem.current != null) {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

}
