using Unity.VisualScripting;
using UnityEngine;

public class Key : MonoBehaviour
{
    public string label = "A"; // The text to display
    public Material keyMaterial;
    public float alpha = 1.0f;
    public float X { get; set; } = 0.0f;
    public float Y { get; set; } = 0.0f;
    public float KeySize { get; set; } = 1.0f / 15.5f;
    public bool IsCurrent { get; set; }

    void Start()
    {
        // Create a new GameObject for the text
        GameObject textObj = new("KeyText");
        textObj.transform.SetParent(transform);

        // Position the text slightly above the cylinder surface
        textObj.transform.SetLocalPositionAndRotation(new Vector3(0, -0.5f, 0), Quaternion.Euler(-90f, 0f, 0f));

        // Add TextMesh component
        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = label;
        textMesh.fontSize = 100;
        textMesh.characterSize = 0.002f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = Color.white;

        keyMaterial = GetComponent<Renderer>().material;
        keyMaterial.SetFloat("_Mode", 3); // Set rendering mode to Transparent
        keyMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        keyMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        keyMaterial.SetInt("_ZWrite", 0);
        keyMaterial.DisableKeyword("_ALPHATEST_ON");
        keyMaterial.EnableKeyword("_ALPHABLEND_ON");
        keyMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        keyMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        keyMaterial.color = new Color(0.15f, 0.15f, 0.15f, alpha);
        IsCurrent = false;

        transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
    }

    void Update()
    {
        keyMaterial.color = new Color(0.15f, 0.15f, 0.15f, alpha);

        if (IsCurrent)
        {
            transform.localPosition = new Vector3(X, Y, -0.02f);
        }
        else
        {
            transform.localPosition = new Vector3(X, Y, 0.0f);
        }
        transform.localScale = new Vector3(KeySize, KeySize / 10, KeySize);
    }
}
