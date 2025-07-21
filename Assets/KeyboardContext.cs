using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;


public class KeyboardContext : MonoBehaviour
{
    public Color backgroundColor = new(0f, 0f, 0f, 0f);
    public Color keyColor = new(0.15f, 0.15f, 0.15f, 1.0f);
    private Color prevColor;
    private const float INACTIVE_NEXT_DEPTH = 3.0f;
    private const float NEXT_DEPTH = 1.0f;
    private const float CURR_DEPTH = 0.0f;
    private const float PREV_DEPTH = -0.5f;
    private const float INACTIVE_PREV_DEPTH = -10.0f;
    private const float DEPTH_MOV_TIME_SEC = 0.1f; // Time to move between depths in seconds
    public static readonly float[] DEPTHS = { INACTIVE_NEXT_DEPTH, NEXT_DEPTH, CURR_DEPTH, PREV_DEPTH, INACTIVE_PREV_DEPTH };

    public float keySize = 1.0f / 15.5f;
    public float keySpacing = 0.5f / 15.5f;
    public float Depth { get; set; }
    public float TargetDepth
    {
        get => DEPTHS[(int)State];
    }
    public float Alpha
    {
        get => Mathf.Clamp(Depth > 0 ? 1.0f - Depth * 0.5f : 1.0f + Depth * 1.9f, 0.0f, 1.0f);
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
    public Key LastSelectedKey { get; private set; } = null;

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
            if (prevState != (int)state && state == KeyboardState.Current)
            {
                LastSelectedKey = null;
            }

            if (prevState < DEPTHS.Length)
            {
                Depth = DEPTHS[prevState];
            }
            depthSpeed = (TargetDepth - Depth) / DEPTH_MOV_TIME_SEC;
        }
    }

    public readonly List<Key> Keys = new();
    private List<List<string>> keyLayout = new()
    {
        new List<string> { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" },
        new List<string> { "A", "S", "D", "F", "G", "H", "J", "K", "L" },
        new List<string> { "Z", "X", "C", "V", "B", "N", "M", "." },
        new List<string> { " "}
    };
    private KeyboardStateMachine keyboardStateMachine;
    private GameObject backgroundRect;
    private Material backgroundMaterial;

    void Start()
    {
        transform.position = new Vector3(0, 0, Depth);

        InitKeyLayout(keyLayout);
    }

    void Update()
    {
        if (prevColor != keyColor)
        {
            prevColor = keyColor;
            foreach (Key key in Keys)
            {
                key.backgroundColor = keyColor;
            }
        }

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
        backgroundMaterial.color = backgroundColor.WithAlpha(Alpha * 0.5f);
        foreach (Key key in Keys)
        {
            key.alpha = Alpha;
        }
        transform.localPosition = new Vector3(0, 0, Depth);

        if (State == KeyboardState.Current && CurrentGaze != Vector3.zero)
        {
            Key selectedKey = keyboardStateMachine.Update(CurrentGaze, out bool changed);
            if (changed)
            {
                LastSelectedKey = selectedKey;
                foreach (Key key in Keys)
                {
                    key.IsCurrent = key == selectedKey;
                }
            }
        }
        else
        {
            keyboardStateMachine.Reset(); // Reset the state machine when not in current state
            foreach (Key key in Keys)
            {
                key.IsCurrent = false; // Reset all keys to not current
            }
        }
    }

    public void InitKeyLayout(List<List<string>> newLayout, List<string> nonKeys = null)
    {
        keyLayout = newLayout;
        CleanUp();
        
        float keyboardOffsetY = -0.4f;
        float keyboardY0 = keyboardOffsetY + 3 * keySpacing + 2.5f * keySize;

        // Create background rectangle
        float padding = 2 * keySpacing;
        float width = keyLayout[0].Count * (keySize + keySpacing) - keySpacing + padding * 2;
        float height = keyLayout.Count * (keySize + keySpacing) - keySpacing + padding * 2;
        backgroundRect = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backgroundRect.transform.SetParent(transform);
        backgroundMaterial = backgroundRect.GetComponent<Renderer>().material;
        backgroundMaterial.SetTransparent();

        // Position the background rectangle slightly behind the keys and centered at the keyboard
        backgroundRect.transform.localPosition = new Vector3(0, keyboardY0 - keyLayout.Count / 2 * keySpacing - keyLayout.Count / 2f * keySize, 1.5f * keySize);
        backgroundRect.transform.localScale = new Vector3(width, height, keySize / 10);

        for (int row = 0; row < keyLayout.Count; row++)
        {
            float x0 = -(keyLayout[row].Count - 1) * (keySpacing + keySize) / 2.0f; // Center the row (x0 is the center of the leftmost key)
            float y0 = keyboardY0 - row * (keySpacing + keySize);

            for (int col = 0; col < keyLayout[row].Count; col++)
            {
                string label = keyLayout[row][col];
                GameObject keyObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                keyObject.AddComponent<Key>();
                Key key = keyObject.GetComponent<Key>();
                key.label = label;
                key.alpha = Alpha;
                key.IsKey = nonKeys?.IndexOf(label) < 0;
                keyObject.name = "Key_" + label;

                keyObject.transform.SetParent(transform);
                key.X = x0 + col * (keySpacing + keySize);
                key.Y = y0;
                key.Width = keySize * (1 + 0.25f * (label == " " ? 8 : 0));
                key.Height = keySize;
                key.Depth = keySize / 10;

                Keys.Add(key);
            }
        }

        keyboardStateMachine = new KeyboardStateMachine(Keys);
    }

    internal void CleanUp()
    {
        foreach (Key key in Keys)
        {
            GameObject.Destroy(key.gameObject);
        }
        Keys.Clear();
        if (backgroundRect != null)
        {
            GameObject.Destroy(backgroundRect);
            backgroundRect = null;
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
    internal static readonly KeyboardKeyState Empty = new KeyboardKeyState(string.Empty, null);

    public KeyboardKeyState(string label, Key keyObject)
    {
        Label = label;
        this.keyObject = keyObject;
    }

    public float GetProbability(Vector3 gazePosition)
    {
        if (IsEmpty)
        {
            return 0.0f;
        }
        Vector3 up = keyObject.transform.up;
        Vector3 right = keyObject.transform.right;
        Vector3 keyPosition = keyObject.transform.position;
        // Separate x and y components for distance calculation, where x and y point to the up and right of the key
        float xDistance = Math.Max(Math.Abs(Vector3.Dot(gazePosition - keyPosition, right)) / right.magnitude - keyObject.Width / 2, 0.0f);
        float yDistance = Math.Max(Math.Abs(Vector3.Dot(gazePosition - keyPosition, up)) / up.magnitude - keyObject.Height / 2, 0.0f);
        float distanceSq = xDistance * xDistance + yDistance * yDistance;
        // Compute the probability based on a gaussian distribution centered at the key position
        float sigma = keyObject.Height / 2.0f;
        float probability = Mathf.Exp(-distanceSq / (2 * sigma * sigma));
        return Mathf.Clamp(probability, 0.0f, 1.0f);
    }
}

class KeyboardStateMachine
{
    private readonly List<KeyboardKeyState> keyStates = new();
    private KeyboardKeyState currentState = KeyboardKeyState.Empty;
    private float lastTimeInState = -1.0f;
    private const float timeToChangeState = 0.1f;
    private const float probRatioThreshold = 1.1f;


    public KeyboardStateMachine(List<Key> keys)
    {
        foreach (Key key in keys)
        {
            keyStates.Add(new KeyboardKeyState(key.label, key));
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