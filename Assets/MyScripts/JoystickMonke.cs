using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO.Ports;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine.InputSystem.LowLevel;
using static Monkey2D;

public class JoystickMonke : MonoBehaviour
{
    wrmhl joystick = new wrmhl();

    public bool usingArduino;

    [Tooltip("SerialPort of your device.")]
    public string portName;

    [Tooltip("Baudrate")]
    public int baudRate = 1000000;

    [Tooltip("Timeout")]
    public int ReadTimeout = 5000;

    [Tooltip("QueueLength")]
    public int QueueLength = 1;

    public float ptbJoyVelMin;
    public float ptbJoyVelMax;
    public float ptbJoyVelStartRange;
    public float ptbJoyVelStart;
    public float ptbJoyVelMu;
    public float ptbJoyVelSigma;
    public float ptbJoyVelGain;
    public float ptbJoyVelEnd;
    public float ptbJoyVelLen;
    public float ptbJoyVelValue;


    public float ptbJoyRotMin;
    public float ptbJoyRotMax;
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

    public float ptbJoyRatio;
    public int ptbJoyOn;
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
    public float RotSpeed = 0.0f;
    public float MaxSpeed = 0.0f;

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

    public bool CtrlDynamicsFlag = false;

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
    public int flagPTBType;
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

    public float JstLinearThreshold;
    public float JstAngularThreshold;

    // Start is called before the first frame update
    void Awake()
    {
        SharedJoystick = this;
    }

