using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    public Vector3 CurrentGaze { get; set; }

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
    private KeyboardStateMachine keyboardStateMachine;

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
                GameObject keyObject = Instantiate(keyPrefab);
                Key key = keyObject.GetComponent<Key>();
                string label = keyLayout[row][col];
                key.label = label;
                key.alpha = Alpha;
                keyObject.name = "Key_" + label;

                keyObject.transform.SetParent(transform);
                key.X = x0 + col * (keySpacing + keySize) + row * rowDx;
                key.Y = y0 - row * (keySpacing + keySize);
                key.KeySize = keySize;

                keys.Add(key);
            }
        }

        keyboardStateMachine = new KeyboardStateMachine(keys, keySize / 2.0f);
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

        if (State == KeyboardState.Current && CurrentGaze != Vector3.zero)
        {
            Key selectedKey = keyboardStateMachine.Update(CurrentGaze, out bool changed);
            if (changed)
            {
                foreach (Key key in keys)
                {
                    key.IsCurrent = key == selectedKey;
                }
            }
        }
        else
        {
            keyboardStateMachine.Reset(); // Reset the state machine when not in current state
            foreach (Key key in keys)
            {
                key.IsCurrent = false; // Reset all keys to not current
            }
        }
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

class KeyboardKeyState
{
    public string Label { get; set; }
    public bool IsEmpty
    {
        get => string.IsNullOrEmpty(Label);
    }

    public readonly Key keyObject;
    private readonly float keyRadius;
    internal static readonly KeyboardKeyState Empty = new KeyboardKeyState(string.Empty, null, 0.0f);

    public KeyboardKeyState(string label, Key keyObject, float keyRadius)
    {
        Label = label;
        this.keyObject = keyObject;
        this.keyRadius = keyRadius;
    }

    public float GetProbability(Vector3 gazePosition)
    {
        if (IsEmpty)
        {
            return 0.0f;
        }
        Vector3 keyPosition = keyObject.transform.position;
        float distance = Mathf.Max(Vector3.Distance(gazePosition, keyPosition) - keyRadius, 0.0f);
        // Compute the probability based on a gaussian distribution centered at the key position
        float sigma = keyRadius;
        float probability = Mathf.Exp(-distance * distance / (2 * sigma * sigma));
        return Mathf.Clamp(probability, 0.0f, 1.0f);
    }
}

class KeyboardStateMachine
{
    private readonly List<KeyboardKeyState> keyStates = new();
    private KeyboardKeyState currentState = KeyboardKeyState.Empty;
    private float lastTimeInState = -1.0f;
    private const float timeToChangeState = 0.05f;
    private const float probRatioThreshold = 1.1f;


    public KeyboardStateMachine(List<Key> keys, float keyRadius)
    {
        foreach (Key key in keys)
        {
            keyStates.Add(new KeyboardKeyState(key.label, key, keyRadius));
        }
    }

    public Key Update(Vector3 gazePosition, out bool changed)
    {
        float maxProbability = 0.2f; // Minimum probability threshold to consider a key
        float curStateProbability = 0;
        KeyboardKeyState bestState = KeyboardKeyState.Empty;
        float now = Time.time;

        foreach (KeyboardKeyState keyState in keyStates)
        {
            float probability = keyState.GetProbability(gazePosition);
            if (keyState == currentState)
            {
                curStateProbability = probability;
            }
            if (probability > maxProbability)
            {
                maxProbability = probability;
                bestState = keyState;
            }
        }

        changed = !currentState.Equals(bestState);
        if (!changed)
        {
            lastTimeInState = now;
        }
        else if (maxProbability > curStateProbability * probRatioThreshold && (lastTimeInState < 0 || now - lastTimeInState > timeToChangeState))
        {
            currentState = bestState;
            lastTimeInState = now;
        }
        else
        {
            changed = false;
        }

        if (!currentState.IsEmpty)
        {
            return currentState.keyObject;
        }
        return null;
    }

    internal void Reset()
    {
        currentState = KeyboardKeyState.Empty; // Reset the current state
    }
}