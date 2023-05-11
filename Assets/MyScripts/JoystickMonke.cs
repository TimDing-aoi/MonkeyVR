﻿using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO.Ports;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine.InputSystem.LowLevel;
using static Monkey2D;
using System.IO;
using System.Globalization;

public class JoystickMonke : MonoBehaviour
{
    wrmhl joystick = new wrmhl();

    public bool usingArduino;

    //Replay?
    bool isReplay = false;
    int framecount = 0;

    [Tooltip("SerialPort of your device.")]
    public string portName;

    [Tooltip("Baudrate")]
    public int baudRate = 1000000;

    [Tooltip("Timeout")]
    public int ReadTimeout = 5000;

    [Tooltip("QueueLength")]
    public int QueueLength = 1;

    public float GaussianPTBVMin;
    public float GaussianPTBVMax;
    public float ptbJoyVelStartRange;
    public float ptbJoyVelStart;
    public float ptbJoyVelMu;
    public float ptbJoyVelSigma;
    public float ptbJoyVelGain;
    public float ptbJoyVelEnd;
    public float ptbJoyVelLen;
    public float ptbJoyVelValue;


    public float GaussianPTBRMin;
    public float GaussianPTBRMax;
    public float ptbJoyRotStartRange;
    public float ptbJoyRotStart;
    public float ptbJoyRotMu;
    public float ptbJoyRotSigma;
    public float ptbJoyRotGain;
    public float ptbJoyRotEnd;
    public float ptbJoyRotLen;
    public float ptbJoyRotValue;

    public int ptbJoyFlag;
    public int ptbJoyFlagTrial = 0;

    public float GaussianPTBRatio;
    public bool GaussianPTB;
    public float ptbJoyEnableTime;

    public static JoystickMonke SharedJoystick;

    public float moveX;
    public float moveY;
    public float circX;
    public int press;
    [ShowOnly] public float currentSpeed = 0.0f;
    [ShowOnly] public float currentRot = 0.0f;
    public float speedPrePtb = 0.0f;
    public float rotPrePtb = 0.0f;
    public float Max_Angular_Speed = 0.0f;
    public float Max_Linear_Speed = 0.0f;

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

    //readonly List<float> t = new List<float>();
    //readonly List<bool> isPtb = new List<bool>();
    //readonly List<float> rawX = new List<float>();
    //readonly List<float> rawY = new List<float>();
    //readonly List<float> v = new List<float>();
    //readonly List<float> w = new List<float>();
    //readonly List<float> vAddPtb = new List<float>();
    //readonly List<float> wAddPtb = new List<float>();

    private System.Random rand;
    [ShowOnly] public int seed;

    float[] data;

    const int count = 90;
    const float R = 7.69711747013104972f;
    const float A = 0.00492867323399f;
    const ulong MAXINT = (1UL << 53) - 1;
    const double INCR = 1.0 / MAXINT;

    float[] x = new float[count + 1];
    float[] y = new float[count + 1];
    ulong[] xcomp;
    float aDivY0;

    public float timeCounter = 0;
    public float timeOnsetJoy = 0;
    public float timeCounterMovement = 0;
    public float stopCounter = 0;
    public float timeCntSecCurr = 0;
    public float timeCntSecStart = 0;
    public float timeCntPTBStart = 0;
    public GameObject FF;

    public Phases currPhase;

    public bool isCtrlDynamics = false;

    //Akis PTB vars
    [HideInInspector]
    public float gainVel = 0.0f;
    [HideInInspector]
    public float gainRot = 0.0f;

