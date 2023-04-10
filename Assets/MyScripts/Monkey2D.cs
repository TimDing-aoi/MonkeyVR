///////////////////////////////////////////////////////////////////////////////////////////
///                                                                                     ///
/// Monkey2D.cs                                                                         ///
/// by Joshua Calugay and Tim Ding                                                      ///
/// hd840@nyu.edu                                                                       ///
/// jcal1696@gmail.com                                                                  ///
/// For the Angelaki Lab                                                                ///
///                                                                                     ///
/// <summary>                                                                           ///
/// This script takes care of the FF behavior.                                          ///
/// </summary>                                                                          ///
///////////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static JoystickMonke;
using static JoystickDrunk;
using static Serial;
using static Particles;
using static Particles2;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using System.IO.Ports;
using System.Globalization;
using PupilLabs;
using System.Linq;

public class Monkey2D : MonoBehaviour
{
    public static Monkey2D SharedMonkey;

    public GameObject firefly;
    private float timeCounter = 0;
    //public GameObject marker;
    //public GameObject panel;
    public Camera Lcam;
    public Camera Rcam;
    public Camera LObscam;
    public Camera RObscam;
    public Camera DrunkCam;
    public GameObject FP;
    [HideInInspector] public GazeVisualizer gazeVisualizer;
    //public GameObject Marker;
    // public GameObject inner;
    // public GameObject outer;
    //wrmhl juiceBox = new wrmhl();

    [Tooltip("Baudrate")]
    [HideInInspector] public int baudRate = 2000000;

    [Tooltip("Timeout")]
    [HideInInspector] public int ReadTimeout = 5000;

    [Tooltip("QueueLength")]
    [HideInInspector] public int QueueLength = 1;
    [Tooltip("Diameter of firefly")]
    [HideInInspector] public float fireflySize;
    [Tooltip("Maximum distance allowed from center of firefly")]
    [HideInInspector] public float fireflyZoneRadius;
    // Enumerable experiment mode selector
    private enum Modes
    {
        ON,
        Flash,
        Fixed
    }
    private Modes mode;
    // Toggle for whether trial is an always on trial or not
    public bool toggle;
    // Toggle for self motion
    public bool motion_toggle = false;

    [Tooltip("Ratio of trials that will have fireflies always on")]
    [HideInInspector] public float ratio;
    [Tooltip("Frequency of flashing firefly (Flashing Firefly Only)")]
    [HideInInspector] public float freq;
    [Tooltip("Duty cycle for flashing firefly (percentage of one period determing how long it stays on during one period) (Flashing Firefly Only)")]
    [HideInInspector] public float duty;
    // Pulse Width; how long in seconds it stays on during one period
    private float PW;
    public GameObject player;
    public GameObject drunkplayer;
    public AudioSource audioSource;
    public AudioClip winSound;
    public AudioClip neutralSound;
    public AudioClip loseSound;
    [Tooltip("Minimum distance firefly can spawn")]
    [HideInInspector] public float minDrawDistance;
    [Tooltip("Maximum distance firefly can spawn")]
    [HideInInspector] public float maxDrawDistance;
    [Tooltip("Ranges for which firefly n spawns inside")]
    [HideInInspector] public List<float> ranges = new List<float>();
    [Tooltip("Minimum angle from forward axis that firefly can spawn")]
    [HideInInspector] public float minPhi;
    [Tooltip("Maximum angle from forward axis that firefly can spawn")]
    [HideInInspector] public float maxPhi;
    [Tooltip("Indicates whether firefly spawns more on the left or right; < 0.5 means more to the left, > 0.5 means more to the right, = 0.5 means equally distributed between left and right")]
    [HideInInspector] public float LR;
    [Tooltip("How long the firefly stays from the beginning of the trial (Fixed Firefly Only)")]
    [HideInInspector] public float lifeSpan;
    [Tooltip("How many fireflies can appear at once")]
    [HideInInspector] public float nFF;
    int multiMode;
    readonly public List<float> velocities = new List<float>();
    readonly public List<float> v_ratios = new List<float>();
    readonly public List<float> colorratios = new List<float>();
    readonly public List<float> colorrewards = new List<float>();
    private List<float> colorchosen = new List<float>();
    private float colorhit = 0;
    readonly public List<float> v_noises = new List<float>();
    readonly public List<Vector3> directions = new List<Vector3>()
    {
        Vector3.left,
        Vector3.right,
        Vector3.forward,
        Vector3.back
    };
    readonly public List<float> durations = new List<float>();
    readonly public List<float> ratios = new List<float>();
    private bool isMoving;
    private bool LRFB;
    private float movingFFmode;
    public GameObject line;
    [HideInInspector] public int lineOnOff = 1;

    private Vector3 move;
    [Tooltip("Trial timeout (how much time player can stand still before trial ends")]
    [HideInInspector] public float timeout;
    [Tooltip("Minimum x value to plug into exponential distribution from which time to wait before check is pulled")]
    [HideInInspector] public float checkMin;
    [Tooltip("Maximum x value to plug into exponential distribution from which time to wait before check is pulled")]
    [HideInInspector] public float checkMax;
    [Tooltip("Minimum x value to plug into exponential distribution from which time to wait before new trial is pulled")]
    [HideInInspector] public float interMax;
    [Tooltip("Maximum x value to plug into exponential distribution from which time to wait before new trial is pulled")]
    [HideInInspector] public float interMin;
    [Tooltip("Player height")]
    [HideInInspector] public float p_height;
    // 1 / Mean for check time exponential distribution
    private float c_lambda;
    // 1 / Mean for intertrial time exponential distribution
    private float i_lambda;
    // x values for exponential distribution
    private float c_min;
    private float c_max;
    private float i_min;
    private float i_max;
    private float velMin;
    private float velMax;
    private float rotMin;
    private float rotMax;
    public enum Phases
    {
        begin = 0,
        trial = 1,
        check = 2,
        //question = 3,
        juice = 3,
        ITI = 4,
        none = 9,
    }
    [HideInInspector] public Phases phase;

    private Vector3 pPos;
    private bool isTimeout = false;

    // Trial number
    readonly List<int> n = new List<int>();

    // Firefly ON Duration
    readonly List<float> onDur = new List<float>();

    //Firefly Colors
    readonly List<string> ffCol = new List<string>();

    // Firefly Check Coords
    readonly List<string> ffPos = new List<string>();
    public readonly List<Vector3> ffPositions = new List<Vector3>();

    // Player position at Check()
    readonly List<string> cPos = new List<string>();
    readonly List<string> cPosTemp = new List<string>();

    // Player rotation at Check()
    readonly List<string> cRot = new List<string>();
    readonly List<string> cRotTemp = new List<string>();

    // Player origin at beginning of trial
    readonly List<string> origin = new List<string>();

    // Player rotation at origin
    readonly List<string> heading = new List<string>();

    // Player linear and angular velocity
    readonly List<float> max_v = new List<float>();
    readonly List<float> max_w = new List<float>();

    //PTBTau
    readonly List<float> CurrentTau = new List<float>();

    // Firefly velocity
    readonly List<float> fv = new List<float>();

    // Distances from player to firefly
    readonly List<string> dist = new List<string>();
    readonly List<float> distances = new List<float>();

    // Times
    readonly List<float> beginTime = new List<float>();
    readonly List<float> checkTime = new List<float>();
    readonly List<string> checkTimeStrList = new List<string>();
    string checkTimeString;
    readonly List<float> rewardTime = new List<float>();
    readonly List<float> juiceDuration = new List<float>();
    readonly List<float> endTime = new List<float>();
    readonly List<float> checkWait = new List<float>();
    readonly List<float> interWait = new List<float>();
    // add when firefly disappears

    // density
    readonly List<float> densities = new List<float>();
    readonly List<float> densities_obsRatio = new List<float>();

    // Rewarded?
    readonly List<int> score = new List<int>();

    // Stimulated?
    private float stimuratio;
    private bool stimulatedTrial = false;
    readonly List<int> stimulated = new List<int>();
    readonly List<float> timeStimuStart = new List<float>();
    readonly List<float> trialStimuDur = new List<float>();

    // Think the FF moves or not?
    //readonly List<int> answer = new List<int>();
    [HideInInspector] public bool isQuestion = false;

    // Timed Out?
    readonly List<int> timedout = new List<int>();

    // Was Always ON?
    readonly List<bool> alwaysON = new List<bool>();

    // JoyStick PTB
    readonly List<float> timeCntPTBStart = new List<float>();    

    readonly List<float> ptbJoyVelMin = new List<float>();
    readonly List<float> ptbJoyVelMax = new List<float>();
    readonly List<float> ptbJoyVelStartRange = new List<float>();
    readonly List<float> ptbJoyVelStart = new List<float>();
    readonly List<float> ptbJoyVelMu = new List<float>();
    readonly List<float> ptbJoyVelSigma = new List<float>();
    readonly List<float> ptbJoyVelGain = new List<float>();
    readonly List<float> ptbJoyVelEnd = new List<float>();
    readonly List<float> ptbJoyVelLen = new List<float>();
    readonly List<float> ptbJoyVelValue = new List<float>();

    readonly List<float> ptbJoyRotMin = new List<float>();
    readonly List<float> ptbJoyRotMax = new List<float>();
    readonly List<float> ptbJoyRotStartRange = new List<float>();
    readonly List<float> ptbJoyRotStart = new List<float>();
    readonly List<float> ptbJoyRotMu = new List<float>();
    readonly List<float> ptbJoyRotSigma = new List<float>();
    readonly List<float> ptbJoyRotGain = new List<float>();
    readonly List<float> ptbJoyRotEnd = new List<float>();
    readonly List<float> ptbJoyRotLen = new List<float>();
    readonly List<float> ptbJoyRotValue = new List<float>();

    readonly List<int> ptbJoyFlag = new List<int>();
    readonly List<int> ptbJoyFlagTrial = new List<int>();

    readonly List<float> ptbJoyRatio = new List<float>();
    readonly List<int> ptbJoyOn = new List<int>();
    readonly List<float> ptbJoyEnableTime = new List<float>();
    

    [HideInInspector] public float FrameTimeElement = 0;

    [HideInInspector] public float delayTime = .2f;

    [HideInInspector] public bool Detector = false;
    //List<float> Frametime = new List<float>();

    [HideInInspector] public int LengthOfRay = 75;
    //[SerializeField] private LineRenderer GazeRayRenderer;

    [HideInInspector] public string sceneTypeVerbose;
    [HideInInspector] public string systemStartTimeVerbose;

    public static Vector3 hitpoint;

    // File paths
    private string path;

    [HideInInspector] public int trialNum;
    [HideInInspector] public float programT0 = 0.0f;

    [HideInInspector] public float points = 0;
    [Tooltip("How long the juice valve is open")]
    [HideInInspector] public float juiceTime;
    private float minJuiceTime;
    private float maxJuiceTime;

    [Tooltip("Maximum number of trials before quitting (0 for infinity)")]
    [HideInInspector] public int ntrials;

    private int seed;
    private System.Random rand;

    private bool on = true;

    private bool isBegin = false;
    private bool isTrial = false;
    private bool isCheck = false;
    private bool isEnd = false;
    public bool isIntertrail = false;
    private float startTime;
    private float MoveStartTime;

    [HideInInspector] public Phases currPhase;

    readonly private List<GameObject> pooledFF = new List<GameObject>();

    private readonly char[] toTrim = { '(', ')' };

    [HideInInspector] public float initialD = 0.0f;

    private Vector3 direction = new Vector3();
    [HideInInspector] public float velocity;
    [HideInInspector] public float noise_SD;
    [HideInInspector] public float velocity_Noised;

    [HideInInspector] public Vector3 player_origin;

    private string contPath;

    private float ipd;

    public ParticleSystem particle_System;
    public ParticleSystem particle_System2;
    private bool isObsNoise;
    public bool isProcessNoise;
    private float ObsNoiseTau;
    public float ObsVelocityNoiseGain;
    public float ObsRotationNoiseGain;
    private float ObsDensityRatio;

    private int loopCount = 0;

    CancellationTokenSource source;
    private Task currentTask;
    private Task flashTask;
    private bool playing = true;
    public bool stimulating = false;
    //private bool writing = true;

    public float offset = 0.01f;
    private float lm02;
    private float rm02;

    private Matrix4x4 lm;
    private Matrix4x4 rm;

    SerialPort juiceBox;

    float prevVel = 0.0f;
    float prevPrevVel = 0.0f;
    float tPrev = 0.0f;

    //observation noise
    public float DistFlowSpeed = 0;
    public float DistFlowRot = 0;

    float separation;

    bool proximity;
    bool isReward;
    string ffPosStr = "";

    //Perturbation
    int ptb;
    float velbrakeThresh;
    float rotbrakeThresh;
    float velStopThreshold;
    float rotStopThreshold;
    readonly List<float> rawX = new List<float>();
    readonly List<float> rawY = new List<float>();

    StringBuilder sb = new StringBuilder();
    [HideInInspector]
    public string sbPacket;
    bool flagMultiFF;
    double timeProgStart = 0.0f;

    private float microStimuDur;
    private float microStimuGap;
    private float trialStimuGap;

    bool SMtrial = false;

