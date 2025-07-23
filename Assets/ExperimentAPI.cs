using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class Participant
{
    public string id;
    public string name;
    public string age;
    public string sex;
}

[Serializable]
public class CurrentParticipantData
{
    public Participant participant;
    public string method;
    public string session;
}

[Serializable]
public class TrialData
{
    public int trial;
    public string sentence;
}

[Serializable]
public class VoidResponse
{
    public bool success;
}

public class KeyPositionData
{
    public string keyName;
    public string keyLabel;
    public string context;
    public float width;
    public float height;
    public Vector2 key2DPosition;

    public KeyPositionData(string keyName, string keyLabel, string context, float width, float height, Vector2 key2DPosition)
    {
        this.keyName = keyName;
        this.keyLabel = keyLabel;
        this.context = context;
        this.width = width;
        this.height = height;
        this.key2DPosition = key2DPosition;
    }

    internal object ToCSV(string delimiter = ",")
    {
        return $"{keyName}{delimiter}{keyLabel}{delimiter}{context}{delimiter}{width}{delimiter}{height}{delimiter}{key2DPosition.x}{delimiter}{key2DPosition.y}";
    }
}

public static class CsvExtensions
{
    public static string ToCSV(this ContextPositionData data, string delimiter = ",")
    {
        return $"{data.contextName}{delimiter}{data.origin.x}{delimiter}{data.origin.y}{delimiter}{data.origin.z}{delimiter}{data.upVector.x}{delimiter}{data.upVector.y}{delimiter}{data.upVector.z}{delimiter}{data.rightVector.x}{delimiter}{data.rightVector.y}{delimiter}{data.rightVector.z}{delimiter}{data.forwardVector.x}{delimiter}{data.forwardVector.y}{delimiter}{data.forwardVector.z}";
    }
}

public class ExperimentAPI
{
    private readonly string baseUrl = "http://localhost:3000/api";

    public ExperimentAPI(string baseUrl)
    {
        this.baseUrl = baseUrl;
    }

    public IEnumerator GetCurrentParticipant(Action<CurrentParticipantData> onComplete)
    {
        string uri = $"{baseUrl}/participants/current";
        UnityWebRequest request = UnityWebRequest.Get(uri);
        request.SetRequestHeader("Content-Type", "application/json");
        yield return RequestAndWait(request, onComplete);
    }

    public IEnumerator StartTrial(Participant participant, string method, int session, float timestamp, Action<TrialData> onComplete = null)
    {
        string uri = $"{baseUrl}/participants/{participant.id}/{method}/sessions/{session}/start";
        string jsonData = $"{{\"timestamp\":\"{timestamp}\"}}";
        UnityWebRequest request = UnityWebRequest.Post(uri, jsonData, "application/json");
        yield return RequestAndWait(request, onComplete);
    }

    public IEnumerator SendEvents(Participant participant, string method, int session, int trial, string[] events, Action<VoidResponse> onComplete = null)
    {
        string uri = $"{baseUrl}/participants/{participant.id}/{method}/sessions/{session}/{trial}";
        string eventData = string.Join("\n", events);
        UnityWebRequest request = UnityWebRequest.Post(uri, eventData, "text/plain");
        yield return RequestAndWait(request, onComplete);
    }

    private IEnumerator RequestAndWait<T>(UnityWebRequest request, Action<T> onComplete = null) where T : class
    {
        var operation = request.SendWebRequest();
        while (!operation.isDone)
        {
            yield return null;
        }
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log($"Error in request: {request.error}");
            yield return null;
        }
        else
        {
            try
            {
                string jsonResponse = request.downloadHandler.text;
                T responseData = JsonUtility.FromJson<T>(jsonResponse);
                onComplete?.Invoke(responseData);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing JSON response: {e.Message}");
            }
        }
    }
}

public class EventBuilder
{
    const string TrialStart = "TRIAL_START";
    const string TrialEnd = "TRIAL_END";
    const string KeyPress = "KEY_PRESS";
    const string KeyPosition = "KEY_POS";
    const string ContextPosition = "CTX_POS";
    const string ContextChange = "CONTEXT_CHANGE";
    // TODO: IMPLEMENT THIS EVENT
    const string CandidateList = "CANDIDATES";
    const string TextChange = "TEXT_CHANGE";
    const string GazeData = "GAZE";

    public static string BuildTrialEndEvent(float timestamp, string sentence)
    {
        return BuildEvent(timestamp, TrialEnd, sentence);
    }

    public static string BuildKeyPositionEvent(float timestamp, KeyPositionData[] keyPositions)
    {
        string data = string.Join(";", Array.ConvertAll(keyPositions, kp => kp.ToCSV(";")));
        return BuildEvent(timestamp, KeyPosition, data);
    }

    public static string BuildContextPositionsEvent(float timestamp, ContextPositionData[] contextPositions)
    {
        string data = string.Join(";", Array.ConvertAll(contextPositions, cp => cp.ToCSV(";")));
        return BuildEvent(timestamp, ContextPosition, data);
    }

    public static string BuildContextChangeEvent(float timestamp, string prevContext, string newContext)
    {
        return BuildEvent(timestamp, ContextChange, $"{prevContext};{newContext}");
    }

    public static string BuildTextChangeEvent(float timestamp, string text)
    {
        return BuildEvent(timestamp, TextChange, text);
    }

    public static string BuildGazeEvent(float timestamp, Vector2 gaze2D, Vector3 gaze3D, Vector3 leftEyePosition, Vector3 rightEyePosition, Vector3 leftEyeDirection, Vector3 rightEyeDirection)
    {
        string data = $"{gaze2D.x};{gaze2D.y};{gaze3D.x};{gaze3D.y};{gaze3D.z};{leftEyePosition.x};{leftEyePosition.y};{leftEyePosition.z};{rightEyePosition.x};{rightEyePosition.y};{rightEyePosition.z};{leftEyeDirection.x};{leftEyeDirection.y};{leftEyeDirection.z};{rightEyeDirection.x};{rightEyeDirection.y};{rightEyeDirection.z}";
        return BuildEvent(timestamp, GazeData, data);
    }

    private static string BuildEvent(float timestamp, string type, string data)
    {
        return $"{timestamp},{type},{data}";
    }
}
