using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private TextMeshProUGUI inputValue;

    [SerializeField] private Camera camera1;
    [SerializeField] private Camera camera2;
    [SerializeField] private Camera camera3;
    [SerializeField] private RenderTexture renderTexture1;
    [SerializeField] private RenderTexture renderTexture2;
    [SerializeField] private RenderTexture renderTexture3;
    [SerializeField] private RenderTexture renderTexture4;

    private Model runtimeModel;
    private IWorker worker;
    private string outputLayerName;

    private bool shouldPredict = false;

    private CarController carController;
    // private string stuff2;
    // private OnnxModel model;
    // // private Tensor inputTensor;


    void Start()
    {
        runtimeModel = ModelLoader.Load(kerasModel);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, runtimeModel);
        outputLayerName = runtimeModel.outputs[runtimeModel.outputs.Count - 1];

        carController = GetComponent<CarController>();
    }

    private void HandlePrediction()
    {
        RenderTexture.active = renderTexture1;

        Texture2D carCameraTexture = new Texture2D(camera1.targetTexture.width, camera1.targetTexture.height);
        carCameraTexture.ReadPixels(new Rect(0, 0, camera1.targetTexture.width, camera1.targetTexture.height), 0, 0);
        carCameraTexture.Apply();

        RenderTexture.active = renderTexture1;

        // Resize the image to (66x200)
        Texture2D resizedTexture = ResizeTexture(carCameraTexture, 200, 66, 125, 200);
        
        Texture2D processedTexture = PreprocessImage(resizedTexture);

        Color[] pixels = processedTexture.GetPixels();
        float[] inputArray = new float[pixels.Length * 3];

        for (int i = 0; i < pixels.Length; i++)
        {
            inputArray[i * 3] = pixels[i].r;
            inputArray[i * 3 + 1] = pixels[i].g;
            inputArray[i * 3 + 2] = pixels[i].b;
        }
        

        TensorShape tensorShape = new TensorShape(1, processedTexture.width, processedTexture.height, 3);

        // print(tensorShape);

        using Tensor inputTensor = new Tensor(tensorShape, inputArray);

        // inputTensor[0] = resizedTexture;
        worker.Execute(inputTensor);

        Tensor outputTensor = worker.PeekOutput();
        float[] outputData = outputTensor.ToReadOnlyArray();

        print(outputTensor[0]*100f);

        float steering_angle = outputTensor[0]*100f;
        double throttle = 1.0 - carController.GetSpeed()/10f * 1000f;
        carController.SetSpeed((float)throttle);
        carController.SetSteering(steering_angle);

        // outputPrediction.text = outputTensor[0].ToString();

        inputTensor.Dispose();
        outputTensor.Dispose();
    }

    void Update()
    {   
        if (Input.GetKeyDown(KeyCode.B))
        {
            shouldPredict = !shouldPredict;
            if (shouldPredict)
            {
                InvokeRepeating("HandlePrediction", 1f, 0.5f);
            }
            else
            {
                CancelInvoke("HandlePrediction");
                carController.SetSpeed(0);
                carController.SetSteering(0);
            }
        }
    }

    public void OnDestory()
    {
        worker?.Dispose();
    }

    private Texture2D ResizeTexture(Texture2D inputTexture, int newWidth, int newHeight, int cropTop, int cropBottom)
    {
        int originalWidth = inputTexture.width;
        int originalHeight = inputTexture.height;

        // Calculate the y-offset to crop from the top
        int cropFromTop = Mathf.Clamp(originalHeight - cropTop - newHeight, 0, originalHeight);

        int startY = Mathf.Max(cropFromTop, 0);
        int endY = Mathf.Min(cropFromTop + newHeight, originalHeight);

        Color[] pixels = new Color[newWidth * newHeight];

        float xRatio = (float)originalWidth / newWidth;
        float yRatio = (float)(endY - startY) / newHeight;

        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                int originalX = Mathf.FloorToInt(x * xRatio);
                int originalY = Mathf.FloorToInt(y * yRatio) + startY;

                pixels[y * newWidth + x] = inputTexture.GetPixel(originalX, originalY);
            }
        }

        Texture2D resizedTexture = new Texture2D(newWidth, newHeight);
        resizedTexture.SetPixels(pixels);
        resizedTexture.Apply();

        return resizedTexture;
    }

    private Texture2D PreprocessImage(Texture2D inputTexture)
    {
        Texture2D yuvTexture = ConvertRGBToYUV(inputTexture);

        Texture2D blurredTexture = ApplyGaussianBlur(yuvTexture);

        return blurredTexture;
    }

    private Texture2D ApplyGaussianBlur(Texture2D inputTexture)
    {
        int kernelSize = 5;
        float sigma = 1f;

        int halfKernelSize = kernelSize / 2;

        Color[] pixels = inputTexture.GetPixels();
        Color[] blurredPixels = new Color[pixels.Length];

        for (int y = 0; y < inputTexture.height; y++)
        {
            for (int x = 0; x < inputTexture.width; x++)
            {
                float r = 0, g = 0, b = 0;

                for (int i = -halfKernelSize; i <= halfKernelSize; i++)
                {
                    for (int j = -halfKernelSize; j <= halfKernelSize; j++)
                    {
                        int pixelX = Mathf.Clamp(x + i, 0, inputTexture.width - 1);
                        int pixelY = Mathf.Clamp(y + j, 0, inputTexture.height - 1);

                        Color currentPixel = pixels[pixelY * inputTexture.width + pixelX];

                        float weight = GaussianWeight(i, j, sigma);
                        r += currentPixel.r * weight;
                        g += currentPixel.g * weight;
                        b += currentPixel.b * weight;
                    }
                }

                blurredPixels[y * inputTexture.width + x] = new Color(r, g, b, 1f);
            }
        }

        Texture2D blurredTexture = new Texture2D(inputTexture.width, inputTexture.height);
        blurredTexture.SetPixels(blurredPixels);
        blurredTexture.Apply();

        return blurredTexture;
    }

    // Calculate the Gaussian weight for a given position in the kernel
    private float GaussianWeight(int x, int y, float sigma)
    {
        return Mathf.Exp(-(x * x + y * y) / (2 * sigma * sigma)) / (2 * Mathf.PI * sigma * sigma);
    }

    private Texture2D ConvertRGBToYUV(Texture2D rgbTexture)
    {
        int width = rgbTexture.width;
        int height = rgbTexture.height;

        Color[] rgbPixels = rgbTexture.GetPixels();
        Color[] yuvPixels = new Color[width * height];

        for (int i = 0; i < rgbPixels.Length; i++)
        {
            float r = rgbPixels[i].r;
            float g = rgbPixels[i].g;
            float b = rgbPixels[i].b;

            float y = 0.299f * r + 0.587f * g + 0.114f * b;
            float u = -0.14713f * r - 0.288862f * g + 0.436f * b;
            float v = 0.615f * r - 0.51498f * g - 0.10001f * b;

            yuvPixels[i] = new Color(y, u, v, rgbPixels[i].a);
        }

        Texture2D yuvTexture = new Texture2D(width, height);
        yuvTexture.SetPixels(yuvPixels);
        yuvTexture.Apply();

        return yuvTexture;
    }
}