    readonly List<float> COMtrialtype = new List<float>();
    public bool isNormal = false;
    public bool isStatic2FF = false;
    public bool isCOM2FF = false;
    public bool isCOM = false;
    bool FF2shown = false;
    bool startedMoving = false;
    float COMlambda;
    float FF2delay;
    float normalRatio;
    float normal2FFRatio;
    float COM2FFRatio;
    int FF1index;
    readonly List<Tuple<float, float>> FFcoordsList = new List<Tuple<float, float>>();
    readonly List<Tuple<float, float>> FFvisibleList = new List<Tuple<float, float>>();
    readonly List<Tuple<float, float>> FFTagetMatchList = new List<Tuple<float, float>>();
    int trial_count = 0;
    private void Awake()
    {
        if(PlayerPrefs.GetFloat("calib") == 1)
        {
            gazeVisualizer = GameObject.Find("Gaze Visualizer").GetComponent<GazeVisualizer>();
        }
    }

    // Start is called before the first frame update
    /// <summary>
    /// From "GoToSettings.cs" you can see that I just hard-coded each of the key
    /// strings in order to retrieve the values associated with each key and
    /// assign them to their respective variable here. Also initialize some 
    /// variables depending on what mode is selected.
    /// 
    /// Catch exception if no mode detected from PlayerPrefs and default to Fixed
    /// 
    /// Set head tracking for VR headset OFF
    /// </summary>
    void OnEnable()
    {
        Application.targetFrameRate = 90;

        PlayerPrefs.SetFloat("FixedYSpeed", 0);
        PlayerPrefs.SetFloat("MovingFFmode", 0);
        Application.runInBackground = true;

        juiceBox = serial.sp;

        SendMarker("f", 1000.0f);
        programT0 = Time.realtimeSinceStartup;

        UnityEngine.XR.InputTracking.disablePositionalTracking = true;
        UnityEngine.XR.XRDevice.DisableAutoXRCameraTracking(LObscam, true);
        UnityEngine.XR.XRDevice.DisableAutoXRCameraTracking(RObscam, true);
        UnityEngine.XR.XRDevice.DisableAutoXRCameraTracking(Lcam, true);
        UnityEngine.XR.XRDevice.DisableAutoXRCameraTracking(Rcam, true);
        XRSettings.occlusionMaskScale = 10f;
        XRSettings.useOcclusionMesh = false;
        Lcam.ResetProjectionMatrix();
        Rcam.ResetProjectionMatrix();
        LObscam.ResetProjectionMatrix();
        RObscam.ResetProjectionMatrix();

        /*lm = Lcam.projectionMatrix;
        lm02 = lm.m02;
        rm = Rcam.projectionMatrix;
        rm02 = rm.m02;
        lm.m02 = lm02 + offset;
        Lcam.SetStereoProjectionMatrix(Camera.StereoscopicEye.Left, lm);
        Lcam.projectionMatrix = lm;
        rm.m02 = rm02 - offset;
        Rcam.SetStereoProjectionMatrix(Camera.StereoscopicEye.Right, rm);
        Rcam.projectionMatrix = rm;

        lm = LObscam.projectionMatrix;
        lm02 = lm.m02;
        rm = RObscam.projectionMatrix;
        rm02 = rm.m02;
        lm.m02 = lm02 + offset;
        LObscam.SetStereoProjectionMatrix(Camera.StereoscopicEye.Left, lm);
        LObscam.projectionMatrix = lm;
        rm.m02 = rm02 - offset;
        RObscam.SetStereoProjectionMatrix(Camera.StereoscopicEye.Right, rm);
        RObscam.projectionMatrix = rm;*/

        List<XRDisplaySubsystem> displaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances<XRDisplaySubsystem>(displaySubsystems);
        //print(XRSettings.loadedDeviceName);
        if (!XRSettings.enabled)
        {
            XRSettings.enabled = true;
        }

        SharedMonkey = this;

        timeout = PlayerPrefs.GetFloat("Timeout");
        path = PlayerPrefs.GetString("Path");
        ntrials = (int)PlayerPrefs.GetFloat("Num Trials");
        if (ntrials == 0) ntrials = 9999;
        seed = UnityEngine.Random.Range(1, 10000);
        rand = new System.Random(seed);
        p_height = PlayerPrefs.GetFloat("Player Height");
        c_lambda = 1.0f / PlayerPrefs.GetFloat("Mean 1");
        i_lambda = 1.0f / PlayerPrefs.GetFloat("Mean 2");
        checkMin = PlayerPrefs.GetFloat("Minimum Wait to Check");
        checkMax = PlayerPrefs.GetFloat("Maximum Wait to Check");
        interMin = PlayerPrefs.GetFloat("Minimum Intertrial Wait");
        interMax = PlayerPrefs.GetFloat("Maximum Intertrial Wait");
        c_min = Tcalc(checkMin, c_lambda);
        c_max = Tcalc(checkMax, c_lambda);
        i_min = Tcalc(interMin, i_lambda);
        i_max = Tcalc(interMax, i_lambda);
        velMin = PlayerPrefs.GetFloat("Min Linear Speed");
        velMax = PlayerPrefs.GetFloat("Max Linear Speed");
        rotMin = PlayerPrefs.GetFloat("Min Angular Speed");
        rotMax = PlayerPrefs.GetFloat("Max Angular Speed");
        nFF = PlayerPrefs.GetFloat("Number of Fireflies");
        multiMode = PlayerPrefs.GetInt("Multiple Firefly Mode");
        separation = PlayerPrefs.GetFloat("Separation");
        minDrawDistance = PlayerPrefs.GetFloat("Minimum Firefly Distance");
        maxDrawDistance = PlayerPrefs.GetFloat("Maximum Firefly Distance");
        microStimuDur = PlayerPrefs.GetFloat("StimuStimuDur");
        microStimuGap = PlayerPrefs.GetFloat("FFstimugap");
        stimuratio = PlayerPrefs.GetFloat("StimulationRatio");
        ptb = (int)PlayerPrefs.GetFloat("PTBType");
        isObsNoise = PlayerPrefs.GetInt("isObsNoise") == 1;
        ObsNoiseTau = PlayerPrefs.GetFloat("ObsNoiseTau");
        ObsVelocityNoiseGain = PlayerPrefs.GetFloat("ObsVelocityNoiseGain");
        ObsRotationNoiseGain = PlayerPrefs.GetFloat("ObsRotationNoiseGain");
        ObsDensityRatio = PlayerPrefs.GetFloat("ObsDensityRatio");
        SMtrial = PlayerPrefs.GetInt("isSM") == 1;
        isCOM = PlayerPrefs.GetInt("is2FFCOM") == 1;
        isProcessNoise = PlayerPrefs.GetInt("isProcessNoise") == 1;
        if (isCOM)
        {
            nFF = 2;
            FFcoordsList.Clear();
            ReadCoordCSV();
        }
        //ReadFFCoordDisc();
        normalRatio = PlayerPrefs.GetFloat("COMNormal");
        normal2FFRatio = PlayerPrefs.GetFloat("Sta2FF");
        COM2FFRatio = PlayerPrefs.GetFloat("COM2FF");
        normal2FFRatio += normalRatio;
        COM2FFRatio += normal2FFRatio;

        if (isObsNoise)
        {
            print("Activating Observe Noise");
            var em = particle_System2.emission;
            em.enabled = true;
            drunkplayer.SetActive(true);
        }
        else
        {
            var em = particle_System2.emission;
            em.enabled = false;
            drunkplayer.SetActive(false);
        }

        if (ptb != 2)
        {
            velStopThreshold = PlayerPrefs.GetFloat("velStopThreshold");
            rotStopThreshold = PlayerPrefs.GetFloat("rotStopThreshold");
        }
        else
        {
            velStopThreshold = 1.0f;
            rotStopThreshold = 1.0f;
        }

        if (nFF > 1 && PlayerPrefs.GetInt("isColored") == 1)
        {
            multiMode = 2;
        }

        if (isCOM)
        {
            ffPositions.Add(Vector3.zero);
            ffPositions.Add(Vector3.zero);
        }
        else if (nFF > 1 && multiMode == 1)
        {
            ranges.Add(minDrawDistance);
            ffPositions.Add(Vector3.zero);

            if (nFF < 3)
            {
                ranges.Add(PlayerPrefs.GetFloat("Range One"));
                ffPositions.Add(Vector3.zero);
            }

            if (nFF >= 3 && nFF < 4)
            {
                ranges.Add(PlayerPrefs.GetFloat("Range Two"));
                ffPositions.Add(Vector3.zero);
            }

            if (nFF >= 4 && nFF < 5)
            {
                ranges.Add(PlayerPrefs.GetFloat("Range Three"));
                ffPositions.Add(Vector3.zero);
            }

            if (nFF >= 5 && nFF < 6)
            {
                ranges.Add(PlayerPrefs.GetFloat("Range Four"));
                ffPositions.Add(Vector3.zero);
            }

            ranges.Add(maxDrawDistance);
        }
        else if (nFF > 1 && multiMode == 2)
        {
            for (int i = 0; i < nFF; i++)
            {
                ffPositions.Add(Vector3.zero);
            }
        }

        //foreach (float range in ranges)
        //{
        //    print(range);
        //}

        minJuiceTime = PlayerPrefs.GetFloat("Min Juice Time");
        maxJuiceTime = PlayerPrefs.GetFloat("Max Juice Time");
        LR = 0.5f;
        if (LR == 0.5f)
        {
            maxPhi = PlayerPrefs.GetFloat("Max Angle");
            minPhi = -maxPhi;
        }
        else
        {
            maxPhi = PlayerPrefs.GetFloat("Max Angle");
            minPhi = PlayerPrefs.GetFloat("Min Angle");
        }
        fireflyZoneRadius = PlayerPrefs.GetFloat("Reward Zone Radius");
        fireflySize = PlayerPrefs.GetFloat("RadiusFF") * 2;
        firefly.transform.localScale = new Vector3(fireflySize, fireflySize, 1);
        ratio = PlayerPrefs.GetFloat("Ratio");

        velocities.Add(PlayerPrefs.GetFloat("V1"));
        velocities.Add(PlayerPrefs.GetFloat("V2"));
        velocities.Add(PlayerPrefs.GetFloat("V3"));
        velocities.Add(PlayerPrefs.GetFloat("V4"));
        velocities.Add(PlayerPrefs.GetFloat("V5"));
        velocities.Add(PlayerPrefs.GetFloat("V6"));
        velocities.Add(PlayerPrefs.GetFloat("V7"));
        velocities.Add(PlayerPrefs.GetFloat("V8"));
        velocities.Add(PlayerPrefs.GetFloat("V9"));
        velocities.Add(PlayerPrefs.GetFloat("V10"));
        velocities.Add(PlayerPrefs.GetFloat("V11"));
        velocities.Add(PlayerPrefs.GetFloat("V12"));

        v_ratios.Add(PlayerPrefs.GetFloat("VR1"));
        v_ratios.Add(PlayerPrefs.GetFloat("VR2"));
        v_ratios.Add(PlayerPrefs.GetFloat("VR3"));
        v_ratios.Add(PlayerPrefs.GetFloat("VR4"));
        v_ratios.Add(PlayerPrefs.GetFloat("VR5"));
        v_ratios.Add(PlayerPrefs.GetFloat("VR6"));
        v_ratios.Add(PlayerPrefs.GetFloat("VR7"));
        v_ratios.Add(PlayerPrefs.GetFloat("VR8"));
        v_ratios.Add(PlayerPrefs.GetFloat("VR9"));
        v_ratios.Add(PlayerPrefs.GetFloat("VR10"));
        v_ratios.Add(PlayerPrefs.GetFloat("VR11"));
        v_ratios.Add(PlayerPrefs.GetFloat("VR12"));

        v_noises.Add(PlayerPrefs.GetFloat("VN1"));
        v_noises.Add(PlayerPrefs.GetFloat("VN2"));
        v_noises.Add(PlayerPrefs.GetFloat("VN3"));
        v_noises.Add(PlayerPrefs.GetFloat("VN4"));
        v_noises.Add(PlayerPrefs.GetFloat("VN5"));
        v_noises.Add(PlayerPrefs.GetFloat("VN6"));
        v_noises.Add(PlayerPrefs.GetFloat("VN7"));
        v_noises.Add(PlayerPrefs.GetFloat("VN8"));
        v_noises.Add(PlayerPrefs.GetFloat("VN9"));
        v_noises.Add(PlayerPrefs.GetFloat("VN10"));
        v_noises.Add(PlayerPrefs.GetFloat("VN11"));
        v_noises.Add(PlayerPrefs.GetFloat("VN12"));

        for (int i = 1; i < 12; i++)
        {
            v_ratios[i] = v_ratios[i] + v_ratios[i - 1];
        }

        durations.Add(PlayerPrefs.GetFloat("D1"));
        durations.Add(PlayerPrefs.GetFloat("D2"));
        durations.Add(PlayerPrefs.GetFloat("D3"));
        durations.Add(PlayerPrefs.GetFloat("D4"));
        durations.Add(PlayerPrefs.GetFloat("D5"));

        ratios.Add(PlayerPrefs.GetFloat("R1"));
        ratios.Add(PlayerPrefs.GetFloat("R2"));
        ratios.Add(PlayerPrefs.GetFloat("R3"));
        ratios.Add(PlayerPrefs.GetFloat("R4"));
        ratios.Add(PlayerPrefs.GetFloat("R5"));

        for (int i = 1; i < 5; i++)
        {
            ratios[i] = ratios[i] + ratios[i - 1];
        }

        isMoving = PlayerPrefs.GetInt("Moving ON") == 1;
        LRFB = PlayerPrefs.GetInt("VertHor") == 0;
        movingFFmode = PlayerPrefs.GetFloat("MovingFFmode");

        if (movingFFmode > 0)
        {
            lineOnOff = 1;//(int)PlayerPrefs.GetFloat("Line OnOff");
        }
        else
        {
            lineOnOff = 0;
        }
        line.transform.localScale = new Vector3(10000f, 0.125f * p_height * 10, 1);
        if (lineOnOff == 1)
        {
            line.SetActive(true);
        }
        else
        {
            line.SetActive(false);
        }

        drawLine(30, 200);

        if (PlayerPrefs.GetInt("isColored") == 1)
        {
            colorratios.Add(PlayerPrefs.GetFloat("red"));
            colorratios.Add(PlayerPrefs.GetFloat("blue"));
            colorratios.Add(PlayerPrefs.GetFloat("green"));
            colorratios.Add(PlayerPrefs.GetFloat("yellow"));
            colorratios.Add(PlayerPrefs.GetFloat("white"));
            for (int i = 1; i < 5; i++)
            {
                colorratios[i] = colorratios[i] + colorratios[i - 1];
            }

            colorrewards.Add(PlayerPrefs.GetFloat("redrew"));
            colorrewards.Add(PlayerPrefs.GetFloat("bluerew"));
            colorrewards.Add(PlayerPrefs.GetFloat("greenrew"));
            colorrewards.Add(PlayerPrefs.GetFloat("yellowrew"));
            colorrewards.Add(PlayerPrefs.GetFloat("whiterew"));
        }

        try
        {
            switch (PlayerPrefs.GetString("Switch Behavior"))
            {
                case "always on":
                    mode = Modes.ON;
                    break;
                case "flashing":
                    mode = Modes.Flash;
                    freq = PlayerPrefs.GetFloat("Frequency");
                    duty = PlayerPrefs.GetFloat("Duty Cycle") / 100f;
                    PW = duty / freq;
                    break;
                case "fixed":
                    mode = Modes.Fixed;
                    break;
                default:
                    throw new System.Exception("No mode selected, defaulting to FIXED");
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError(e, this);
            mode = Modes.Fixed;
        }
        if (nFF > 1)
        {
            for (int i = 0; i < nFF; i++)
            {
                GameObject obj = Instantiate(firefly);
                obj.name = ("Firefly " + i).ToString();
                pooledFF.Add(obj);
                obj.SetActive(true);
                obj.GetComponent<SpriteRenderer>().enabled = true;
                if (multiMode == 1)
                {
                    switch (i)
                    {
                        case 0:
                            obj.GetComponent<SpriteRenderer>().color = Color.black;
                            break;
                        case 1:
                            obj.GetComponent<SpriteRenderer>().color = Color.red;
                            break;
                        case 2:
                            obj.GetComponent<SpriteRenderer>().color = Color.blue;
                            break;
                        case 3:
                            obj.GetComponent<SpriteRenderer>().color = Color.yellow;
                            break;
                        case 4:
                            obj.GetComponent<SpriteRenderer>().color = Color.green;
                            break;
                    }
                }
            }
            firefly.SetActive(false);
        }

        trialNum = 0;

        currPhase = Phases.begin;
        phase = Phases.begin;

        player.transform.SetPositionAndRotation(Vector3.up * p_height, Quaternion.Euler(0.0f, 0.0f, 0.0f));
        drunkplayer.transform.SetPositionAndRotation(Vector3.up * p_height, Quaternion.Euler(0.0f, 0.0f, 0.0f));

        if (PlayerPrefs.GetFloat("calib") == 0)
        {
            flagMultiFF = nFF > 1;
            if (flagMultiFF)
            {
                var str = "";
                for (int i = 0; i < SharedMonkey.nFF; i++)
                {
                    str = string.Concat(str, string.Format("FFX{0},FFY{0},FFZ{0},", i));
                }
                sb.Append(string.Format("Trial,Time,Phase,FF On/Off,MonkeyX,MonkeyY,MonkeyZ,MonkeyRX,MonkeyRY,MonkeyRZ,MonkeyRW,Linear Velocity,Angular Velocity,{0}FFV,MappingContext,Confidence,GazeX,GazeY,GazeZ,GazeDistance,RCenterX,RCenterY,RCenterZ,LCenterX,LCenterY,LCenterZ,RNormalX,RNormalY,RNormalZ,LNormalX,LNormalY,LNormalZ,ObsLinNoise,ObsAngNoise,", str) + PlayerPrefs.GetString("Name") + "," + PlayerPrefs.GetString("Date") + "," + PlayerPrefs.GetInt("Run Number").ToString("D3") + "\n");
            }
            else
            {
                if ((int)PlayerPrefs.GetFloat("PTBType") == 99)
                {
                    sb.Append("Trial,Time,Phase,FF On/Off,MonkeyX,MonkeyY,MonkeyZ,MonkeyRX,MonkeyRY,MonkeyRZ,MonkeyRW,Linear Velocity,Angular Velocity,FFX,FFY,FFZ,FFV,MappingContext,Confidence,GazeX,GazeY,GazeZ,GazeDistance,RCenterX,RCenterY,RCenterZ,LCenterX,LCenterY,LCenterZ,RNormalX,RNormalY,RNormalZ,LNormalX,LNormalY,LNormalZ,ObsLinNoise,ObsAngNoise," + PlayerPrefs.GetString("Name") + "," + PlayerPrefs.GetString("Date") + "," + PlayerPrefs.GetInt("Run Number").ToString("D3") + "\n");
                }
                else
                {
                    sb.Append("Trial,Time,Phase,FF On/Off,MonkeyX,MonkeyY,MonkeyZ,MonkeyRX,MonkeyRY,MonkeyRZ,MonkeyRW,FFX,FFY,FFZ,FFV,MappingContext,Confidence,GazeX,GazeY," +
                        "GazeZ,GazeDistance,RCenterX,RCenterY,RCenterZ,LCenterX,LCenterY,LCenterZ,RNormalX,RNormalY,RNormalZ,LNormalX,LNormalY,LNormalZ,VKsi,Veta,RotKsi,RotEta," +
                        "PTBLV,PTBRV,CleanLV,CleanRV,RawX,RawY,ObsLinNoise,ObsAngNoise," + PlayerPrefs.GetString("Name") + "," + PlayerPrefs.GetString("Date") + "," + PlayerPrefs.GetInt("Run Number").ToString("D3") + "\n");
                }
            }
        }
    }

    private void OnDisable()
    {
        juiceBox.Close();
    }

    /// <summary>
    /// Update is called once per frame
    /// 
    /// for some reason, I can't set the camera's local rotation to 0,0,0 in Start()
    /// so I'm doing it here, and it gets called every frame, so added bonus of 
    /// ensuring it stays at 0,0,0.
    /// 
    /// SharedInstance.fill was an indicator of how many objects loaded in properly,
    /// but I found a way to make it so that everything loads pretty much instantly,
    /// so I don't really need it, but it's nice to have to ensure that the experiment
    /// doesn't start until the visual stimulus (i.e. floor triangles) are ready. 
    /// 
    /// Every frame, add the time it occurs, the trial time (resets every new trial),
    /// trial number, and position and rotation of player.
    /// 
    /// Switch phases here to ensure that each phase occurs on a frame
    /// 
    /// For Flashing and Fixed, toggle will be true or false depending on whether or not
    /// nextDouble returns a number smaller than or equal to the ratio
    /// 
    /// In the case of multiple FF, I turned the sprite renderer on and off, rather than
    /// using SetActive(). I was trying to do something will colliders to detect whether
    /// or not there is already another FF within a certain range, and in order to do that
    /// I would have to keep the sprite active, so I couldn't use SetActive(false). 
    /// The thing I was trying to do didn't work, but I already started turning the 
    /// sprite renderer on and off, and it works fine, so it's staying like that. This
    /// applies to all other instances of GetComponent<SpriteRenderer>() in the code.
    /// </summary>
    void Update()
    {
        if (isObsNoise)
        {
            particle_System2.transform.position = drunkplayer.transform.position - (Vector3.up * (p_height - 0.0002f));
        }
        particle_System.transform.position = player.transform.position - (Vector3.up * (p_height - 0.0002f));
        //print(particle_System.transform.position);
        if (playing && Time.realtimeSinceStartup - programT0 > 0.3f)
        {
            switch (phase)
            {

                case Phases.begin:
                    phase = Phases.none;
                    if (mode == Modes.ON)
                    {
                        if (nFF > 1)
                        {
                            toggle = true;
                        }
                    }
                    else
                    {
                        toggle = rand.NextDouble() <= ratio;
                    }
                    currentTask = Begin();
                    break;

                case Phases.trial:
                    phase = Phases.none;
                    currentTask = Trial();
                    break;

                case Phases.check:
                    phase = Phases.none;
                    if (mode == Modes.ON)
                    {
                        if (nFF > 1)
                        {
                            for (int i = 0; i < nFF; i++)
                            {
                                pooledFF[i].SetActive(false);
                            }
                        }
                        else
                        {
                            firefly.SetActive(false);
                        }
                    }
                    currentTask = Check();
                    break;

                case Phases.none:
                    break;
            }

            if (isMoving && nFF < 2)
            {
                if(movingFFmode == 0)
                {
                    firefly.transform.position += move * Time.deltaTime;
                }
                else
                {
                    System.Random randNoise = new System.Random();
                    double u1 = 1.0 - randNoise.NextDouble(); //uniform(0,1] random doubles
                    double u2 = 1.0 - randNoise.NextDouble();
                    double randStdNormal = noise_SD * Math.Sqrt(-2.0 * Math.Log(u1)) *
                                 Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
                                                               //double randNormal =
                                                               //mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
                                                               //print(randStdNormal);
                    if (PlayerPrefs.GetFloat("FixedYSpeed") != 0)
                    {
                        //print(timeCounter);
                        timeCounter += velocity_Noised * 0.001f;
                        velocity_Noised = velocity + (float)randStdNormal;
                        float x = (minDrawDistance + maxDrawDistance) * Mathf.Cos(timeCounter) / 2;
                        float y = 0.0001f;
                        float z = (minDrawDistance + maxDrawDistance) * Mathf.Sin(timeCounter) / 2;
                        //print(x);
                        //print(z);
                        firefly.transform.position = new Vector3(x, y, z);
                    }
                    else
                    {
                        Vector3 temp = move;
                        move = move + (direction * (float)randStdNormal);
                        velocity_Noised = velocity + (float)randStdNormal;
                        firefly.transform.position += move * Time.deltaTime;
                        move = temp;
                    }
                }
            }

            if (currentTask.IsFaulted)
            {
                print(currentTask.Exception);
            }
        }
    }

    /// <summary>
    /// Capture data at 120 Hz
    /// 
    /// Set Unity's fixed timestep to 1/120 (0.00833333...) in order to get 120 Hz recording
    /// Edit -> Project Settings -> Time -> Fixed Timestep
    /// </summary>
    public void FixedUpdate()
    {
        var tNow = Time.realtimeSinceStartup;

        var keyboard = Keyboard.current;
        if ((keyboard.enterKey.isPressed || trialNum > ntrials) && playing)
        {
            playing = false;

            Save();
            SendMarker("x", 1000.0f);

            juiceBox.Close();

            SceneManager.LoadScene("MainMenu");
        }

        if (isBegin)
        {
            isBegin = false;
            trialNum++;
            if (trialNum <= ntrials)
            {
                n.Add(trialNum);
            }

            SendMarker("s", 1000.0f);
        }

        if (isTrial)
        {
            float JstLinearThreshold = PlayerPrefs.GetFloat("LinearThreshold");
            float JstAngularThreshold = PlayerPrefs.GetFloat("AngularThreshold");
            if (isCOM2FF && Mathf.Abs(SharedJoystick.currentSpeed) >= JstLinearThreshold && !startedMoving)
            {
                startedMoving = true;
                MoveStartTime = Time.realtimeSinceStartup;
            }

            //print(Time.realtimeSinceStartup - MoveStartTime);
            if (isCOM2FF && Time.realtimeSinceStartup - MoveStartTime >= FF2delay && !FF2shown)
            {
                FF2shown = true;
                Vector3 position;
                int FFindex = rand.Next(FFcoordsList.Count);
                float r = FFcoordsList[FFindex].Item1;
                float angle = FFcoordsList[FFindex].Item2;
                position = player.transform.position - new Vector3(0.0f, p_height, 0.0f) + Quaternion.AngleAxis(angle, Vector3.up) * player.transform.forward * r;
                position.y = 0.0001f;
                pooledFF[1].transform.position = position;
                while(Vector3.Distance(position, pooledFF[0].transform.position) < 1.666666 * fireflyZoneRadius)
                {
                    FFindex = rand.Next(FFcoordsList.Count);
                    r = FFcoordsList[FFindex].Item1;
                    angle = FFcoordsList[FFindex].Item2;
                    position = player.transform.position - new Vector3(0.0f, p_height, 0.0f) + Quaternion.AngleAxis(angle, Vector3.up) * player.transform.forward * r;
                    position.y = 0.0001f;
                    pooledFF[1].transform.position = position;
                }
                OnOff(pooledFF[1]);
                /*float r;
                float angle;
                Vector3 position;
                foreach (var coord in FFcoordsList)
                {
                    r = coord.Item1;
                    angle = coord.Item2;
                    position = Vector3.zero - new Vector3(0.0f, p_height, 0.0f) + Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * r;
                    Vector3 player_vec = Quaternion.AngleAxis(player.transform.rotation.eulerAngles.y, Vector3.up) * Vector3.forward;
                    Vector3 FF_vec = new Vector3(position.x - player.transform.position.x, 0, position.z - player.transform.position.z);
                    float AngleBetween = Vector3.Angle(player_vec, FF_vec);
                    //print(FF_vec);
                    if(AngleBetween < 45 && Vector3.Distance(position, pooledFF[0].transform.position) > 1.666666 * fireflyZoneRadius)
                    {
                        if(!(coord.Item1 == FFcoordsList[FF1index].Item1 && coord.Item2 == FFcoordsList[FF1index].Item2))
                        {

                            FFvisibleList.Add(coord);
                        }
                    }
                }
                //print("possible FFs: " + FFvisibleList.Count.ToString());
                if(FFvisibleList.Count == 0)
                {
                    print("No possible FF for COM. Converting to normal.");
                }
                else
                {
                    int FFindex = rand.Next(FFvisibleList.Count);
                    r = FFvisibleList[FFindex].Item1;
                    angle = FFvisibleList[FFindex].Item2;
                    position = Vector3.zero - new Vector3(0.0f, p_height, 0.0f) + Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * r;
                    position.y = 0.0001f;
                    pooledFF[1].transform.position = position;
                    print("Trial FF2 r:" + r.ToString());
                    print("Trial FF2 a:" + angle.ToString());
                    FFvisibleList.Clear();
                    OnOff(pooledFF[1]);
                    //ffPositions[1] = position;
                }*/
            }

            if(PlayerPrefs.GetInt("isFFstimu") == 1 && (tNow - startTime) > trialStimuGap && !toggle)
            {
                isTrial = false;
                float stimr = (float)rand.NextDouble();
                if(stimr < stimuratio)
                {
                    SendMarker("m", microStimuDur * 1000.0f);
                    stimulatedTrial = true;
                    timeStimuStart.Add(tNow - programT0);
                }
            }
        }

        if (PlayerPrefs.GetInt("isFFstimu") == 1 && (tNow - startTime) > trialStimuGap && (tNow - startTime) < (trialStimuGap + microStimuDur) && stimulatedTrial)
        {
            stimulating = true;
        }
        else
        {
            stimulating = false;
        }

        if (isCheck)
        {
            isCheck = false;
            if (nFF > 1 && multiMode == 1)
            {
                if (loopCount + 1 < nFF && (proximity && isReward))
                {
                    checkTimeString = string.Concat(checkTimeString, ",", (Time.realtimeSinceStartup - programT0).ToString("F5")).Substring(1);
                }
                else
                {
                    for (int i = loopCount; i < nFF; i++)
                    {
                        checkTimeString = string.Concat(checkTimeString, ",", "0.00000").Substring(1);
                    }
                    checkTimeStrList.Add(checkTimeString);
                    checkTimeString = "";
                }
            }
            else
            {
                checkTime.Add(Time.realtimeSinceStartup - programT0);
            }
        }

        if (isEnd)
        {
            isEnd = false;
            SendMarker("e", 1000.0f);
        }

        if (playing && tNow - tPrev > 0.001f)
        {
            tPrev = tNow;
        }

        if (PlayerPrefs.GetFloat("calib") == 0)
        {
            if (timeProgStart == 0.0)
            {
                timeProgStart = (double)programT0;
            }

            var trial = trialNum;
            var epoch = (int)currPhase;
            var onoff = firefly.activeInHierarchy ? 1 : 0;
            var position = player.transform.position.ToString("F5").Trim('(', ')').Replace(" ", "");
            var rotation = player.transform.rotation.ToString("F5").Trim('(', ')').Replace(" ", "");
            var linear = SharedJoystick.currentSpeed;
            var angular = SharedJoystick.currentRot;
            var FFlinear = velocity;
            var FFposition = string.Empty;
            var VKsi = SharedJoystick.velKsi;
            var VEta = SharedJoystick.velEta;
            var RKsi = SharedJoystick.rotKsi;
            var REta = SharedJoystick.rotEta;
            var PTBLV = SharedJoystick.currentSpeed;
            var PTBRV = SharedJoystick.currentRot;
            var RawX = SharedJoystick.rawX;
            var RawY = SharedJoystick.rawY;
            var CleanLV = SharedJoystick.cleanVel;
            var CleanRV = SharedJoystick.prevCleanRot;

            var ptbJoyVelValueTmp = SharedJoystick.ptbJoyVelValue;
            var ptbJoyRotValueTmp = SharedJoystick.ptbJoyRotValue;
            var ptbJoyFlagTmp = SharedJoystick.ptbJoyFlag;
            var currentSpeedTmp = SharedJoystick.currentSpeed;
            var currentRotTmp = SharedJoystick.currentRot;
            var speedPrePtbTmp = SharedJoystick.speedPrePtb;
            var rotPrePtbTmp = SharedJoystick.rotPrePtb;
            var ObsLinNoise = SharedDrunkstick.DistFlowSpeed;
            var ObsAngNoise = SharedDrunkstick.DistFlowRot;

            if (flagMultiFF)
            {
                foreach (Vector3 pos in ffPositions)
                {
                    FFposition = string.Concat(FFposition, ",", pos.ToString("F5").Trim('(', ')').Replace(" ", "")).Substring(1);
                }
            }
            else
            {
                FFposition = firefly.transform.position.ToString("F5").Trim('(', ')').Replace(" ", "");
            }

            var lllin = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27}",
                    trial,
                    (double)Time.realtimeSinceStartup - timeProgStart,
                    epoch,
                    onoff,
                    position,
                    rotation,
                    FFposition,
                    FFlinear,
                    0,
                    0,
                    "0,0,0",
                    0,
                    "0,0,0",
                    "0,0,0",
                    "0,0,0",
                    "0,0,0",
                    VKsi,
                    VEta,
                    RKsi,
                    REta,
                    PTBLV,
                    PTBRV,
                    CleanLV,
                    CleanRV,
                    RawX,
                    RawY,
                    ObsLinNoise,
                    ObsAngNoise);

            sb.AppendLine(lllin);
        }
    }

    /// <summary>
    /// Wait until the player is not moving, then:
    /// 1. Add trial begin time to respective list
    /// 2. Update position; 
    ///     r is calculated so that all distances between the min and max are equally likely to occur,
    ///     angle is calculated in much the same way,
    ///     side just determines whether it'll appear on the left or right side of the screen,
    ///     position is calculated by adding an offset to the player's current position;
    ///         Quaternion.AngleAxis calculates a rotation based on an angle (first arg)
    ///         and an axis (second arg, Vector3.up is shorthand for the y-axis). Multiply that by the 
    ///         forward vector and radius r (or how far away the firefly should be from the player) to 
    ///         get the final position of the firefly
    /// 3. Record player origin and rotation, as well as firefly location
    /// 4. Start firefly behavior depending on mode, and switch phase to trial
    /// </summary>
    async Task Begin()
    {
        //Debug.Log("Begin Phase start.");
        await new WaitForEndOfFrame();

        int randomNumber = rand.Next(1, 5); // Generates a random integer between 1 and 4 (inclusive)

        switch (randomNumber)
        {
            case 1:
                Console.WriteLine("Case 1");
                isProcessNoise = false;
                ObsVelocityNoiseGain = 0;
                ObsRotationNoiseGain = 0;
                break;
            case 2:
                Console.WriteLine("Case 2");
                isProcessNoise = false;
                ObsVelocityNoiseGain = PlayerPrefs.GetFloat("ObsVelocityNoiseGain");
                ObsRotationNoiseGain = PlayerPrefs.GetFloat("ObsRotationNoiseGain");
                break;
            case 3:
                Console.WriteLine("Case 3");
                isProcessNoise = true;
                ObsVelocityNoiseGain = 0;
                ObsRotationNoiseGain = 0;
                break;
            case 4:
                Console.WriteLine("Case 4");
                isProcessNoise = true;
                ObsVelocityNoiseGain = PlayerPrefs.GetFloat("ObsVelocityNoiseGain");
                ObsRotationNoiseGain = PlayerPrefs.GetFloat("ObsRotationNoiseGain");
                break;
            default:
                Console.WriteLine("Invalid case");
                break;
        }

        SharedJoystick.MaxSpeed = RandomizeSpeeds(velMin, velMax);
        SharedJoystick.RotSpeed = RandomizeSpeeds(rotMin, rotMax);

        //print(CtrlDynamicsFlag);
        if (ptb != 2)
        {
            switch (ptb)
            {
                case 0:
                    SharedJoystick.DiscreteTau();
                    break;

                case 1:
                    SharedJoystick.ContinuousTau();
                    break;

                default:
                    break;
            }
            //tautau.Add(SharedJoystick.currentTau);
            //filterTau.Add(SharedJoystick.filterTau);
            max_v.Add(SharedJoystick.MaxSpeed);
            max_w.Add(SharedJoystick.RotSpeed);
            CurrentTau.Add(SharedJoystick.savedTau);
        }
        else
        {
            max_v.Add(SharedJoystick.MaxSpeed);
            max_w.Add(SharedJoystick.RotSpeed);
        }

        loopCount = 0;

        float density = particles.SwitchDensity();
        if (particles.changedensityflag && isObsNoise)
        {
            particles2.SwitchDensity2();
        }

        densities.Add(density);
        if (isObsNoise)
        {
            densities_obsRatio.Add(ObsDensityRatio);
        }
        else
        {
            densities_obsRatio.Add(0);
        }

        currPhase = Phases.begin;
        isBegin = true;

        bool isDiscrete = PlayerPrefs.GetInt("isDiscrete") == 1;
        if (isCOM)
        {
            Vector3 position;
            int FFindex = rand.Next(FFcoordsList.Count);
            FF1index = FFindex;
            float r = FFcoordsList[FFindex].Item1;
            float angle = FFcoordsList[FFindex].Item2;
            position = Vector3.zero - new Vector3(0.0f, p_height, 0.0f) + Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * r;
            position.y = 0.0001f;
            pooledFF[0].transform.position = position;
            Vector3 position1 = player.transform.position - new Vector3(0.0f, 0.0f, 10.0f);
            pooledFF[1].transform.position = position1;
            pooledFF[1].SetActive(false);
            print("Trial FF1 r:" + r.ToString());
            print("Trial FF1 a:" + angle.ToString());
            float COMdecider = (float)rand.NextDouble();
            if(COMdecider < normalRatio)
            {
                isNormal = true;
                isStatic2FF = false;
                isCOM2FF = false;
                COMtrialtype.Add(1);
            }
            else if(COMdecider < normal2FFRatio)
            {
                isNormal = false;
                isStatic2FF = true;
                isCOM2FF = false;
                COMtrialtype.Add(2);
            }
            else
            {
                isNormal = false;
                isStatic2FF = false;
                isCOM2FF = true;
                COMtrialtype.Add(3);
            }
        }
        else if (nFF > 1 && multiMode == 1)
        {
            for (int i = 0; i < nFF; i++)
            {
                bool tooClose;
                do
                {
                    tooClose = false;
                    Vector3 position;
                    float r = ranges[i] + (ranges[i + 1] - ranges[i]) * Mathf.Sqrt((float)rand.NextDouble());
                    float angle = (float)rand.NextDouble() * (maxPhi - minPhi) + minPhi;
                    if (LR != 0.5f)
                    {
                        float side = rand.NextDouble() < LR ? 1 : -1;
                        position = (player.transform.position - new Vector3(0.0f, p_height, 0.0f)) + Quaternion.AngleAxis(angle * side, Vector3.up) * player.transform.forward * r;
                    }
                    else
                    {
                        position = (player.transform.position - new Vector3(0.0f, p_height, 0.0f)) + Quaternion.AngleAxis(angle, Vector3.up) * player.transform.forward * r;
                    }
                    position.y = 0.0001f;
                    if (i > 0) for (int k = 0; k < i; k++) { if (Vector3.Distance(position, pooledFF[k].transform.position) <= separation) tooClose = true; } // || Mathf.Abs(position.x - pooledFF[k - 1].transform.position.x) >= 0.5f || Mathf.Abs(position.z - pooledFF[k - 1].transform.position.z) <= 0.5f) tooClose = true; }
                    pooledFF[i].transform.position = position;
                    ffPositions[i] = position;
                } while (tooClose);
            }
        }
        else if (nFF > 1 && multiMode == 2)
        {
            for (int i = 0; i < nFF; i++)
            {
                bool tooClose;
                do
                {
                    tooClose = false;
                    Vector3 position;
                    float r = minDrawDistance + (maxDrawDistance - minDrawDistance) * Mathf.Sqrt((float)rand.NextDouble());
                    float angle = (float)rand.NextDouble() * (maxPhi - minPhi) + minPhi;
                    if (LR != 0.5f)
                    {
                        float side = rand.NextDouble() < LR ? 1 : -1;
                        position = (player.transform.position - new Vector3(0.0f, p_height, 0.0f)) + Quaternion.AngleAxis(angle * side, Vector3.up) * player.transform.forward * r;
                    }
                    else
                    {
                        position = (player.transform.position - new Vector3(0.0f, p_height, 0.0f)) + Quaternion.AngleAxis(angle, Vector3.up) * player.transform.forward * r;
                    }
                    position.y = 0.0001f;
                    if (i > 0) for (int k = 0; k < i; k++) { if (Vector3.Distance(position, pooledFF[k].transform.position) <= separation) tooClose = true; } // || Mathf.Abs(position.x - pooledFF[k - 1].transform.position.x) >= 0.5f || Mathf.Abs(position.z - pooledFF[k - 1].transform.position.z) <= 0.5f) tooClose = true; }
                    pooledFF[i].transform.position = position;
                    ffPositions[i] = position;
                } while (tooClose);
            }
        }
        else
        {
            Vector3 position;
            float r = minDrawDistance + (maxDrawDistance - minDrawDistance) * Mathf.Sqrt((float)rand.NextDouble());
            float angle = (float)rand.NextDouble() * (maxPhi - minPhi) + minPhi;
            if (isDiscrete)
            {
                int num_lin = (int)PlayerPrefs.GetFloat("No_Linspace");
                int num_rot = (int)PlayerPrefs.GetFloat("No_Angspace");
                float[] linspace = Enumerable.Range(0, num_lin).Select(i => minDrawDistance + (maxDrawDistance - minDrawDistance) * i / (num_lin - 1)).ToArray();
                float[] rotspace = Enumerable.Range(0, num_rot).Select(i => minPhi + (maxPhi - minPhi) * i / (num_rot - 1)).ToArray();
                int randomLin = rand.Next(1, num_lin + 1);
                int randomRot = rand.Next(1, num_rot + 1);
                r = linspace[randomLin - 1];
                angle = rotspace[randomRot - 1];
                print(r);
                print(angle);
            }

            if (LR != 0.5f)
            {
                float side = rand.NextDouble() < LR ? 1 : -1;
                position = (player.transform.position - new Vector3(0.0f, p_height, 0.0f)) + Quaternion.AngleAxis(angle * side, Vector3.up) * player.transform.forward * r;
            }
            else
            {
                position = (player.transform.position - new Vector3(0.0f, p_height, 0.0f)) + Quaternion.AngleAxis(angle, Vector3.up) * player.transform.forward * r;
            }
            position.y = 0.0001f;
            //position.x = FFTagetMatchList[trial_count].Item1;
            //position.z = FFTagetMatchList[trial_count].Item2;
            trial_count++;
            firefly.transform.position = position;
            ffPositions.Add(position);
        }

        if (isStatic2FF)
        {
            Vector3 position;
            int FFindex = rand.Next(FFcoordsList.Count);
            float r = FFcoordsList[FFindex].Item1;
            float angle = FFcoordsList[FFindex].Item2;
            position = Vector3.zero - new Vector3(0.0f, p_height, 0.0f) + Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * r;
            position.y = 0.0001f;
            while (FFindex == FF1index || Vector3.Distance(position,pooledFF[0].transform.position) <= 1.666666 * fireflyZoneRadius)
            {
                FFindex = rand.Next(FFcoordsList.Count);
                r = FFcoordsList[FFindex].Item1;
                angle = FFcoordsList[FFindex].Item2;
                position = Vector3.zero - new Vector3(0.0f, p_height, 0.0f) + Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * r;
                position.y = 0.0001f;
            }
            pooledFF[1].transform.position = position;
            pooledFF[1].SetActive(false);
            print("Trial FF2 r:" + r.ToString());
            print("Trial FF2 a:" + angle.ToString());
        }

        float velocityThreshold = PlayerPrefs.GetFloat("velBrakeThresh");
        float rotationThreshold = PlayerPrefs.GetFloat("rotBrakeThresh");
        float SMr = (float)rand.NextDouble();
        if (isCOM)
        {
            //await new WaitUntil(() => Mathf.Abs(SharedJoystick.currentSpeed) <= velocityThreshold && Mathf.Abs(SharedJoystick.currentRot) <= rotationThreshold); // Used to be rb.velocity.magnitude
        }

        if (SMtrial)
        {
            if (SMr > 0.5)
            {
                await new WaitUntil(() => Mathf.Abs(SharedJoystick.currentSpeed) >= velocityThreshold); // Used to be rb.velocity.magnitude
            }
            else
            {
                await new WaitUntil(() => Mathf.Abs(SharedJoystick.currentSpeed) <= velocityThreshold && Mathf.Abs(SharedJoystick.currentRot) <= rotationThreshold); // Used to be rb.velocity.magnitude
            }
        }
        else if (isCOM2FF)
        {
            //TODO: save delays in disc
            FF2delay = PlayerPrefs.GetFloat("FF2delay");//(float)rand.NextDouble();//(float)(rand.NextDouble() * FFcoordsList[FF1index].Item1/SharedJoystick.MaxSpeed);
        }

        player_origin = player.transform.position;
        origin.Add(player_origin.ToString("F5").Trim(toTrim).Replace(" ", ""));
        heading.Add(player.transform.rotation.ToString("F5").Trim(toTrim).Replace(" ", ""));

        if (isMoving && nFF < 2)
        {
            float r = (float)rand.NextDouble();

            if (r <= v_ratios[0])
            {
                //v1
                velocity = velocities[0];
                noise_SD = v_noises[0];
            }
            else if (r > v_ratios[0] && r <= v_ratios[1])
            {
                //v2
                velocity = velocities[1];
                noise_SD = v_noises[1];
            }
            else if (r > v_ratios[1] && r <= v_ratios[2])
            {
                //v3
                velocity = velocities[2];
                noise_SD = v_noises[2];
            }
            else if (r > v_ratios[2] && r <= v_ratios[3])
            {
                //v4
                velocity = velocities[3];
                noise_SD = v_noises[3];
            }
            else if (r > v_ratios[3] && r <= v_ratios[4])
            {
                //v5
                velocity = velocities[4];
                noise_SD = v_noises[4];
            }
            else if (r > v_ratios[4] && r <= v_ratios[5])
            {
                //v6
                velocity = velocities[5];
                noise_SD = v_noises[5];
            }
            else if (r > v_ratios[5] && r <= v_ratios[6])
            {
                //v7
                velocity = velocities[6];
                noise_SD = v_noises[6];
            }
            else if (r > v_ratios[6] && r <= v_ratios[7])
            {
                //v8
                velocity = velocities[7];
                noise_SD = v_noises[7];
            }
            else if (r > v_ratios[7] && r <= v_ratios[8])
            {
                //v9
                velocity = velocities[8];
                noise_SD = v_noises[8];
            }
            else if (r > v_ratios[8] && r <= v_ratios[9])
            {
                //v10
                velocity = velocities[9];
                noise_SD = v_noises[9];
            }
            else if (r > v_ratios[9] && r <= v_ratios[10])
            {
                //v11
                velocity = velocities[10];
                noise_SD = v_noises[10];
            }
            else
            {
                //v12
                velocity = velocities[11];
                noise_SD = v_noises[11];
            }

            if (LRFB)
            {
                direction = Vector3.right;
            }
            else
            {
                direction = Vector3.forward;
            }
            fv.Add(velocity);
            move = direction * velocity;
        }
        else
        {
            fv.Add(0.0f);
        }

        // Debug.Log("Begin Phase End.");
        if (nFF > 1)
        {
            if (PlayerPrefs.GetInt("isColored") == 1)
            {
                foreach (GameObject FF in pooledFF)
                {
                    float r = (float)rand.NextDouble();

                    if (r <= colorratios[0])
                    {
                        FF.GetComponent<SpriteRenderer>().color = Color.red;
                        colorchosen.Add(1);
                    }
                    else if (r > colorratios[0] && r <= colorratios[1])
                    {
                        FF.GetComponent<SpriteRenderer>().color = Color.blue;
                        colorchosen.Add(2);
                    }
                    else if (r > colorratios[1] && r <= colorratios[2])
                    {
                        FF.GetComponent<SpriteRenderer>().color = Color.green;
                        colorchosen.Add(3);
                    }
                    else if (r > colorratios[2] && r <= colorratios[3])
                    {
                        FF.GetComponent<SpriteRenderer>().color = Color.yellow;
                        colorchosen.Add(4);
                    }
                    else if (r > colorratios[3] && r <= colorratios[4])
                    {
                        FF.GetComponent<SpriteRenderer>().color = Color.white;
                        colorchosen.Add(5);
                    }
                    else
                    {
                        FF.GetComponent<SpriteRenderer>().color = Color.black;
                        colorchosen.Add(6);
                    }
                }
            }

            switch (mode)
            {
                case Modes.ON:
                    foreach (GameObject FF in pooledFF)
                    {
                        FF.SetActive(true);
                    }
                    break;
                case Modes.Flash:
                    on = true;
                    foreach (GameObject FF in pooledFF)
                    {
                        flashTask = Flash(FF);
                    }
                    break;
                case Modes.Fixed:
                    if (toggle && !isCOM || toggle && isNormal)
                    {
                        foreach (GameObject FF in pooledFF)
                        {
                            FF.SetActive(true);
                            // Add alwaysON for all fireflies
                        }
                    }
                    else
                    {
                        float r = (float)rand.NextDouble();

                        if (r <= ratios[0])
                        {
                            // duration 1
                            lifeSpan = durations[0];
                        }
                        else if (r > ratios[0] && r <= ratios[1])
                        {
                            // duration 2
                            lifeSpan = durations[1];
                        }
                        else if (r > ratios[1] && r <= ratios[2])
                        {
                            // duration 3
                            lifeSpan = durations[2];
                        }
                        else if (r > ratios[2] && r <= ratios[3])
                        {
                            // duration 4
                            lifeSpan = durations[3];
                        }
                        else
                        {
                            // duration 5
                            lifeSpan = durations[4];
                        }
                        onDur.Add(lifeSpan);
                        if (isCOM)
                        {
                            OnOff(pooledFF[0]);
                            if (isStatic2FF)
                            {
                                OnOff(pooledFF[1]);
                            }
                        }
                        else
                        {
                            foreach (GameObject FF in pooledFF)
                            {
                                OnOff(FF);
                            }
                        }
                    }
                    break;
            }
        }
        else
        {
            switch (mode)
            {
                case Modes.ON:
                    firefly.SetActive(true);
                    break;
                case Modes.Flash:
                    on = true;
                    flashTask = Flash(firefly);
                    break;
                case Modes.Fixed:
                    if (toggle)
                    {
                        firefly.SetActive(true);
                        alwaysON.Add(true);
                    }
                    else
                    {
                        alwaysON.Add(false);
                        float r = (float)rand.NextDouble();

                        if (r <= ratios[0])
                        {
                            // duration 1
                            lifeSpan = durations[0];
                        }
                        else if (r > ratios[0] && r <= ratios[1])
                        {
                            // duration 2
                            lifeSpan = durations[1];
                        }
                        else if (r > ratios[1] && r <= ratios[2])
                        {
                            // duration 3
                            lifeSpan = durations[2];
                        }
                        else if (r > ratios[2] && r <= ratios[3])
                        {
                            // duration 4
                            lifeSpan = durations[3];
                        }
                        else
                        {
                            // duration 5
                            lifeSpan = durations[4];
                        }
                        onDur.Add(lifeSpan);
                        OnOff();
                    }
                    break;
            }
        }

        if (PlayerPrefs.GetInt("isFFstimu") == 1)
        {
            trialStimuGap = microStimuGap * (float)rand.NextDouble();
        }

        phase = Phases.trial;
        currPhase = Phases.trial;
    }

    /// <summary>
    /// Doesn't really do much besides wait for the player to start moving, and, afterwards,
    /// wait until the player stops moving and then start the check phase. Also will go back to
    /// begin phase if player doesn't move before timeout
    /// </summary>
    async Task Trial()
    {
        MoveStartTime = 999999f;
        startedMoving = false;

        isTrial = true;

        //Debug.Log("Trial Phase Start.");
        startTime = Time.realtimeSinceStartup;

        velbrakeThresh = PlayerPrefs.GetFloat("velBrakeThresh");
        rotbrakeThresh = PlayerPrefs.GetFloat("rotBrakeThresh");

        if (PlayerPrefs.GetFloat("ThreshTauMultiplier") != 0)
        {
            float k = PlayerPrefs.GetFloat("ThreshTauMultiplier");
            velbrakeThresh = k * SharedJoystick.currentTau + velStopThreshold;
            rotbrakeThresh = k * SharedJoystick.currentTau + rotStopThreshold;
        }

        source = new CancellationTokenSource();

        if (toggle && isMoving)
        {
            motion_toggle = true;
            await new WaitForSeconds(0.15f);
            motion_toggle = false;
        }

        if (isCOM)
        {
            /*foreach (var coord in FFcoordsList)
            {
                float r1 = coord.Item1;
                float angle1 = coord.Item2;
                Vector3 position2 = (player.transform.position - new Vector3(0.0f, p_height, 0.0f)) + Quaternion.AngleAxis(angle1, Vector3.up) * player.transform.forward * r1;
                position2.y = 0.0001f;
                pooledFF[0].transform.position = position2;
                pooledFF[0].SetActive(true);
                await new WaitForSeconds(1f);
                pooledFF[0].SetActive(false);
            }*/
        }

        if (ptb != 2)
        {
            print("PTB trial started");
            isIntertrail = false;
            var t = Task.Run(async () => {
                await new WaitUntil(() => Mathf.Abs(SharedJoystick.currentSpeed) >= velbrakeThresh); // Used to be rb.velocity.magnitude
            }, source.Token);

            var t1 = Task.Run(async () => {
                await new WaitForSeconds(timeout); // Used to be rb.velocity.magnitude
            }, source.Token);

            if (await Task.WhenAny(t, t1) == t)
            {
                float joystickT = PlayerPrefs.GetFloat("JoystickThreshold");
                await new WaitUntil(() => Mathf.Abs(SharedJoystick.currentSpeed) < velbrakeThresh && Mathf.Abs(SharedJoystick.currentRot) < rotbrakeThresh && (float)Math.Abs(SharedJoystick.moveX) <= joystickT && (float)Math.Abs(SharedJoystick.moveY) <= joystickT || t1.IsCompleted); // Used to be rb.velocity.magnitude // || (angleL > 3.0f or angleR > 3.0f)
                if (t1.IsCompleted)
                {
                    isTimeout = true;
                }
                isIntertrail = true;
            }
            else
            {
                isIntertrail = true;
                print("Timed out");
                isTimeout = true;
            }
        }
        else
        {
            float JstLinearThreshold = PlayerPrefs.GetFloat("LinearThreshold");
            float JstAngularThreshold = PlayerPrefs.GetFloat("AngularThreshold");

            isIntertrail = false;
            var t = Task.Run(async () => {
                await new WaitUntil(() => Vector3.Distance(player_origin, player.transform.position) > 0.5f || playing == false); // Used to be rb.velocity.magnitude
            }, source.Token);

            var t1 = Task.Run(async () => {
                await new WaitForSeconds(timeout);
            }, source.Token);

            if (await Task.WhenAny(t, t1) == t || player == null)
            {
                await new WaitUntil(() => ((Mathf.Abs(SharedJoystick.currentSpeed) <= JstLinearThreshold && Mathf.Abs(SharedJoystick.currentRot) <= JstAngularThreshold && !SharedJoystick.CtrlDynamicsFlag) && prevVel == 0.0f && prevPrevVel == 0.0f) || t1.IsCompleted); // Used to be rb.velocity.magnitude // || (angleL > 3.0f or angleR > 3.0f)
                if (t1.IsCompleted) isTimeout = true;
            }
            else
            {
                //print("Timed out");
                isTimeout = true;
            }
        }

        source.Cancel();

        if (mode == Modes.Flash)
        {
            on = false;
        }

        if (toggle)
        {
            if (nFF > 1 && multiMode == 1)
            {
                if (isTimeout || Vector3.Distance(player.transform.position, pooledFF[loopCount].transform.position) > fireflyZoneRadius)
                {
                    foreach (GameObject FF in pooledFF)
                    {
                        FF.SetActive(false);
                    }
                }
                else
                {
                    pooledFF[loopCount].SetActive(false);
                }

                if (toggle && (isTimeout || loopCount + 1 >= nFF))
                {
                    onDur.Add(Time.realtimeSinceStartup - beginTime[beginTime.Count - 1] - programT0);
                }
            }
            else if (nFF > 1 && multiMode == 2)
            {
                foreach (GameObject FF in pooledFF)
                {
                    FF.SetActive(false);
                }
            }
            else
            {
                firefly.SetActive(false);
            }

            if (toggle && multiMode != 1)
            {
                onDur.Add(Time.realtimeSinceStartup - beginTime[beginTime.Count - 1] - programT0);
            }
        }

        move = new Vector3(0.0f, 0.0f, 0.0f);
        velocity = 0.0f;
        phase = Phases.check;
        currPhase = Phases.check;
        // Debug.Log("Trial Phase End.");
    }

    /// <summary>
    /// Save the player's position (pPos) and the firefly (reward zone)'s position (fPos)
    /// and start a coroutine to wait for some random amount of time between the user's
    /// specified minimum and maximum wait times
    /// </summary>
    async Task Check()
    {
        FF2shown = false;
        proximity = false;

        isTrial = false;
        isReward = true;

        float distance = 0.0f;
        float curdistance = 9999f;

        Vector3 pos;
        Quaternion rot;

        pPos = player.transform.position - new Vector3(0.0f, p_height, 0.0f);

        pos = player.transform.position;
        rot = player.transform.rotation;

        isCheck = true;

        if (!isTimeout)
        {
            source = new CancellationTokenSource();
            //Debug.Log("Check Phase Start.");

            float delay = c_lambda * Mathf.Exp(-c_lambda * ((float)rand.NextDouble() * (c_max - c_min) + c_min));
            // Debug.Log("firefly delay: " + delay);
            checkWait.Add(delay);
            if (ptb != 2 && PlayerPrefs.GetFloat("FixedYSpeed") == 0)
            {
                await new WaitForSeconds(delay);
            }
        }
        else
        {
            checkWait.Add(0.0f);

            audioSource.clip = loseSound;
        }

        if (isCOM)
        {
            for (int i = 0; i < 2; i++)
            {
                if(!(pooledFF[i].transform.position.x == 0 && pooledFF[i].transform.position.z == 0))
                {
                    ffPosStr = string.Concat(ffPosStr, ",", pooledFF[i].transform.position.ToString("F5").Trim(toTrim).Replace(" ", "")).Substring(1);
                    distance = Vector3.Distance(pPos, pooledFF[i].transform.position);
                    //print(distance);
                    if (distance <= fireflyZoneRadius && distance < curdistance)
                    {
                        curdistance = distance;
                        proximity = true;
                        colorhit = i;
                    }
                }
            }
            distances.Add(distance);
        }
        else if (nFF > 1 && multiMode == 1)
        {
            ffPosStr = string.Concat(ffPosStr, ",", pooledFF[loopCount].transform.position.ToString("F5").Trim(toTrim).Replace(" ", "")).Substring(1);
            distance = Vector3.Distance(pPos, pooledFF[loopCount].transform.position);
            //print(distance);
            distances.Add(distance);
            if (distances[loopCount] <= fireflyZoneRadius)
            {
                proximity = true;
            }
        }
        else if (nFF > 1 && multiMode == 2)
        {
            for (int i = 0; i < nFF; i++)
            {
                ffPosStr = string.Concat(ffPosStr, ",", pooledFF[i].transform.position.ToString("F5").Trim(toTrim).Replace(" ", "")).Substring(1);
                distance = Vector3.Distance(pPos, pooledFF[i].transform.position);
                //print(distance);
                if (distance <= fireflyZoneRadius && distance < curdistance)
                {
                    curdistance = distance;
                    proximity = true;
                    colorhit = i;
                }
                distances.Add(distance);
            }
        }
        else
        {
            if (Vector3.Distance(pPos, firefly.transform.position) <= fireflyZoneRadius) proximity = true;
            distance = Vector3.Distance(pPos, firefly.transform.position);
            ffPosStr = firefly.transform.position.ToString("F5").Trim(toTrim).Replace(" ", "");
            distances.Add(distance);
        }

        if (isReward && proximity)
        {
             if (isCOM && PlayerPrefs.GetInt("isColored") == 1)
            {
                print(colorhit);
                print(colorchosen[(int)colorhit]);
                juiceTime = colorrewards[(int)colorchosen[(int)colorhit] - 1];
                audioSource.clip = winSound;
                juiceDuration.Add(juiceTime);
                audioSource.Play();
                points++;
                SendMarker("j", juiceTime);
                await new WaitForSeconds((juiceTime / 1000.0f) + 0.25f);
                juiceTime = 0;
            }
            else if (isCOM)
            {
                audioSource.clip = winSound;
                juiceTime = Mathf.Lerp(maxJuiceTime, minJuiceTime, Mathf.InverseLerp(0.0f, fireflyZoneRadius, distance));
                //print(juiceTime);
                juiceDuration.Add(juiceTime);
                audioSource.Play();

                points++;
                SendMarker("j", juiceTime);

                await new WaitForSeconds((juiceTime / 1000.0f) + 0.25f);
            }
            else if (nFF > 1 && multiMode == 1)
            {
                if (loopCount + 1 < nFF)
                {
                    juiceTime += Mathf.Lerp(maxJuiceTime, minJuiceTime, Mathf.InverseLerp(0.0f, fireflyZoneRadius, distance));
                    //Debug.Log(string.Format("Firefly {0} Hit.", loopCount + 1));
                    audioSource.clip = neutralSound;
                    audioSource.Play();

                    player_origin = player.transform.position;
                }
                else
                {
                    juiceTime += Mathf.Lerp(maxJuiceTime, minJuiceTime, Mathf.InverseLerp(0.0f, fireflyZoneRadius, distance));
                    //Debug.Log(string.Format("Firefly {0} Hit. Reward: {1}", loopCount + 1, juiceTime));
                    audioSource.clip = winSound;
                    //print(juiceTime);
                    juiceDuration.Add(juiceTime);
                    audioSource.Play();
                    points++;
                    SendMarker("j", juiceTime);
                    await new WaitForSeconds((juiceTime / 1000.0f) + 0.25f);
                    juiceTime = 0;
                    //Debug.Log("Juice: " + DateTime.Now.ToLongTimeString());
                }
            }
            else
            {
                audioSource.clip = winSound;
                juiceTime = Mathf.Lerp(maxJuiceTime, minJuiceTime, Mathf.InverseLerp(0.0f, fireflyZoneRadius, distance));
                //print(juiceTime);
                juiceDuration.Add(juiceTime);
                audioSource.Play();

                points++;
                SendMarker("j", juiceTime);

                await new WaitForSeconds((juiceTime / 1000.0f) + 0.25f);
                //Debug.Log("Juice: " + DateTime.Now.ToLongTimeString());
            }
        }
        else
        {
            audioSource.clip = loseSound;
            juiceDuration.Add(0.0f);
            rewardTime.Add(0.0f);
            audioSource.Play();

            await new WaitForSeconds((juiceTime / 1000.0f) + 0.25f);
        }


        if (nFF > 1 && multiMode == 1)
        {
            if (loopCount + 1 < nFF)
            {
                if (!isTimeout && isReward && proximity)
                {
                    distances.Add(Vector3.Distance(pPos, pooledFF[loopCount].transform.position));
                    cPosTemp.Add(pos.ToString("F5").Trim(toTrim).Replace(" ", ""));
                    cRotTemp.Add(rot.ToString("F5").Trim(toTrim).Replace(" ", ""));

                    loopCount++;

                    phase = Phases.trial;
                    currPhase = Phases.trial;
                }
                else
                {
                    if (loopCount + 1 < nFF)
                    {
                        for (int i = loopCount + 1; i < nFF; i++)
                        {
                            ffPosStr = string.Concat(ffPosStr, ",", pooledFF[loopCount].transform.position.ToString("F5").Trim(toTrim).Replace(" ", "")).Substring(1);
                            distances.Add(Vector3.Distance(pPos, pooledFF[i].transform.position));
                            cPosTemp.Add(Vector3.zero.ToString("F5").Trim(toTrim).Replace(" ", ""));
                            cRotTemp.Add(Quaternion.identity.ToString("F5").Trim(toTrim).Replace(" ", ""));
                        }
                    }

                    player.transform.position = Vector3.up * p_height;
                    player.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                    drunkplayer.transform.position = Vector3.up * p_height;
                    drunkplayer.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

                    score.Add(isReward && proximity ? 1 : 0);
                    timedout.Add(isTimeout ? 1 : 0);
                    cPos.Add(string.Join(",", cPosTemp));
                    cRot.Add(string.Join(",", cRotTemp));
                    dist.Add(string.Join(",", distances));
                    ffPos.Add(ffPosStr);

                    float wait = i_lambda * Mathf.Exp(-i_lambda * ((float)rand.NextDouble() * (i_max - i_min) + i_min));

                    currPhase = Phases.ITI;

                    interWait.Add(wait);

                    isEnd = true;

                    ffPosStr = "";
                    cPosTemp.Clear();
                    cRotTemp.Clear();
                    distances.Clear();
                    isTimeout = false;

                    await new WaitForSeconds(wait);

                    phase = Phases.begin;
                    // Debug.Log("Check Phase End.");
                }
            }
            else
            {
                loopCount = 0;

                score.Add(isReward && proximity ? 1 : 0);
                timedout.Add(isTimeout ? 1 : 0);
                cPos.Add(string.Join(",", cPosTemp));
                cRot.Add(string.Join(",", cRotTemp));
                dist.Add(string.Join(",", distances));
                ffPos.Add(ffPosStr);

                float wait = i_lambda * Mathf.Exp(-i_lambda * ((float)rand.NextDouble() * (i_max - i_min) + i_min));

                currPhase = Phases.ITI;
                
                interWait.Add(wait);

                isEnd = true;

                distances.Clear();
                cPosTemp.Clear();
                cRotTemp.Clear();
                ffPosStr = "";
                isTimeout = false;

                await new WaitForSeconds(wait);

                phase = Phases.begin;
            }
        }
        else if (nFF > 1 && multiMode == 2)
        {
            score.Add(isReward && proximity ? 1 : 0);
            timedout.Add(isTimeout ? 1 : 0);
            cPos.Add(pos.ToString("F5").Trim(toTrim).Replace(" ", ""));
            cRot.Add(rot.ToString("F5").Trim(toTrim).Replace(" ", ""));
            dist.Add(string.Join(",", distances));
            ffPos.Add(ffPosStr);

            float wait = i_lambda * Mathf.Exp(-i_lambda * ((float)rand.NextDouble() * (i_max - i_min) + i_min));

            currPhase = Phases.ITI;

            interWait.Add(wait);

            isEnd = true;

            distances.Clear();
            ffPosStr = "";
            isTimeout = false;

            if (PlayerPrefs.GetInt("isColored") == 1)
            {
                ffCol.Add(string.Format("{0},{1}", colorchosen[0], colorchosen[1]));
                colorchosen.Clear();
            }

            await new WaitForSeconds(wait);

            phase = Phases.begin;
        }
        else
        {
            timedout.Add(isTimeout ? 1 : 0);
            score.Add(isReward && proximity ? 1 : 0);
            ffPos.Add(ffPosStr);
            dist.Add(distances[0].ToString("F5"));
            cPos.Add(pos.ToString("F5").Trim(toTrim).Replace(" ", ""));
            cRot.Add(rot.ToString("F5").Trim(toTrim).Replace(" ", ""));


            timeCntPTBStart.Add(SharedJoystick.timeCntPTBStart - programT0);
            SharedJoystick.timeCntPTBStart = programT0;

            ptbJoyVelMin.Add(SharedJoystick.ptbJoyVelMin);
            ptbJoyVelMax.Add(SharedJoystick.ptbJoyVelMax);
            ptbJoyVelStartRange.Add(SharedJoystick.ptbJoyVelStartRange);
            ptbJoyVelStart.Add(SharedJoystick.ptbJoyVelStart);
            ptbJoyVelMu.Add(SharedJoystick.ptbJoyVelMu);
            ptbJoyVelSigma.Add(SharedJoystick.ptbJoyVelSigma);
            ptbJoyVelGain.Add(SharedJoystick.ptbJoyVelGain);
            ptbJoyVelEnd.Add(SharedJoystick.ptbJoyVelEnd);
            ptbJoyVelLen.Add(SharedJoystick.ptbJoyVelLen);
            ptbJoyVelValue.Add(SharedJoystick.ptbJoyVelValue);

            ptbJoyRotMin.Add(SharedJoystick.ptbJoyRotMin);
            ptbJoyRotMax.Add(SharedJoystick.ptbJoyRotMax);
            ptbJoyRotStartRange.Add(SharedJoystick.ptbJoyRotStartRange);
            ptbJoyRotStart.Add(SharedJoystick.ptbJoyRotStart);
            ptbJoyRotMu.Add(SharedJoystick.ptbJoyRotMu);
            ptbJoyRotSigma.Add(SharedJoystick.ptbJoyRotSigma);
            ptbJoyRotGain.Add(SharedJoystick.ptbJoyRotGain);
            ptbJoyRotEnd.Add(SharedJoystick.ptbJoyRotEnd);
            ptbJoyRotLen.Add(SharedJoystick.ptbJoyRotLen);
            ptbJoyRotValue.Add(SharedJoystick.ptbJoyRotValue);

            ptbJoyFlag.Add(SharedJoystick.ptbJoyFlag);
            ptbJoyFlagTrial.Add(SharedJoystick.ptbJoyFlagTrial);
            SharedJoystick.ptbJoyFlagTrial = Convert.ToInt32(rand.NextDouble() <= SharedJoystick.ptbJoyRatio); ;

            ptbJoyRatio.Add(SharedJoystick.ptbJoyRatio);
            ptbJoyOn.Add(SharedJoystick.ptbJoyOn);
            ptbJoyEnableTime.Add(SharedJoystick.ptbJoyEnableTime);

            if(PlayerPrefs.GetInt("isFFstimu") == 1)
            {
                if (stimulatedTrial)
                {
                    trialStimuDur.Add(microStimuDur);
                    stimulated.Add(1);
                }
                else
                {
                    timeStimuStart.Add(0);
                    trialStimuDur.Add(0);
                    stimulated.Add(0);
                }
            }

            ffPositions.Clear();
            distances.Clear();
            ffPosStr = "";

            isTimeout = false;

            float wait = i_lambda * Mathf.Exp(-i_lambda * ((float)rand.NextDouble() * (i_max - i_min) + i_min));
            if (PlayerPrefs.GetInt("isFFstimu") == 1 && stimulatedTrial)
            {
                stimulatedTrial = false;
                wait += microStimuDur; //wait more if it was a stimulated trail
            }

            currPhase = Phases.ITI;

            interWait.Add(wait);

            //Debug.Log("Trial End: " + DateTime.Now.ToLongTimeString());

            isEnd = true;

            //print(wait);
            isIntertrail = true;
            float joystickT = PlayerPrefs.GetFloat("JoystickThreshold");
            float startthreshold = PlayerPrefs.GetFloat("JoystickStartThreshold");
            if (!isCOM)
            {
                player.transform.position = Vector3.up * p_height;
                player.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                drunkplayer.transform.position = Vector3.up * p_height;
                drunkplayer.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                await new WaitUntil(() => Mathf.Abs(SharedJoystick.currentSpeed) < velStopThreshold && Mathf.Abs(SharedJoystick.currentRot) < rotStopThreshold && (float)Math.Abs(SharedJoystick.rawX) <= startthreshold && (float)Math.Abs(SharedJoystick.rawY) <= startthreshold);
                await new WaitForSeconds(wait);
            }
            else
            {
                await new WaitUntil(() => Mathf.Abs(SharedJoystick.currentSpeed) < velStopThreshold && Mathf.Abs(SharedJoystick.currentRot) < rotStopThreshold);
                await new WaitForSeconds(wait);
                player.transform.position = Vector3.up * p_height;
                player.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                drunkplayer.transform.position = Vector3.up * p_height;
                drunkplayer.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            }

            phase = Phases.begin;
            Debug.Log("Check Phase End.");
        }

        timeCounter = 0;
    }

    /// <summary>
    /// Used when user specifies that the FF flashes.
    /// 
    /// Pulse Width (s) is the length of the pulse, i.e. how long the firefly stays on. This
    /// is calculated with Duty Cycle (%), which is a percentage of the frequency of the
    /// desired signal. Frequency (Hz) is how often you want the object to flash per second.
    /// Here, we have 1 / frequency because the inverse of frequency is Period (s), denoted
    /// as T, which is the same definition as Frequency except it is given in seconds.
    /// </summary>
    /// <param name="obj">Object to flash</param>
    public async Task Flash(GameObject obj)
    {
        while (on)
        {
            if (toggle && !obj.activeInHierarchy)
            {
                obj.GetComponent<SpriteRenderer>().enabled = false;
            }
            else
            {
                obj.GetComponent<SpriteRenderer>().enabled = true;
                await new WaitForSeconds(PW);
                obj.GetComponent<SpriteRenderer>().enabled = false;
                await new WaitForSeconds((1f / freq) - PW);
            }
        }
    }

    public async void OnOff()
    {
        firefly.SetActive(true);
        await new WaitForSeconds(lifeSpan);
        firefly.SetActive(false);
    }

    public async void OnOff(GameObject obj)
    {
        obj.SetActive(true);
        await new WaitForSeconds(lifeSpan);
        obj.SetActive(false);
    }

    public async void SendMarker(string mark, float time)
    {
        string toSend = "i" + mark + time.ToString();
        
        switch (mark)
        {
            case "j":
                rewardTime.Add(Time.realtimeSinceStartup - programT0);
                break;
            case "s":
                beginTime.Add(Time.realtimeSinceStartup - programT0);
                break;
            case "e":
                endTime.Add(Time.realtimeSinceStartup - programT0);
                break;
            default:
                break;
        }

        if(PlayerPrefs.GetFloat("calib") != 0)
        {
            juiceBox.Write(toSend);
        }

        await new WaitForSeconds(time / 1000.0f);
    }

    private float Tcalc(float t, float lambda)
    {
        return -1.0f / lambda * Mathf.Log(t / lambda);
    }

    public float RandomizeSpeeds(float min, float max)
    {
        return (float)(rand.NextDouble() * (max - min) + min);
    }

    /// <summary>
    /// If you provide filepaths beforehand, the program will save all of your data as .csv files.
    /// 
    /// I did something weird where I saved the rotation/position data as strings; I did this
    /// because the number of columns that the .csv file will have will vary depending on the
    /// number of FF. Each FF has it's own position and distance from the player, and that data
    /// has to be saved along with everything else, and I didn't want to allocate memory for all
    /// the maximum number of FF if not every experiment will have 5 FF, so concatenating all of
    /// the available FF positions and distances into one string and then adding each string as
    /// one entry in a list was my best idea.
    /// </summary>
    public void Save()
    {
        try
        {
            string firstLine;

            List<int> temp;

            StringBuilder csvDisc = new StringBuilder();

            if (nFF > 1)
            {
                string ffPosStr = "";
                string distStr = "";
                string checkStr = "";

                for (int i = 0; i < nFF; i++)
                {
                    ffPosStr = string.Concat(ffPosStr, string.Format("ffX{0},ffY{0},ffZ{0},", i));
                    distStr = string.Concat(distStr, string.Format("distToFF{0},", i));
                    checkStr = string.Concat(checkStr, string.Format("checkTime{0},", i));
                }

                firstLine = string.Format("n,max_v,max_w,ffv,onDuration,density,PosX0,PosY0,PosZ0,RotX0,RotY0,RotZ0,RotW0,{0}pCheckX,pCheckY,pCheckZ,rCheckX,rCheckY,rCheckZ,rCheckW,{1}rewarded,", ffPosStr, distStr) +
                    "timeout,juiceDuration,beginTime,checkTime,rewardTime,endTime,checkWait,interWait,CurrentTau,PTBType,SessionTauTau,ProcessNoiseTau,ProcessNoiseVelGain,ProcessNoiseRotGain,nTaus,minTaus,maxTaus,MeanDist," +
                    "MeanTravelTime,VelStopThresh,RotStopThresh,VelBrakeThresh,RotBrakeThresh,StimulationTime,StimulationDuration,StimulationRatio,ObsNoiseTau,ObsNoiseVelGain,ObsNoiseRotGain,DistractorFlowRatio,ColoredOpticFlow,COMTrialType,"
                    + PlayerPrefs.GetString("Name") + "," + PlayerPrefs.GetString("Date") + "," + PlayerPrefs.GetInt("Run Number").ToString("D3");
            }
            else
            {
                firstLine = "n,max_v,max_w,ffv,onDuration,density,PosX0,PosY0,PosZ0,RotX0,RotY0,RotZ0,RotW0,ffX,ffY,ffZ,pCheckX,pCheckY,pCheckZ,rCheckX,rCheckY,rCheckZ,rCheckW,distToFF,rewarded," +
                    "timeout,juiceDuration,beginTime,checkTime,rewardTime,endTime,checkWait,interWait,CurrentTau,PTBType,SessionTauTau,ProcessNoiseTau,ProcessNoiseVelGain,ProcessNoiseRotGain,nTaus,minTaus,maxTaus,MeanDist," +
                    "MeanTravelTime,VelStopThresh,RotStopThresh,VelBrakeThresh,RotBrakeThresh,StimulationTime,StimulationDuration,StimulationRatio,ObsNoiseTau,ObsNoiseVelGain,ObsNoiseRotGain,DistractorFlowRatio,ColoredOpticFlow,COMTrialType,"
                    + PlayerPrefs.GetString("Name") + "," + PlayerPrefs.GetString("Date") + "," + PlayerPrefs.GetInt("Run Number").ToString("D3");
            }
            csvDisc.AppendLine(firstLine);

            temp = new List<int>()
            {
                origin.Count,
                heading.Count,
                ffPos.Count,
                dist.Count,
                n.Count,
                cPos.Count,
                cRot.Count,
                beginTime.Count,
                rewardTime.Count,
                endTime.Count,
                checkWait.Count,
                interWait.Count,
                score.Count,
                timedout.Count,
                max_v.Count,
                max_w.Count,
                fv.Count,
                onDur.Count,
                densities.Count,
                juiceDuration.Count,
                densities_obsRatio.Count
            };

            if (nFF > 1 && multiMode == 1)
            {
                temp.Add(checkTimeStrList.Count);
            }
            else
            {
                temp.Add(checkTime.Count);
            }

            if (PlayerPrefs.GetInt("isColored") == 1)
            {
                temp.Add(ffCol.Count);
            }
            if (PlayerPrefs.GetInt("isFFstimu") == 1)
            {
                temp.Add(stimulated.Count);
                temp.Add(timeStimuStart.Count);
                temp.Add(trialStimuDur.Count);
            }
            if (isCOM)
            {
                temp.Add(COMtrialtype.Count);
            }
            //foreach (int count in temp)
            //{
            //    print(count);
            //}
            temp.Sort();

            var totalScore = 0;
            int j;

            if (ptb != 2)
            {
                j = 0;
                temp.Add(CurrentTau.Count);
            }
            else
            {
                j = 0;
            }

            for (int i = j; i < temp[0]; i++)
            {
                var line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20}",
                    n[i],
                    max_v[i],
                    max_w[i],
                    fv[i],
                    onDur[i],
                    densities[i],
                    origin[i],
                    heading[i],
                    ffPos[i],
                    cPos[i],
                    cRot[i],
                    dist[i],
                    score[i],
                    timedout[i],
                    juiceDuration[i],
                    beginTime[i],
                    checkTime[i].ToString("F5"),
                    rewardTime[i],
                    endTime[i],
                    checkWait[i],
                    interWait[i]);

                if (ptb != 2)
                {
                    line = line + "," + CurrentTau[i];
                    line = line + ',' + SharedJoystick.flagPTBType + ',' + SharedJoystick.TauTau + ',' + SharedJoystick.NoiseTau + ',' + PlayerPrefs.GetFloat("VelocityNoiseGain") + ',' +
            PlayerPrefs.GetFloat("RotationNoiseGain") + ',' + (int)PlayerPrefs.GetFloat("NumTau") + ',' + PlayerPrefs.GetFloat("MinTau") + ',' + PlayerPrefs.GetFloat("MaxTau")
            + ',' + PlayerPrefs.GetFloat("MeanDistance") + ',' + PlayerPrefs.GetFloat("MeanTime") + ',' + velStopThreshold + ',' + rotStopThreshold + ',' + PlayerPrefs.GetFloat("velBrakeThresh")
            + ',' + PlayerPrefs.GetFloat("rotBrakeThresh");
                }
                else
                {
                    line += string.Format(",0,0,0,0,0,0,0,0,0,0,0,0,0,0,0");
                }

                if (PlayerPrefs.GetInt("isFFstimu") == 1)
                {
                    line += string.Format(",{0},{1},{2}", timeStimuStart[i], trialStimuDur[i], stimuratio);
                }
                else
                {
                    line += string.Format(",0,0,0");
                }

                if (isObsNoise)
                {
                    line += string.Format(",{0},{1},{2},{3}", ObsNoiseTau, ObsVelocityNoiseGain, ObsRotationNoiseGain, densities_obsRatio[i]);
                }
                else
                {
                    line += string.Format(",0,0,0,0");
                }

                if (PlayerPrefs.GetInt("isColored") == 1)
                {
                    line += string.Format(",{0}", ffCol[i]);
                }
                else
                {
                    line += string.Format(",0");
                }

                if (isCOM)
                {
                    line += string.Format(",{0}", COMtrialtype[i]);
                }
                else
                {
                    line += string.Format(",0");
                }

                csvDisc.AppendLine(line);

                totalScore += score[i];
            }
            string discPath = path + "/discontinuous_data_" + PlayerPrefs.GetString("Name") + "_" + DateTime.Today.ToString("MMddyyyy") + "_" + PlayerPrefs.GetInt("Run Number").ToString("D3") + ".txt";

            File.WriteAllText(discPath, csvDisc.ToString());

            if (PlayerPrefs.GetFloat("calib") == 0)
            {
                string contpath = path + "/continuous_data_" + PlayerPrefs.GetString("Name") + "_" + DateTime.Today.ToString("MMddyyyy") + "_" + PlayerPrefs.GetInt("Run Number").ToString("D3") + ".txt";
                File.AppendAllText(contpath, sb.ToString());
            }

            PlayerPrefs.SetInt("Good Trials", totalScore);
            print(temp[0]);
            PlayerPrefs.SetInt("Total Trials", n[n.Count - 1]);

            SaveConfigs();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError(e);
        }
    }

    void drawLine(float radius, int segments)
    {
        LineRenderer lr;
        lr = line.GetComponent<LineRenderer>();
        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i < segments; i++)
        {
            float angle = ((float)i / (float)segments) * 360 * Mathf.Deg2Rad;
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;
            points[i] = new Vector3(x, 0f, z);
        }
        points[segments] = points[0];
        lr.positionCount = segments + 1;
        lr.SetPositions(points);
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

    public void SaveConfigs()
    {
        print("Saving Configs");

        System.IO.Directory.CreateDirectory(path + "/configs/");
        string configPath = path + "/configs/" + "config" + "_" + PlayerPrefs.GetString("Name") + "_" + DateTime.Today.ToString("yyyyMMdd") + "_" + PlayerPrefs.GetInt("Run Number").ToString("D3") + ".xml";

        var settings = new XmlWriterSettings();
        settings.OmitXmlDeclaration = true;
        settings.Indent = true;
        settings.NewLineOnAttributes = true;

        XmlWriter xmlWriter = XmlWriter.Create(configPath, settings);

        xmlWriter.WriteStartDocument();

        xmlWriter.WriteStartElement("Settings");

        xmlWriter.WriteStartElement("Setting");
        xmlWriter.WriteAttributeString("Type", "Optic Flow Settings");

        xmlWriter.WriteStartElement("LifeSpan");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Life Span").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("DrawDistance");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Draw Distance").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("DensityLow");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Density Low").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("DensityHigh");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Density High").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("DensityLowRatio");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Density Low Ratio").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("TriangleHeight");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Triangle Height").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("PlayerHeight");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Player Height").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Setting");
        xmlWriter.WriteAttributeString("Type", "Joystick Settings");

        xmlWriter.WriteStartElement("MinLinearSpeed");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Min Linear Speed").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("MaxLinearSpeed");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Max Linear Speed").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("MinAngularSpeed");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Min Angular Speed").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("MaxAngularSpeed");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Max Angular Speed").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("PerturbationOn");
        xmlWriter.WriteString(PlayerPrefs.GetInt("Perturbation On").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("PerturbVelocityMin");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Perturb Velocity Min").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("PerturbVelocityMax");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Perturb Velocity Max").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("PerturbRotationMin");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Perturb Rotation Min").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("PerturbRotationMax");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Perturb Rotation Max").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("PerturbRatio");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("PerturbRatio").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("LinearThreshold");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("LinearThreshold").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("AngularThreshold");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("AngularThreshold").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Setting");
        xmlWriter.WriteAttributeString("Type", "Firefly Settings");

        xmlWriter.WriteStartElement("RadiusFF");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("RadiusFF").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("RewardZoneRadius");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Reward Zone Radius").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("MinimumFireflyDistance");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Minimum Firefly Distance").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("MaximumFireflyDistance");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Maximum Firefly Distance").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("MinAngle");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Min Angle").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("MaxAngle");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Max Angle").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("MinJuiceTime");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Min Juice Time").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("MaxJuiceTime");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Max Juice Time").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Ratio");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Ratio").ToString());
        xmlWriter.WriteEndElement();

        //xmlWriter.WriteStartElement("Reward");
        //xmlWriter.WriteString(PlayerPrefs.GetFloat("Reward").ToString());
        //xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("NumberofFireflies");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Number of Fireflies").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("MultipleFireflyMode");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Multiple Firefly Mode").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Separation");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Seaparation").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("D1");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("D1").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("D2");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("D2").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("D3");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("D3").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("D4");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("D4").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("D5");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("D5").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("R1");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("R1").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("R2");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("R2").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("R3");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("R3").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("R4");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("R4").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("R5");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("R5").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Timeout");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Timeout").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Frequency");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Frequency").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("DutyCycle");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Duty Cycle").ToString());
        xmlWriter.WriteEndElement();

        //xmlWriter.WriteStartElement("FireflyLifeSpan");
        //xmlWriter.WriteString(PlayerPrefs.GetFloat("Firefly Life Span").ToString());
        //xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("MinimumWaittoCheck");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Minimum Wait to Check").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("MaximumWaittoCheck");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Maximum Wait to Check").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Mean1");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Mean 1").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("MinimumIntertrialWait");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Minimum Intertrial Wait").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("MaximumIntertrialWait");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Maximum Intertrial Wait").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Mean2");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Mean 2").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("OpticFlowSeed");
        xmlWriter.WriteString(PlayerPrefs.GetInt("Optic Flow Seed").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("FireflySeed");
        xmlWriter.WriteString(seed.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Setting");
        xmlWriter.WriteAttributeString("Type", "Moving Firefly Settings");

        xmlWriter.WriteStartElement("MovingON");
        xmlWriter.WriteString(PlayerPrefs.GetInt("Moving ON").ToString());
        xmlWriter.WriteEndElement();

        //xmlWriter.WriteStartElement("RatioMoving");
        //xmlWriter.WriteString(PlayerPrefs.GetFloat("Ratio Moving").ToString());
        //xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("VertHor");
        xmlWriter.WriteString(PlayerPrefs.GetInt("VertHor").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("V1");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("V1").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("V2");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("V2").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("V3");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("V3").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("V4");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("V4").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("V5");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("V5").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("V6");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("V6").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("V7");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("V7").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("V8");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("V8").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("V9");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("V9").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("V10");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("V10").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("V11");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("V11").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("V12");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("V12").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("VR1");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("VR1").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("VR2");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("VR2").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("VR3");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("VR3").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("VR4");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("VR4").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("VR5");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("VR5").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("VR6");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("VR6").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("VR7");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("VR7").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("VR8");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("VR8").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("VR9");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("VR9").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("VR10");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("VR10").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("VR11");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("VR11").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("VR12");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("VR12").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Setting");
        xmlWriter.WriteAttributeString("Type", "Data Collection Settings");

        xmlWriter.WriteStartElement("Path");
        xmlWriter.WriteString(PlayerPrefs.GetString("Path"));
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Name");
        xmlWriter.WriteString(PlayerPrefs.GetString("Name"));
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Date");
        xmlWriter.WriteString(PlayerPrefs.GetString("Date"));
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("RunNumber");
        xmlWriter.WriteString((PlayerPrefs.GetInt("Run Number") + 1).ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Setting");
        xmlWriter.WriteAttributeString("Type", "Calibration Settings");

        xmlWriter.WriteStartElement("isAuto");
        xmlWriter.WriteString(PlayerPrefs.GetInt("isAuto").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("GracePeriod");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Grace Period").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("FixationTime");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Fixation Time").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("IntertrialInterval");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Intertrial Interval").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("XThreshold");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("X Threshold").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("YThreshold");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Y Threshold").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("MarkerSize");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Marker Size").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("CalibrationJuiceTime");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Calibration Juice Time").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Setting");
        xmlWriter.WriteAttributeString("Type", "Perturbation Settings");

        xmlWriter.WriteStartElement("isProcessNoise");
        xmlWriter.WriteString(PlayerPrefs.GetInt("isProcessNoise").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("PTBType");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("PTBType").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("TauColoredFloor");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("TauColoredFloor").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("MeanDistance");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("MeanDistance").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("MeanTime");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("MeanTime").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("MinTau");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("MinTau").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("MaxTau");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("MaxTau").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("NumTau");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("NumTau").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("TauTau");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("TauTau").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("rotStopThreshold");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("rotStopThreshold").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("velStopThreshold");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("velStopThreshold").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("JoystickThreshold");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("JoystickThreshold").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("NoiseTau");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("NoiseTau").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("NoiseTauTau");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("NoiseTauTau").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("VelocityNoiseGain");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("VelocityNoiseGain").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("RotationNoiseGain");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("RotationNoiseGain").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("LinearNoiseScale");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("LinearNoiseScale").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("RotationNoiseScale");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("RotationNoiseScale").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("MeanAngle");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("MeanAngle").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("ThreshTauMultiplier");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("ThreshTauMultiplier").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("velBrakeThresh");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("velBrakeThresh").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("rotBrakeThresh");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("rotBrakeThresh").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("JoystickStartThreshold");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("JoystickStartThreshold").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("StimuITI");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("StimuITI").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("StimuTrialDur");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("StimuTrialDur").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("StimuStimuDur");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("StimuStimuDur").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("StimuRewardDur");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("StimuRewardDur").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("StimuGapMin");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("StimuGapMin").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("StimuGapMax");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("StimuGapMax").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("StimNumTrials");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("StimNumTrials").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("RewardGap");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("RewardGap").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("RewardThresh");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("RewardThresh").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("StimuAmp");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("StimuAmp").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("FFstimugap");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("FFstimugap").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("isFFstimu");
        xmlWriter.WriteString(PlayerPrefs.GetInt("isFFstimu").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("StimulationRatio");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("StimulationRatio").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("isObsNoise");
        xmlWriter.WriteString(PlayerPrefs.GetInt("isObsNoise").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("ObsNoiseTau");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("ObsNoiseTau").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("ObsVelocityNoiseGain");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("ObsVelocityNoiseGain").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("ObsRotationNoiseGain");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("ObsRotationNoiseGain").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("ObsDensityRatio");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("ObsDensityRatio").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Setting");
        xmlWriter.WriteAttributeString("Type", "Calibration Parameters");

        xmlWriter.WriteStartElement("xSliderValue");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("xSliderValue").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("ySliderValue");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("ySliderValue").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("markerSliderValue");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("markerSliderValue").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("xScaleSliderValue");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("xScaleSliderValue").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("yScaleSliderValue");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("yScaleSliderValue").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("xOffsetSliderValue");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("xOffsetSliderValue").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("yOffsetSliderValue");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("yOffsetSliderValue").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Setting");
        xmlWriter.WriteAttributeString("Type", "moreOnFF");

        xmlWriter.WriteStartElement("red");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("red").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("blue");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("blue").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("green");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("green").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("yellow");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("yellow").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("white");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("white").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("redrew");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("redrew").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("bluerew");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("bluerew").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("greenrew");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("greenrew").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("yellowrew");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("yellowrew").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("whiterew");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("whiterew").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("isColored");
        xmlWriter.WriteString(PlayerPrefs.GetInt("isColored").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("is2FFCOM");
        xmlWriter.WriteString(PlayerPrefs.GetInt("is2FFCOM").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("COMNormal");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("COMNormal").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("COM2FF");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("COM2FF").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Sta2FF");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Sta2FF").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteEndElement();

        xmlWriter.WriteEndDocument();
        xmlWriter.Close();
    }

    public void ReadCoordCSV()
    {
        StreamReader strReader = new StreamReader(path + "\\Config_2FF.csv");
        bool endoffile = false;
        while (!endoffile)
        {
            string data_string = strReader.ReadLine();
            if(data_string == null)
            {
                break;
            }
            var data_values = data_string.Split(',');
            Tuple<float, float> New_Coord_Tuple;
            float x = float.Parse(data_values[0], CultureInfo.InvariantCulture.NumberFormat);
            float y = float.Parse(data_values[1], CultureInfo.InvariantCulture.NumberFormat);
            New_Coord_Tuple = new Tuple<float, float>(x/10, y);
            FFcoordsList.Add(New_Coord_Tuple);
        }
    }

    public void ReadFFCoordDisc()
    {
        StreamReader strReader = new StreamReader("C:\\Users\\lab\\Desktop\\data\\noises-stable\\proc.txt");
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
            float x = float.Parse(data_values[13], CultureInfo.InvariantCulture.NumberFormat);
            float y = float.Parse(data_values[15], CultureInfo.InvariantCulture.NumberFormat);
            New_Coord_Tuple = new Tuple<float, float>(x, y);
            FFTagetMatchList.Add(New_Coord_Tuple);
        }
    }
}