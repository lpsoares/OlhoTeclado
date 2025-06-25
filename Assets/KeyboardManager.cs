using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

// Optionally, for XR Interaction Toolkit, use UnityEngine.XR.Interaction.Toolkit;

public class KeyboardManager : MonoBehaviour
{
    public Camera mainCamera; // Assign in Inspector or find in Start
    public float distanceInFront = 2f; // Distance in front of the camera
    private ContextGazeInteraction contextGazeInteraction; 
    public GameObject keyboardContextPrefab;
    public GameObject textOutputPrefab;

    
    public XRNode controllerNode = XRNode.RightHand;
    public InputHelpers.Button xrButton = InputHelpers.Button.PrimaryButton;

    public bool debugGaze = true; 

    public List<KeyboardContext> keyboardContexts;

    private GameObject gazeDebugObject;
    private KeyboardState curState = KeyboardState.Initial;
    private KeyboardContext curContext = null;
    private TextOutput textOutput;

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

        textOutput = Instantiate(textOutputPrefab).GetComponent<TextOutput>();
        textOutput.transform.SetParent(transform);
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
    }

    void Update()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(controllerNode);
        bool buttonPressed = false;
        if (device.isValid)
        {
            device.TryGetFeatureValue(CommonUsages.primaryButton, out buttonPressed);
        }

        if (buttonPressed)
        {
            Vector3 inFront = mainCamera.transform.position + mainCamera.transform.forward * distanceInFront;
            transform.SetPositionAndRotation(inFront, Quaternion.LookRotation(mainCamera.transform.forward, mainCamera.transform.up));
        }

        KeyboardContext context = contextGazeInteraction.GetCurrentContext(keyboardContexts, out Vector3 gazeInContext);
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
                }
            }
            else if (curState != context.State)
            {
                KeyboardContext previousContext = curContext;
                curContext = context;
                KeyboardState previousState = curState;
                curState = context.State;

                int stateDiff = 0;
                if (previousState == KeyboardState.Current && curState == KeyboardState.Next)
                {
                    stateDiff = 1;
                    textOutput.text += previousContext.LastSelectedKey == null ? "" : previousContext.LastSelectedKey.label;
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
}