    [HideInInspector]
    public float meanDist;
    [HideInInspector]
    public float meanTime;
    [HideInInspector]
    public float meanAngle;
    [HideInInspector]
    public int flagCtrlDynamics;
    [HideInInspector]
    public float minTau;
    [HideInInspector]
    public float maxTau;
    [HideInInspector]
    public float numTau;
    [HideInInspector]
    public float meanLogSpace;
    [HideInInspector]
    public float stdDevLogSpace;
    [HideInInspector]
    public float TauTau;
    [HideInInspector]
    public float NoiseTau;
    [HideInInspector]
    public float NoiseTauTau;
    [HideInInspector]
    public float velProcessNoiseGain;
    [HideInInspector]
    public float rotProcessNoiseGain;
    [HideInInspector]
    public float kappa;
    [HideInInspector]
    public float meanTau;
    [HideInInspector]
    public float stdDevTau;
    [HideInInspector]
    public float logSample = 1f;

    [HideInInspector]
    public bool ProcessNoiseFlag;

    [HideInInspector]
    public float velKsi = 0.0f;
    [HideInInspector]
    public float prevVelKsi = 0.0f;
    [HideInInspector]
    public float rotKsi = 0.0f;
    [HideInInspector]
    public float prevRotKsi = 0.0f;
    [HideInInspector]
    public float velEta = 0.0f;
    [HideInInspector]
    public float prevVelEta = 0.0f;
    [HideInInspector]
    public float rotEta = 0.0f;
    [HideInInspector]
    public float prevRotEta = 0.0f;
    [HideInInspector]
    public float cleanVel = 0.0f;
    [HideInInspector]
    public float prevCleanVel = 0.0f;
    [HideInInspector]
    public float cleanRot = 0.0f;
    [HideInInspector]
    public float prevCleanRot = 0.0f;

    public float currentTau;
    public float savedTau;
    public List<float> taus = new List<float>();

    public float rawX;
    public float rawY;

    CTIJoystick USBJoystick;
    float prevX = 0.0f;
    float prevY = 0.0f;

    float velInfluenceOnProcessNoise = 1.0f; // scaling of linear control on process noise magnitude
    float rotInfluenceOnProcessNoise = 0.3f; // scaling of angular control on process noise magnitude

    public bool BrakeFlag;
    public bool StopFlag;
    float TrialEndThreshold;

    public float velbrakeThresh;
    public float rotbrakeThresh;

    readonly List<Tuple<float, float>> XYInputList = new List<Tuple<float, float>>();
    // Start is called before the first frame update
    void Awake()
    {
        SharedJoystick = this;
    }

