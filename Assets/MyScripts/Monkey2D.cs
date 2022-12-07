﻿///////////////////////////////////////////////////////////////////////////////////////////
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

public class Monkey2D : MonoBehaviour
{
    //Shared instance of script
    public static Monkey2D SharedMonkey;

    //FF
    public GameObject firefly;
    //Size of FF (diameter)
    public float fireflySize;
    //Maximum distance allowed from center of firefly
    public float fireflyZoneRadius;
    //Min/max of which FF can spawn
    public float minDrawDistance;
    public float maxDrawDistance;
    //Min/max angle from forward(y) axis FF can spawn
    public float minPhi;
    public float maxPhi;
    //Left-Right ratio, spawn FF more on the left or right, [0:0.5:5] = [left:equal:right]
    public float LR;
    //Lifespan of the FF, if applicable
    public float lifeSpan;
    //possible Lifespans and ratios
    readonly public List<float> lifespan_durations = new List<float>();
    readonly public List<float> lifespan_ratios = new List<float>();
    //Number of FFs, if applicable
    public float nFF;
    //Multiple FF mode, 0 for don't apply, 1 for normal multiple, 2 for COM
    int multiple_FF_mode;

    //Moving FF, FF velocities and their corresponding ratios, and noises
    private bool isMoving;
    readonly public List<float> velocities = new List<float>();
    readonly public List<float> v_ratios = new List<float>();
    readonly public List<float> v_noises = new List<float>();
    //FF moves left-right or forward-backward? true for lr, false for fb
    private bool LRFB;
    //FF move direction
    private Vector3 direction = new Vector3();
    private bool noised_moving_FF;
    public GameObject line;
    private bool lineOnOff;

    //Cameras
    public Camera Lcam;
    public Camera Rcam;
    public Camera LObscam;
    public Camera RObscam;
    public Camera DrunkCam;

    //Gaze visualized from eye tracker
    [HideInInspector] public GazeVisualizer gazeVisualizer;

    //Arduino
    public int baudRate = 2000000;
    public int ReadTimeout = 5000;

    // Use flashing FF?(for training, normal FF task only)
    public bool flashing;
    //Frequency of flashing firefly(Flashing Firefly Only)
    public float flashing_frequency;
    //Flashing duty cycle
    public float duty_cycle;
    // Pulse Width; how long in seconds it stays on during one period
    private float Pulse_Width;

    // Toggle for whether trial is an always on trial or not
    public bool is_always_on_trial;
    //Ratio of trials that will have fireflies always on
    public float ratio_always_on;

    //player objects, real and fake
    public GameObject player;
    public GameObject drunkplayer;
    //player height
    public float p_height;

    //sounds
    public AudioSource audioSource;
    public AudioClip winSound;
    public AudioClip neutralSound;
    public AudioClip loseSound;

    //Colored FF
    readonly public List<float> colorratios = new List<float>();
    readonly public List<float> colorrewards = new List<float>();
    private List<float> colorchosen = new List<float>();
    private float colorhit = 0;

    private Vector3 move;
    //Trial timeout (how much time player can stand still before trial ends
    public float timeout;
    //Check time interval min max
    public float checkMin;
    public float checkMax;
    //Intertrail interval min max
    public float interMax;
    public float interMin;

    // 1 / Mean for check time exponential distribution
    private float c_lambda;
    // 1 / Mean for intertrial time exponential distribution
    private float i_lambda;
    // x values for exponential distribution
    private float c_min;
    private float c_max;
    private float i_min;
    private float i_max;

    //Speed max and min
    private float velMin;
    private float velMax;
    private float rotMin;
    private float rotMax;

    //Player position
    private Vector3 player_position;

    //Trial timeout?
    private bool isTimeout = false;

    // Discontinuous Data
    // Trial number
    readonly List<int> trial_number = new List<int>();
    // Firefly ON Duration
    readonly List<float> FF_on_duration = new List<float>();
    //Firefly Colors
    readonly List<string> FF_color = new List<string>();
    // Firefly Check Coords
    readonly List<string> FF_final_positions = new List<string>();
    // FF individual positions
    public readonly List<Vector3> FF_positions = new List<Vector3>();
    // Player position at Check()
    readonly List<string> player_final_position = new List<string>();
    // Player rotation at Check()
    readonly List<string> player_final_rotation = new List<string>();
    // Player origin at beginning of trial
    readonly List<string> player_starting_position = new List<string>();
    // Player rotation at origin
    readonly List<string> player_starting_rotation = new List<string>();
    // Player linear and angular velocity
    readonly List<float> player_max_vel = new List<float>();
    readonly List<float> player_max_rot = new List<float>();
    //PTBTau
    readonly List<float> CurrentTau = new List<float>();
    // Firefly velocity
    readonly List<float> FF_velocity = new List<float>();
    // Distances from player to firefly
    readonly List<string> distance_to_FF = new List<string>();
    readonly List<float> distances_to_FF = new List<float>();
    // Times
    readonly List<float> beginTime = new List<float>();
    readonly List<float> checkTime = new List<float>();
    readonly List<float> rewardTime = new List<float>();
    readonly List<float> juiceDuration = new List<float>();
    readonly List<float> endTime = new List<float>();
    readonly List<float> checkWait = new List<float>();
    readonly List<float> interWait = new List<float>();
    // add when firefly disappears

