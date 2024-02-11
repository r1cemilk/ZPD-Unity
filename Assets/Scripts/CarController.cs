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

        // Settings
        [SerializeField] private float motorForce, breakForce, maxSteerAngle;

        // Wheel Colliders
        [SerializeField] private WheelCollider frontLeftWheelCollider, frontRightWheelCollider;
        [SerializeField] private WheelCollider rearLeftWheelCollider, rearRightWheelCollider;
        [SerializeField] private Transform com;
        // [SerializeField] private Rigidbody rb;

        // Wheels
        [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
        [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;


        // var model = new OnnxModel("Assets/onnx_model_test.onnx");

        private Rigidbody rb;
        private void FixedUpdate() {
            GetInput();
            HandleMotor();
            HandleSteering();
            UpdateWheels();
            // print(shouldPredict);
        }

        private void GetInput() {
            // Steering Input
            horizontalInput = Input.GetAxis("Horizontal");

            // Acceleration Input
            verticalInput = Input.GetAxis("Vertical");

            // Breaking Input
            isBreaking = Input.GetKey(KeyCode.Space);
        }

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            // rb.centerOfMass = com.position;
            // Create an input tensor
            // var inputTensor = new Tensor(model.inputs[0].shape, random: new System.Random());
        }

        private void HandleMotor() {
            frontLeftWheelCollider.motorTorque = AISpeed > 0 ? AISpeed * motorForce : verticalInput * motorForce;
            frontRightWheelCollider.motorTorque =  AISpeed > 0 ? AISpeed * motorForce : verticalInput * motorForce;
            // frontRightWheelCollider.motorTorque = 80;
            // frontLeftWheelCollider.motorTorque = 80;
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
            // print(AISteeringAngle);
            // currentSteerAngle = GetSteering();
            // currentSteerAngle = AISteeringAngle;
            // currentSteerAngle = AISteeringAngle > 0 ? AISteeringAngle : maxSteerAngle * horizontalInput;
            currentSteerAngle = AISteeringAngle;
            frontLeftWheelCollider.steerAngle = currentSteerAngle;
            frontRightWheelCollider.steerAngle = currentSteerAngle;
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
            //
            // return horizontalInput;
            return currentSteerAngle;
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

        public void GetShouldPredict(bool should_predict)
        {
            shouldPredict = should_predict;
        }
        
    }
}