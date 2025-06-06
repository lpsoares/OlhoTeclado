using System.Collections.Generic;
using UnityEngine;

public class KeyboardContext : MonoBehaviour
{
    public GameObject keyPrefab; // Prefab for the keys
    public float keySize = 1.0f / 15.5f;
    public float keySpacing = 0.5f / 15.5f;
    private float depth;
    public float Depth
    {
        get => depth;
        set => depth = Mathf.Clamp(value, -1.0f, 1.0f);
    }
    public float Alpha
    {
        get => Mathf.Clamp(Depth > 0 ? 1.0f - Depth * 0.9f : 1.0f + Depth * 1.9f, 0.0f, 1.0f);
    }
    public Plane Plane
    {
        get
        {
            Vector3 position = transform.position;
            Vector3 normal = -transform.forward;
            return new Plane(normal, position);
        }
    }
    private List<Key> keys = new List<Key>();
    private List<List<string>> keyLayout = new List<List<string>>()
    {
        new List<string> { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" },
        new List<string> { "A", "S", "D", "F", "G", "H", "J", "K", "L" },
        new List<string> { "Z", "X", "C", "V", "B", "N", "M", "." },
    };

    void Start()
    {
        transform.position = new Vector3(0, 0, depth);

        float x0 = -(9.0f * keySpacing + 9.0f * keySize) / 2.0f;
        float y0 = -0.5f + 3 * keySpacing + 2.5f * keySize;
        float rowDx = (keySize + keySpacing) / 2.0f;
        for (int row = 0; row < keyLayout.Count; row++)
        {
            for (int col = 0; col < keyLayout[row].Count; col++)
            {
                GameObject key = Instantiate(keyPrefab);
                Key keyScript = key.GetComponent<Key>();
                string label = keyLayout[row][col];
                keyScript.label = label;
                keyScript.alpha = Alpha;
                key.name = "Key_" + label;

                key.transform.SetParent(transform);
                key.transform.SetLocalPositionAndRotation(new Vector3(x0 + col * (keySpacing + keySize) + row * rowDx, y0 - row * (keySpacing + keySize), 0), Quaternion.Euler(90f, 0f, 0f));
                key.transform.localScale = new Vector3(keySize, keySize / 10, keySize);

                keys.Add(keyScript);
            }
        }
    }

    void Update()
    {
        foreach (Key key in keys)
        {
            key.alpha = Alpha;
        }
        transform.localPosition = new Vector3(0, 0, depth);
    }
}