    void Start()
    {
        Max_Linear_Speed = PlayerPrefs.GetFloat("Max_Linear_Speed");
        Max_Angular_Speed = PlayerPrefs.GetFloat("Max_Angular_Speed");

        portName = PlayerPrefs.GetString("Port");

        ProcessNoiseFlag = PlayerPrefs.GetInt("isProcessNoise") == 1;
        GaussianPTB = PlayerPrefs.GetInt("GaussianPTB") == 1;

        seed = UnityEngine.Random.Range(1, 10000);
        rand = new System.Random(seed);

        GaussianPTBVMax = PlayerPrefs.GetFloat("GaussianPTBVMax");
        GaussianPTBVMin = -GaussianPTBVMax;

        GaussianPTBRMax = PlayerPrefs.GetFloat("GaussianPTBRMax");
        GaussianPTBRMin = -GaussianPTBRMax;

        GaussianPTBRatio = PlayerPrefs.GetFloat("GaussianPTBRatio");

        isReplay = PlayerPrefs.GetInt("isReplay") == 1;
        if (isReplay)
        {
            ReadXYInputCont();
        }

        ptbJoyVelStartRange = 1.0f;
        ptbJoyVelSigma = 0.2f;
        ptbJoyVelLen = 1.0f;

        ptbJoyRotStartRange = 1.0f;
        ptbJoyRotSigma = 0.2f;
        ptbJoyRotLen = 1.0f;

        calcPtbJoyTrial();

        if (usingArduino)
        {
            joystick.set(portName, baudRate, ReadTimeout, QueueLength);
            joystick.connect();
        }
        else
        {
            USBJoystick = CTIJoystick.current;
        }

        //load PTB vars
        isCtrlDynamics = (int)PlayerPrefs.GetFloat("Acceleration_Control_Type") != 0;
        flagCtrlDynamics = (int)PlayerPrefs.GetFloat("Acceleration_Control_Type");
        meanDist = PlayerPrefs.GetFloat("MeanDistance");
        meanTime = PlayerPrefs.GetFloat("MeanTime");
        meanAngle = PlayerPrefs.GetFloat("MeanAngle");
        minTau = PlayerPrefs.GetFloat("MinTau");
        maxTau = PlayerPrefs.GetFloat("MaxTau");
        numTau = (int)PlayerPrefs.GetFloat("NumTau");
        TauTau = PlayerPrefs.GetFloat("TauTau");
        NoiseTau = PlayerPrefs.GetFloat("NoiseTau");
        NoiseTauTau = PlayerPrefs.GetFloat("NoiseTauTau");
        velProcessNoiseGain = PlayerPrefs.GetFloat("VelocityNoiseGain");
        rotProcessNoiseGain = PlayerPrefs.GetFloat("RotationNoiseGain");

        velbrakeThresh = PlayerPrefs.GetFloat("velBrakeThresh");
        rotbrakeThresh = PlayerPrefs.GetFloat("rotBrakeThresh");
        float velStopThreshold = PlayerPrefs.GetFloat("velStopThreshold");
        float rotStopThreshold = PlayerPrefs.GetFloat("rotStopThreshold");

        ObsNoiseTau = PlayerPrefs.GetFloat("ObsNoiseTau");
        ObsVelocityNoiseGain = PlayerPrefs.GetFloat("ObsVelocityNoiseGain");
        ObsRotationNoiseGain = PlayerPrefs.GetFloat("ObsRotationNoiseGain");

        if (PlayerPrefs.GetFloat("ThreshTauMultiplier") != 0)
        {
            float k = PlayerPrefs.GetFloat("ThreshTauMultiplier");
            velbrakeThresh = k * savedTau + velStopThreshold;
            rotbrakeThresh = k * savedTau + rotStopThreshold;
        }

        x[0] = R;
        y[0] = GaussianPDFDenorm(R);

        x[1] = R;
        y[1] = y[0] + (A / x[1]);

        for (int i = 2; i < count; i++)
        {
            x[i] = GaussianPDFDenormInv(y[i - 1]);
            y[i] = y[i - 1] + (A / x[i]);
        }

        x[count] = 0.0f;

        aDivY0 = A / y[0];
        xcomp = new ulong[count];

        xcomp[0] = (ulong)(R * y[0] / A * (double)MAXINT);

        for (int i = 1; i < count - 1; i++)
        {
            xcomp[i] = (ulong)(x[i + 1] / x[i] * (double)MAXINT);
        }

        xcomp[count - 1] = 0;

        //PTB set up
        kappa = Mathf.Exp(-1f / TauTau);

        switch (flagCtrlDynamics)
        {

            case 0:
                break;

            case 1:
                var linspace = (maxTau - minTau) / (numTau - 1);

                for (int i = 0; i < numTau; i++)
                {
                    taus.Add(minTau + (i * linspace));
                    //print(string.Format("tau{0} = {1}", i, taus[i]));
                }

                if (taus[taus.Count - 1] != maxTau)
                {
                    taus[taus.Count - 1] = maxTau;
                }

                currentTau = taus[rand.Next(0, taus.Count)]; 

                break;

            case 2:
                meanTau = 0.5f * (Mathf.Log(minTau) + Mathf.Log(maxTau));
                stdDevTau = 0.5f * (meanTau - Mathf.Log(minTau));
                meanLogSpace = meanTau * (1.0f - kappa);
                stdDevLogSpace = stdDevTau * Mathf.Sqrt(1.0f - (kappa * kappa));
                //print(string.Format("muPhi = {0}, sigPhi = {1}, muEta = {2}, sigEta = {3}", meanNoise, stdDevNoise, meanLogSpace, stdDevLogSpace));
                break;
        }
    }

