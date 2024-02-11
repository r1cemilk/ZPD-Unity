using System;
using UnityEngine;

public class PredictionClient : MonoBehaviour
{
    private PredictionRequest predictionRequester;

    private void Start() => InitializeServer();

    public void InitializeServer()
    {
        predictionRequester = new PredictionRequest();
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