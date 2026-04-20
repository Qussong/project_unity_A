using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class TouchHandler : MonoBehaviour, IPointerDownHandler
{
    public UnityEvent onPointerDown = new UnityEvent();

    public void OnPointerDown(PointerEventData eventData)
    {
        onPointerDown.Invoke();
    }
}
