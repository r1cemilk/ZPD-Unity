using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
// using Unity.MLAgents; 
// using NatSuite.ML;
// using NatSuite.ML.Onnx;
using Unity.Barracuda;
using CarControls;
using TMPro;

public class PredictSteering : MonoBehaviour
{
    [SerializeField] private NNModel kerasModel;
    [SerializeField] private RenderTexture renderTexture1;
    [SerializeField] private SignLogic signLogic;

    private Model runtimeModel;
    private IWorker worker;
    private string outputLayerName;

    private bool shouldPredict = false;

    public PredictionClient client1;
    public PredictionClient client2;

    public float prediction;
    private string newPred;

    private string sign;
    private int signIndex;
    public string[] ActiveSigns;

    private CarController carController;


    void Start()
    {
        runtimeModel = ModelLoader.Load(kerasModel);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, runtimeModel);
        outputLayerName = runtimeModel.outputs[runtimeModel.outputs.Count - 1];

        carController = GetComponent<CarController>();
    }

    private void PredictTrafficSign()
    {
        Texture2D screenshot = new Texture2D(renderTexture1.width, renderTexture1.height, TextureFormat.RGB24, false);
        RenderTexture.active = renderTexture1;
        screenshot.ReadPixels(new Rect(0, 0, renderTexture1.width, renderTexture1.height), 0, 0);
        RenderTexture.active = null;

        string base64Image = ConvertTextureToBase64(screenshot);
        
        client2.Predict(base64Image, output =>
        {
            signIndex = (int)output;
            print("OUTPUT: " + output);
            signLogic.UpdatePendingSigns(signIndex);
            signLogic.UpdateSigns();
        }, error =>
        {
            print("ERROR: " + error);
        });

    }

    private void PredictSteeringAngle()
    {
        Texture2D screenshot = new Texture2D(renderTexture1.width, renderTexture1.height, TextureFormat.RGB24, false);
        RenderTexture.active = renderTexture1;
        screenshot.ReadPixels(new Rect(0, 0, renderTexture1.width, renderTexture1.height), 0, 0);
        RenderTexture.active = null;

        string base64Image = ConvertTextureToBase64(screenshot);
        
        client1.Predict(base64Image, output => {
            prediction = output;
        }, error =>
        {
            print("ERROR: " + error);
        });
        
        carController.SetSteering(prediction);
    }
    IEnumerator PredictSteeringAngleCoroutine()
    {
        while (true)
        {
            PredictSteeringAngle();
            yield return new WaitForSeconds(0.2f);
        }
    }
    IEnumerator PredictTrafficSignCoroutine()
    {
        while (true)
        {
            PredictTrafficSign();
            yield return new WaitForSeconds(0.8f);
        }
    }
    void Update()
    {   
        carController.GetShouldPredict(shouldPredict);
        if (Input.GetKeyDown(KeyCode.B))
        {
            shouldPredict = !shouldPredict;
            if (shouldPredict)
            {
                StartCoroutine(PredictSteeringAngleCoroutine());
                StartCoroutine(PredictTrafficSignCoroutine());;
                carController.SetSpeed(8.0f);
            }
            else
            {
                carController.SetSpeed(0);
                carController.SetSteering(0);
            }
        }
    }
    
    private string ConvertTextureToBase64(Texture2D texture)
    {
        byte[] imageBytes = texture.EncodeToPNG();
        string base64Image = Convert.ToBase64String(imageBytes);
        return base64Image;
    }

    public void OnDestory()
    {
        worker?.Dispose();
    }
}
