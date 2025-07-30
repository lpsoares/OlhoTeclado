using TMPro;
using UnityEngine;

// This class draws the text in the scene.
public class TextOutput : MonoBehaviour
{
    public string text = "Hello, World!";
    private TextMeshPro textMeshPro;
    public Color textColor = Color.white;

    void Start()
    {
        // Create a new GameObject for the text
        GameObject textObj = new("Output Text");
        textObj.transform.SetParent(transform);
        textObj.transform.SetLocalPositionAndRotation(new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));

        // Add TextMesh component
        textMeshPro = textObj.AddComponent<TextMeshPro>();
        textMeshPro.text = text;
        textMeshPro.fontSize = 0.3f;
        textMeshPro.alignment = TextAlignmentOptions.Center;
        textMeshPro.color = textColor;
    }

    void Update()
    {
        if (textMeshPro != null)
        {
            textMeshPro.text = text;
            textMeshPro.color = textColor;
        }
    }
}