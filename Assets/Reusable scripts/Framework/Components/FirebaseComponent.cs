using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;

[Serializable]
public readonly struct FirebaseResult
{
    public readonly bool Success;
    public readonly string Data;
    public readonly string Error;

    public FirebaseResult(bool success, string data = null, string error = null)
    {
        Success = success;
        Data = data;
        Error = error;
    }
}

public class FirebaseComponent : MonoBehaviour
{
    [SerializeField] private string firebaseToken = "";
    private CancellationTokenSource cancellationTokenSource;

    void Awake()
    {
        transform.SetParent(null);
        DontDestroyOnLoad(this);
        cancellationTokenSource = new CancellationTokenSource();
    }

    protected virtual void OnDestroy()
    {
        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();
    }

    protected async Task<bool> WaitForUnityWebRequest(UnityWebRequestAsyncOperation operation, UnityWebRequest request)
    {
        while (!operation.isDone)
        {
            if (cancellationTokenSource.IsCancellationRequested)
            {
                request.Abort();
                return false;
            }

            await Task.Yield();
        }

        return true;
    }

    public async Task<FirebaseResult> GetData(string firebaseResourceURL)
    {
        using UnityWebRequest request = UnityWebRequest.Get(firebaseResourceURL);
        UnityWebRequestAsyncOperation operation = request.SendWebRequest();

        bool completed = await WaitForUnityWebRequest(operation, request);
        if (!completed)
        {
            return new FirebaseResult(false, error: "Request cancelled.");
        }

        if (request.result != UnityWebRequest.Result.Success)
            {
                return new FirebaseResult(false, error: request.error);
            }

        return new FirebaseResult(true, data: request.downloadHandler.text);
    }

    public async Task<FirebaseResult> PatchData(string firebaseResourceURL, Dictionary<string, int> payload)
    {
        string json = JsonConvert.SerializeObject(payload);

        using UnityWebRequest request = new UnityWebRequest(firebaseResourceURL, "PATCH");

        // Convert JSON to Byte Array (UploadHandlerRaw requires byte[])
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        if (!string.IsNullOrEmpty(firebaseToken))
        {
            // Attach the API key
            request.SetRequestHeader("X-API-KEY", firebaseToken);
        }

        UnityWebRequestAsyncOperation operation = request.SendWebRequest();

        bool completed = await WaitForUnityWebRequest(operation, request);
        if (!completed)
        {
            return new FirebaseResult(false, error: "Request cancelled.");
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            return new FirebaseResult(false, error: request.error);
        }

        return new FirebaseResult(true, data: request.downloadHandler.text);
    }
}
