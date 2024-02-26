using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using CarControls;

namespace CarUI
{
    public class GetSteering : MonoBehaviour
    {   
        [SerializeField] private CarController car;

        private TextMeshProUGUI text;
        // Start is called before the first frame update
        void Start()
        {
            text = GetComponent<TextMeshProUGUI>();
        }

        // Update is called once per frame
        void Update()
        {
            float roundedAngle = Mathf.Round(car.GetSteering() * 30f * 100.0f) * 0.01f;
            text.text = roundedAngle.ToString();
        }
    }

}