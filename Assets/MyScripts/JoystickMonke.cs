using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO.Ports;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine.InputSystem.LowLevel;

public class JoystickMonke : MonoBehaviour
{
    wrmhl joystick = new wrmhl();

    public bool usingArduino;

    [Tooltip("SerialPort of your device.")]
    public string portName = "COM3";

    [Tooltip("Baudrate")]
    public int baudRate = 1000000;

    [Tooltip("Timeout")]
    public int ReadTimeout = 5000;

    [Tooltip("QueueLength")]
    public int QueueLength = 1;

    private float ptbVelMin;
    private float ptbVelMax;
    private float ptbRotMin;
    private float ptbRotMax;

    public static JoystickMonke SharedJoystick;

    public float moveX;
    public float moveY;
    public int press;
    [ShowOnly] public float currentSpeed = 0.0f;
    [ShowOnly] public float currentRot = 0.0f;
    private float currentSpeedPtb = 0.0f;
    private float currentRotPtb = 0.0f;
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

    public bool ptb = false;

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
    public float timeConstant;
    [HideInInspector]
    public float filterTau;
    [HideInInspector]
    public float velFilterGain;
    [HideInInspector]
    public float rotFilterGain;
    [HideInInspector]
    public float gamma;
    [HideInInspector]
    public float meanNoise;
    [HideInInspector]
    public float stdDevNoise;
    [HideInInspector]
    public float logSample = 1f;

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
    public List<float> taus = new List<float>();

    public float rawX;
    public float rawY;

    CTIJoystick USBJoystick;
    float prevX = 0.0f;
    float prevY = 0.0f;

    // Start is called before the first frame update
    void Awake()
    {
        SharedJoystick = this;
    }

    void Start()
    {
        //MaxSpeed = 0.0f;
        //RotSpeed = 0.0f;
        ptbVelMin = PlayerPrefs.GetFloat("Perturb Velocity Min");
        ptbVelMax = PlayerPrefs.GetFloat("Perturb Velocity Max");
        ptbRotMin = PlayerPrefs.GetFloat("Perturb Rotation Min");
        ptbRotMax = PlayerPrefs.GetFloat("Perturb Rotation Max");
        seed = UnityEngine.Random.Range(1, 10000);
        rand = new System.Random(seed);

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
        ptb = (int)PlayerPrefs.GetFloat("PTBType") != 2;
        meanDist = PlayerPrefs.GetFloat("Mean Distance");
        meanTime = PlayerPrefs.GetFloat("Mean Time");
        meanAngle = 3.0f * PlayerPrefs.GetFloat("Max Angle");
        flagPTBType = (int)PlayerPrefs.GetFloat("PTBType");
        minTau = PlayerPrefs.GetFloat("MinTau");
        maxTau = PlayerPrefs.GetFloat("Max Tau");
        numTau = (int)PlayerPrefs.GetFloat("NumTau");
        timeConstant = PlayerPrefs.GetFloat("TauTau");
        filterTau = PlayerPrefs.GetFloat("FilterTau");
        velFilterGain = PlayerPrefs.GetFloat("VelocityNoiseGain");
        rotFilterGain = PlayerPrefs.GetFloat("RotationNoiseGain");

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
        gamma = Mathf.Exp(-1f / timeConstant);

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
                meanNoise = 0.5f * (Mathf.Log(minTau) + Mathf.Log(maxTau));
                stdDevNoise = 0.5f * (meanNoise - Mathf.Log(minTau));
                meanLogSpace = meanNoise * (1.0f - gamma);
                stdDevLogSpace = stdDevNoise * Mathf.Sqrt(1.0f - (gamma * gamma));
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

            //save filtered joystick X & Y
            rawX = moveX;
            rawY = moveY;

            //Akis PTB noise
            if (ptb)
            {
                ProcessNoise();
                //print(currentSpeed);
            }
            else
            {
                if (moveX > 0.11f || moveX < -0.11f)
                {
                    currentSpeed = -moveX * MaxSpeed;
                }
                else
                {
                    currentSpeed = 0.0f;
                }
                
                if (moveY > 0.11f || moveY < -0.11f)
                {
                    currentRot = moveY * RotSpeed;
                    //sumx += moveX * RotSpeed;
                    //period++;
                }
                else
                {
                    currentRot = 0.0f;
                    //sumx += 0.0f;
                    //period++;
                }
                //if (period == 4)
                //{
                //    currentRot = sumx / period;
                //    sumx = 0.0f;
                //    period = 0;
                //}
                currentSpeedPtb = currentSpeed;
                currentRotPtb = currentRot;
            }
            transform.position = transform.position + transform.forward * currentSpeed * Time.fixedDeltaTime;
            transform.Rotate(0f, currentRot * Time.fixedDeltaTime, 0f);
            //print(string.Format("{0},{1}",moveX, moveY));
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
        if (rand.NextDouble() > gamma)
        {
            var temp = currentTau;

            do
            {
                currentTau = taus[rand.Next(0, taus.Count)];
            } while (temp == currentTau);
        }

        //print(currentTau);

        CalculateMaxValues();
    }

    public void ContinuousTau()
    {
        logSample = gamma * logSample + ((stdDevLogSpace * BoxMullerGaussianSample()) + meanLogSpace);
        currentTau = Mathf.Exp(logSample);

        //print(currentTau);

        CalculateMaxValues();
    }

    private void CalculateMaxValues()
    {
        MaxSpeed = (meanDist / meanTime) * (1.0f / (-1.0f + (2 * (currentTau / meanTime)) * Mathf.Log((1 + Mathf.Exp(meanTime / currentTau)) / 2.0f)));
        RotSpeed = (meanAngle / meanTime) * (1.0f / (-1.0f + (2 * (currentTau / meanTime)) * Mathf.Log((1 + Mathf.Exp(meanTime / currentTau)) / 2.0f)));
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
        float alpha = Mathf.Exp(-Time.fixedDeltaTime / currentTau);
        float beta = (1.0f - alpha);
        float gamma = Mathf.Exp(-Time.fixedDeltaTime / filterTau);
        float delta = (1.0f - gamma);

        //print(string.Format("{0},{1},{2},{3}", alpha, beta, delta, gamma));
        //print(currentTau);

        velKsi = gamma * prevVelKsi + delta * BoxMullerGaussianSample();
        velEta = gamma * prevVelEta + delta * velFilterGain * velKsi;
        cleanVel = alpha * prevCleanVel + MaxSpeed * beta * moveY;
        prevCleanVel = cleanVel;
        prevVelKsi = velKsi;
        prevVelEta = velEta;

        rotKsi = gamma * prevRotKsi + delta * BoxMullerGaussianSample();
        rotEta = gamma * prevRotEta + delta * rotFilterGain * rotKsi;
        cleanRot = alpha * prevCleanRot + RotSpeed * beta * moveX;
        prevCleanRot = cleanRot;
        prevRotKsi = rotKsi;
        prevRotEta = rotEta;

        //currentSpeed = Mathf.Sign(velEta) * Mathf.Abs((1.0f + velEta) * cleanVel);
        //currentRot = Mathf.Sign(velEta) * Mathf.Abs((1.0f + rotEta) * cleanRot);

        currentSpeed = cleanVel + Mathf.Sign(velEta) * Mathf.Abs(velEta * cleanVel);
        currentRot = cleanRot + Mathf.Sign(velEta) * Mathf.Abs(rotEta * cleanRot);
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
}