    // density
    readonly List<float> optic_flow_densities = new List<float>();
    readonly List<float> optic_flow_densities_obsRatio = new List<float>();
    // Rewarded?
    readonly List<int> score = new List<int>();
    // Stimulated?
    private float stimuratio;
    private bool stimulatedTrial = false;
    readonly List<int> stimulated = new List<int>();
    readonly List<float> timeStimuStart = new List<float>();
    readonly List<float> trialStimuDur = new List<float>();
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

    // File paths
    private string path;

    //Trial Number
    [HideInInspector] public int trialNum;

    //Program Start Time
    [HideInInspector] public float programT0 = 0.0f;

    //Number of good trials so far
    [HideInInspector] public float good_trial_count = 0;
    
    //Juice Times
    public float juiceTime;
    private float minJuiceTime;
    private float maxJuiceTime;

    //number of trials desired, 0 for infinity
    public int ntrials;

    //Randomization
    private int seed;
    private System.Random rand;

    //Flashing FF
    private bool flashing_FF_on = true;

    //Stimulation trial start time
    private float startTime;

    //2FF player moving start time
    private float MoveStartTime;

    //Phases
    private bool isBegin = false;
    private bool isTrial = false;
    private bool isCheck = false;
    private bool isEnd = false;
    public bool isIntertrail = false;
    public enum Phases
    {
        begin = 0,
        trial = 1,
        check = 2,
        juice = 3,
        ITI = 4,
        none = 9,
    }
    [HideInInspector] public Phases phase;
    //Current phase
    [HideInInspector] public Phases currPhase;

    //List of FFs for multiple FF
    readonly private List<GameObject> Multiple_FF_List = new List<GameObject>();

    //Trim the brackets
    private readonly char[] toTrim = { '(', ')' };

    //Moving FF task with noise
    public float velocity;
    public float noise_SD;
    public float velocity_Noised;

    //Player trial starting position
    public Vector3 player_trial_origin;

    //Observation Noise
    public ParticleSystem particle_System;
    public ParticleSystem particle_System2;
    private bool isObsNoise;
    private float ObsNoiseTau;
    private float ObsVelocityNoiseGain;
    private float ObsRotationNoiseGain;
    private float ObsDensityRatio;

    //Async Tasking
    CancellationTokenSource source;
    private Task currentTask;
    private Task flashTask;
    private bool playing = true;
    public bool stimulating = false;

    //VR cameras
    public float offset = 0.01f;
    private float lm02;
    private float rm02;
    private Matrix4x4 lm;
    private Matrix4x4 rm;

    //Juice
    SerialPort juiceBox;

    //observation noise
    public float DistFlowSpeed = 0;
    public float DistFlowRot = 0;

    //Multiple FF separation
    float separation;

    //Close enough to the FF to get reward?
    bool FF_proximity;

    //Multiple FF position strings
    string ffPosStr = "";

    //Perturbation
    int control_dynamics;
    float velbrakeThresh;
    float rotbrakeThresh;
    float velStopThreshold;
    float rotStopThreshold;

    //Continuous Data Saving
    StringBuilder sb_cont_data = new StringBuilder();
    //send sb packet to matlab
    public string sbPacket;

    //Is task multiple FF
    bool flagMultiFF;

    //Micro Stimulation
    private float microStimuDur;
    private float microStimuGap;
    private float trialStimuGap;

    //Selfmotion experiment or not (moving FF)
    bool SMtrial = false;

    //2FF change of mind task
    readonly List<float> COMtrialtype = new List<float>();
    public bool isNormal = false;
    public bool isStatic2FF = false;
    public bool isCOM2FF = false;
    public bool isCOM = false;
    bool FF2shown = false;
    bool startedMoving = false;
    float FF2delay;
    float normalRatio;
    float normal2FFRatio;
    int FF1index;
    readonly List<Tuple<float, float>> FFcoordsList = new List<Tuple<float, float>>();

