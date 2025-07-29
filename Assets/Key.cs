using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Key : MonoBehaviour
{
    public string label = "A"; // The text to display
    public Material keyMaterial;
    public Color backgroundColor = new(0.15f, 0.15f, 0.15f);
    public Color textColor = new(1.0f, 1.0f, 1.0f);
    public Color highlightColor = new(0.3f, 0.3f, 0.3f, 1.0f);
    public Color dwellColor = new(0.5f, 0.5f, 1.0f, 0.5f);
    public float alpha = 1.0f;
    private TextMesh textMesh;

    public float X { get; set; } = 0.0f;
    public float Y { get; set; } = 0.0f;
    public float Width { get; set; } = 1.0f / 15.5f;
    public float Height { get; set; } = 1.0f / 15.5f;
    public float Depth { get; set; } = 1.0f / 155f;
    public bool IsCurrent { get; set; }
    public bool IsKey = true; // Indicates if this is a key or part of a larger gesture keyboard
    public bool IsCandidateKey = false; // Indicates if this key is a candidate key
    public float Probability = 0.0f; // Probability of the key being pressed (used for non keys only)
    private GameObject keyRectangle;
    public bool dwellEnabled = false;
    private bool dwelling = false;
    private float elapsedDwellTime = Mathf.Infinity;
    public float dwellTime = 0.6f;
    public Material dwellMaterial;
    private GameObject dwellObject;
    public IKeyPressListener keyPressListener;
    
    void Start()
    {
        keyRectangle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        keyRectangle.transform.SetParent(transform);
        keyRectangle.transform.localPosition = new Vector3(0, 0, 0);
        keyRectangle.transform.localScale = new Vector3(Width, Height, Depth);

        float delta = Depth / 100;

        dwellObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dwellObject.transform.SetParent(transform);
        dwellObject.transform.localPosition = new Vector3(0, 0, -Depth / 2 - delta);
        dwellObject.transform.localScale = new Vector3(Width, Height, delta);
        dwellMaterial = dwellObject.GetComponent<Renderer>().material;
        dwellMaterial.color = Color.clear;
        dwellObject.SetActive(false);

        // Create a new GameObject for the text
        GameObject textObj = new("KeyText");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = new Vector3(0, 0, -Depth / 2 - 2 * delta);

        // Add TextMesh component
        textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = label;
        textMesh.fontSize = 100;
        textMesh.characterSize = 0.02f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = textColor.WithAlpha(alpha);

        keyMaterial = keyRectangle.GetComponent<Renderer>().material;
        keyMaterial.SetTransparent();

        keyMaterial.color = IsKey ? backgroundColor.WithAlpha(Math.Min(backgroundColor.a, alpha)) : Color.clear;
        IsCurrent = false;
    }

    void Update()
    {
        if (dwellEnabled)
        {
            if (IsCurrent)
            {
                dwellObject.SetActive(true);
                dwellMaterial.color = dwellColor.WithAlpha(alpha);
                if (!dwelling)
                {
                    dwelling = true;
                    elapsedDwellTime = 0.0f;
                }

                if (elapsedDwellTime != Mathf.Infinity)
                {
                    elapsedDwellTime += Time.deltaTime;
                    if (elapsedDwellTime >= dwellTime)
                    {
                        elapsedDwellTime = Mathf.Infinity;
                        if (keyPressListener == null)
                        {
                            // Fallback to default behavior if no listener is set
                            Debug.Log($"Key {label} pressed without listener.");
                        }
                        else
                        {
                            // Notify the key press listener
                            keyPressListener.OnKeyPress(this);
                        }
                    }
                }
                else
                {
                    dwellObject.SetActive(false);
                    dwellMaterial.color = Color.clear;
                }
            }
            else
            {
                dwellObject.SetActive(false);
                dwellMaterial.color = Color.clear;
                dwelling = false;
                elapsedDwellTime = Mathf.Infinity;
            }
            float dwellRectWidth = Width * (1.0f - Math.Min(0.99999f, elapsedDwellTime / dwellTime));
            float dwellRectHeight = Height * (1.0f - Math.Min(0.99999f, elapsedDwellTime / dwellTime));
            dwellObject.transform.localScale = new Vector3(dwellRectWidth, dwellRectHeight, Depth / 100);   
        }

        textMesh.text = label;
        keyMaterial.color = IsKey ? (IsCurrent ? highlightColor : backgroundColor).WithAlpha(Math.Min(backgroundColor.a, alpha)) : Color.clear;
        textMesh.color = textColor.WithAlpha(alpha);

        float z = 0.0f;
        float scaleFactor = 1.0f;
        if (!IsKey)
        {
            z = -0.02f * Probability; // Adjust z position based on probability for non-key elements
            scaleFactor = 1.0f + Probability * 0.5f; // Scale up based on probability
        }
        transform.localPosition = new Vector3(X, Y, z);
        float rectWidth = Width * scaleFactor;
        float rectHeight = Height * scaleFactor;
        keyRectangle.transform.localScale = new Vector3(rectWidth, rectHeight, Depth);

        FitTextToKey(rectWidth);
    }

    private void FitTextToKey(float rectWidth)
    {
        int totalChars = Math.Max(label.Length, 1);
        float charWidth = GetCharWidth();
        float textWidth = charWidth * totalChars;
        float maxTextWidth = rectWidth * totalChars / (totalChars + 2);
        
        float scale = maxTextWidth / textWidth;
        textMesh.transform.localScale = new Vector3(scale, scale, 1f);
    }

    private float GetCharWidth()
    {
        // https://discussions.unity.com/t/computing-exact-size-of-text-line-with-textmesh/672587/3
        float width = 0f;
        foreach (char symbol in "W")
        {
            if (textMesh.font.GetCharacterInfo(symbol, out CharacterInfo info, textMesh.fontSize, textMesh.fontStyle))
            {
                width += info.advance;
            }
        }
        return width * textMesh.characterSize * 0.1f;
    }
}

public interface IKeyPressListener
{
    void OnKeyPress(Key key);
}