    void Start()
    {
        PlayerPrefs.SetFloat("FixedYSpeed", 0);
        PlayerPrefs.SetFloat("MovingFFmode", 0);
        portName = PlayerPrefs.GetString("Port");
        ProcessNoiseFlag = PlayerPrefs.GetInt("isProcessNoise") == 1;
        //MaxSpeed = 0.0f;
        //RotSpeed = 0.0f;

        seed = UnityEngine.Random.Range(1, 10000);
        rand = new System.Random(seed);

        ptbJoyOn = PlayerPrefs.GetInt("Perturbation On");
        
        //ptbJoyVelMin = PlayerPrefs.GetFloat("Perturb Velocity Min");
        ptbJoyVelMax = PlayerPrefs.GetFloat("Perturb Velocity Max");
        ptbJoyVelMin = -ptbJoyVelMax;

        //ptbJoyRotMin = PlayerPrefs.GetFloat("Perturb Rotation Min");
        ptbJoyRotMax = PlayerPrefs.GetFloat("Perturb Rotation Max");
        ptbJoyRotMin = -ptbJoyRotMax;

        ptbJoyRatio = PlayerPrefs.GetFloat("PerturbRatio"); //0.5f;

        ptbJoyVelStartRange = 1.0f;
        ptbJoyVelSigma = 0.2f;
        ptbJoyVelLen = 1.0f;

        ptbJoyRotStartRange = 1.0f;
        ptbJoyRotSigma = 0.2f;
        ptbJoyRotLen = 1.0f;

        //ptbJoyFlagTrial = Convert.ToInt32(rand.NextDouble() <= ptbJoyRatio);

        //ptbJoyVelStart;
        //ptbJoyVelMu;        
        //ptbJoyVelGain;
        //ptbJoyVelEnd;        
        //ptbJoyVelValue;
        //ptbJoyFlag;

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

        //load Akis PTB vars
        CtrlDynamicsFlag = (int)PlayerPrefs.GetFloat("PTBType") != 2;
        meanDist = PlayerPrefs.GetFloat("MeanDistance");
        meanTime = PlayerPrefs.GetFloat("MeanTime");
        //Used to be
        //meanAngle = 3.0f * PlayerPrefs.GetFloat("Max Angle");
        meanAngle = PlayerPrefs.GetFloat("MeanAngle");
        flagPTBType = (int)PlayerPrefs.GetFloat("PTBType");
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
        JstLinearThreshold = PlayerPrefs.GetFloat("LinearThreshold");
        JstAngularThreshold = PlayerPrefs.GetFloat("AngularThreshold");
        float velStopThreshold = PlayerPrefs.GetFloat("velStopThreshold");
        float rotStopThreshold = PlayerPrefs.GetFloat("rotStopThreshold");

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

        //Akis PTB set up
        kappa = Mathf.Exp(-1f / TauTau);

        switch (flagPTBType)
        {
            case 0:
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

            case 1:
                meanTau = 0.5f * (Mathf.Log(minTau) + Mathf.Log(maxTau));
                stdDevTau = 0.5f * (meanTau - Mathf.Log(minTau));
                meanLogSpace = meanTau * (1.0f - kappa);
                stdDevLogSpace = stdDevTau * Mathf.Sqrt(1.0f - (kappa * kappa));
                //print(string.Format("muPhi = {0}, sigPhi = {1}, muEta = {2}, sigEta = {3}", meanNoise, stdDevNoise, meanLogSpace, stdDevLogSpace));
                break;

            case 2:
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
            catch (Exception e)
            {
                // It's gonna be the same exception everytime, but I'm purposely doing this.
                // It's just that this code will read serial in faster than it's actually
                // coming in so there'll be an error saying there's no object or something
                // like that.
            }
            //print(line);
        }
        else
        {
            moveX = -USBJoystick.x.ReadValue();
            moveY = -USBJoystick.y.ReadValue();

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

            //save filtered joystick X & Y
            rawX = moveY;
            rawY = -moveX;
            //print(CtrlDynamicsFlag);
            //Akis PTB noise
            //print(SharedMonkey.isAccelControlTrial);
            //print(savedTau);
            if (CtrlDynamicsFlag)
            {
                updateControlDynamics();
                if (SharedMonkey.isIntertrail)
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
                    //print("stoping");
                    //moveX = 0;
                    //moveY = 0;
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
                /*if (Mathf.Abs(moveX) > JstLinearThreshold)
                {
                    currentSpeed = -moveX * MaxSpeed;
                }
                else
                {
                    currentSpeed = 0.0f;
                }
                
                if (Mathf.Abs(moveY) > JstAngularThreshold)
                {
                    currentRot = moveY * RotSpeed;
                }
                else
                {
                    currentRot = 0.0f;
                }*/
                currentSpeed = -moveX * MaxSpeed;
                currentRot = moveY * RotSpeed;
                cleanVel = currentSpeed;
                cleanRot = currentRot;
                velProcessNoiseGain = PlayerPrefs.GetFloat("VelocityNoiseGain");
                rotProcessNoiseGain = PlayerPrefs.GetFloat("RotationNoiseGain");
                ProcessNoise();
                speedPrePtb = currentSpeed;
                rotPrePtb = currentRot;

                if (ptbJoyOn <= 0)
                {
                    //print("NoptbJoy");                    
                }
                else
                {
                    if (ptbJoyFlagTrial > 0) 
                    {

                        //Monkey2D.Phase;                
                        if (SharedMonkey.phase == Phases.check)
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
                                //timeOnsetJoy = timeCounter;
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
                            //timeCntPTBStart = 0.0f;
                            ptbJoyVelValue = 0.0f;
                            ptbJoyRotValue = 0.0f;
                            ptbJoyFlag = 0;
                            //ptbJoyVelGain = 0.0f;
                            //ptbJoyRotGain = 0.0f;
                        }

                        currentSpeed = currentSpeed + ptbJoyVelValue;
                        currentRot = currentRot + ptbJoyRotValue;
                    }
                }
                
            }

            //print(string.Format("current speed:{0}", currentSpeed));
            //print(string.Format("current rotation:{0}", currentRot));
            if (SharedMonkey.isIntertrail && SharedMonkey.phase == Phases.check)
                {
                    print("stopping");
                    currentSpeed = 0;
                    currentRot = 0;
                }
            transform.position = transform.position + transform.forward * currentSpeed * Time.fixedDeltaTime;
            transform.Rotate(0f, currentRot * Time.fixedDeltaTime, 0f);
        }
    }

    void Update()
    {

    }

    public void OnApplicationQuit()
    {
        //string firstLine = "t,rawX,rawY,v,w,v_ptb,w_ptb";

        //File.AppendAllText("C:/Users/jc10487/Documents/joydata.csv", firstLine + "\n");

        //List<int> temp = new List<int>()
        //{
        //    t.Count,
        //    rawX.Count,
        //    rawY.Count,
        //    v.Count,
        //    w.Count,
        //    vAddPtb.Count,
        //    wAddPtb.Count
        //};
        //temp.Sort();

        //for (int i = 0; i < temp[0]; i++)
        //{
        //    var line = string.Format("{0},{1},{2},{3},{4},{5},{6}", t[i], rawX[i], rawY[i], v[i], w[i], vAddPtb[i], wAddPtb[i]);
        //    File.AppendAllText("C:/Users/jc10487/Documents/joydata.csv", line + "\n");
        //}
    }

    private void calcPtbJoyTrial()
    {
        // Linear
        ptbJoyVelStart = (float) rand.NextDouble() * ptbJoyVelStartRange;
        ptbJoyVelMu = ptbJoyVelStart + (ptbJoyVelLen / 2);
        ptbJoyVelEnd = ptbJoyVelStart + ptbJoyVelLen;
        ptbJoyVelGain = (float) rand.NextDouble() * (ptbJoyVelMax - ptbJoyVelMin) + ptbJoyVelMin;

        //Angular
        ptbJoyRotStart = ptbJoyVelStart;// (float)rand.NextDouble() * ptbJoyRotStartRange;
        ptbJoyRotMu = ptbJoyVelMu;// ptbJoyRotStart + (ptbJoyRotLen / 2);
        ptbJoyRotEnd = ptbJoyVelEnd;// ptbJoyRotStart + ptbJoyRotLen;
        ptbJoyRotGain = (float)rand.NextDouble() * (ptbJoyRotMax - ptbJoyRotMin) + ptbJoyRotMin;

        //timeCntPTBStart = 0.0f;
    }

    private float GaussianShapedPtb(float t, float mu, float sigma, float gain)
    {
        //float sigma = 0.2f;
        //float maxNum = (1.0f / Math.Sqrt(2.0f * sigma));// * Math.Exp(-0.5f * Math.Pow((t - mu) / sigma, 2));
        //return (1.0f / Math.Sqrt(2.0f * sigma)) * Math.Exp(-0.5f * Math.Pow((t - mu) / sigma, 2));
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
        MaxSpeed = (meanDist / meanTime) * (1.0f / (-1.0f + (2 * (savedTau / meanTime)) * Mathf.Log((1 + Mathf.Exp(meanTime / savedTau)) / 2.0f)));
        RotSpeed = (meanAngle / meanTime) * (1.0f / (-1.0f + (2 * (savedTau / meanTime)) * Mathf.Log((1 + Mathf.Exp(meanTime / savedTau)) / 2.0f)));
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
            if (RotSpeed == 0)
            {
                ProcessNoiseMagnitude = Mathf.Sqrt((cleanVel / MaxSpeed) * (cleanVel / MaxSpeed));
            }
            else
            {
                ProcessNoiseMagnitude = Mathf.Sqrt((cleanVel / MaxSpeed) * (cleanVel / MaxSpeed) + (cleanRot / RotSpeed) * (cleanRot / RotSpeed));
            }

            if (Mathf.Abs(velEta * ProcessNoiseMagnitude) > MaxSpeed)
            {
                currentSpeed = cleanVel + Mathf.Sign(velEta) * cleanVel;
            }
            else
            {
                currentSpeed = cleanVel + velEta * ProcessNoiseMagnitude * MaxSpeed;
            }

            if (Mathf.Abs(rotEta * ProcessNoiseMagnitude) > RotSpeed)
            {
                currentRot = cleanRot + Mathf.Sign(rotEta) * cleanRot;
            }
            else
            {
                currentRot = cleanRot + rotEta * ProcessNoiseMagnitude * RotSpeed;
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
        cleanVel = alpha * prevCleanVel + MaxSpeed * beta * -moveX;
        cleanRot = alpha * prevCleanRot + RotSpeed * beta * moveY;
        //print(RotSpeed);
        //print(cleanRot);
    }
}