    //On Start up, get gaze visualizer
    private void Awake()
    {
        if(PlayerPrefs.GetFloat("calib") == 1)
        {
            gazeVisualizer = GameObject.Find("Gaze Visualizer").GetComponent<GazeVisualizer>();
        }
    }

    // Program start up
    void OnEnable()
    {
        //Desired Frame rate is always 90hz to make up to Vive headsets
        Application.targetFrameRate = 90;

        //Keep app running in background
        Application.runInBackground = true;

        //Juice connection
        juiceBox = serial.sp;
        minJuiceTime = PlayerPrefs.GetFloat("Min Juice Time");
        maxJuiceTime = PlayerPrefs.GetFloat("Max Juice Time");

        //Send block start marker
        SendMarker("f", 1000.0f);
        programT0 = Time.realtimeSinceStartup;

        //VR set up
        InputTracking.disablePositionalTracking = true;
        XRDevice.DisableAutoXRCameraTracking(LObscam, true);
        XRDevice.DisableAutoXRCameraTracking(RObscam, true);
        XRDevice.DisableAutoXRCameraTracking(Lcam, true);
        XRDevice.DisableAutoXRCameraTracking(Rcam, true);
        XRSettings.occlusionMaskScale = 10f;
        XRSettings.useOcclusionMesh = false;
        Lcam.ResetProjectionMatrix();
        Rcam.ResetProjectionMatrix();
        LObscam.ResetProjectionMatrix();
        RObscam.ResetProjectionMatrix();

        //VR cameras set up
        lm = Lcam.projectionMatrix;
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
        RObscam.projectionMatrix = rm;

        List<XRDisplaySubsystem> displaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances(displaySubsystems);
        if (!XRSettings.enabled)
        {
            XRSettings.enabled = true;
        }

        //Shared instance
        SharedMonkey = this;

        //Get variables from settings
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
        multiple_FF_mode = PlayerPrefs.GetInt("Multiple Firefly Mode");
        separation = PlayerPrefs.GetFloat("Separation");
        minDrawDistance = PlayerPrefs.GetFloat("Minimum Firefly Distance");
        maxDrawDistance = PlayerPrefs.GetFloat("Maximum Firefly Distance");
        microStimuDur = PlayerPrefs.GetFloat("StimuStimuDur");
        microStimuGap = PlayerPrefs.GetFloat("FFstimugap");
        stimuratio = PlayerPrefs.GetFloat("StimulationRatio");
        control_dynamics = (int)PlayerPrefs.GetFloat("PTBType");
        isObsNoise = PlayerPrefs.GetInt("isObsNoise") == 1;
        ObsNoiseTau = PlayerPrefs.GetFloat("ObsNoiseTau");
        ObsVelocityNoiseGain = PlayerPrefs.GetFloat("ObsVelocityNoiseGain");
        ObsRotationNoiseGain = PlayerPrefs.GetFloat("ObsRotationNoiseGain");
        ObsDensityRatio = PlayerPrefs.GetFloat("ObsDensityRatio");
        SMtrial = PlayerPrefs.GetInt("isSM") == 1;
        isCOM = PlayerPrefs.GetInt("is2FFCOM") == 1;

        //2FF Change of mind?
        if (isCOM)
        {
            nFF = 2;
            FFcoordsList.Clear();
            ReadCoordCSV();
        }
        normalRatio = PlayerPrefs.GetFloat("COMNormal");
        normal2FFRatio = PlayerPrefs.GetFloat("Sta2FF");
        normal2FFRatio += normalRatio;

        //Observation Noise?
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

        //Control Dynamics?
        if (control_dynamics != 0)
        {
            velStopThreshold = PlayerPrefs.GetFloat("velStopThreshold");
            rotStopThreshold = PlayerPrefs.GetFloat("rotStopThreshold");
        }
        else
        {
            velStopThreshold = 1.0f;
            rotStopThreshold = 1.0f;
        }

        //Multiple FF?
        if (multiple_FF_mode == 2)
        {
            FF_positions.Add(Vector3.zero);
            FF_positions.Add(Vector3.zero);
        }
        else if (multiple_FF_mode == 1)
        {
            for (int i = 0; i < nFF; i++)
            {
                FF_positions.Add(Vector3.zero);
            }
        }

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
        ratio_always_on = PlayerPrefs.GetFloat("Ratio");

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

        lifespan_durations.Add(PlayerPrefs.GetFloat("D1"));
        lifespan_durations.Add(PlayerPrefs.GetFloat("D2"));
        lifespan_durations.Add(PlayerPrefs.GetFloat("D3"));
        lifespan_durations.Add(PlayerPrefs.GetFloat("D4"));
        lifespan_durations.Add(PlayerPrefs.GetFloat("D5"));

        lifespan_ratios.Add(PlayerPrefs.GetFloat("R1"));
        lifespan_ratios.Add(PlayerPrefs.GetFloat("R2"));
        lifespan_ratios.Add(PlayerPrefs.GetFloat("R3"));
        lifespan_ratios.Add(PlayerPrefs.GetFloat("R4"));
        lifespan_ratios.Add(PlayerPrefs.GetFloat("R5"));

        for (int i = 1; i < 5; i++)
        {
            lifespan_ratios[i] = lifespan_ratios[i] + lifespan_ratios[i - 1];
        }

        isMoving = PlayerPrefs.GetInt("Moving ON") == 1;
        LRFB = PlayerPrefs.GetInt("VertHor") == 0;
        noised_moving_FF = PlayerPrefs.GetFloat("MovingFFmode") == 1;

        lineOnOff = false;
        line.transform.localScale = new Vector3(10000f, 0.125f * p_height * 10, 1);
        if (lineOnOff)
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
                case "flashing":
                    flashing = true;
                    flashing_frequency = PlayerPrefs.GetFloat("Frequency");
                    duty_cycle = PlayerPrefs.GetFloat("Duty Cycle") / 100f;
                    Pulse_Width = duty_cycle / flashing_frequency;
                    break;
                case "fixed":
                    flashing = false;
                    break;
                default:
                    throw new System.Exception("No mode selected, defaulting to FIXED");
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError(e, this);
            flashing = false;
        }
        if (nFF > 1)
        {
            for (int i = 0; i < nFF; i++)
            {
                GameObject obj = Instantiate(firefly);
                obj.name = ("Firefly " + i).ToString();
                Multiple_FF_List.Add(obj);
                obj.SetActive(true);
                obj.GetComponent<SpriteRenderer>().enabled = true;
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
                sb_cont_data.Append(string.Format("Trial,Time,Phase,FF On/Off,MonkeyX,MonkeyY,MonkeyZ,MonkeyRX,MonkeyRY,MonkeyRZ,MonkeyRW,Linear Velocity,Angular Velocity,{0}FFV,MappingContext,Confidence,GazeX,GazeY,GazeZ,GazeDistance,RCenterX,RCenterY,RCenterZ,LCenterX,LCenterY,LCenterZ,RNormalX,RNormalY,RNormalZ,LNormalX,LNormalY,LNormalZ,ObsLinNoise,ObsAngNoise,", str) + PlayerPrefs.GetString("Name") + "," + PlayerPrefs.GetString("Date") + "," + PlayerPrefs.GetInt("Run Number").ToString("D3") + "\n");
            }
            else
            {
                if ((int)PlayerPrefs.GetFloat("PTBType") == 2)
                {
                    sb_cont_data.Append("Trial,Time,Phase,FF On/Off,MonkeyX,MonkeyY,MonkeyZ,MonkeyRX,MonkeyRY,MonkeyRZ,MonkeyRW,Linear Velocity,Angular Velocity,FFX,FFY,FFZ,FFV,MappingContext,Confidence,GazeX,GazeY,GazeZ,GazeDistance,RCenterX,RCenterY,RCenterZ,LCenterX,LCenterY,LCenterZ,RNormalX,RNormalY,RNormalZ,LNormalX,LNormalY,LNormalZ,ObsLinNoise,ObsAngNoise," + PlayerPrefs.GetString("Name") + "," + PlayerPrefs.GetString("Date") + "," + PlayerPrefs.GetInt("Run Number").ToString("D3") + "\n");
                }
                else
                {
                    sb_cont_data.Append("Trial,Time,Phase,FF On/Off,MonkeyX,MonkeyY,MonkeyZ,MonkeyRX,MonkeyRY,MonkeyRZ,MonkeyRW,FFX,FFY,FFZ,FFV,MappingContext,Confidence,GazeX,GazeY,GazeZ,GazeDistance,RCenterX,RCenterY,RCenterZ,LCenterX,LCenterY,LCenterZ,RNormalX,RNormalY,RNormalZ,LNormalX,LNormalY,LNormalZ,VKsi,Veta,RotKsi,RotEta,PTBLV,PTBRV,CleanLV,CleanRV,RawX,RawY,ObsLinNoise,ObsAngNoise," + PlayerPrefs.GetString("Name") + "," + PlayerPrefs.GetString("Date") + "," + PlayerPrefs.GetInt("Run Number").ToString("D3") + "\n");
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
    /// For Flashing and Fixed, is_always_on_trial will be true or false depending on whether or not
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
                    is_always_on_trial = rand.NextDouble() <= ratio_always_on;
                    currentTask = Begin();
                    break;

                case Phases.trial:
                    phase = Phases.none;
                    currentTask = Trial();
                    break;

                case Phases.check:
                    phase = Phases.none;
                    currentTask = Check();
                    break;

                case Phases.none:
                    break;
            }

            if (isMoving && nFF < 2)
            {
                if(!noised_moving_FF)
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
                    Vector3 temp = move;
                    move = move + (direction * (float)randStdNormal);
                    velocity_Noised = velocity + (float)randStdNormal;
                    firefly.transform.position += move * Time.deltaTime;
                    move = temp;
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
                trial_number.Add(trialNum);
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
                Multiple_FF_List[1].transform.position = position;
                while(Vector3.Distance(position, Multiple_FF_List[0].transform.position) < 1.666666 * fireflyZoneRadius)
                {
                    FFindex = rand.Next(FFcoordsList.Count);
                    r = FFcoordsList[FFindex].Item1;
                    angle = FFcoordsList[FFindex].Item2;
                    position = player.transform.position - new Vector3(0.0f, p_height, 0.0f) + Quaternion.AngleAxis(angle, Vector3.up) * player.transform.forward * r;
                    position.y = 0.0001f;
                    Multiple_FF_List[1].transform.position = position;
                }
                OnOff(Multiple_FF_List[1]);
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

            if(PlayerPrefs.GetInt("isFFstimu") == 1 && (tNow - startTime) > trialStimuGap && !is_always_on_trial)
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
            checkTime.Add(Time.realtimeSinceStartup - programT0);
        }

        if (isEnd)
        {
            isEnd = false;
            SendMarker("e", 1000.0f);
        }

        if (PlayerPrefs.GetFloat("calib") == 0)
        {
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

            var ObsLinNoise = DistFlowSpeed;
            var ObsAngNoise = DistFlowRot;

            if (flagMultiFF)
            {
                foreach (Vector3 pos in FF_positions)
                {
                    FFposition = string.Concat(FFposition, ",", pos.ToString("F5").Trim('(', ')').Replace(" ", "")).Substring(1);
                }
            }
            else
            {
                FFposition = firefly.transform.position.ToString("F5").Trim('(', ')').Replace(" ", "");
            }
            if (SharedJoystick.CtrlDynamicsFlag)
            {
                sb_cont_data.Append(string.Format("{0},{1, 4:F9},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27}\n",
                    trial,
                    (double)Time.realtimeSinceStartup - programT0,
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
                    ObsAngNoise));
            }
            else
            {
                var lllin = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29}",
                        trial, (double)Time.realtimeSinceStartup - programT0, epoch, onoff, position, rotation,
                        linear, angular, FFposition, FFlinear, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, ObsLinNoise, ObsAngNoise);                
                
                sb_cont_data.AppendLine(lllin);
            }
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

        SharedJoystick.MaxSpeed = RandomizeSpeeds(velMin, velMax);
        SharedJoystick.RotSpeed = RandomizeSpeeds(rotMin, rotMax);

        //print(CtrlDynamicsFlag);
        if (control_dynamics != 0)
        {
            switch (control_dynamics)
            {
                case 1:
                    SharedJoystick.DiscreteTau();
                    break;

                case 2:
                    SharedJoystick.ContinuousTau();
                    break;

                default:
                    break;
            }
            //tautau.Add(SharedJoystick.currentTau);
            //filterTau.Add(SharedJoystick.filterTau);
            player_max_vel.Add(SharedJoystick.MaxSpeed);
            player_max_rot.Add(SharedJoystick.RotSpeed);
            CurrentTau.Add(SharedJoystick.savedTau);
        }
        else
        {
            player_max_vel.Add(SharedJoystick.MaxSpeed);
            player_max_rot.Add(SharedJoystick.RotSpeed);
        }

        float density = particles.SwitchDensity();
        if (particles.changedensityflag && isObsNoise)
        {
            particles2.SwitchDensity2();
        }

        optic_flow_densities.Add(density);
        if (isObsNoise)
        {
            optic_flow_densities_obsRatio.Add(ObsDensityRatio);
        }
        else
        {
            optic_flow_densities_obsRatio.Add(0);
        }

        currPhase = Phases.begin;
        isBegin = true;

        if (isCOM)
        {
            Vector3 position;
            int FFindex = rand.Next(FFcoordsList.Count);
            FF1index = FFindex;
            float r = FFcoordsList[FFindex].Item1;
            float angle = FFcoordsList[FFindex].Item2;
            position = Vector3.zero - new Vector3(0.0f, p_height, 0.0f) + Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * r;
            position.y = 0.0001f;
            Multiple_FF_List[0].transform.position = position;
            Vector3 position1 = player.transform.position - new Vector3(0.0f, 0.0f, 10.0f);
            Multiple_FF_List[1].transform.position = position1;
            Multiple_FF_List[1].SetActive(false);
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
        else if (multiple_FF_mode == 1)
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
                    if (i > 0) for (int k = 0; k < i; k++) { if (Vector3.Distance(position, Multiple_FF_List[k].transform.position) <= separation) tooClose = true; } // || Mathf.Abs(position.x - pooledFF[k - 1].transform.position.x) >= 0.5f || Mathf.Abs(position.z - pooledFF[k - 1].transform.position.z) <= 0.5f) tooClose = true; }
                    Multiple_FF_List[i].transform.position = position;
                    FF_positions[i] = position;
                } while (tooClose);
            }
        }
        else
        {
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
            firefly.transform.position = position;
            FF_positions.Add(position);
        }

        if (isStatic2FF)
        {
            Vector3 position;
            int FFindex = rand.Next(FFcoordsList.Count);
            float r = FFcoordsList[FFindex].Item1;
            float angle = FFcoordsList[FFindex].Item2;
            position = Vector3.zero - new Vector3(0.0f, p_height, 0.0f) + Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * r;
            position.y = 0.0001f;
            while (FFindex == FF1index || Vector3.Distance(position,Multiple_FF_List[0].transform.position) <= 1.666666 * fireflyZoneRadius)
            {
                FFindex = rand.Next(FFcoordsList.Count);
                r = FFcoordsList[FFindex].Item1;
                angle = FFcoordsList[FFindex].Item2;
                position = Vector3.zero - new Vector3(0.0f, p_height, 0.0f) + Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * r;
                position.y = 0.0001f;
            }
            Multiple_FF_List[1].transform.position = position;
            Multiple_FF_List[1].SetActive(false);
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

        player_trial_origin = player.transform.position;
        player_starting_position.Add(player_trial_origin.ToString("F5").Trim(toTrim).Replace(" ", ""));
        player_starting_rotation.Add(player.transform.rotation.ToString("F5").Trim(toTrim).Replace(" ", ""));

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
            FF_velocity.Add(velocity);
            move = direction * velocity;
        }
        else
        {
            FF_velocity.Add(0.0f);
        }

        // Debug.Log("Begin Phase End.");
        if (nFF > 1)
        {
            if (PlayerPrefs.GetInt("isColored") == 1)
            {
                foreach (GameObject FF in Multiple_FF_List)
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
            if (is_always_on_trial && !isCOM || is_always_on_trial && isNormal)
            {
                foreach (GameObject FF in Multiple_FF_List)
                {
                    FF.SetActive(true);
                    // Add alwaysON for all fireflies
                }
            }
            else
            {
                float r = (float)rand.NextDouble();

                if (r <= lifespan_ratios[0])
                {
                    // duration 1
                    lifeSpan = lifespan_durations[0];
                }
                else if (r > lifespan_ratios[0] && r <= lifespan_ratios[1])
                {
                    // duration 2
                    lifeSpan = lifespan_durations[1];
                }
                else if (r > lifespan_ratios[1] && r <= lifespan_ratios[2])
                {
                    // duration 3
                    lifeSpan = lifespan_durations[2];
                }
                else if (r > lifespan_ratios[2] && r <= lifespan_ratios[3])
                {
                    // duration 4
                    lifeSpan = lifespan_durations[3];
                }
                else
                {
                    // duration 5
                    lifeSpan = lifespan_durations[4];
                }
                FF_on_duration.Add(lifeSpan);
                if (isCOM)
                {
                    OnOff(Multiple_FF_List[0]);
                    if (isStatic2FF)
                    {
                        OnOff(Multiple_FF_List[1]);
                    }
                }
                else
                {
                    foreach (GameObject FF in Multiple_FF_List)
                    {
                        OnOff(FF);
                    }
                }
            }
        }
        else
        {
            if (flashing)
            {
                flashing_FF_on = true;
                flashTask = Flash(firefly);
            }
            else
            {
                if (is_always_on_trial)
                {
                    firefly.SetActive(true);
                    alwaysON.Add(true);
                }
                else
                {
                    alwaysON.Add(false);
                    float r = (float)rand.NextDouble();

                    if (r <= lifespan_ratios[0])
                    {
                        // duration 1
                        lifeSpan = lifespan_durations[0];
                    }
                    else if (r > lifespan_ratios[0] && r <= lifespan_ratios[1])
                    {
                        // duration 2
                        lifeSpan = lifespan_durations[1];
                    }
                    else if (r > lifespan_ratios[1] && r <= lifespan_ratios[2])
                    {
                        // duration 3
                        lifeSpan = lifespan_durations[2];
                    }
                    else if (r > lifespan_ratios[2] && r <= lifespan_ratios[3])
                    {
                        // duration 4
                        lifeSpan = lifespan_durations[3];
                    }
                    else
                    {
                        // duration 5
                        lifeSpan = lifespan_durations[4];
                    }
                    FF_on_duration.Add(lifeSpan);
                    OnOff();
                }
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

        if (control_dynamics != 0)
        {
            print("PTB trial started");
            isIntertrail = false;
            var t = Task.Run(async () => {
                //TODO: Using the brake threshold is probably wrong
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
                await new WaitUntil(() => Vector3.Distance(player_trial_origin, player.transform.position) > 0.5f || playing == false); // Used to be rb.velocity.magnitude
            }, source.Token);

            var t1 = Task.Run(async () => {
                await new WaitForSeconds(timeout);
            }, source.Token);

            if (await Task.WhenAny(t, t1) == t || player == null)
            {
                await new WaitUntil(() => ((Mathf.Abs(SharedJoystick.currentSpeed) <= JstLinearThreshold && Mathf.Abs(SharedJoystick.currentRot) <= JstAngularThreshold && !SharedJoystick.CtrlDynamicsFlag)) || t1.IsCompleted); // Used to be rb.velocity.magnitude // || (angleL > 3.0f or angleR > 3.0f)
                if (t1.IsCompleted) isTimeout = true;
            }
            else
            {
                //print("Timed out");
                isTimeout = true;
            }
        }

        source.Cancel();

        if (flashing)
        {
            flashing_FF_on = false;
        }

        if (is_always_on_trial)
        {
            if (multiple_FF_mode == 1)
            {
                foreach (GameObject FF in Multiple_FF_List)
                {
                    FF.SetActive(false);
                }
            }
            else
            {
                firefly.SetActive(false);
            }

            FF_on_duration.Add(Time.realtimeSinceStartup - beginTime[beginTime.Count - 1] - programT0);
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
        FF_proximity = false;

        isTrial = false;

        float distance = 0.0f;
        float curdistance = 9999f;

        Vector3 pos;
        Quaternion rot;

        player_position = player.transform.position - new Vector3(0.0f, p_height, 0.0f);

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
            await new WaitForSeconds(delay);
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
                if(!(Multiple_FF_List[i].transform.position.x == 0 && Multiple_FF_List[i].transform.position.z == 0))
                {
                    ffPosStr = string.Concat(ffPosStr, ",", Multiple_FF_List[i].transform.position.ToString("F5").Trim(toTrim).Replace(" ", "")).Substring(1);
                    distance = Vector3.Distance(player_position, Multiple_FF_List[i].transform.position);
                    //print(distance);
                    if (distance <= fireflyZoneRadius && distance < curdistance)
                    {
                        curdistance = distance;
                        FF_proximity = true;
                        colorhit = i;
                    }
                }
            }
            distances_to_FF.Add(distance);
        }
        else if (multiple_FF_mode == 1)
        {
            for (int i = 0; i < nFF; i++)
            {
                ffPosStr = string.Concat(ffPosStr, ",", Multiple_FF_List[i].transform.position.ToString("F5").Trim(toTrim).Replace(" ", "")).Substring(1);
                distance = Vector3.Distance(player_position, Multiple_FF_List[i].transform.position);
                //print(distance);
                if (distance <= fireflyZoneRadius && distance < curdistance)
                {
                    curdistance = distance;
                    FF_proximity = true;
                    colorhit = i;
                }
                distances_to_FF.Add(distance);
            }
        }
        else
        {
            if (Vector3.Distance(player_position, firefly.transform.position) <= fireflyZoneRadius) FF_proximity = true;
            distance = Vector3.Distance(player_position, firefly.transform.position);
            ffPosStr = firefly.transform.position.ToString("F5").Trim(toTrim).Replace(" ", "");
            distances_to_FF.Add(distance);
        }

        if (FF_proximity)
        {
             if (isCOM && PlayerPrefs.GetInt("isColored") == 1)
            {
                print(colorhit);
                print(colorchosen[(int)colorhit]);
                juiceTime = colorrewards[(int)colorchosen[(int)colorhit] - 1];
                audioSource.clip = winSound;
                juiceDuration.Add(juiceTime);
                audioSource.Play();
                good_trial_count++;
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

                good_trial_count++;
                SendMarker("j", juiceTime);

                await new WaitForSeconds((juiceTime / 1000.0f) + 0.25f);
            }
            else
            {
                audioSource.clip = winSound;
                juiceTime = Mathf.Lerp(maxJuiceTime, minJuiceTime, Mathf.InverseLerp(0.0f, fireflyZoneRadius, distance));
                //print(juiceTime);
                juiceDuration.Add(juiceTime);
                audioSource.Play();

                good_trial_count++;
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


        if (multiple_FF_mode == 1)
        {
            score.Add(FF_proximity ? 1 : 0);
            timedout.Add(isTimeout ? 1 : 0);
            player_final_position.Add(pos.ToString("F5").Trim(toTrim).Replace(" ", ""));
            player_final_rotation.Add(rot.ToString("F5").Trim(toTrim).Replace(" ", ""));
            distance_to_FF.Add(string.Join(",", distances_to_FF));
            FF_final_positions.Add(ffPosStr);

            float wait = i_lambda * Mathf.Exp(-i_lambda * ((float)rand.NextDouble() * (i_max - i_min) + i_min));

            currPhase = Phases.ITI;

            interWait.Add(wait);

            isEnd = true;

            distances_to_FF.Clear();
            ffPosStr = "";
            isTimeout = false;

            if (PlayerPrefs.GetInt("isColored") == 1)
            {
                FF_color.Add(string.Format("{0},{1}", colorchosen[0], colorchosen[1]));
                colorchosen.Clear();
            }

            await new WaitForSeconds(wait);

            phase = Phases.begin;
        }
        else
        {
            timedout.Add(isTimeout ? 1 : 0);
            score.Add(FF_proximity ? 1 : 0);
            FF_final_positions.Add(ffPosStr);
            distance_to_FF.Add(distances_to_FF[0].ToString("F5"));
            player_final_position.Add(pos.ToString("F5").Trim(toTrim).Replace(" ", ""));
            player_final_rotation.Add(rot.ToString("F5").Trim(toTrim).Replace(" ", ""));


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

            FF_positions.Clear();
            distances_to_FF.Clear();
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
        while (flashing_FF_on)
        {
            if (is_always_on_trial && !obj.activeInHierarchy)
            {
                obj.GetComponent<SpriteRenderer>().enabled = false;
            }
            else
            {
                obj.GetComponent<SpriteRenderer>().enabled = true;
                await new WaitForSeconds(Pulse_Width);
                obj.GetComponent<SpriteRenderer>().enabled = false;
                await new WaitForSeconds((1f / flashing_frequency) - Pulse_Width);
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
                player_starting_position.Count,
                player_starting_rotation.Count,
                FF_final_positions.Count,
                distance_to_FF.Count,
                trial_number.Count,
                player_final_position.Count,
                player_final_rotation.Count,
                beginTime.Count,
                rewardTime.Count,
                endTime.Count,
                checkWait.Count,
                interWait.Count,
                score.Count,
                timedout.Count,
                player_max_vel.Count,
                player_max_rot.Count,
                FF_velocity.Count,
                FF_on_duration.Count,
                optic_flow_densities.Count,
                juiceDuration.Count,
                optic_flow_densities_obsRatio.Count
            };

            temp.Add(checkTime.Count);

            if (PlayerPrefs.GetInt("isColored") == 1)
            {
                temp.Add(FF_color.Count);
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

            if (control_dynamics != 0)
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
                    trial_number[i],
                    player_max_vel[i],
                    player_max_rot[i],
                    FF_velocity[i],
                    FF_on_duration[i],
                    optic_flow_densities[i],
                    player_starting_position[i],
                    player_starting_rotation[i],
                    FF_final_positions[i],
                    player_final_position[i],
                    player_final_rotation[i],
                    distance_to_FF[i],
                    score[i],
                    timedout[i],
                    juiceDuration[i],
                    beginTime[i],
                    checkTime[i].ToString("F5"),
                    rewardTime[i],
                    endTime[i],
                    checkWait[i],
                    interWait[i]);

                if (control_dynamics != 0)
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
                    line += string.Format(",{0},{1},{2},{3}", ObsNoiseTau, ObsVelocityNoiseGain, ObsRotationNoiseGain, optic_flow_densities_obsRatio[i]);
                }
                else
                {
                    line += string.Format(",0,0,0,0");
                }

                if (PlayerPrefs.GetInt("isColored") == 1)
                {
                    line += string.Format(",{0}", FF_color[i]);
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
                File.AppendAllText(contpath, sb_cont_data.ToString());
            }

            PlayerPrefs.SetInt("Good Trials", totalScore);
            print(temp[0]);
            PlayerPrefs.SetInt("Total Trials", trial_number[trial_number.Count - 1]);

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
}