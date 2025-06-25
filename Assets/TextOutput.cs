using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// This class draws the text in the scene.
public class TextOutput : MonoBehaviour
{
    public string text = "Hello, World!";
    private TextMesh textMesh;

    void Start()
    {
        // Create a new GameObject for the text
        GameObject textObj = new("Output Text");
        textObj.transform.SetParent(transform);
        textObj.transform.SetLocalPositionAndRotation(new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));
        transform.localPosition = new Vector3(0, 0.05f, KeyboardContext.DEPTHS[(int)KeyboardState.Current]);

        // Add TextMesh component
        textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.fontSize = 100;
        textMesh.characterSize = 0.002f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = Color.white;
    }

    void Update()
    {
        // Update the text in the TextMesh
        if (textMesh != null)
        {
            textMesh.text = text;
        }
    }
}