    private void FixedUpdate()
    {
        if (usingArduino)
        {
            try
            {
                string[] line = joystick.readQueue().Split(',');
                moveX = (float.Parse(line[0]) - 511.5f) / 511.5f;
                moveY = (float.Parse(line[1]) - 511.5f) / 511.5f;
                press = int.Parse(line[2]);
            }
            catch (Exception)
            {
                // read serial in faster with error
            }
        }
        else
        {
            if (isReplay)
            {
                moveX = 0;
                moveY = 0;
            }
            else
            {
                moveX = -USBJoystick.x.ReadValue();
                moveY = -USBJoystick.y.ReadValue();
            }
                
            //print(moveX);

            if (moveX < 0.0f)
            {
                moveX += 1.0f;
            }
            else if (moveX > 0.0f)
            {
                moveX -= 1.0f;
            }
            else if (moveX == 0)
            {
                if (prevX < 0.0f)
                {
                    moveX -= 1.0f;
                }
                else if (prevX > 0.0f)
                {
                    moveX += 1.0f;
                }
            }
            prevX = moveX;

            if (moveY < 0.0f)
            {
                moveY += 1.0f;
            }
            else if (moveY > 0.0f)
            {
                moveY -= 1.0f;
            }
            else if (moveY == 0)
            {
                if (prevY < 0.0f)
                {
                    moveY -= 1.0f;
                }
                else if (prevY > 0.0f)
                {
                    moveY += 1.0f;
                }
            }
            prevY = moveY;

            float minR = PlayerPrefs.GetFloat("Minimum Firefly Distance");
            float maxR = PlayerPrefs.GetFloat("Maximum Firefly Distance");

            if (isReplay)
            {
                moveX = -XYInputList[framecount].Item2;
                moveY = XYInputList[framecount].Item1;
                framecount++;
            }
            //save filtered joystick X & Y
            rawX = moveY;
            rawY = -moveX;

            //PTB noise
            if (isCtrlDynamics)
            {
                updateControlDynamics();
                if (SharedMonkey.Joystick_Disabled)
                {
                    //print("stoping");
                    moveX = 0;
                    moveY = 0;
                    StopFlag = true;
                    BrakeFlag = false;
                    currentTau = savedTau / 4;
                    velProcessNoiseGain = 0;
                    rotProcessNoiseGain = 0;
                    ProcessNoise();
                }
                else if (Mathf.Abs(SharedJoystick.currentSpeed) < velbrakeThresh && Mathf.Abs(SharedJoystick.currentRot) < rotbrakeThresh)
                {
                    StopFlag = false;
                    BrakeFlag = true;
                    currentTau = savedTau / 4;
                    velProcessNoiseGain = 0;
                    rotProcessNoiseGain = 0;
                    ProcessNoise();
                }
                else
                {
                    StopFlag = false;
                    BrakeFlag = false;
                    currentTau = savedTau;
                    velProcessNoiseGain = PlayerPrefs.GetFloat("VelocityNoiseGain");
                    rotProcessNoiseGain = PlayerPrefs.GetFloat("RotationNoiseGain");
                    ProcessNoise();
                }
            }
            else
            {
                currentSpeed = -moveX * Max_Linear_Speed;
                currentRot = moveY * Max_Angular_Speed;
                cleanVel = currentSpeed;
                cleanRot = currentRot;
                velProcessNoiseGain = PlayerPrefs.GetFloat("VelocityNoiseGain");
                rotProcessNoiseGain = PlayerPrefs.GetFloat("RotationNoiseGain");
                ProcessNoise();
                if (SharedMonkey.Joystick_Disabled && SharedMonkey.isCOM)
                {
                    currentSpeed = 0;
                    currentRot = 0;
                }
                speedPrePtb = currentSpeed;
                rotPrePtb = currentRot;

                if (GaussianPTB && ptbJoyFlagTrial > 0)
                {
                    //Monkey2D.Phase;                
                    if (SharedMonkey.phase_task_selecter == Phases.check)
                    {
                        moveX = 0;
                        moveY = 0;
                        timeCounter = 0;
                        timeCounterMovement = 0;
                        timeCntSecStart = Time.realtimeSinceStartup;
                        ptbJoyEnableTime = 0;

                        calcPtbJoyTrial();
                    }
                    else
                    {
                        timeCounter += 0.005f;
                        timeCntSecCurr = Time.realtimeSinceStartup - timeCntSecStart;

                        if (currentSpeed == 0)
                        {
                            timeCounterMovement = 0;
                            ptbJoyEnableTime = timeCounter;
                        }
                        else
                        {
                            timeCounterMovement += 0.005f;
                        }
                    }

                    if (timeCounterMovement >= ptbJoyVelStart & timeCounterMovement <= ptbJoyVelEnd)
                    {
                        if (ptbJoyFlag == 0)
                        {
                            timeCntPTBStart = Time.realtimeSinceStartup;
                        }

                        ptbJoyVelValue = GaussianShapedPtb(timeCounterMovement, ptbJoyVelMu, ptbJoyVelSigma, ptbJoyVelGain);
                        ptbJoyRotValue = GaussianShapedPtb(timeCounterMovement, ptbJoyRotMu, ptbJoyRotSigma, ptbJoyRotGain);
                        ptbJoyFlag = 1;
                    }
                    else
                    {
                        ptbJoyVelValue = 0.0f;
                        ptbJoyRotValue = 0.0f;
                        ptbJoyFlag = 0;
                    }

                    currentSpeed += ptbJoyVelValue;
                    currentRot += ptbJoyRotValue;
                }
                
            }

            //print(string.Format("current speed:{0}", currentSpeed));
            //print(string.Format("current rotation:{0}", currentRot));
            if(transform.name == "MonkeDrunk")
            {
                DistFlowSpeed = observationNoiseVel(ObsNoiseTau, ObsVelocityNoiseGain);
                DistFlowRot = observationNoiseRot(ObsNoiseTau, ObsRotationNoiseGain);
                transform.position = transform.position + transform.forward * (currentSpeed + DistFlowSpeed) * Time.fixedDeltaTime;
                transform.Rotate(0f, (currentRot + DistFlowRot) * Time.fixedDeltaTime, 0f);
            }
            else
            {
                transform.position = transform.position + transform.forward * currentSpeed * Time.fixedDeltaTime;
                transform.Rotate(0f, currentRot * Time.fixedDeltaTime, 0f);
            }
        }
    }

