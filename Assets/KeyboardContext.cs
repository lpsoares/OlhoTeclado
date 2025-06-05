using System.Collections.Generic;
using UnityEngine;

public class KeyboardContext
{
    private readonly float depth;
    private List<Key> keys = new List<Key>();
    private List<List<string>> keyLayout = new List<List<string>>()
    {
        new List<string> { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" },
        new List<string> { "A", "S", "D", "F", "G", "H", "J", "K", "L" },
        new List<string> { "Z", "X", "C", "V", "B", "N", "M", "." },
    };

    public KeyboardContext(float initialDepth, System.Func<GameObject> instantiateKey)
    {
        float keySize = 1.0f / 15.5f;
        float keySpacing = keySize / 2;
        float x0 = -(9.0f * keySpacing + 9.0f * keySize) / 2.0f;
        float y0 = -0.5f + 3 * keySpacing + 2.5f * keySize;
        float rowDx = (keySize + keySpacing) / 2.0f;
        for (int row = 0; row < keyLayout.Count; row++)
        {
            for (int col = 0; col < keyLayout[row].Count; col++)
            {
                GameObject key = instantiateKey();
                Key keyScript = key.GetComponent<Key>();
                string label = keyLayout[row][col];
                keyScript.label = label;
                keyScript.Depth = initialDepth;
                key.name = "Key_" + label;

                key.transform.SetLocalPositionAndRotation(new Vector3(x0 + col * (keySpacing + keySize) + row * rowDx, y0 - row * (keySpacing + keySize), initialDepth), Quaternion.Euler(90f, 0f, 0f));
                key.transform.localScale = new Vector3(keySize, keySize / 10, keySize);

                keys.Add(keyScript);
            }
        }

        depth = initialDepth;
    }
}