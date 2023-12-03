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
            text.text = car.GetSteering().ToString();
        }
    }

}