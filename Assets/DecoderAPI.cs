using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DecoderAPI
{
    private readonly string baseUrl = "http://localhost:8000";
    private readonly PeekEndQueue<DecoderRequest> requestQueue = new();
    private string decoderId = string.Empty;
    public Dictionary<string, List<float>> Keys;
    private bool isReady = false;

    public DecoderAPI(string baseUrl = "http://localhost:8000")
    {
        this.baseUrl = baseUrl;
    }

    public IEnumerator StartRequestLoop()
    {
        while (Keys == null)
        {
            yield return new WaitForSeconds(0.2f);
        }

        string body = "{";
        foreach (var key in Keys)
        {
            body += $"\"{key.Key}\": [{string.Join(", ", key.Value)}],";
        }
        body = body.TrimEnd(',') + "}";
        Debug.Log($"Initializing decoder with body: {body}");

        while (decoderId.Length == 0)
        {
            UnityWebRequest startRequest = UnityWebRequest.Post($"{baseUrl}/keyboard", body, "application/json");
            var operation = startRequest.SendWebRequest();
            while (!operation.isDone)
            {
                yield return null;
            }
            if (startRequest.result == UnityWebRequest.Result.ConnectionError || startRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log($"Error in request: {startRequest.error}");
                yield return null;
            }
            else
            {
                try
                {
                    string jsonResponse = startRequest.downloadHandler.text;
                    KeyboardResponse responseData = JsonUtility.FromJson<KeyboardResponse>(jsonResponse);
                    decoderId = responseData.decoder_id;
                    isReady = responseData.status == "ready";
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing JSON response: {e.Message}");
                }
            }
        }

        while (!isReady)
        {
            yield return new WaitForSeconds(1f);
            UnityWebRequest statusRequest = UnityWebRequest.Get($"{baseUrl}/keyboard/status");
            statusRequest.SetRequestHeader("Content-Type", "application/json");
            var statusOperation = statusRequest.SendWebRequest();
            while (!statusOperation.isDone)
            {
                yield return null;
            }
            if (statusRequest.result == UnityWebRequest.Result.ConnectionError || statusRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log($"Error in request: {statusRequest.error}");
                yield return null;
            }
            else
            {
                try
                {
                    string jsonResponse = statusRequest.downloadHandler.text;
                    KeyboardResponse responseData = JsonUtility.FromJson<KeyboardResponse>(jsonResponse);
                    isReady = responseData.status == "ready";
                    Debug.Log($"Decoder status: {responseData.status}, ID: {responseData.decoder_id}, Original data: {jsonResponse}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing JSON response: {e.Message}");
                }
            }
        }

        Debug.Log("Decoder is ready.");

        while (true)
        {
            if (requestQueue.Count > 0)
            {
                var request = requestQueue.Dequeue();
                UnityWebRequest webRequest = request.BuildRequest(baseUrl);
                var operation = webRequest.SendWebRequest();
                while (!operation.isDone)
                {
                    yield return null;
                }
                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log($"Error in request: {webRequest.error}");
                    yield return null;
                }
                else
                {
                    request.callback?.Invoke(webRequest.downloadHandler.text);
                }
            }
            yield return null;  // Wait for the next frame
        }
    }

    public void SetContext(string context)
    {
        string body = $"{{\"context\": \"{context}\"}}";
        Debug.Log($"Setting context: {body}");
        var request = new DecoderRequest
        {
            endpoint = "/context",
            method = "POST",
            data = body,
        };
        requestQueue.Enqueue(request);
    }

    public void AddGesturePoint(float timestamp, float x, float y)
    {
        // If last request is not an AddGesturePointsRequest, create a new one
        if (requestQueue.Count == 0 || requestQueue.PeekEnd() is not AddGesturePointsRequest)
        {
            requestQueue.Enqueue(new AddGesturePointsRequest());
        }
        var addGesturePointsRequest = requestQueue.PeekEnd() as AddGesturePointsRequest;
        addGesturePointsRequest?.AddPoint(timestamp, x, y);
    }

    public void DecodeGesture(Action<List<string>> onDecoded)
    {
        requestQueue.Enqueue(new DecoderRequest
        {
            endpoint = "/decode",
            method = "POST",
            callback = (response) =>
            {
                DecodeGestureResponse decoded = JsonUtility.FromJson<DecodeGestureResponse>(response);
                onDecoded?.Invoke(decoded.decoded_words);
            },
        });
    }

    public void ResetPoints()
    {
        requestQueue.Enqueue(new DecoderRequest
        {
            endpoint = "/points/reset",
            method = "POST",
        });
    }
}

public class DecoderRequest
{
    public string endpoint;
    public string method;  // GET or POST
    public string data;  // JSON data for POST requests
    public Action<string> callback;  // Callback to handle the response
    public Type T;

    public UnityWebRequest BuildRequest(string baseUrl)
    {
        var url = $"{baseUrl}{endpoint}";
        if (method == "GET")
        {
            var request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Content-Type", "application/json");
            return request;
        }
        else if (method == "POST")
        {
            return UnityWebRequest.Post(url, GetData(), "application/json");
        }
        else
        {
            throw new InvalidOperationException($"Unsupported method: {method}");
        }
    }

    protected virtual string GetData()
    {
        return data ?? string.Empty;
    }
}

public class AddGesturePointsRequest : DecoderRequest
{
    private readonly List<string> points = new();

    public AddGesturePointsRequest()
    {
        endpoint = "/points/post";
        method = "POST";
    }

    public void AddPoint(float timestamp, float x, float y)
    {
        points.Add($"[{timestamp}, {x}, {y}]");
    }

    protected override string GetData()
    {
        return $"{{ \"points\": [ {string.Join(", ", points)} ] }}";
    }
}

public class PeekEndQueue<T> : Queue<T>
{
    private T lastItem;

    public new void Enqueue(T item)
    {
        base.Enqueue(item);
        lastItem = item;
    }

    public T PeekEnd()
    {
        if (Count == 0)
            lastItem = default(T);
        return lastItem;
    }
}

[Serializable]
public class KeyboardResponse
{
    public string decoder_id;
    public string status;
}

[Serializable]
public class DecodeGestureResponse
{
    public List<string> decoded_words;
}