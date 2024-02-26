using System;
using System.Text;
using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;

public class PredictionRequest : RunAbleThread
{
    private RequestSocket client;

    private Action<float> onOutputReceived;
    private Action<Exception> onFail;

    private string port;
    public PredictionRequest(string port)
    {
        this.port = port;
    }
    
    protected override void Run()
    {
        ForceDotNet.Force();
        using (RequestSocket client = new RequestSocket())
        {
            this.client = client;
            client.Connect(port);

            while (Running)
            {
                byte[] outputBytes = new byte[0];
                bool gotMessage = false;
                while (Running)
                {
                    try
                    {
                        gotMessage = client.TryReceiveFrameBytes(out outputBytes);
                        if (gotMessage) break;
                    }
                    catch (Exception e)
                    {
                    }
                }

                if (gotMessage)
                {
                    float prediction = BitConverter.ToSingle(outputBytes, 0);
                    Console.WriteLine("IS THIS REAL: " + prediction);
                    onOutputReceived?.Invoke(prediction);
                }
            }
        }

        NetMQConfig.Cleanup();
    }

    public void SendInput(string input)
    {
        try
        {
            client.SendFrame(input);
        }
        catch (Exception e)
        {
            onFail(e);
        }
    }

    public void SetOnTextReceivedListener(Action<float> onOutputReceived, Action<Exception> fallback)
    {
        this.onOutputReceived = onOutputReceived;
        onFail = fallback;
    }
}
