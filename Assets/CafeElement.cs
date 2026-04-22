using UnityEngine;
using TMPro;

public class CafeElement : MonoBehaviour
{
    [Header("Label Settings")]
    public string elementName = "Vegemite";
    public Color labelColor = new Color(0.3f, 0.1f, 0f);

    [Header("Label Reference")]
    public TextMeshPro labelText;

    void Start()
    {
        if (labelText != null)
        {
            labelText.text = elementName;
            labelText.color = labelColor;
        }
    }
}