using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ExperimentManager : MonoBehaviour, IContextChangeListener, ITextChangeListener, IGazeDataListener
{
    private enum TrialState
    {
        NotStarted,
        Starting,
        Ongoing,
    }

    Participant participant;
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

    void Start()
    {
        startTrialAction = InputSystem.actions.FindAction("Start Trial");
        stopTrialAction = InputSystem.actions.FindAction("Stop Trial");

        experimentAPI = new ExperimentAPI(BaseUrl);

        StartCoroutine(WaitForExperimentStartCoroutine());
        StartCoroutine(ContinuouslySendEventsCoroutine());
    }

    void Update()
    {
        if (participant != null && sessionId > 0)
        {
            if (startTrialAction.triggered && trialState == TrialState.NotStarted)
            {
                trialState = TrialState.Starting;
                StartCoroutine(StartTrialCoroutine());
            }

            if (stopTrialAction.triggered && trialState == TrialState.Ongoing && GetTimestamp() - trialStartTime > 500f)
            {
                events.Add(EventBuilder.BuildTrialEndEvent(GetTimestamp(), TargetSentence));
                Debug.Log($"Trial {trial} completed. Waiting for next trial.");
                trialState = TrialState.NotStarted; // Reset trial state
            }
        }
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
                        sessionId = int.Parse(response.session);
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
        yield return experimentAPI.StartTrial(participant, sessionId, timestamp, trialData =>
        {
            if (trialData != null && trialData.trial > 0)
            {
                trial = trialData.trial;
                TargetSentence = trialData.sentence;
                trialStartTime = GetTimestamp();
                trialState = TrialState.Ongoing;
                Debug.Log($"Trial {trial} started at {timestamp} with sentence: {TargetSentence}");
            }
            else
            {
                trial = 0;
                trialStartTime = 0f;
                trialState = TrialState.NotStarted;
                Debug.LogWarning("Failed to start trial or no valid trial data received.");
            }
        });
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

                yield return experimentAPI.SendEvents(participant, sessionId, trial, eventArray, (_) =>
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
