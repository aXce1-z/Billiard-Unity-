using TMPro;
using UnityEngine;

public class UILabel : MonoBehaviour
{
    private TextMeshProUGUI t;
    private void Awake()
    {
        t=GetComponent<TextMeshProUGUI>();
    }

    public void UpdateLabel(object text)
    {
        t.text = (string)text;
    }
}
