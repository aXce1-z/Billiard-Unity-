using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HitController : MonoBehaviour, IPointerUpHandler, IDragHandler, IPointerDownHandler
{
    [SerializeField] private GameEvent cueballStrikeBegan, cueballStrikeFinished;
    [SerializeField] private Cuestick cuestick;

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (slider.value > 0)
        {
            cuestick.Strike(slider.value);
            oldVal=slider.value = 0;
            cueballStrikeFinished.Raise();
        }
    }

    private float oldVal;

    public void OnDrag(PointerEventData eventData)
    {
        cuestick.Pull(slider.value - oldVal);
        oldVal = slider.value;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        cueballStrikeBegan.Raise();
    }
}
