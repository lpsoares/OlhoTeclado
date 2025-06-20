using System.Collections.Generic;
using UnityEngine;

public class KeyboardContext : MonoBehaviour
{
    private const float INACTIVE_NEXT_DEPTH = -2.0f;
    private const float NEXT_DEPTH = -0.5f;
    private const float CURR_DEPTH = 0.0f;
    private const float PREV_DEPTH = 1.0f;
    private const float INACTIVE_PREV_DEPTH = 10.0f;
    private const float DEPTH_MOV_TIME_SEC = 0.1f;
    private static readonly float[] DEPTHS = { INACTIVE_NEXT_DEPTH, NEXT_DEPTH, CURR_DEPTH, PREV_DEPTH, INACTIVE_PREV_DEPTH };

    public GameObject keyPrefab; // Prefab for the keys
    public float keySize = 1.0f / 15.5f;
    public float keySpacing = 0.5f / 15.5f;
    public float Depth { get; set; }
    public float TargetDepth
    {
        get => DEPTHS[(int)State];
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
    public Plane TargetPlane
    {
        get
        {
            Vector3 position = transform.parent != null
                ? transform.parent.TransformPoint(new Vector3(0, 0, TargetDepth))
                : transform.TransformPoint(new Vector3(0, 0, TargetDepth));
            Vector3 normal = -transform.forward;
            return new Plane(normal, position);
        }
    }

    private float depthSpeed = 1.0f;
    private KeyboardState state = KeyboardState.Initial;
    internal KeyboardState State
    {
        get => state;
        set
        {
            int prevState = (int)state;
            state = value;
            if (state.IsActive())
            {
                gameObject.SetActive(true);
            }

            if (prevState < DEPTHS.Length)
            {
                Depth = DEPTHS[prevState];
            }
            depthSpeed = (TargetDepth - Depth) / DEPTH_MOV_TIME_SEC;
        }
    }

    private readonly List<Key> keys = new();
    private readonly List<List<string>> keyLayout = new()
    {
        new List<string> { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" },
        new List<string> { "A", "S", "D", "F", "G", "H", "J", "K", "L" },
        new List<string> { "Z", "X", "C", "V", "B", "N", "M", "." },
    };

    void Start()
    {
        transform.position = new Vector3(0, 0, Depth);

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
        if (Depth != TargetDepth)
        {
            Depth += depthSpeed * Time.deltaTime;
            if ((depthSpeed > 0 && Depth >= TargetDepth) || (depthSpeed < 0 && Depth <= TargetDepth))
            {
                Depth = TargetDepth;
                if (!State.IsActive())
                {
                    gameObject.SetActive(false);
                }
            }
        }
        foreach (Key key in keys)
        {
            key.alpha = Alpha;
        }
        transform.localPosition = new Vector3(0, 0, Depth);
    }
}

public enum KeyboardState
{
    InactiveNext,
    Next,
    Current,
    Previous,
    InactivePrevious,
    Initial,
}

public static class KeyboardStateExtensions
{
    public static bool IsActive(this KeyboardState state)
    {
        return state != KeyboardState.InactiveNext && state != KeyboardState.InactivePrevious;
    }
}