    void Update()
    {

    }

    public void OnApplicationQuit()
    {

    }

    private void calcPtbJoyTrial()
    {
        // Linear
        ptbJoyVelStart = (float) rand.NextDouble() * ptbJoyVelStartRange;
        ptbJoyVelMu = ptbJoyVelStart + (ptbJoyVelLen / 2);
        ptbJoyVelEnd = ptbJoyVelStart + ptbJoyVelLen;
        ptbJoyVelGain = (float) rand.NextDouble() * (GaussianPTBVMax - GaussianPTBVMin) + GaussianPTBVMin;

        //Angular
        ptbJoyRotStart = ptbJoyVelStart;
        ptbJoyRotMu = ptbJoyVelMu;
        ptbJoyRotEnd = ptbJoyVelEnd;
        ptbJoyRotGain = (float)rand.NextDouble() * (GaussianPTBRMax - GaussianPTBRMin) + GaussianPTBRMin;
    }

    private float GaussianShapedPtb(float t, float mu, float sigma, float gain)
    {
        return (float) (gain * Math.Exp(-0.5f * Math.Pow((t - mu) / sigma, 2)));
    }

    public float[] MakeProfile(float x)
    {
        float sig = 0.3f;
        int size = 120;
        float[] t = new float[size];
        t[0] = -0.5f;
        for (int i = 1; i < size; i++)
        {
            t[i] = t[i - 1] + (1.0f / size);
        }
        for (int i = 0; i < size; i++)
        {
            t[i] = x * Mathf.Exp(-Mathf.Pow(t[i], 2.0f) / (2.0f * Mathf.Pow(sig, 2.0f)));
        }
        float sub = t[0];
        for (int i = 0; i < size; i++)
        {
            t[i] = t[i] - sub;
        }
        return t;
    }

