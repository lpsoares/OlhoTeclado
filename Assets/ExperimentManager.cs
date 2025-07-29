using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ExperimentManager : MonoBehaviour, IContextChangeListener, ITextChangeListener, IGazeDataListener, IContextPositionsListener, ICandidateListListener, IKeyPressListener
{
    private enum TrialState
    {
        NotStarted,
        Starting,
        Ongoing,
    }

    Participant participant;
    string method;
    int sessionId = 0;
    int trial = 0;
    TrialState trialState = TrialState.NotStarted;
    float trialStartTime = 0f;
    public string TargetSentence = "";
    public string BaseUrl = "http://localhost:3000/api";
    private ExperimentAPI experimentAPI;

    private readonly List<string> events = new();

    InputAction startTrialAction;
    InputAction stopTrialAction;
    private string currentText;
    private string curContextName = "None";
    private KeyboardManager keyboardManager;

    void Start()
    {
        startTrialAction = InputSystem.actions.FindAction("Start Trial");
        stopTrialAction = InputSystem.actions.FindAction("Stop Trial");

        keyboardManager = FindObjectOfType<KeyboardManager>();
        if (keyboardManager == null)
        {
            Debug.LogError("KeyboardManager not found in the scene. Please ensure it is present.");
            return;
        }

        experimentAPI = new ExperimentAPI(BaseUrl);

        StartCoroutine(WaitForExperimentStartCoroutine());
        StartCoroutine(ContinuouslySendEventsCoroutine());
    }

    void Update()
    {
        if (participant != null && method != null && sessionId > 0)
        {
            if (startTrialAction.triggered && trialState == TrialState.NotStarted)
            {
                trialState = TrialState.Starting;
                StartCoroutine(StartTrialCoroutine());
            }

            if (stopTrialAction.triggered && trialState == TrialState.Ongoing && GetTimestamp() - trialStartTime > 500f)
            {
                string outputText = keyboardManager.Keyboard.Text;
                events.Add(EventBuilder.BuildTrialEndEvent(GetTimestamp(), outputText));
                Debug.Log($"Trial {trial} completed with output: \"{outputText}\". Waiting for next trial.");
                trialState = TrialState.NotStarted; // Reset trial state
                keyboardManager.SetReferenceText("");
                keyboardManager.Keyboard.ResetText();
            }
        }
    }

    public void OnKeyPress(Key key)
    {
        if (trialState != TrialState.Ongoing)
        {
            return;
        }

        string eventData = EventBuilder.BuildKeyPressEvent(GetTimestamp(), key.name, key.label, key.label.ToLower());
        events.Add(eventData);
    }

    public void OnContextPositionsChanged(ContextPositionData[] contextPositionData)
    {
        if (trialState != TrialState.Ongoing)
        {
            return;
        }

        events.Add(EventBuilder.BuildContextPositionsEvent(GetTimestamp(), contextPositionData));
    }

    public void OnContextChanged(KeyboardContext newContext)
    {
        string newContextName = newContext != null ? newContext.State.ToString() : "None";
        if (trialState == TrialState.Ongoing)
        {
            string eventData = EventBuilder.BuildContextChangeEvent(GetTimestamp(), curContextName, newContextName);
            events.Add(eventData);
            Debug.Log($"Context changed from {curContextName} to {newContextName}");
        }
        curContextName = newContextName;
    }

    public void OnTextChanged(string newText)
    {
        if (trialState != TrialState.Ongoing || currentText == newText)
        {
            return;
        }

        string eventData = EventBuilder.BuildTextChangeEvent(GetTimestamp(), newText);
        events.Add(eventData);
        Debug.Log($"Text changed: {newText}");
        currentText = newText; // Update the current text to avoid duplicate events
    }

    public void OnCandidateListChanged(string[] candidates)
    {
        if (trialState != TrialState.Ongoing)
        {
            return;
        }
        
        string eventData = EventBuilder.BuildCandidateListEvent(GetTimestamp(), candidates);
        events.Add(eventData);
    }

    public void OnGaze(Vector2 gaze2D, Vector3 gaze3D, Vector3 leftEyePosition, Vector3 rightEyePosition, Vector3 leftEyeDirection, Vector3 rightEyeDirection)
    {
        if (trialState != TrialState.Ongoing)
        {
            return;
        }

        string eventData = EventBuilder.BuildGazeEvent(GetTimestamp(), gaze2D, gaze3D, leftEyePosition, rightEyePosition, leftEyeDirection, rightEyeDirection);
        events.Add(eventData);
    }

    private IEnumerator WaitForExperimentStartCoroutine()
    {
        bool running = false;
        while (participant == null)
        {
            if (running)
            {
                yield return new WaitForSeconds(5f);
            }
            else
            {
                running = true;
                yield return experimentAPI.GetCurrentParticipant(response =>
                {
                    if (response != null && response.participant?.name != null && response.participant.name.Length > 0 && response.session != null)
                    {
                        Debug.Log($"Current Participant: {response.participant.name}, Session: {response.session}");
                        participant = response.participant;
                        method = response.method;
                        sessionId = int.Parse(response.session);

                        switch (method)
                        {
                            case "red":
                                keyboardManager.SetKeyboardType(KeyboardType.Red);
                                break;
                            case "blue":
                                keyboardManager.SetKeyboardType(KeyboardType.Blue);
                                break;
                            case "green":
                                keyboardManager.SetKeyboardType(KeyboardType.Green);
                                break;
                            default:
                                Debug.LogError($"Unknown method: {method}. Defaulting to Red Keyboard.");
                                keyboardManager.SetKeyboardType(KeyboardType.Red);
                                break;
                        }
                    }
                    else
                    {
                        Debug.Log("No current participant found or error in fetching data.");
                    }
                    running = false;
                });
            }
        }
    }

    private IEnumerator StartTrialCoroutine()
    {
        float timestamp = GetTimestamp();
        yield return experimentAPI.StartTrial(participant, method, sessionId, timestamp, trialData =>
        {
            if (trialData != null && trialData.trial > 0)
            {
                trial = trialData.trial;
                TargetSentence = trialData.sentence.ToLower();
                trialStartTime = GetTimestamp();
                trialState = TrialState.Ongoing;
                keyboardManager.SetReferenceText(TargetSentence);
                keyboardManager.Keyboard.ResetText();
                events.Add(EventBuilder.BuildContextPositionsEvent(trialStartTime, keyboardManager.Keyboard.GetContextPositions()));
                events.Add(EventBuilder.BuildKeyPositionEvent(trialStartTime, GetKeyPositions()));
                Debug.Log($"Trial {trial} started at {timestamp} with sentence: {TargetSentence}");
            }
            else
            {
                trial = 0;
                trialStartTime = 0f;
                trialState = TrialState.NotStarted;
                keyboardManager.SetReferenceText("");
                Debug.LogWarning("Failed to start trial or no valid trial data received.");
            }
        });
    }

    private KeyPositionData[] GetKeyPositions()
    {
        List<KeyPositionData> keyPositions = new();
        foreach (var context in keyboardManager.Keyboard.keyboardContexts)
        {
            if (context == null) continue;

            foreach (var key in context.Keys)
            {
                if (key == null || key.transform == null)
                    continue;

                keyPositions.Add(new KeyPositionData(key.name, key.label, context.State.ToString(), key.Width, key.Height, new Vector2(key.X, key.Y)));
            }
        }

        keyPositions.Add(new KeyPositionData("textField", "---", "Current", 1.0f, 1.0f, keyboardManager.textOutput.transform.localPosition));
        keyPositions.Add(new KeyPositionData("textReference", "---", "Reference", 1.0f, 1.0f, keyboardManager.textReference.transform.localPosition));

        return keyPositions.ToArray();
    }

    private IEnumerator ContinuouslySendEventsCoroutine()
    {
        bool sending = false;
        while (true)
        {
            if (!sending && events.Count > 0)
            {
                sending = true;
                string[] eventArray = events.ToArray();
                events.Clear();

                yield return experimentAPI.SendEvents(participant, method, sessionId, trial, eventArray, (_) =>
                {
                    sending = false;
                });
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private float GetTimestamp()
    {
        return Time.time * 1000;
    }
}
