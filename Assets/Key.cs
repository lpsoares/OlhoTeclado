using System;
using Unity.VisualScripting;
using UnityEngine;

public class Key : MonoBehaviour
{
    public string label = "A"; // The text to display
    public Material keyMaterial;
    public Color backgroundColor = new(0.15f, 0.15f, 0.15f);
    public float alpha = 1.0f;
    private TextMesh textMesh;

    public float X { get; set; } = 0.0f;
    public float Y { get; set; } = 0.0f;
    public float Width { get; set; } = 1.0f / 15.5f;
    public float Height { get; set; } = 1.0f / 15.5f;
    public float Depth { get; set; } = 1.0f / 155f;
    public bool IsCurrent { get; set; }
    public bool IsKey = true; // Indicates if this is a key or part of a larger gesture keyboard

    void Start()
    {
        // Create a new GameObject for the text
        GameObject textObj = new("KeyText");
        textObj.transform.SetParent(transform);

        // Position the text slightly above the cylinder surface
        textObj.transform.localPosition = new Vector3(0, 0, -0.5f);

        // Add TextMesh component
        textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = label;
        textMesh.fontSize = 100;
        textMesh.characterSize = 0.02f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = Color.white.WithAlpha(alpha);

        keyMaterial = GetComponent<Renderer>().material;
        keyMaterial.SetTransparent();

        keyMaterial.color = backgroundColor.WithAlpha(Math.Min(backgroundColor.a, alpha));
        IsCurrent = false;
    }

    void Update()
    {
        keyMaterial.color = IsKey ? backgroundColor.WithAlpha(Math.Min(backgroundColor.a, alpha)) : Color.clear;
        textMesh.color = Color.white.WithAlpha(alpha);

        if (IsCurrent)
        {
            transform.localPosition = new Vector3(X, Y, -0.02f);
        }
        else
        {
            transform.localPosition = new Vector3(X, Y, 0.0f);
        }
        transform.localScale = new Vector3(Width, Height, Depth);
    }
}
