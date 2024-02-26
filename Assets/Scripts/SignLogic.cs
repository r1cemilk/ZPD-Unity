using System;
using System.Collections;
using System.Collections.Generic;
using CarControls;
using Unity.VisualScripting;
using UnityEngine;

public class SignLogic : MonoBehaviour
{
    [SerializeField] private CarController carController;
    private List<int> PendingSigns = new List<int>();
    private List<int> ActiveSigns = new List<int>();

    private Quaternion desiredRotation;

    
    // Update is called once per frame
    public void UpdateSigns()
    {
        if (PendingSigns.Count > 0)
        {
            foreach (int id in PendingSigns)
            {


                // print("IN PENDING: " + id);
                if (ActiveSigns.Contains(33) && !PendingSigns.Contains(33))
                {
                    carController.SetRotation(20);
                    ActiveSigns.Remove(33);
                }
                
                if (ActiveSigns.Contains(14) && id == 14)
                {
                    PendingSigns.Remove(14);
                }
                
                
                if (ActiveSigns.Contains(34) && !PendingSigns.Contains(34))
                {
                    carController.SetRotation(-20);
                    ActiveSigns.Remove(34);
                }
                
                // if (ActiveSigns.Contains(id))
                // {
                //     PendingSigns.Remove(id);
                //     return;
                // }

                if (id == 4)
                {
                    carController.SetSpeed(8.0f);
                    ActiveSigns.Add(id);
                }

                if (id == 14)
                {
                    carController.SetSpeed(0.5f);
                    ActiveSigns.Add(id);
                }

                if (id == 2)
                {
                    carController.SetSpeed(5.0f);
                    ActiveSigns.Add(id);
                }
                
                // 1) if it is 33, then record an initial angle
                // 2) adjust the angle of prediction with the hyperparameter angle
                // 3) once it has turned 65 degrees to the right/left, we remove the sign from all the lists
                
                if (ActiveSigns.Contains(id))
                {
                    PendingSigns.Remove(id);
                    return;
                }

                if (id == 33)
                {
                    // Rotate the car towards the target rotation
                    ActiveSigns.Add(id);
                }


            }
        }
    }

    private void Update()
    {
        if (ActiveSigns.Contains(14))
        {
            if (carController.GetSpeed() < 2f)
            {
                ActiveSigns.Remove(14);
                carController.SetSpeed(8f);
            }
        }
    }


    public void UpdatePendingSigns(int indexToAdd)
    {
        PendingSigns.Add(indexToAdd);
    }
}
