using System;
using UnityEngine;

public class PredictionClient : MonoBehaviour
{
    private PredictionRequest predictionRequester;
    public string port;

    private void Start() => InitializeServer();

    public void InitializeServer()
    {
        predictionRequester = new PredictionRequest(port);
        predictionRequester.Start();
    }

    public void Predict(string input, Action<float> onOutputReceived, Action<Exception> fallback)
    {
        predictionRequester.SetOnTextReceivedListener(onOutputReceived, fallback);
        predictionRequester.SendInput(input);
    }

    private void OnDestroy()
    {
        predictionRequester.Stop();
    }
}