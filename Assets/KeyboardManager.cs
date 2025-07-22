using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class KeyboardManager : MonoBehaviour, ITextChangeListener
{
    public Camera mainCamera; // Assign in Inspector or find in Start
    public float distanceInFront = 2f; // Distance in front of the camera
    
    public GameObject keyboardContextPrefab;
    
    public GameObject textOutputPrefab;

    
    public KeyboardType keyboardType;
    
    public bool debugGaze = true;

    private GameObject gazeDebugObject;
    private ContextGazeInteraction contextGazeInteraction;
    private TextOutput textReference;
    private TextOutput textOutput;
    private readonly List<ITextChangeListener> textChangeListeners = new();
    private readonly List<IContextChangeListener> contextChangeListeners = new();
    private readonly List<IContextPositionsListener> contextPositionListeners = new();

    public AbstractKeyboard Keyboard
    {
        get; private set;
    }

    InputAction resetKeyboardAction;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
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

        EyeTracker eyeTracker = GetComponent<EyeTracker>();
        List<IGazeDataListener> gazeDataListeners = new();
        GetComponents(gazeDataListeners);
        contextGazeInteraction = new ContextGazeInteraction(eyeTracker, gazeDataListeners);

        textReference = Instantiate(textOutputPrefab).GetComponent<TextOutput>();
        textReference.transform.SetParent(transform);
        textReference.transform.localPosition = new Vector3(0, 0.1f, KeyboardContext.DEPTHS[(int)KeyboardState.Current]);
        textReference.text = "";

        textOutput = Instantiate(textOutputPrefab).GetComponent<TextOutput>();
        textOutput.transform.SetParent(transform);
        textOutput.transform.localPosition = new Vector3(0, 0.05f, KeyboardContext.DEPTHS[(int)KeyboardState.Current]);
        textOutput.text = "";

        // Instantiate a red sphere for gaze debugging
        gazeDebugObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gazeDebugObject.transform.localScale = Vector3.one * 0.02f; // Scale down the sphere
        gazeDebugObject.GetComponent<Renderer>().material.color = Color.red; // Set color to red
        gazeDebugObject.SetActive(false); // Initially hide the debug object

        resetKeyboardAction = InputSystem.actions.FindAction("Reset Keyboard Position");
    }

    void Update()
    {
        if (Keyboard?.Type != keyboardType)
        {
            SetKeyboardType(keyboardType);
        }

        if (Keyboard != null)
        {
            if (resetKeyboardAction.IsPressed())
            {
                Vector3 inFront = mainCamera.transform.position + mainCamera.transform.forward * distanceInFront;
                transform.SetPositionAndRotation(inFront, Quaternion.LookRotation(mainCamera.transform.forward, mainCamera.transform.up));
                Keyboard.NotifyContextPositionsListeners();
            }

            KeyboardContext context = Keyboard.Update(out Vector3 gazeInContext, out _);
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
            }
        }
    }

    public void SetKeyboardType(KeyboardType type)
    {
        if (keyboardType == type)
        {
            return; // No change needed
        }
        keyboardType = type;

        CleanUpContexts();

        switch (type)
        {
            case KeyboardType.Red:
                Keyboard = new RedKeyboard(contextGazeInteraction, InstantiateContext, textChangeListeners, contextChangeListeners, contextPositionListeners);
                break;
            case KeyboardType.Blue:
                Keyboard = new BlueKeyboard(contextGazeInteraction, InstantiateContext, textChangeListeners, contextChangeListeners, contextPositionListeners);
                break;
            case KeyboardType.Green:
                var decoderAPI = new DecoderAPI();
                StartCoroutine(decoderAPI.StartRequestLoop());
                Keyboard = new GreenKeyboard(contextGazeInteraction, InstantiateContext, decoderAPI, textChangeListeners, contextChangeListeners, contextPositionListeners);
                break;
            default:
                Debug.LogError($"Unknown keyboard type: {type}. Using RedKeyboard as default.");
                break;
        }
    }

    KeyboardContext InstantiateContext()
    {
        KeyboardContext context = Instantiate(keyboardContextPrefab).GetComponent<KeyboardContext>();
        context.transform.SetParent(transform);
        return context;
    }

    private void CleanUpContexts()
    {
        if (Keyboard != null)
        {
            foreach (var context in Keyboard.keyboardContexts)
            {
                context.CleanUp();
            }
            Keyboard.keyboardContexts.Clear();
        }
    }

    public void SetReferenceText(string text)
    {
        if (textReference != null)
        {
            textReference.text = text;
        }
    }

    public void OnTextChanged(string newText)
    {
        textOutput.text = newText;
    }
}
