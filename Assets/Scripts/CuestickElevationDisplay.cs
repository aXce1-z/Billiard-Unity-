using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CuestickElevationDisplay : MonoBehaviour
{
    private TextMeshProUGUI label;

    private void Awake()
    {
        label = GetComponent<TextMeshProUGUI>();
    }

    public void UpdateLabel(object currentAngle)
    {
        label.text = ((float)currentAngle).ToString("0");
    }
}
