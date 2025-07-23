using TMPro;
using UnityEngine;

// This class draws the text in the scene.
public class TextOutput : MonoBehaviour
{
    public string text = "Hello, World!";
    private TextMeshPro textMeshPro;

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
        textMeshPro.color = Color.white;
    }

    void Update()
    {
        // Update the text in the TextMesh
        if (textMeshPro != null)
        {
            textMeshPro.text = text;
        }
    }
}