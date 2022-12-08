using UnityEngine;
using System.Collections.Generic;
using System;
using static JoystickMonke;
using static Monkey2D;

public class JoystickDrunk : MonoBehaviour
{
    public float moveX;
    public float moveY;
    [ShowOnly] public float currentSpeed = 0.0f;
    [ShowOnly] public float currentRot = 0.0f;

    //Observation Noise
    float prevVelObsEps = 0;
    float prevVelObsZet = 0;
    float prevRotObsEps = 0;
    float prevRotObsZet = 0;
    public float DistFlowSpeed = 0;
    public float DistFlowRot = 0;
    private float ObsNoiseTau;
    private float ObsVelocityNoiseGain;
    private float ObsRotationNoiseGain;

    private System.Random rand;
    [ShowOnly] public int seed;

    // Start is called before the first frame update
    void Start()
    {
        seed = UnityEngine.Random.Range(1, 10000);
        rand = new System.Random(seed);

        ObsNoiseTau = PlayerPrefs.GetFloat("ObsNoiseTau");
        ObsVelocityNoiseGain = PlayerPrefs.GetFloat("ObsVelocityNoiseGain");
        ObsRotationNoiseGain = PlayerPrefs.GetFloat("ObsRotationNoiseGain");
    }

    private void FixedUpdate()
    {
        currentSpeed = SharedJoystick.currentSpeed;
        currentRot = SharedJoystick.currentRot;

        //print(string.Format("current speed:{0}", currentSpeed));
        //print(string.Format("current rotation:{0}", currentRot));
        DistFlowSpeed = observationNoiseVel(ObsNoiseTau, ObsVelocityNoiseGain);
        DistFlowRot = observationNoiseRot(ObsNoiseTau, ObsRotationNoiseGain);
        //print(DistFlowSpeed);
        //print(DistFlowRot);
        //print(transform.position);
        transform.position = transform.position + transform.forward * (currentSpeed + DistFlowSpeed) * Time.fixedDeltaTime;
        transform.Rotate(0f, (currentRot + DistFlowRot) * Time.fixedDeltaTime, 0f);
    }

    void Update()
    {

    }


    public float BoxMullerGaussianSample()
    {
        float u1, u2, S;
        do
        {
            u1 = 2.0f * (float)rand.NextDouble() - 1.0f;
            u2 = 2.0f * (float)rand.NextDouble() - 1.0f;
            S = u1 * u1 + u2 * u2;
        }
        while (S >= 1.0f);
        return u1 * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);
    }

    float observationNoiseVel(float tau, float Gain)
    {
        float kappa;
        float lamda;
        float epsilon;
        float zeta;
        float result_speed;

        kappa = Mathf.Exp(-Time.fixedDeltaTime / tau);
        lamda = 1 - kappa;
        epsilon = kappa * prevVelObsEps + lamda * Gain * BoxMullerGaussianSample();
        zeta = kappa * prevVelObsZet + lamda * epsilon;
        float ObsNoiseMagnitude = Mathf.Sqrt((SharedJoystick.currentSpeed / SharedJoystick.MaxSpeed) * (SharedJoystick.currentSpeed / SharedJoystick.MaxSpeed)
            + (SharedJoystick.currentRot / SharedJoystick.RotSpeed) * (SharedJoystick.currentRot / SharedJoystick.RotSpeed));
        result_speed = zeta * ObsNoiseMagnitude * SharedJoystick.MaxSpeed;
        prevVelObsEps = epsilon;
        prevVelObsZet = zeta;

        return result_speed;
    }

    float observationNoiseRot(float tau, float Gain)
    {
        float kappa;
        float lamda;
        float epsilon;
        float zeta;
        float result_speed;

        kappa = Mathf.Exp(-Time.fixedDeltaTime / tau);
        lamda = 1 - kappa;
        epsilon = kappa * prevRotObsEps + lamda * Gain * BoxMullerGaussianSample();
        zeta = kappa * prevRotObsZet + lamda * epsilon;
        float ObsNoiseMagnitude = Mathf.Sqrt((SharedJoystick.currentSpeed / SharedJoystick.MaxSpeed) * (SharedJoystick.currentSpeed / SharedJoystick.MaxSpeed)
            + (SharedJoystick.currentRot / SharedJoystick.RotSpeed) * (SharedJoystick.currentRot / SharedJoystick.RotSpeed));

        result_speed = zeta * ObsNoiseMagnitude * SharedJoystick.RotSpeed;
        prevRotObsEps = epsilon;
        prevRotObsZet = zeta;

        return result_speed;
    }
}