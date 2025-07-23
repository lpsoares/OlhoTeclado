using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SingleControllerEyeTracker : EyeTracker
{
    public Transform controllerTransform; // Assign in Inspector

    private GameObject leftEye;
    private GameObject rightEye;
    private KeyboardState currentContextState = KeyboardState.Current;
    private InputAction moveGazeDepthAction;
    private KeyboardManager keyboardManager;

    private void Start()
    {
        keyboardManager = FindObjectOfType<KeyboardManager>();
        if (keyboardManager == null)
        {
            Debug.LogError("KeyboardManager not found in the scene. Please ensure it is present.");
            return;
        }

        leftEye = new GameObject("LeftEye");
        rightEye = new GameObject("RightEye");

        leftEye.transform.SetParent(Camera.main.transform);
        rightEye.transform.SetParent(Camera.main.transform);

        leftEye.transform.localPosition = new Vector3(-0.05f, 0, 0);
        rightEye.transform.localPosition = new Vector3(0.05f, 0, 0);

        moveGazeDepthAction = InputSystem.actions.FindAction("Move gaze depth");

        if (moveGazeDepthAction == null)
        {
            Debug.LogError("Input action for gaze depth movement not found. Please ensure it is defined in the Input System.");
            return;
        }
    }

    private void Update()
    {
        Vector2 gazeDepth = moveGazeDepthAction.ReadValue<Vector2>();
        if (gazeDepth.y > 0.5f)
        {
            currentContextState = KeyboardState.Next;
        }
        else if (gazeDepth.y < -0.5f)
        {
            currentContextState = KeyboardState.Previous;
        }
        else
        {
            currentContextState = KeyboardState.Current;
        }
    }

    public override Transform LeftEye => leftEye.transform;
    public override Transform RightEye => rightEye.transform;

    /// <summary>
    /// Estimates the current gaze point based on the controller position and the current plane.
    /// </summary>
    /// <returns>The current GazePoint</returns>
    public override GazePoint CurrentGazePoint()
    {
        if (keyboardManager.Keyboard.keyboardContexts == null || keyboardManager.Keyboard.keyboardContexts.Count == 0)
        {
            Debug.LogWarning("No keyboard contexts available. Returning default gaze point.");
            return GazePoint.Empty;
        }
        
        KeyboardContext currentContext = keyboardManager.Keyboard.keyboardContexts.Find(ctx => ctx.State == currentContextState);
        if (currentContext == null)
        {
            currentContext = keyboardManager.Keyboard.keyboardContexts.Find(ctx => ctx.State == KeyboardState.Current);
            if (currentContext == null)
            {
                Debug.LogWarning("No current context found. Using the first context as fallback.");
                currentContext = keyboardManager.Keyboard.keyboardContexts[0];
            }
        }

        Ray controllerRay = new(controllerTransform.position, controllerTransform.forward);
        bool intersectedPlane = currentContext.Plane.Raycast(controllerRay, out float enter);
        if (!intersectedPlane)
        {
            Debug.LogWarning("Controller gaze ray did not intersect with the keyboard plane.");
            return GazePoint.Empty;
        }

        Vector3 gazePosition = controllerRay.GetPoint(enter);
        Vector3 cyclopsEyePosition = (leftEye.transform.position + rightEye.transform.position) / 2.0f;
        float gazeDepth = (gazePosition - cyclopsEyePosition).magnitude;
        Ray cyclopsRay = new(cyclopsEyePosition, gazePosition - cyclopsEyePosition);
        return new GazePoint(gazePosition, cyclopsRay, gazeDepth, 0);
    }
}
