using UnityEngine;
using UnityEngine.UI;

public class BallDisplay : MonoBehaviour
{
    public static Color disabledColor;
    private Image icon;
    private Color enabledColor = Color.white, emptyColor = new Color(0, 0, 0, 0);
    private void Awake()
    {
        icon=GetComponentsInChildren<Image>()[1];
    }

    public void Show(Sprite sprite)
    {
        icon.color = enabledColor;
        icon.sprite = sprite;
    }

    public void Hide()
    {
        icon.color = disabledColor;
    }
    
    public void Clear()
    {
        icon.color = emptyColor;
    }
}
