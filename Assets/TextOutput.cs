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