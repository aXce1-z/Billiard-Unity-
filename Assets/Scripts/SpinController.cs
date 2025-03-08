using UnityEngine;
using UnityEngine.EventSystems;

public class SpinController : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private GameEvent cueballSpinChanged;
    [SerializeField] private Transform controllerArea, strikePoint;
    [SerializeField] private float controllerRadius, snapOffset;

    public void OnDrag(PointerEventData eventData)
    {
        UpdateSpin(eventData.position);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateSpin(eventData.position);
    }

    private void UpdateSpin(Vector3 pointerPos)
    {
        Vector3 p = controllerArea.InverseTransformPoint(pointerPos);

        if (Mathf.Abs(p.x) < snapOffset) p.x = 0;
        if (Mathf.Abs(p.y) < snapOffset) p.y = 0;

        float radius = p.magnitude;
        if (radius > controllerRadius)
        {
            p *= controllerRadius / radius;
        }
        strikePoint.localPosition = p;

        Vector2 spin = new Vector2(p.x / controllerRadius, p.y / controllerRadius);

        cueballSpinChanged.Raise(spin);
    }
   
    public void ResetSpin()
    {
        strikePoint.localPosition = Vector3.zero;
        cueballSpinChanged.Raise(Vector2.zero);
    }
}