    public void DiscreteTau()
    {
        if (rand.NextDouble() > kappa)
        {
            var temp = currentTau;

            do
            {
                currentTau = taus[rand.Next(0, taus.Count)];
            } while (temp == currentTau);
        }

        //print(currentTau);

        savedTau = currentTau;
        CalculateMaxValues();
    }

    public void ContinuousTau()
    {
        logSample = kappa * logSample + ((stdDevLogSpace * BoxMullerGaussianSample()) + meanLogSpace);
        currentTau = Mathf.Exp(logSample);

        //print(stdDevLogSpace);

        savedTau = currentTau;
        CalculateMaxValues();
    }

    private void CalculateMaxValues()
    {
        Max_Linear_Speed = (meanDist / meanTime) * (1.0f / (-1.0f + (2 * (savedTau / meanTime)) * Mathf.Log((1 + Mathf.Exp(meanTime / savedTau)) / 2.0f)));
        Max_Angular_Speed = (meanAngle / meanTime) * (1.0f / (-1.0f + (2 * (savedTau / meanTime)) * Mathf.Log((1 + Mathf.Exp(meanTime / savedTau)) / 2.0f)));
    }

    public void ResetValues()
    {
        prevVelEta = 0f;
        prevVelKsi = 0f;
        prevRotEta = 0f;
        prevRotKsi = 0f;
    }

