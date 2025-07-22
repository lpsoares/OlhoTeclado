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
    public float Probability = 0.0f; // Probability of the key being pressed (used for non keys only)

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

        keyMaterial.color = IsKey ? backgroundColor.WithAlpha(Math.Min(backgroundColor.a, alpha)) : Color.clear;
        IsCurrent = false;
    }

    void Update()
    {
        keyMaterial.color = IsKey ? backgroundColor.WithAlpha(Math.Min(backgroundColor.a, alpha)) : Color.clear;
        textMesh.color = Color.white.WithAlpha(alpha);

        float z = IsCurrent ? -0.02f : 0.0f;
        float scaleFactor = 1.0f;
        if (!IsKey)
        {
            z = -0.02f * Probability; // Adjust z position based on probability for non-key elements
            scaleFactor = 1.0f + Probability * 0.5f; // Scale up based on probability
        }
        transform.localPosition = new Vector3(X, Y, z);
        transform.localScale = new Vector3(Width * scaleFactor, Height * scaleFactor, Depth);
    }
}
