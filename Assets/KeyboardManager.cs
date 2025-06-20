using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

// Optionally, for XR Interaction Toolkit, use UnityEngine.XR.Interaction.Toolkit;

public class KeyboardMain : MonoBehaviour
{
    public Camera mainCamera; // Assign in Inspector or find in Start
    public float distanceInFront = 2f; // Distance in front of the camera
    private ContextGazeInteraction contextGazeInteraction; 
    public GameObject keyboardContextPrefab;

    // XR input variables
    public XRNode controllerNode = XRNode.RightHand; // Change to LeftHand if needed
    public InputHelpers.Button xrButton = InputHelpers.Button.PrimaryButton; // A/X button
    public List<KeyboardContext> keyboardContexts;
    private GameObject gazeDebugObject;
    private KeyboardState state = KeyboardState.Initial;
    

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

        keyboardContexts = new List<KeyboardContext>
        {
            Instantiate(keyboardContextPrefab).GetComponent<KeyboardContext>(),
            Instantiate(keyboardContextPrefab).GetComponent<KeyboardContext>(),
            Instantiate(keyboardContextPrefab).GetComponent<KeyboardContext>(),
            Instantiate(keyboardContextPrefab).GetComponent<KeyboardContext>(),
            Instantiate(keyboardContextPrefab).GetComponent<KeyboardContext>(),
        };
        KeyboardState[] states = { KeyboardState.InactiveNext, KeyboardState.Next, KeyboardState.Current, KeyboardState.Previous, KeyboardState.InactivePrevious };
        for (int i = 0; i < keyboardContexts.Count; i++)
        {
            keyboardContexts[i].transform.SetParent(transform);
            keyboardContexts[i].State = states[i];
            keyboardContexts[i].Depth = keyboardContexts[i].TargetDepth;
        }

        // Instantiate a red sphere for gaze debugging
        gazeDebugObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gazeDebugObject.transform.localScale = Vector3.one * 0.1f; // Scale down the sphere
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
            gazeDebugObject.transform.position = gazeInContext;
            gazeDebugObject.SetActive(true);

            if (state == KeyboardState.Initial)
            {
                if (context.State == KeyboardState.Current)
                {
                    state = KeyboardState.Current;
                }
            }
            else if (state != context.State)
            {
                KeyboardState previousState = state;
                state = context.State;
                int stateDiff = 0;
                if (previousState == KeyboardState.Current && state == KeyboardState.Next)
                {
                    stateDiff = 1;
                }
                else if (previousState == KeyboardState.Current && state == KeyboardState.Previous)
                {
                    stateDiff = -1;
                }
                foreach (var ctx in keyboardContexts)
                {
                    ctx.State = (KeyboardState)((int)(ctx.State + stateDiff + 5) % 5);
                }
            }
        }
    }
}
