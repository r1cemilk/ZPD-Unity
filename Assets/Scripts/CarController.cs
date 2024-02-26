using System;
using System.Collections;
using System.Collections.Generic;
// using NatSuite.ML;
// using NatSuite.ML.Onnx;
using UnityEngine;

namespace CarControls
{
    public class CarController : MonoBehaviour
    {
        private float horizontalInput, verticalInput;
        private float currentSteerAngle, currentbreakForce;
        private bool isBreaking;
        private bool startPrediction;
        private bool shouldPredict;

        private float AISpeed;
        private float AIThrottle;
        private float AISteeringAngle;
        private float currentSpeed;
        private float TurnSteeringAngle;
        
        private Coroutine resetSteeringCoroutine;

        // Settings
        [SerializeField] private float motorForce, breakForce, maxSteerAngle;
        [SerializeField] private Vector3 centerOfMass;

        // Wheel Colliders
        [SerializeField] private WheelCollider frontLeftWheelCollider, frontRightWheelCollider;
        [SerializeField] private WheelCollider rearLeftWheelCollider, rearRightWheelCollider;
        [SerializeField] private Transform com;

        // Wheels
        [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
        [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;
        
        // Car
        [SerializeField] private GameObject Car;

        private Rigidbody rb;
        private void FixedUpdate() {
            GetInput();
            HandleMotor();
            HandleSteering();
            UpdateWheels();
        }

        private void GetInput() {
            // Steering Input
            horizontalInput = Input.GetAxis("Horizontal");

            // Acceleration Input
            verticalInput = Input.GetAxis("Vertical");

            // Breaking Input
            isBreaking = Input.GetKey(KeyCode.Space);

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Car.transform.position = new Vector3(-1305.5f, 156.8003f, 252.9f);
                Car.transform.rotation = Quaternion.Euler(0f, 215.16f, 0f);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Car.transform.position = new Vector3(-943.8f, 156.8003f, -403.14f);
                Car.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
            
            // Check if the Escape key is pressed
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Quit the application
                Application.Quit();
            }
        }

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.centerOfMass = centerOfMass;
        }

        private void HandleMotor() {
            // Not the best syntax, but this works too
            if (AISpeed == 0)
            {
                frontLeftWheelCollider.motorTorque = verticalInput * 125 * motorForce * Time.deltaTime;
                frontRightWheelCollider.motorTorque = verticalInput * 125 * motorForce * Time.deltaTime;
                rearLeftWheelCollider.motorTorque = verticalInput * 125 * motorForce * Time.deltaTime;
                rearRightWheelCollider.motorTorque = verticalInput * 125 * motorForce * Time.deltaTime;
            }
            else
            {
                if (GetSpeed() < AISpeed)
                {
                    frontLeftWheelCollider.motorTorque = 1 * 125 * motorForce * Time.deltaTime;
                    frontRightWheelCollider.motorTorque = 1 * 125 * motorForce * Time.deltaTime;
                    rearLeftWheelCollider.motorTorque = 1 * 125 * motorForce * Time.deltaTime;
                    rearRightWheelCollider.motorTorque = 1 * 125 * motorForce * Time.deltaTime;
                }
                else if (GetSpeed() > AISpeed)
                {
                    frontLeftWheelCollider.motorTorque = -1 * 125 * motorForce * Time.deltaTime;
                    frontRightWheelCollider.motorTorque = -1 * 125 * motorForce * Time.deltaTime;
                    rearLeftWheelCollider.motorTorque = -1 * 125 * motorForce * Time.deltaTime;
                    rearRightWheelCollider.motorTorque = -1 * 125 * motorForce * Time.deltaTime;
                }
                else
                {
                    frontLeftWheelCollider.motorTorque = 0 * 125 * motorForce * Time.deltaTime;
                    frontRightWheelCollider.motorTorque = 0 * 125 * motorForce * Time.deltaTime;
                    rearLeftWheelCollider.motorTorque = 0 * 125 * motorForce * Time.deltaTime;
                    rearRightWheelCollider.motorTorque = 0 * 125 * motorForce * Time.deltaTime;
                }
            }
 
            currentbreakForce = isBreaking ? breakForce : 0f;
            ApplyBreaking();
        }

        private void ApplyBreaking() {
            frontRightWheelCollider.brakeTorque = currentbreakForce;
            frontLeftWheelCollider.brakeTorque = currentbreakForce;
            rearLeftWheelCollider.brakeTorque = currentbreakForce;
            rearRightWheelCollider.brakeTorque = currentbreakForce;
        }

        private void HandleSteering() {
  
            if (TurnSteeringAngle != 0)
            {
                currentSteerAngle = Mathf.Lerp(currentSteerAngle, TurnSteeringAngle, Time.deltaTime * 0.5f);
            
                if (Mathf.Abs(currentSteerAngle - TurnSteeringAngle) < 5f)
                {
                    TurnSteeringAngle = 0;
                }
            }
            else
            {
                currentSteerAngle = AISteeringAngle != 0 ? AISteeringAngle * maxSteerAngle : maxSteerAngle * horizontalInput;
            } 
            
            frontLeftWheelCollider.steerAngle = Mathf.Lerp(frontLeftWheelCollider.steerAngle, currentSteerAngle, 2.0f);
            frontRightWheelCollider.steerAngle = Mathf.Lerp(frontRightWheelCollider.steerAngle, currentSteerAngle, 2.0f);
            
        }

        private void UpdateWheels() {
            UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
            UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
            UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
            UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
        }

        private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform) {
            Vector3 pos;
            Quaternion rot; 
            wheelCollider.GetWorldPose(out pos, out rot);
            wheelTransform.rotation = rot;
            wheelTransform.position = pos;
        }

        public float GetSpeed()
        {
            return Mathf.Round(rb.velocity.magnitude * 10.0f) * 0.1f;
        }

        public float GetThrottle()
        {
            return Mathf.Round(verticalInput * motorForce * 100.0f) * 0.01f;
        }

        public float GetSteering()
        {
            // if (AISteeringAngle > 0)
            // {
            //     return AISteeringAngle;
            // }
            // return currentSteerAngle;
            return horizontalInput;
        }
        

        public float GetBrakes()
        {
            return Mathf.Round(currentbreakForce * 100.0f) * 0.01f;
        }

        public void SetSpeed(float speed)
        {
            AISpeed = speed;
        }

        public void SetThrottle(float throttle)
        {
            AIThrottle = throttle;
        }

        public void SetSteering(float steering_angle)
        {
            AISteeringAngle = steering_angle;
        }

        public Quaternion GetRotation()
        {
            return rb.rotation;
        }

        public void SetRotation(float targetRotation)
        {
            TurnSteeringAngle = targetRotation;
        }

        public void GetShouldPredict(bool should_predict)
        {
            shouldPredict = should_predict;
        }
        
    }
}