    private void ProcessNoise()
    {
        float gamma;
        float delta;
        if (!ProcessNoiseFlag /*&& CtrlDynamicsFlag*/)
        {
            gamma = 0;

            velKsi = 0;
            velEta = 0;
            prevCleanVel = cleanVel;
            prevVelKsi = velKsi;
            prevVelEta = velEta;

            rotKsi = 0;
            rotEta = 0;
            prevCleanRot = cleanRot;
            prevRotKsi = rotKsi;
            prevRotEta = rotEta;

            currentSpeed = cleanVel;
            currentRot = cleanRot;
        }
        else if(ProcessNoiseFlag)
        {
            if (NoiseTau == 0)
            {
                gamma = 0;
            }
            else
            {
                gamma = Mathf.Exp(-Time.fixedDeltaTime / NoiseTau);
            }

            delta = (1.0f - gamma);

            velKsi = gamma * prevVelKsi + delta * velProcessNoiseGain * BoxMullerGaussianSample();
            velEta = gamma * prevVelEta + delta * velKsi;
            prevCleanVel = cleanVel;
            prevVelKsi = velKsi;
            prevVelEta = velEta;

            rotKsi = gamma * prevRotKsi + delta * rotProcessNoiseGain * BoxMullerGaussianSample();
            rotEta = gamma * prevRotEta + delta * rotKsi;
            prevCleanRot = cleanRot;
            prevRotKsi = rotKsi;
            prevRotEta = rotEta;

            float ProcessNoiseMagnitude;
            if (Max_Angular_Speed == 0)
            {
                ProcessNoiseMagnitude = Mathf.Sqrt((cleanVel / Max_Linear_Speed) * (cleanVel / Max_Linear_Speed));
            }
            else
            {
                ProcessNoiseMagnitude = Mathf.Sqrt((cleanVel / Max_Linear_Speed) * (cleanVel / Max_Linear_Speed) + (cleanRot / Max_Angular_Speed) * (cleanRot / Max_Angular_Speed));
            }

            if (Mathf.Abs(velEta * ProcessNoiseMagnitude) > Max_Linear_Speed)
            {
                currentSpeed = cleanVel + Mathf.Sign(velEta) * cleanVel;
            }
            else
            {
                currentSpeed = cleanVel + velEta * ProcessNoiseMagnitude * Max_Linear_Speed;
            }

            if (Mathf.Abs(rotEta * ProcessNoiseMagnitude) > Max_Angular_Speed)
            {
                currentRot = cleanRot + Mathf.Sign(rotEta) * cleanRot;
            }
            else
            {
                currentRot = cleanRot + rotEta * ProcessNoiseMagnitude * Max_Angular_Speed;
            }
        }
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

    public float ZigguratGaussianSample()
    {
        byte[] bytes = new byte[8];
        rand.NextBytes(bytes);
        for (; ; )
        {
            ulong u = BitConverter.ToUInt64(bytes, 0);

            int s = (int)((u >> 3) & 0x7f);

            float sign = ((u & 0x400) == 0) ? 1.0f : -1.0f;

            ulong u2 = u >> 11;

            if (0 == s)
            {
                if (u2 < xcomp[0])
                {
                    return (float)(u2 * INCR * aDivY0 * sign);
                }
                return SampleTail() * sign;
            }

            if (u2 < xcomp[s])
            {
                return (float)(u2 * INCR * x[s] * sign);
            }

            float _x = (float)(u2 * INCR * x[s]);

            if ((y[s - 1] + ((y[s] - y[s - 1]) * rand.NextDouble()) < GaussianPDFDenorm(_x)))
            {
                return _x * sign;
            }
        }
    }

    public float GaussianPDFDenorm(float x)
    {
        return Mathf.Exp(-(x * x / 2.0f));
    }

    public float GaussianPDFDenormInv(float y)
    {
        return Mathf.Sqrt(-2.0f * Mathf.Log(y));
    }

    public float SampleTail()
    {
        float x, y;
        do
        {
            x = -Mathf.Log((float)rand.NextDouble()) / R;
            y = -Mathf.Log((float)rand.NextDouble());
        }
        while (y + y < x * x && (x == 0 || y == 0));
        return R + x;
    }

    void updateControlDynamics()
    {
        float alpha = Mathf.Exp(-Time.fixedDeltaTime / currentTau);
        float beta = (1.0f - alpha);
        cleanVel = alpha * prevCleanVel + Max_Linear_Speed * beta * -moveX;
        cleanRot = alpha * prevCleanRot + Max_Angular_Speed * beta * moveY;
        //print(RotSpeed);
        //print(cleanRot);
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
        float ObsNoiseMagnitude = Mathf.Sqrt((SharedJoystick.currentSpeed / SharedJoystick.Max_Linear_Speed) * (SharedJoystick.currentSpeed / SharedJoystick.Max_Linear_Speed)
            + (SharedJoystick.currentRot / SharedJoystick.Max_Angular_Speed) * (SharedJoystick.currentRot / SharedJoystick.Max_Angular_Speed));
        result_speed = zeta * ObsNoiseMagnitude * SharedJoystick.Max_Linear_Speed;
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
        float ObsNoiseMagnitude = Mathf.Sqrt((SharedJoystick.currentSpeed / SharedJoystick.Max_Linear_Speed) * (SharedJoystick.currentSpeed / SharedJoystick.Max_Linear_Speed)
            + (SharedJoystick.currentRot / SharedJoystick.Max_Angular_Speed) * (SharedJoystick.currentRot / SharedJoystick.Max_Angular_Speed));

        result_speed = zeta * ObsNoiseMagnitude * SharedJoystick.Max_Angular_Speed;
        prevRotObsEps = epsilon;
        prevRotObsZet = zeta;

        return result_speed;
    }

    public void ReadXYInputCont()
    {
        StreamReader strReader = new StreamReader("replay_cont_path");
        bool endoffile = false;
        string data_string = strReader.ReadLine();
        while (!endoffile)
        {
            data_string = strReader.ReadLine();
            if (data_string == null)
            {
                break;
            }
            var data_values = data_string.Split(',');
            Tuple<float, float> New_Coord_Tuple;
            float x = float.Parse(data_values[41], CultureInfo.InvariantCulture.NumberFormat);
            float y = float.Parse(data_values[42], CultureInfo.InvariantCulture.NumberFormat);
            New_Coord_Tuple = new Tuple<float, float>(x, y);
            XYInputList.Add(New_Coord_Tuple);
        }
    }
}