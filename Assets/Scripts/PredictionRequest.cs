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
        ForceDotNet.Force(); // this line is needed to prevent unity freeze after one use, not sure why yet
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
                        gotMessage = client.TryReceiveFrameBytes(out outputBytes); // this returns true if it's successful
                        if (gotMessage) break;
                    }
                    catch (Exception e)
                    {
                    }
                }

                if (gotMessage)
                {
                    float prediction = BitConverter.ToSingle(outputBytes, 0);
                    // var output = new float[outputBytes.Length / 4];
                    // Buffer.BlockCopy(outputBytes, 0, output, 0, outputBytes.Length);
                    // onOutputReceived?.Invoke(output);
                    onOutputReceived?.Invoke(prediction);
                }
            }
        }

        NetMQConfig.Cleanup(); // this line is needed to prevent unity freeze after one use, not sure why yet
    }

    public void SendInput(string input)
    {
        try
        {
            // Debug.Log("YAYYY!");
            // var byteArray = new byte[input.Length * 4];
            // Buffer.BlockCopy(input, 0, byteArray, 0, byteArray.Length);
            // Debug.Log("Size of sent array: " + byteArray.Length);
            client.SendFrame(input);
            
            
            // byte[] messageBytes = Encoding.UTF8.GetBytes("GRRRRRRRRR");
            // client.SendFrame(messageBytes);
        }
        catch (Exception e)
        {
            Debug.Log("Exception error in PredictionRequest.cs");

            onFail(e);
        }
    }

    public void SetOnTextReceivedListener(Action<float> onOutputReceived, Action<Exception> fallback)
    {
        this.onOutputReceived = onOutputReceived;
        onFail = fallback;
    }
}
