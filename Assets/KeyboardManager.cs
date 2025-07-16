using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public interface ITextChangeListener
{
    void OnTextChanged(string newText);
}

public interface IContextChangeListener
{
    void OnContextChanged(KeyboardContext newContext);
}

public interface IContextPositionsListener
{
    void OnContextPositionsChanged(ContextPositionData[] contextPositionData);
}

public class ContextPositionData
{
    public string contextName;
    public Vector3 origin;
    public Vector3 upVector;
    public Vector3 rightVector;
    public Vector3 forwardVector;

    public ContextPositionData(string contextName, Vector3 origin, Vector3 upVector, Vector3 rightVector, Vector3 forwardVector)
    {
        this.contextName = contextName;
        this.origin = origin;
        this.upVector = upVector;
        this.rightVector = rightVector;
        this.forwardVector = forwardVector;
    }
}

public class KeyboardManager : MonoBehaviour
{
    public Camera mainCamera; // Assign in Inspector or find in Start
    public float distanceInFront = 2f; // Distance in front of the camera
    private ContextGazeInteraction contextGazeInteraction;
    public GameObject keyboardContextPrefab;
    public GameObject textOutputPrefab;

    public bool debugGaze = true;

    public List<KeyboardContext> keyboardContexts;

    private GameObject gazeDebugObject;
    private KeyboardState curState = KeyboardState.Initial;
    private KeyboardContext curContext = null;
    private TextOutput textReference;
    private TextOutput textOutput;
    private readonly List<ITextChangeListener> textChangeListeners = new();
    private readonly List<IContextChangeListener> contextChangeListeners = new();
    private readonly List<IContextPositionsListener> contextPositionListeners = new();

    InputAction resetKeyboardAction;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (contextGazeInteraction == null)
        {
            contextGazeInteraction = FindObjectOfType<ContextGazeInteraction>();
        }

        GetComponents(textChangeListeners);
        if (textChangeListeners.Count == 0)
        {
            Debug.LogWarning("No text change listeners found. Please add ITextChangeListener components to the scene.");
        }
        GetComponents(contextChangeListeners);
        if (contextChangeListeners.Count == 0)
        {
            Debug.LogWarning("No context change listeners found. Please add IContextChangeListener components to the scene.");
        }
        GetComponents(contextPositionListeners);
        if (contextPositionListeners.Count == 0)
        {
            Debug.LogWarning("No context position listeners found. Please add IContextPositionListener components to the scene.");
        }

        textReference = Instantiate(textOutputPrefab).GetComponent<TextOutput>();
        textReference.transform.SetParent(transform);
        textReference.transform.localPosition = new Vector3(0, 0.1f, KeyboardContext.DEPTHS[(int)KeyboardState.Current]);
        textReference.text = "";

        textOutput = Instantiate(textOutputPrefab).GetComponent<TextOutput>();
        textOutput.transform.SetParent(transform);
        textOutput.transform.localPosition = new Vector3(0, 0.05f, KeyboardContext.DEPTHS[(int)KeyboardState.Current]);
        textOutput.text = "";

        keyboardContexts = new List<KeyboardContext>();
        KeyboardState[] states = { KeyboardState.InactiveNext, KeyboardState.Next, KeyboardState.Current, KeyboardState.Previous, KeyboardState.InactivePrevious };
        for (int i = 0; i < states.Length; i++)
        {
            KeyboardContext context = Instantiate(keyboardContextPrefab).GetComponent<KeyboardContext>();
            keyboardContexts.Add(context);
            context.transform.SetParent(transform);
            context.State = states[i];
            context.Depth = context.TargetDepth;
        }

        // Instantiate a red sphere for gaze debugging
        gazeDebugObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gazeDebugObject.transform.localScale = Vector3.one * 0.02f; // Scale down the sphere
        gazeDebugObject.GetComponent<Renderer>().material.color = Color.red; // Set color to red
        gazeDebugObject.SetActive(false); // Initially hide the debug object

        resetKeyboardAction = InputSystem.actions.FindAction("Reset Keyboard Position");
    }

    void Update()
    {
        if (resetKeyboardAction.IsPressed())
        {
            Vector3 inFront = mainCamera.transform.position + mainCamera.transform.forward * distanceInFront;
            transform.SetPositionAndRotation(inFront, Quaternion.LookRotation(mainCamera.transform.forward, mainCamera.transform.up));
            NotifyContextPositionsListeners();
        }

        KeyboardContext context = contextGazeInteraction.GetCurrentContext(keyboardContexts, out Vector3 gazeInContext, out _);
        if (context != null)
        {
            if (debugGaze)
            {
                gazeDebugObject.transform.position = gazeInContext;
                gazeDebugObject.SetActive(true);
            }
            else
            {
                gazeDebugObject.SetActive(false);
            }

            if (curState == KeyboardState.Initial)
            {
                if (context.State == KeyboardState.Current)
                {
                    curState = KeyboardState.Current;
                    curContext = context;
                    NotifyContextChangeListeners(curContext);
                }
            }
            else if (curState != context.State)
            {
                KeyboardContext previousContext = curContext;
                curContext = context;
                NotifyContextChangeListeners(context);
                KeyboardState previousState = curState;
                curState = context.State;

                int stateDiff = 0;
                if (previousState == KeyboardState.Current && curState == KeyboardState.Next)
                {
                    stateDiff = 1;
                    textOutput.text += previousContext.LastSelectedKey == null ? "" : previousContext.LastSelectedKey.label.ToLower();
                }
                else if (previousState == KeyboardState.Current && curState == KeyboardState.Previous)
                {
                    stateDiff = -1;
                    if (textOutput.text.Length > 0)
                    {
                        textOutput.text = textOutput.text[..^1];
                    }
                }
                if (stateDiff != 0)
                {
                    NotifyTextChangeListeners(textOutput.text);
                    foreach (var ctx in keyboardContexts)
                    {
                        ctx.State = (KeyboardState)(((int)ctx.State + stateDiff + 5) % 5);
                        ctx.CurrentGaze = Vector3.zero;
                    }
                }
            }

            if (curContext != null && curContext.State == KeyboardState.Current)
            {
                curContext.CurrentGaze = gazeInContext;
            }
        }
    }

    public ContextPositionData[] GetContextPositions()
    {
        List<ContextPositionData> contextPositions = new List<ContextPositionData>();
        foreach (var ctx in keyboardContexts)
        {
            var transform = ctx.transform;
            contextPositions.Add(new ContextPositionData(ctx.State.ToString(), transform.position, transform.up, transform.right, transform.forward));
        }
        ContextPositionData[] contextPositionData = contextPositions.ToArray();
        return contextPositionData;
    }

    public void SetReferenceText(string text)
    {
        if (textReference != null)
        {
            textReference.text = text;
        }
    }

    public string GetOutputText()
    {
        return textOutput != null ? textOutput.text : string.Empty;
    }

    public void ResetOutputText()
    {
        if (textOutput != null)
        {
            textOutput.text = "";
        }
    }

    private void NotifyTextChangeListeners(string newText)
    {
        foreach (var listener in textChangeListeners)
        {
            listener.OnTextChanged(newText);
        }
    }

    private void NotifyContextChangeListeners(KeyboardContext newContext)
    {
        foreach (var listener in contextChangeListeners)
        {
            listener.OnContextChanged(newContext);
        }
    }

    private void NotifyContextPositionsListeners()
    {
        ContextPositionData[] contextPositionData = GetContextPositions();
        foreach (var listener in contextPositionListeners)
        {
            listener.OnContextPositionsChanged(contextPositionData);
        }
    }
}
