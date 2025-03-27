using UnityEngine;
using UnityEngine.EventSystems;

public class ElevationController : MonoBehaviour, IDragHandler
{
    [SerializeField] private GameEvent elevationChanged;
    [SerializeField] private Transform angleBar, angleBarBtn;


    private float currentAngle;
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 deltaVector = eventData.delta;
        float delta = deltaVector.magnitude * Mathf.Sign(deltaVector.y) * .1f;

        currentAngle = Mathf.Clamp(currentAngle + delta, 0, 45);
        UpdateElevation();
    }
    public void ResetElevation()
    {
        currentAngle = 0;
        UpdateElevation();
    }

    private void UpdateElevation()
    {
        angleBar.rotation = angleBarBtn.rotation= new Quaternion()
        {
            eulerAngles = Vector3.forward * currentAngle
        };

        elevationChanged.Raise(currentAngle);
    }
}
