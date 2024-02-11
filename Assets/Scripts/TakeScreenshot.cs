using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using CarControls;
using UnityEngine;

namespace CarUI
{
    public class TakeScreenshot : MonoBehaviour
    {

        public Camera camera1;
        public Camera camera2;
        public Camera camera3;
        
        [SerializeField] private RenderTexture renderTexture1;
        [SerializeField] private RenderTexture renderTexture2;
        [SerializeField] private RenderTexture renderTexture3;

        [SerializeField] private CarController Car;

        private RenderTexture[] valueArray;
        private object[] carProperties;
        private string csvFilename;
        private bool capturingScreenshots = false;


        // Start is called before the first frame update
        void Start()
        {
            valueArray = new RenderTexture[3];
            carProperties = new object[7];


            
            valueArray[0] = renderTexture1;
            valueArray[1] = renderTexture2;
            valueArray[2] = renderTexture3;

            csvFilename = "/big_drive/ZPD_Data_2/test.csv";
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                capturingScreenshots = !capturingScreenshots;
                if (capturingScreenshots)
                {
                    print("STARTED TAKING SCREENSHOTS");
                    InvokeRepeating("CaptureScreenshot", 1f, 0.15f);
                }
                else
                {
                    print("STOPPED TAKING SCREENSHOTS");
                    // Stop the repeating method
                    CancelInvoke("CaptureScreenshot");
                }
            }
            
        }

        void CaptureScreenshot()
        {
            string[] screenshotPaths = new string[3];

            foreach (RenderTexture renderTexture in valueArray)
            {
                // dataArray = new object[7];

                // get nessecary data
                

                // dataArray[3] = 

                // Create a Texture2D and read pixels from the Render Texture
                Texture2D screenshot = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
                RenderTexture.active = renderTexture;
                screenshot.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                RenderTexture.active = null;

                // generate a file name
                DateTime currentDate = DateTime.Now;
                string timestamp = currentDate.ToString("yyyyMMdd_HHmmssfff");
                string currentCameraName = "";
                // print("CURRENT CAMERA RENDER TEXTURE -> " + camera1.targetTexture);

                if (renderTexture == camera1.targetTexture)
                {
                    currentCameraName = "center";
                }
                else if (renderTexture == camera2.targetTexture)
                {
                    currentCameraName = "left";
                } else if (renderTexture == camera3.targetTexture)
                {
                    currentCameraName = "right";
                }

                // print(currentCameraName);

                string filename = "/big_drive/ZPD_Data_2/Screenshots/" + currentCameraName + "_" + timestamp + ".png";

                int id = Array.IndexOf(valueArray, renderTexture);

                screenshotPaths[id] = filename;



                // Convert to PNG and save
                byte[] bytes = screenshot.EncodeToPNG();
                System.IO.File.WriteAllBytes(filename, bytes);
                // Debug.Log("Screenshot saved!");

                // Write to csv
            }
            // print(screenshotPaths[0]);

            WriteCSV(screenshotPaths);
        }

        private void WriteCSV(string[] screenshotPaths)
        {
            // print("WRITING TO CSV...");
            if (screenshotPaths.Length > 0)
            {
                TextWriter tw = new StreamWriter(csvFilename, true);
                tw.WriteLine(screenshotPaths[0] + "," + screenshotPaths[1] + "," + screenshotPaths[2] + "," + Car.GetSteering() + "," + Car.GetThrottle() + "," + Car.GetBrakes() + "," + Car.GetSpeed());
                tw.Close();
            }
        }
        
    }

}