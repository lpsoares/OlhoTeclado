using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Key : MonoBehaviour
{
    public string label = "A"; // The text to display
    private float depth = 0.0f;

    public float Depth { get => depth; set => depth = Math.Min(Math.Max(value, -1.0f), 1.0f); }

    void Start()
    {
        // Create a new GameObject for the text
        GameObject textObj = new GameObject("KeyText");
        textObj.transform.SetParent(transform);

        // Position the text slightly above the cylinder surface
        textObj.transform.localPosition = new Vector3(0, -0.5f, 0);
        textObj.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        // Add TextMesh component
        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = label;
        textMesh.fontSize = 100;
        textMesh.characterSize = 0.002f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = Color.white;

        Material material = GetComponent<Renderer>().material;
        material.SetFloat("_Mode", 3); // Set rendering mode to Transparent
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        material.color = (Color.gray * 0.3f).WithAlpha(1.0f - Math.Abs(Depth) * 0.8f);
    }
}
