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
    //Shared instance of script
    public static Monkey2D SharedMonkey;

    //FF
    public GameObject firefly;
    //Size of FF (diameter)
    public float fireflySize;
    //Reward radius
    public float reward_zone_radius;
    //Min/max of which FF can spawn
    public float minDrawDistance;
    public float maxDrawDistance;
    //Min/max angle from forward(y) axis FF can spawn
    public float minPhi;
    public float maxPhi;
    public float PhiRatio;
    public float minPhi2;
    public float maxPhi2;
    //Lifespan of the FF, if applicable
    public float lifeSpan;
    //possible Lifespans and ratios
    readonly public List<float> lifespan_durations = new List<float>();
    readonly public List<float> lifespan_ratios = new List<float>();
    //Number of FFs, if applicable
    public float NumberOfFF;
    //Multiple FF mode, 0 for don't apply, 1 for normal multiple, 2 for COM
    int multiple_FF_mode;

    //Moving FF, FF velocities and their corresponding ratios, and noises
    private bool isMoving;
    readonly public List<float> velocities = new List<float>();
    readonly public List<float> v_ratios = new List<float>();
    readonly public List<float> v_noises = new List<float>();
    //FF moves left-right or forward-backward? true for lr, false for fb
    private bool isLeftRightnotForBack;
    //FF move direction
    private Vector3 FFMoveDirection = new Vector3();
    private bool noised_moving_FF;
    public GameObject line;
    private bool lineOnOff;

    //Cameras
    public Camera Lcam;
    public Camera Rcam;
    public float offset = 0.01f;
    private float lm02;
    private float rm02;
    private Matrix4x4 lm;
    private Matrix4x4 rm;
    public Camera LObscam;
    public Camera RObscam;
    public Camera DrunkCam;

    //Gaze visualized from eye tracker
    [HideInInspector] public GazeVisualizer gazeVisualizer;

    //Arduino
    public int baudRate = 2000000;
    public int ReadTimeout = 5000;

    // Use flashing FF?(for training, normal FF task only)
    public bool isFlashing;
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
    public float Player_Height;

    //sounds
    public AudioSource audioSource;
    public AudioClip winSound;
    public AudioClip neutralSound;
    public AudioClip loseSound;

    //Colored FF
    bool isColored;
    readonly public List<float> colorratios = new List<float>();
    readonly public List<float> colorrewards = new List<float>();
    private List<float> colorchosen = new List<float>();
    private float colorhit = 0;

    //FF movement vector
    private Vector3 FFMovement;
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
    // Distances from player to firefly (closest one in case of multiple FF)
    readonly List<string> distance_to_FF = new List<string>();
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
    public int Num_Trials;

    //Randomization
    private int seed;
    private System.Random rand;

    //Flashing FF
    private bool flashing_FF_on = true;

    //trial start time
    private float trial_start_time;
    private float MoveStartTime;

    //Phases
    private bool isBegin = false;
    private bool isTrial = false;
    private bool isCheck = false;
    private bool isEnd = false;
    public bool trialStimulated = false;
    public bool Joystick_Disabled = false;
    public enum Phases
    {
        begin = 0,
        trial = 1,
        check = 2,
        juice = 3,
        ITI = 4,
        none = 9,
    }
    //Phase switch
    [HideInInspector] public Phases phase_task_selecter;
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
    public float DistFlowSpeed = 0;
    public float DistFlowRot = 0;

    //Async Tasking
    CancellationTokenSource source;
    private Task currentTask;
    private Task flashTask;
    private bool isPlaying = true;
    public bool isStimulating = false;

    //Juice
    SerialPort juiceBox;

    //Multiple FF separation
    float FFseparation;

    //Close enough to the FF to get reward?
    bool rewarded_FF_trial;

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
    bool flagFFstimulation;
    private float microStimuDur;
    private float microStimuGap;
    private float trialStimuGap;

    //Selfmotion experiment or not (moving FF)
    bool SMtrial = false;

    //2FF change of mind task
    readonly List<float> COMtrialtype = new List<float>();
    public bool isCOM = false;
    public bool isNormal = false;
    public bool isStatic2FF = false;
    public bool isCOM2FF = false;
    public bool isCOMtest = false;
    bool FF2shown = false;
    float FF2delay;
    float normalRatio;
    float normal2FFRatio;
    int FF1index;
    readonly List<Tuple<float, float>> FFcoordsList = new List<Tuple<float, float>>();
    readonly List<Tuple<float, float>> FF2coordsList = new List<Tuple<float, float>>();
    readonly List<Tuple<float, float>> FFvisibleList = new List<Tuple<float, float>>();
    readonly List<Tuple<float, float>> FFTagetMatchList = new List<Tuple<float, float>>();
    readonly public List<float> FF1historyList = new List<float>();
    readonly public List<float> FF2historyList = new List<float>();
    int trial_count = 0;

    //Human subject?
    bool isHuman = false;

    //Replay?
    public bool isReplay = false;

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
        minJuiceTime = PlayerPrefs.GetFloat("minJuiceTime");
        maxJuiceTime = PlayerPrefs.GetFloat("maxJuiceTime");

        //Send block start marker
        SendMarker("f", 1000.0f);
        programT0 = Time.realtimeSinceStartup;

        //VR cameras set up, and tilt cameras if monkey
        isHuman = PlayerPrefs.GetInt("isHuman") == 1;
        UnityEngine.XR.InputTracking.disablePositionalTracking = true;
        UnityEngine.XR.XRDevice.DisableAutoXRCameraTracking(Lcam, true);
        UnityEngine.XR.XRDevice.DisableAutoXRCameraTracking(Rcam, true);
        XRSettings.occlusionMaskScale = 10f;
        XRSettings.useOcclusionMesh = false;
        Lcam.ResetProjectionMatrix();
        Rcam.ResetProjectionMatrix();
        if (!isHuman)
        {
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
        }
        List<XRDisplaySubsystem> displaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances<XRDisplaySubsystem>(displaySubsystems);
        foreach(XRDisplaySubsystem system in displaySubsystems)
        {
            system.Start();
        }

        //Shared instance
        SharedMonkey = this;

        //Get basic variables from settings
        timeout = PlayerPrefs.GetFloat("Timeout");
        path = PlayerPrefs.GetString("Path");
        Num_Trials = (int)PlayerPrefs.GetFloat("Num_Trials");
        if (Num_Trials == 0) Num_Trials = 9999;
        isReplay = PlayerPrefs.GetInt("isReplay") == 1;
        if (isReplay)
        {
            ReadFFCoordDisc();
            Num_Trials = FFTagetMatchList.Count + 1;
        }
        seed = UnityEngine.Random.Range(1, 10000);
        rand = new System.Random(seed);
        Player_Height = PlayerPrefs.GetFloat("Player_Height");
        
        //ITI time settings
        c_lambda = 1.0f / PlayerPrefs.GetFloat("CheckMean");
        i_lambda = 1.0f / PlayerPrefs.GetFloat("IntertialMean");
        checkMin = PlayerPrefs.GetFloat("checkMin");
        checkMax = PlayerPrefs.GetFloat("checkMax");
        interMin = PlayerPrefs.GetFloat("interMin");
        interMax = PlayerPrefs.GetFloat("interMax");
        c_min = Tcalc(checkMin, c_lambda);
        c_max = Tcalc(checkMax, c_lambda);
        i_min = Tcalc(interMin, i_lambda);
        i_max = Tcalc(interMax, i_lambda);

        //FF settings and spawn settings
        minDrawDistance = PlayerPrefs.GetFloat("minDrawDistance");
        maxDrawDistance = PlayerPrefs.GetFloat("maxDrawDistance");
        maxPhi = PlayerPrefs.GetFloat("maxPhi");
        minPhi = PlayerPrefs.GetFloat("minPhi");
        PhiRatio = PlayerPrefs.GetFloat("ratiophi");
        maxPhi2 = PlayerPrefs.GetFloat("maxPhi2");
        minPhi2 = PlayerPrefs.GetFloat("minPhi2");
        reward_zone_radius = PlayerPrefs.GetFloat("reward_zone_radius");
        fireflySize = PlayerPrefs.GetFloat("RadiusFF") * 2;
        firefly.transform.localScale = new Vector3(fireflySize, fireflySize, 1);
        ratio_always_on = PlayerPrefs.GetFloat("ratio_always_on");

        //Selfmotion Task
        SMtrial = PlayerPrefs.GetInt("isSM") == 1;

        //Observation Noise?
        isObsNoise = PlayerPrefs.GetInt("isObsNoise") == 1;
        ObsNoiseTau = PlayerPrefs.GetFloat("ObsNoiseTau");
        ObsVelocityNoiseGain = PlayerPrefs.GetFloat("ObsVelocityNoiseGain");
        ObsRotationNoiseGain = PlayerPrefs.GetFloat("ObsRotationNoiseGain");
        ObsDensityRatio = PlayerPrefs.GetFloat("ObsDensityRatio");
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
        control_dynamics = (int)PlayerPrefs.GetFloat("Acceleration_Control_Type");
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

        //Multiple FF? Is it change of mind?
        NumberOfFF = PlayerPrefs.GetFloat("NumberOfFF");
        multiple_FF_mode = PlayerPrefs.GetInt("multiple_FF_mode");
        flagMultiFF = multiple_FF_mode > 0;
        isCOM = PlayerPrefs.GetInt("is2FFCOM") == 1;
        isCOMtest = PlayerPrefs.GetFloat("COMNormal") == 999;
        if (isCOM)
        {
            ratio_always_on = 0;
            NumberOfFF = 2;
            FFcoordsList.Clear();
            ReadCoordCSV();
            ReadCoord2CSV();
        }
        FFseparation = PlayerPrefs.GetFloat("FFseparation");
        if (multiple_FF_mode == 2)
        {
            //2FF, 2 fireflies
            FF_positions.Add(Vector3.zero);
            FF_positions.Add(Vector3.zero);
        }
        else if (multiple_FF_mode == 1)
        {
            for (int i = 0; i < NumberOfFF; i++)
            {
                FF_positions.Add(Vector3.zero);
            }
        }
        if (flagMultiFF)
        {
            for (int i = 0; i < NumberOfFF; i++)
            {
                GameObject obj = Instantiate(firefly);
                obj.name = ("Firefly " + i).ToString();
                Multiple_FF_List.Add(obj);
                obj.SetActive(true);
                obj.GetComponent<SpriteRenderer>().enabled = true;
            }
            firefly.SetActive(false);
        }

        //2FF Change of mind?
        normalRatio = PlayerPrefs.GetFloat("COMNormal");
        normal2FFRatio = PlayerPrefs.GetFloat("Sta2FF");
        normal2FFRatio += normalRatio;

        //FF stimulation?
        microStimuDur = PlayerPrefs.GetFloat("StimuStimuDur");
        microStimuGap = PlayerPrefs.GetFloat("FFstimugap");
        stimuratio = PlayerPrefs.GetFloat("StimulationRatio");
        flagFFstimulation = PlayerPrefs.GetInt("isFFstimu") == 1;

        //Moving FF
        isMoving = PlayerPrefs.GetInt("isMoving") == 1;
        isLeftRightnotForBack = PlayerPrefs.GetInt("VertHor") == 0;
        noised_moving_FF = PlayerPrefs.GetFloat("MovingFFmode") == 1;
        for (int i = 1; i <= 12; i++)
        {
            string PPFetchName = "V" + i.ToString();
            velocities.Add(PlayerPrefs.GetFloat(PPFetchName));
            PPFetchName = "VR" + i.ToString();
            v_ratios.Add(PlayerPrefs.GetFloat(PPFetchName));
            PPFetchName = "VN" + i.ToString();
            v_noises.Add(PlayerPrefs.GetFloat(PPFetchName));
        }
        for (int i = 1; i < 12; i++)
        {
            v_ratios[i] = v_ratios[i] + v_ratios[i - 1];
        }

        //FF observation
        for (int i = 1; i <= 5; i++)
        {
            string PPFetchName = "LifespanDuration" + i.ToString();
            lifespan_durations.Add(PlayerPrefs.GetFloat(PPFetchName));
            PPFetchName = "LifespanRatio" + i.ToString();
            lifespan_ratios.Add(PlayerPrefs.GetFloat(PPFetchName));
        }
        for (int i = 1; i < 5; i++)
        {
            lifespan_ratios[i] = lifespan_ratios[i] + lifespan_ratios[i - 1];
        }

        //Line for moving FF obsv?
        lineOnOff = false;
        line.transform.localScale = new Vector3(10000f, 0.125f * Player_Height * 10, 1);
        if (lineOnOff)
        {
            line.SetActive(true);
        }
        else
        {
            line.SetActive(false);
        }

        //Colored FF?
        isColored = PlayerPrefs.GetInt("isColored") == 1;
        if (isColored)
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

        //Flash the FF?
        isFlashing = PlayerPrefs.GetInt("isFlashing") == 1;
        if (isFlashing)
        {
            flashing_frequency = PlayerPrefs.GetFloat("flashing_frequency");
            duty_cycle = PlayerPrefs.GetFloat("duty_cycle");
            Pulse_Width = duty_cycle / flashing_frequency;
        }

        //If not using eye tracking/calibration, the cont. data is saved here
        if (PlayerPrefs.GetFloat("calib") == 0)
        {
            var multiple_FF_string = "";
            if (flagMultiFF)
            {
                for (int i = 0; i < SharedMonkey.NumberOfFF; i++)
                {
                    multiple_FF_string = string.Concat(multiple_FF_string, string.Format("FFX{0},FFY{0},FFZ{0},", i));
                }
            }
            else
            {
                multiple_FF_string = "FFX,FFY,FFZ,";
            }
            string first_row = string.Format("Trial,Time,Phase,FF On/Off,MonkeyX,MonkeyY,MonkeyZ,MonkeyRX,MonkeyRY,MonkeyRZ,MonkeyRW,{0}FFV,MappingContext,Confidence," +
                    "GazeX,GazeY,GazeZ,GazeDistance,RCenterX,RCenterY,RCenterZ,LCenterX,LCenterY,LCenterZ,RNormalX,RNormalY,RNormalZ,LNormalX,LNormalY,LNormalZ,VKsi,Veta," +
                    "RotKsi,RotEta,PTBLV,PTBRV,CleanLV,CleanRV,RawX,RawY,ObsLinNoise,ObsAngNoise,",multiple_FF_string)
                    + PlayerPrefs.GetString("Name") + "," + DateTime.Today.ToString("MMddyyyy") + "," + PlayerPrefs.GetInt("Run Number").ToString("D3") + "\n";
            sb_cont_data.Append(first_row);
        }

        //Set up for first trial
        trialNum = 0;
        currPhase = Phases.begin;
        phase_task_selecter = Phases.begin;
        player.transform.SetPositionAndRotation(Vector3.up * Player_Height, Quaternion.Euler(0.0f, 0.0f, 0.0f));
        drunkplayer.transform.SetPositionAndRotation(Vector3.up * Player_Height, Quaternion.Euler(0.0f, 0.0f, 0.0f));
    }

    private void OnDisable()
    {
        //Close the juice box
        juiceBox.Close();
    }

    /// <summary>
    /// Update is called once per frames, the essential update for gameplay
    /// Basically only cahnge phase and move FF rn
    /// 
    /// Use sprite renderer for multi FF, don't use setactive, due to distance calculations
    /// </summary>
    void Update()
    {
        //Update the optic flow, if have observation noise update that too
        particle_System.transform.position = player.transform.position - (Vector3.up * (Player_Height - 0.0002f));
        if (isObsNoise)
        {
            particle_System2.transform.position = drunkplayer.transform.position - (Vector3.up * (Player_Height - 0.0002f));
        }
        //print(particle_System.transform.position);

        //Switch phases here to ensure that each phase change occurs on a frame
        if (isPlaying && Time.realtimeSinceStartup - programT0 > 0.3f)
        {
            switch (phase_task_selecter)
            {
                case Phases.begin:
                    phase_task_selecter = Phases.none;
                    currentTask = Begin();
                    break;

                case Phases.trial:
                    phase_task_selecter = Phases.none;
                    currentTask = Trial();
                    break;

                case Phases.check:
                    phase_task_selecter = Phases.none;
                    currentTask = Check();
                    break;

                case Phases.none:
                    break;
            }

            //Move the FF depending on task. We only move FF if there is one FF.
            if (noised_moving_FF && !flagMultiFF)
            {
                System.Random randNoise = new System.Random();
                double u1 = 1.0 - randNoise.NextDouble(); //uniform(0,1] random doubles
                double u2 = 1.0 - randNoise.NextDouble();
                double randStdNormal = noise_SD * Math.Sqrt(-2.0 * Math.Log(u1)) *
                             Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
                                                           //double randNormal =
                                                           //mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
                                                           //print(randStdNormal);
                Vector3 temp = FFMovement;
                FFMovement = FFMovement + (FFMoveDirection * (float)randStdNormal);
                velocity_Noised = velocity + (float)randStdNormal;
                firefly.transform.position += FFMovement * Time.deltaTime;
                FFMovement = temp;
            }
            else if(isMoving && !flagMultiFF)
            {
                firefly.transform.position += FFMovement * Time.deltaTime;
            }

            //Print erros for debug
            if (currentTask.IsFaulted)
            {
                print(currentTask.Exception);
            }
        }
    }

    /// <summary>
    /// Happens at a rate of 90Hz (which should be print on screen, if not there is an error)
    /// Captures data, check game status and send markers
    /// </summary>
    public void FixedUpdate()
    {
        //Check if quitting the game; Send block end marker
        var keyboard = Keyboard.current;
        if ((keyboard.enterKey.isPressed || trialNum > Num_Trials) && isPlaying)
        {
            isPlaying = false;
            Save();
            SendMarker("x", 1000.0f);
            juiceBox.Close();

            if(PlayerPrefs.GetFloat("calib") == 0)
            {
                var num = PlayerPrefs.GetInt("Run Number") + 1;
                PlayerPrefs.SetInt("Run Number", num);
            }

            SceneManager.LoadScene("MainMenu");
        }

        //Trial begin marker
        if (isBegin)
        {
            isBegin = false;
            trialNum++;
            if (trialNum <= Num_Trials)
            {
                trial_number.Add(trialNum);
            }
            SendMarker("s", 1000.0f);
        }

        //Update stuff for COM and stimulation, depending on task.
        var tNow = Time.realtimeSinceStartup;
        if (isTrial)
        {
            //TODO: add newest Change of mind way of update here.
            if (isCOM2FF && Time.realtimeSinceStartup - MoveStartTime >= FF2delay && !FF2shown)
            {
                FF2shown = true;
                Vector3 position;
                int FFindex = 4;//rand.Next(FF2coordsList.Count);
                //FF2s.Add(FFindex);
                float VectorX = FF2coordsList[FFindex].Item1;
                float VectorY = FF2coordsList[FFindex].Item2;
                float r = FFcoordsList[FF1index].Item1;
                float angle = FFcoordsList[FF1index].Item2;
                position = Vector3.zero - new Vector3(0.0f, Player_Height, 0.0f) + Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * r;
                position.y = 0.0001f;
                Vector3 rotation;
                rotation = Vector3.zero;
                rotation.x += VectorX;
                rotation.z += VectorY;
                rotation = Quaternion.Euler(0, angle, 0) * rotation;
                Multiple_FF_List[1].transform.position = position + rotation;
                print("Trial FF2 x:" + position.x);
                print("Trial FF2 y:" + position.z);
                OnOff(Multiple_FF_List[1]);
            }

            //print(string.Format("trial elapsed: {0}", tNow - trial_start_time));
            if (PlayerPrefs.GetInt("isFFstimu") == 1 && (tNow - trial_start_time) > trialStimuGap && !trialStimulated)
            {
                trialStimulated = true;
                float stimr = (float)rand.NextDouble();
                if (stimr < stimuratio)
                {
                    SendMarker("m", microStimuDur * 1000.0f);
                    stimulatedTrial = true;
                    timeStimuStart.Add(tNow - programT0);
                }
            }
        }

        //Status check for stimulation
        if (PlayerPrefs.GetInt("isFFstimu") == 1 && (tNow - trial_start_time) > trialStimuGap && (tNow - trial_start_time) < (trialStimuGap + microStimuDur/1000.0f) && stimulatedTrial)
        {
            isStimulating = true;
        }
        else
        {
            isStimulating = false;
        }

        //Check phase starts
        if (isCheck)
        {
            isCheck = false;
            checkTime.Add(Time.realtimeSinceStartup - programT0);
        }

        //Trial end marker
        if (isEnd)
        {
            isEnd = false;
            SendMarker("e", 1000.0f);
        }

        //if not using calibration/eye tracker, save cont. data here (each frame)
        //TODO: check if everything is here
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
            sb_cont_data.Append(string.Format("{0},{1, 4:F9},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20}," +
                "{21},{22},{23},{24},{25},{26},{27}\n",
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
    }

    /// <summary>
    /// The begin phase does the following after player is not moving:
    /// TODO: check how "player not moving" is checked
    /// 1. Save time data
    /// 2. Update FF position, depending on FF radius and angle, LR if training, and based on player pos
    ///         Note that Quaternion.AngleAxis calculates a rotation based on a angle and an axis.
    ///         In our case, angle is FF angle and axis is y-axis(top-down axis).
    ///         Multiply that by the forward vector and FF radius, and you get the final position of the firefly
    /// 3. Record player origin and rotation and firefly location
    /// 4. Decide firefly behavior
    /// </summary>
    async Task Begin()
    {
        //Wait for end of frame and start the begin phase
        //Debug.Log("Begin Phase start.");
        await new WaitForEndOfFrame();
        float StartThreshold = PlayerPrefs.GetFloat("StartThreshold");
        await new WaitUntil(() => Mathf.Abs(SharedJoystick.rawX) <= StartThreshold && Mathf.Abs(SharedJoystick.rawY) <= StartThreshold);
        isBegin = true;

        //Randomize player max speeds based on given trial control dynamics flag
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
            player_max_vel.Add(SharedJoystick.Max_Linear_Speed);
            player_max_rot.Add(SharedJoystick.Max_Angular_Speed);
            CurrentTau.Add(SharedJoystick.savedTau);
        }
        else
        {
            player_max_vel.Add(SharedJoystick.Max_Linear_Speed);
            player_max_rot.Add(SharedJoystick.Max_Angular_Speed);
            CurrentTau.Add(0);
        }

        //Optic flow densities
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

        //FF set up, for single FF/multi/COM
        if (multiple_FF_mode == 2)
        {
            player.transform.position = Vector3.up * Player_Height;
            player.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            Vector3 position;
            int FFindex = 2;//rand.Next(FFcoordsList.Count);
            FF1historyList.Add(FFindex);
            FF1index = FFindex;
            float r = FFcoordsList[FFindex].Item1;
            float angle = FFcoordsList[FFindex].Item2;
            position = Vector3.zero - new Vector3(0.0f, Player_Height, 0.0f) + Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * r;
            position.y = 0.0001f;
            Multiple_FF_List[0].transform.position = position;
            Vector3 position1 = player.transform.position - new Vector3(0.0f, 0.0f, 10.0f);
            Multiple_FF_List[1].transform.position = position1;
            Multiple_FF_List[1].SetActive(false);
            print("Trial FF1 r:" + r.ToString());
            print("Trial FF1 a:" + angle.ToString());
            float COMdecider = (float)rand.NextDouble();
            if (COMdecider < normalRatio)
            {
                isNormal = true;
                isStatic2FF = false;
                isCOM2FF = false;
                COMtrialtype.Add(1);
                FF2historyList.Add(-1);
            }
            else if (COMdecider < normal2FFRatio)
            {
                isNormal = false;
                isStatic2FF = true;
                isCOM2FF = false;
                COMtrialtype.Add(2);
                FFindex = rand.Next(FF2coordsList.Count);
                FF2historyList.Add(FFindex);
                float VectorX = FF2coordsList[FFindex].Item1;
                float VectorY = FF2coordsList[FFindex].Item2;
                r = FFcoordsList[FF1index].Item1;
                angle = FFcoordsList[FF1index].Item2;
                position = Vector3.zero - new Vector3(0.0f, Player_Height, 0.0f) + Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * r;
                position.y = 0.0001f;
                Vector3 rotation;
                rotation = Vector3.zero;
                rotation.x += VectorX;
                rotation.z += VectorY;
                rotation = Quaternion.Euler(0, angle, 0) * rotation;
                Multiple_FF_List[1].transform.position = position + rotation;
                print("Trial FF2 x:" + position.x);
                print("Trial FF2 y:" + position.z);
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
            for (int i = 0; i < NumberOfFF; i++)
            {
                bool tooClose;
                do
                {
                    tooClose = false;
                    Vector3 position;
                    float r = minDrawDistance + (maxDrawDistance - minDrawDistance) * Mathf.Sqrt((float)rand.NextDouble());
                    float angle = (float)rand.NextDouble() * (maxPhi - minPhi) + minPhi;
                    position = (player.transform.position - new Vector3(0.0f, Player_Height, 0.0f)) + Quaternion.AngleAxis(angle, Vector3.up) * player.transform.forward * r;
                    position.y = 0.0001f;
                    if (i > 0)
                    {
                        for (int k = 0; k < i; k++)
                        {
                            if (Vector3.Distance(position, Multiple_FF_List[k].transform.position) <= FFseparation) tooClose = true;
                        }
                    }
                    Multiple_FF_List[i].transform.position = position;
                    FF_positions[i] = position;
                } while (tooClose);
            }
        }
        else if (isReplay)
        {
            Vector3 position; 
            position.y = 0.0001f;
            position.x = FFTagetMatchList[trial_count].Item1;
            position.z = FFTagetMatchList[trial_count].Item2;
            firefly.transform.position = position;
            trial_count++;
            FF_positions.Add(position);
        }
        else
        {
            bool isDiscrete = PlayerPrefs.GetInt("isDiscrete") == 1;
            Vector3 position;
            float r;
            float angle;
            if (isDiscrete)
            {
                player.transform.position = Vector3.up * Player_Height;
                player.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                int num_lin = (int)PlayerPrefs.GetFloat("NumDiscreteDistances");
                int num_rot = (int)PlayerPrefs.GetFloat("NumDiscreteAngles");
                float[] linspace = Enumerable.Range(0, num_lin).Select(i => minDrawDistance + (maxDrawDistance - minDrawDistance) * i / (num_lin - 1)).ToArray();
                float[] rotspace = Enumerable.Range(0, num_rot).Select(i => minPhi + (maxPhi - minPhi) * i / (num_rot - 1)).ToArray();
                int randomLin = rand.Next(1, num_lin + 1);
                int randomRot = rand.Next(1, num_rot + 1);
                r = linspace[randomLin - 1];
                angle = rotspace[randomRot - 1];
            }
            else
            {

                bool UsePhi1 = (float)rand.NextDouble() < PhiRatio;
                r = minDrawDistance + (maxDrawDistance - minDrawDistance) * Mathf.Sqrt((float)rand.NextDouble());
                angle = (float)rand.NextDouble() * (maxPhi - minPhi) + minPhi;

                //Two Dist. training
                if (!UsePhi1)
                {
                    angle = (float)rand.NextDouble() * (maxPhi2 - minPhi2) + minPhi2;
                }
            }
            position = (player.transform.position - new Vector3(0.0f, Player_Height, 0.0f)) + Quaternion.AngleAxis(angle, Vector3.up) * player.transform.forward * r;
            position.y = 0.0001f;
            firefly.transform.position = position;
            FF_positions.Add(position);
        }

        //COM2FF
        if (isCOM2FF)
        {
            //TODO: save delays in disc
            float DelayMin = PlayerPrefs.GetFloat("FF2Delaymin");
            float DelayMax = PlayerPrefs.GetFloat("FF2Delaymax");
            FF2delay = DelayMin + (float)rand.NextDouble() * (DelayMax - DelayMin);
            FF2delay = PlayerPrefs.GetFloat("FF2delay");
        }

        //Self motion task, thresholds for starting the trial
        float velocityThreshold = PlayerPrefs.GetFloat("velBrakeThresh");
        float rotationThreshold = PlayerPrefs.GetFloat("rotBrakeThresh");
        float SMr = (float)rand.NextDouble();
        if (SMtrial)
        {
            if (SMr > 0.5)
            {
                await new WaitUntil(() => Mathf.Abs(SharedJoystick.currentSpeed) >= velocityThreshold);
            }
            else
            {
                await new WaitUntil(() => Mathf.Abs(SharedJoystick.currentSpeed) <= velocityThreshold && Mathf.Abs(SharedJoystick.currentRot) <= rotationThreshold);
            }
        }

        //Save player starting positions
        player_trial_origin = player.transform.position;
        player_starting_position.Add(player_trial_origin.ToString("F5").Trim(toTrim).Replace(" ", ""));
        player_starting_rotation.Add(player.transform.rotation.ToString("F5").Trim(toTrim).Replace(" ", ""));

        if (isMoving && NumberOfFF < 2)
        {
            float r = (float)rand.NextDouble();

            if (r <= v_ratios[0])
            {
                //v1
                velocity = velocities[0];
                noise_SD = v_noises[0];
            }
            else if (r > v_ratios[10])
            {
                velocity = velocities[11];
                noise_SD = v_noises[11];
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    if (r > v_ratios[i] && r <= v_ratios[i + 1])
                    {
                        velocity = velocities[i + 1];
                        noise_SD = v_noises[i + 1];
                    }
                }
            }

            if (isLeftRightnotForBack)
            {
                FFMoveDirection = Vector3.right;
            }
            else
            {
                FFMoveDirection = Vector3.forward;
            }
            FFMovement = FFMoveDirection * velocity;
            FF_velocity.Add(velocity);
        }
        else
        {
            FF_velocity.Add(0.0f);
        }

        if (flagMultiFF && isColored)
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


        //Check if is always on trial
        is_always_on_trial = rand.NextDouble() <= ratio_always_on;
        if (is_always_on_trial)
        {
            if (flagMultiFF)
            {
                foreach (GameObject FF in Multiple_FF_List)
                {
                    FF.SetActive(true);
                }
                alwaysON.Add(true);
            }
            else
            {
                firefly.SetActive(true);
                alwaysON.Add(true);
            }
        }
        else if (isFlashing)
        {
            firefly.SetActive(true);
            flashing_FF_on = true;
            flashTask = Flash(firefly);
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
                MoveStartTime = Time.realtimeSinceStartup;
                OnOff(Multiple_FF_List[0]);
                if (isStatic2FF)
                {
                    OnOff(Multiple_FF_List[1]);
                }
            }
            else if (flagMultiFF)
            {
                foreach (GameObject FF in Multiple_FF_List)
                {
                    OnOff(FF);
                }
            }
            else
            {
                OnOff();
            }
            FF_on_duration.Add(lifeSpan);
        }
        // Debug.Log("Begin Phase End.");.

        if (flagFFstimulation)
        {
            trialStimuGap = microStimuGap * (float)rand.NextDouble() + lifeSpan;
        }

        phase_task_selecter = Phases.trial;
        currPhase = Phases.trial;
    }

    /// <summary>
    /// Wait for the player to start moving, then perform various checks,
    /// and wait until the player stops moving and then start the check phase. Also will go back to
    /// begin phase if player doesn't move before timeout
    /// </summary>
    async Task Trial()
    {
        isTrial = true;

        //Debug.Log("Trial Phase Start.");
        trial_start_time = Time.realtimeSinceStartup;

        velbrakeThresh = PlayerPrefs.GetFloat("velBrakeThresh");
        rotbrakeThresh = PlayerPrefs.GetFloat("rotBrakeThresh");

        if (PlayerPrefs.GetFloat("ThreshTauMultiplier") != 0)
        {
            float k = PlayerPrefs.GetFloat("ThreshTauMultiplier");
            velbrakeThresh = k * SharedJoystick.currentTau + velStopThreshold;
            rotbrakeThresh = k * SharedJoystick.currentTau + rotStopThreshold;
        }

        source = new CancellationTokenSource();

        if (isCOMtest)
        {
            foreach (var coord in FFcoordsList)
            {
                float r1 = coord.Item1;
                float angle1 = coord.Item2;
                Vector3 position2 = (player.transform.position - new Vector3(0.0f, Player_Height, 0.0f)) + Quaternion.AngleAxis(angle1, Vector3.up) * player.transform.forward * r1;
                position2.y = 0.0001f;
                Multiple_FF_List[0].transform.position = position2;
                Multiple_FF_List[0].SetActive(true);
                await new WaitForSeconds(1f);
                Multiple_FF_List[0].SetActive(false);
            }
        }

        //Action start
        Joystick_Disabled = false;

        //using control dynamics
        if (control_dynamics != 0)
        {
            print("PTB trial started");
            //Wait when the player to start moving
            var started_moving = Task.Run(async () => {
                //TODO: Using the brake threshold is probably wrong
                await new WaitUntil(() => Mathf.Abs(SharedJoystick.currentSpeed) >= velbrakeThresh);
            }, source.Token);

            //Or wait until time out
            var time_out = Task.Run(async () => {
                await new WaitForSeconds(timeout);
            }, source.Token);

            //Either time out or start moving happened, and if started moving
            if (await Task.WhenAny(started_moving, time_out) == started_moving)
            {
                float joystickT = PlayerPrefs.GetFloat("JoystickThreshold");

                //Wait until stopped moving and jst no movement or time out
                await new WaitUntil(() => Mathf.Abs(SharedJoystick.currentSpeed) < velbrakeThresh && Mathf.Abs(SharedJoystick.currentRot) < rotbrakeThresh && 
                (float)Math.Abs(SharedJoystick.moveX) <= joystickT && (float)Math.Abs(SharedJoystick.moveY) <= joystickT || time_out.IsCompleted);
                
                //Timed out
                if (time_out.IsCompleted)
                {
                    print("Trial timed out");
                    isTimeout = true;
                }
                else
                {
                    print("Stopped moving");
                }
            }
            //If timed out without moving
            else
            {
                print("Timed out without moving");
                isTimeout = true;
            }
        }
        //not using control dynamics
        else
        {
            float LinVelStopThresh = PlayerPrefs.GetFloat("LinVelStopThresh");
            float AngVelStopThresh = PlayerPrefs.GetFloat("AngVelStopThresh");

            //Wait until start moving
            var started_moving = Task.Run(async () => {
                await new WaitUntil(() => Vector3.Distance(player_trial_origin, player.transform.position) > 0.5f);
            }, source.Token);

            //Wait until timed out
            var time_out = Task.Run(async () => {
                await new WaitForSeconds(timeout);
            }, source.Token);

            //Either time out or start moving happened, and if started moving
            if (await Task.WhenAny(started_moving, time_out) == started_moving || player == null)
            {
                //Only wait for stopped moving here, or time out
                await new WaitUntil(() => ((Mathf.Abs(SharedJoystick.currentSpeed) <= LinVelStopThresh && Mathf.Abs(SharedJoystick.currentRot) <= AngVelStopThresh && 
                !SharedJoystick.isCtrlDynamics)) || time_out.IsCompleted);
                //Timed out
                if (time_out.IsCompleted)
                {
                    print("Trial timed out");
                    isTimeout = true;
                }
                else
                {
                    print("Stopped moving");
                }
            }
            //Timed out without moving
            else
            {
                print("Timed out without moving");
                isTimeout = true;
            }
        }

        //Action ends
        Joystick_Disabled = true;
        source.Cancel();

        //Stop flashing in case of it
        if (isFlashing)
        {
            flashing_FF_on = false;
        }

        //Turn off FF if always on
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

        FFMovement = new Vector3(0.0f, 0.0f, 0.0f);
        velocity = 0.0f;
        isTrial = false;
        currPhase = Phases.check;
        phase_task_selecter = Phases.check;
        // Debug.Log("Trial Phase End.");
    }

    /// <summary>
    /// Save the player's position (pPos) and the firefly (reward zone)'s position (fPos)
    /// and start a coroutine to wait for some random amount of time between the user's
    /// specified minimum and maximum wait times
    /// </summary>
    async Task Check()
    {
        //Entering check phase
        isCheck = true;
        rewarded_FF_trial = false;
        FF2shown = false;
        MoveStartTime = 99999f;
        if (flagMultiFF)
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

        //Distances to FF
        float distance = 0.0f;
        float current_smallest_distance = 9999f;

        Vector3 player_check_pos;
        Quaternion player_check_rot;

        //Player plane position
        player_position = player.transform.position - new Vector3(0.0f, Player_Height, 0.0f);

        //Player save positions
        player_check_pos = player.transform.position;
        player_check_rot = player.transform.rotation;

        //Did not time out
        if (!isTimeout)
        {
            source = new CancellationTokenSource();

            float delay = c_lambda * Mathf.Exp(-c_lambda * ((float)rand.NextDouble() * (c_max - c_min) + c_min));
            checkWait.Add(delay);
            await new WaitForSeconds(delay);
        }
        //Timed out
        else
        {
            checkWait.Add(0.0f);

            audioSource.clip = loseSound;
        }

        //Calculating distances
        if (isCOM)
        {
            for (int i = 0; i < 2; i++)
            {
                if(!(Multiple_FF_List[i].transform.position.x == 0 && Multiple_FF_List[i].transform.position.z == 0))
                {
                    ffPosStr = string.Concat(ffPosStr, ",", Multiple_FF_List[i].transform.position.ToString("F5").Trim(toTrim).Replace(" ", "")).Substring(1);
                    distance = Vector3.Distance(player_position, Multiple_FF_List[i].transform.position);
                    //print(distance);
                    if (distance <= reward_zone_radius && distance < current_smallest_distance)
                    {
                        current_smallest_distance = distance;
                        rewarded_FF_trial = true;
                        colorhit = i;
                    }
                }
            }
        }
        else if (multiple_FF_mode == 1)
        {
            for (int i = 0; i < NumberOfFF; i++)
            {
                ffPosStr = string.Concat(ffPosStr, ",", Multiple_FF_List[i].transform.position.ToString("F5").Trim(toTrim).Replace(" ", "")).Substring(1);
                distance = Vector3.Distance(player_position, Multiple_FF_List[i].transform.position);
                //print(distance);
                if (distance <= reward_zone_radius && distance < current_smallest_distance)
                {
                    current_smallest_distance = distance;
                    rewarded_FF_trial = true;
                    colorhit = i;
                }
            }
        }
        else
        {
            if (Vector3.Distance(player_position, firefly.transform.position) <= reward_zone_radius) rewarded_FF_trial = true;
            distance = Vector3.Distance(player_position, firefly.transform.position);
            ffPosStr = firefly.transform.position.ToString("F5").Trim(toTrim).Replace(" ", "");
        }

        //Calculate and give rewards
        if (rewarded_FF_trial)
        {
            audioSource.clip = winSound;
            if (isCOM && isColored)
            {
                juiceTime = colorrewards[(int)colorchosen[(int)colorhit] - 1];
            }
            else if (isCOM)
            {
                juiceTime = Mathf.Lerp(maxJuiceTime, minJuiceTime, Mathf.InverseLerp(0.0f, reward_zone_radius, distance));
                
            }
            else
            {
                juiceTime = Mathf.Lerp(maxJuiceTime, minJuiceTime, Mathf.InverseLerp(0.0f, reward_zone_radius, distance));
            }
            juiceDuration.Add(juiceTime);
            audioSource.Play();
            good_trial_count++;
            if (!isHuman)
            {
                SendMarker("j", juiceTime);
                await new WaitForSeconds((juiceTime / 1000.0f) + 0.25f);
            }
        }
        else
        {
            juiceTime = 0;
            audioSource.clip = loseSound;
            juiceDuration.Add(0.0f);
            rewardTime.Add(0.0f);
            audioSource.Play();
            await new WaitForSeconds(0.25f);
        }

        //Save general final position values
        player_final_position.Add(player_check_pos.ToString("F5").Trim(toTrim).Replace(" ", ""));
        player_final_rotation.Add(player_check_rot.ToString("F5").Trim(toTrim).Replace(" ", ""));
        print(player_check_rot.ToString("F5").Trim(toTrim).Replace(" ", ""));
        score.Add(rewarded_FF_trial ? 1 : 0);
        timedout.Add(isTimeout ? 1 : 0);
        FF_final_positions.Add(ffPosStr);

        //PTB values (0 will be added if not using) TODO: check
        if (true)
        {
            timeCntPTBStart.Add(SharedJoystick.timeCntPTBStart - programT0);
            SharedJoystick.timeCntPTBStart = programT0;
            ptbJoyVelMin.Add(SharedJoystick.GaussianPTBVMin);
            ptbJoyVelMax.Add(SharedJoystick.GaussianPTBVMax);
            ptbJoyVelStartRange.Add(SharedJoystick.ptbJoyVelStartRange);
            ptbJoyVelStart.Add(SharedJoystick.ptbJoyVelStart);
            ptbJoyVelMu.Add(SharedJoystick.ptbJoyVelMu);
            ptbJoyVelSigma.Add(SharedJoystick.ptbJoyVelSigma);
            ptbJoyVelGain.Add(SharedJoystick.ptbJoyVelGain);
            ptbJoyVelEnd.Add(SharedJoystick.ptbJoyVelEnd);
            ptbJoyVelLen.Add(SharedJoystick.ptbJoyVelLen);
            ptbJoyVelValue.Add(SharedJoystick.ptbJoyVelValue);
            ptbJoyRotMin.Add(SharedJoystick.GaussianPTBRMin);
            ptbJoyRotMax.Add(SharedJoystick.GaussianPTBRMax);
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
            SharedJoystick.ptbJoyFlagTrial = Convert.ToInt32(rand.NextDouble() <= SharedJoystick.GaussianPTBRatio); ;
            ptbJoyRatio.Add(SharedJoystick.GaussianPTBRatio);
            ptbJoyOn.Add(SharedJoystick.GaussianPTB ? 1 : 0);
            ptbJoyEnableTime.Add(SharedJoystick.ptbJoyEnableTime);
        }

        //nff
        if (multiple_FF_mode != 0)
        {
            distance_to_FF.Add(string.Format(",{0}",current_smallest_distance));
            if (isColored)
            {
                FF_color.Add(string.Format(",{0},{1}", colorchosen[0], colorchosen[1]));
                colorchosen.Clear();
            }
            else
            {
                FF_color.Add(",0,0");
            }
        }
        else
        {
            distance_to_FF.Add(string.Format(",{0}", distance));
        }

        //Stimulation
        if (flagFFstimulation && stimulatedTrial)
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

        float wait = i_lambda * Mathf.Exp(-i_lambda * ((float)rand.NextDouble() * (i_max - i_min) + i_min));
        if (flagFFstimulation && stimulatedTrial)
        {
            stimulatedTrial = false;
            wait += microStimuDur; //wait more if it was a stimulated trail
        }
        interWait.Add(wait);
        isEnd = true;
        ffPosStr = "";
        currPhase = Phases.ITI;
        float joystickT = PlayerPrefs.GetFloat("JoystickThreshold");
        float startthreshold = PlayerPrefs.GetFloat("JoystickStartThreshold");

        //TODO: check COM stop conditions
        if (!isCOM)
        {
            player.transform.SetPositionAndRotation(Vector3.up * Player_Height, Quaternion.Euler(0.0f, 0.0f, 0.0f));
            drunkplayer.transform.SetPositionAndRotation(Vector3.up * Player_Height, Quaternion.Euler(0.0f, 0.0f, 0.0f));
            await new WaitUntil(() => Mathf.Abs(SharedJoystick.currentSpeed) < velStopThreshold && Mathf.Abs(SharedJoystick.currentRot) < rotStopThreshold &&
            (float)Math.Abs(SharedJoystick.rawX) <= startthreshold && (float)Math.Abs(SharedJoystick.rawY) <= startthreshold);
            await new WaitForSeconds(wait);
        }
        else
        {
            await new WaitUntil(() => Mathf.Abs(SharedJoystick.currentSpeed) < velStopThreshold && Mathf.Abs(SharedJoystick.currentRot) < rotStopThreshold);
            await new WaitForSeconds(wait);
            player.transform.SetPositionAndRotation(Vector3.up * Player_Height, Quaternion.Euler(0.0f, 0.0f, 0.0f));
            drunkplayer.transform.SetPositionAndRotation(Vector3.up * Player_Height, Quaternion.Euler(0.0f, 0.0f, 0.0f));
        }

        isTimeout = false;
        //Debug.Log("Check Phase End.");
        phase_task_selecter = Phases.begin;
        currPhase = Phases.begin;
    }

    /// <summary>
    /// Flash function for FF (obj), async, or flash it while flashing flag is on
    /// Pulse_Width: FF on time for cycle, in seconds
    /// flashing_frequency: FF flashing cycles per second, or Hz
    /// </summary>
    public async Task Flash(GameObject obj)
    {
        while (flashing_FF_on && obj.activeInHierarchy)
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

    //Blink the single FF
    public async void OnOff()
    {
        //print(string.Format("blinking for {0}",lifeSpan));
        firefly.SetActive(true);
        await new WaitForSeconds(lifeSpan/1000);
        firefly.SetActive(false);
    }

    //Blink a specific FF in multiple FF
    public async void OnOff(GameObject obj)
    {
        obj.SetActive(true);
        await new WaitForSeconds(lifeSpan/1000);
        obj.SetActive(false);
    }

    //Sending markers
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

        if (!isHuman)
        {
            juiceBox.Write(toSend);

            await new WaitForSeconds(time / 1000.0f);
        }
    }

    private float Tcalc(float t, float lambda)
    {
        return -1.0f / lambda * Mathf.Log(t / lambda);
    }

    /// <summary>
    /// Save data to file path.
    /// 
    /// TODO: discuss multiple FF data saving.
    /// Check if the format matches.
    /// </summary>
    public void Save()
    {
        try
        {
            string disc_header;

            List<int> temp;

            StringBuilder csvDisc = new StringBuilder();

            if (NumberOfFF > 1)
            {
                string ffPosStr = "";
                string distStr = "";
                string checkStr = "";

                for (int i = 0; i < NumberOfFF; i++)
                {
                    ffPosStr = string.Concat(ffPosStr, string.Format("ffX{0},ffY{0},ffZ{0},", i));
                    distStr = string.Concat(distStr, string.Format("distToFF{0},", i));
                    checkStr = string.Concat(checkStr, string.Format("checkTime{0},", i));
                }

                disc_header = string.Format("n,max_v,max_w,ffv,onDuration,density,PosX0,PosY0,PosZ0,RotX0,RotY0,RotZ0,RotW0,{0}pCheckX,pCheckY,pCheckZ,rCheckX,rCheckY,rCheckZ,rCheckW,{1}rewarded,", ffPosStr, distStr) +
                    "timeout,juiceDuration,beginTime,checkTime,rewardTime,endTime,checkWait,interWait,CurrentTau,PTBType,SessionTauTau,ProcessNoiseTau,ProcessNoiseVelGain,ProcessNoiseRotGain,nTaus,minTaus,maxTaus,MeanDist," +
                    "MeanTravelTime,VelStopThresh,RotStopThresh,VelBrakeThresh,RotBrakeThresh,StimulationTime,StimulationDuration,StimulationRatio,ObsNoiseTau,ObsNoiseVelGain,ObsNoiseRotGain,DistractorFlowRatio,ColoredOpticFlow,COMTrialType,"
                    + PlayerPrefs.GetString("Name") + "," + DateTime.Today.ToString("MMddyyyy") + "," + PlayerPrefs.GetInt("Run Number").ToString("D3");
            }
            else
            {
                disc_header = "n,max_v,max_w,ffv,onDuration,density,PosX0,PosY0,PosZ0,RotX0,RotY0,RotZ0,RotW0,ffX,ffY,ffZ,pCheckX,pCheckY,pCheckZ,rCheckX,rCheckY,rCheckZ,rCheckW,distToFF,rewarded," +
                    "timeout,juiceDuration,beginTime,checkTime,rewardTime,endTime,checkWait,interWait,CurrentTau,PTBType,SessionTauTau,ProcessNoiseTau,ProcessNoiseVelGain,ProcessNoiseRotGain,nTaus,minTaus,maxTaus,MeanDist," +
                    "MeanTravelTime,VelStopThresh,RotStopThresh,VelBrakeThresh,RotBrakeThresh,StimulationTime,StimulationDuration,StimulationRatio,ObsNoiseTau,ObsNoiseVelGain,ObsNoiseRotGain,DistractorFlowRatio,ColoredOpticFlow,COMTrialType,"
                    + PlayerPrefs.GetString("Name") + "," + DateTime.Today.ToString("MMddyyyy") + "," + PlayerPrefs.GetInt("Run Number").ToString("D3");
            }
            csvDisc.AppendLine(disc_header);

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

            if (isColored)
            {
                temp.Add(FF_color.Count);
            }
            if (flagFFstimulation)
            {
                temp.Add(stimulated.Count);
                temp.Add(timeStimuStart.Count);
                temp.Add(trialStimuDur.Count);
            }
            if (isCOM)
            {
                temp.Add(COMtrialtype.Count);
            }
            if(control_dynamics != 0)
            {
                temp.Add(CurrentTau.Count);
            }
            //foreach (int count in temp)
            //{
            //    print(count);
            //}
            temp.Sort();

            //Score count
            var totalScore = 0;

            for (int i = 0; i < temp[0]; i++)
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
                    line = line + ',' + SharedJoystick.flagCtrlDynamics + ',' + SharedJoystick.TauTau + ',' + SharedJoystick.NoiseTau + ',' + PlayerPrefs.GetFloat("VelocityNoiseGain") + ',' +
            PlayerPrefs.GetFloat("RotationNoiseGain") + ',' + (int)PlayerPrefs.GetFloat("NumTau") + ',' + PlayerPrefs.GetFloat("MinTau") + ',' + PlayerPrefs.GetFloat("MaxTau")
            + ',' + PlayerPrefs.GetFloat("MeanDistance") + ',' + PlayerPrefs.GetFloat("MeanTime") + ',' + velStopThreshold + ',' + rotStopThreshold + ',' + PlayerPrefs.GetFloat("velBrakeThresh")
            + ',' + PlayerPrefs.GetFloat("rotBrakeThresh");
                }
                else
                {
                    line += string.Format(",0,0,0,0,0,0,0,0,0,0,0,0,0,0,0");
                }

                if (flagFFstimulation)
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

                if (isColored)
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
            PlayerPrefs.SetInt("Total Trials", trial_number[trial_number.Count - 1]);

            SaveConfigs();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError(e);
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

    //Config Saving
    //TODO: check all is saved after GUI update
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

        xmlWriter.WriteStartElement("Life_Span");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Life_Span").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Draw_Distance");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Draw_Distance").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Density_Low");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Density_Low").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Density_High");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Density_High").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Density_Low_Ratio");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Density_Low_Ratio").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Floor_Height");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Floor_Height").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Player_Height");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Player_Height").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Setting");
        xmlWriter.WriteAttributeString("Type", "Joystick Settings");

        xmlWriter.WriteStartElement("Max_Linear_Speed");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Max_Linear_Speed").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Max_Angular_Speed");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Max_Angular_Speed").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("GaussianPTB");
        xmlWriter.WriteString(PlayerPrefs.GetInt("GaussianPTB").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("GaussianPTBVMax");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("GaussianPTBVMax").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("GaussianPTBRMax");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("GaussianPTBRMax").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("GaussianPTBRatio");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("GaussianPTBRatio").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("LinVelStopThresh");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("LinVelStopThresh").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("AngVelStopThresh");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("AngVelStopThresh").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Setting");
        xmlWriter.WriteAttributeString("Type", "Firefly Settings");

        xmlWriter.WriteStartElement("RadiusFF");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("RadiusFF").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("reward_zone_radius");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("reward_zone_radius").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("minDrawDistance");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("minDrawDistance").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("maxDrawDistance");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("maxDrawDistance").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("minPhi");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("minPhi").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("maxPhi");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("maxPhi").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("minPhi2");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("minPhi2").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("maxPhi2");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("maxPhi2").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("minJuiceTime");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("minJuiceTime").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("maxJuiceTime");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("maxJuiceTime").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("NumDiscreteDistances");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("NumDiscreteDistances").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("NumDiscreteAngles");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("NumDiscreteAngles").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("isDiscrete");
        xmlWriter.WriteString(PlayerPrefs.GetInt("isDiscrete").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("ratio_always_on");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("ratio_always_on").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("NumberOfFF");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("NumberOfFF").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("MultipleFireflyMode");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("multiple_FF_mode").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("FFseparation");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("FFseparation").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("LifespanDuration1");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("LifespanDuration1").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("LifespanDuration2");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("LifespanDuration2").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("LifespanDuration3");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("LifespanDuration3").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("LifespanDuration4");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("LifespanDuration4").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("LifespanDuration5");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("LifespanDuration5").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("LifespanRatio1");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("LifespanRatio1").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("LifespanRatio2");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("LifespanRatio2").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("LifespanRatio3");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("LifespanRatio3").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("LifespanRatio4");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("LifespanRatio4").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("LifespanRatio5");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("LifespanRatio5").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Timeout");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Timeout").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Frequency");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("Frequency").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("duty_cycle");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("duty_cycle").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("checkMin");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("checkMin").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("checkMax");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("checkMax").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("CheckMean");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("CheckMean").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("interMin");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("interMin").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("interMax");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("interMax").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("IntertialMean");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("IntertialMean").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("FireflySeed");
        xmlWriter.WriteString(seed.ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Setting");
        xmlWriter.WriteAttributeString("Type", "Moving Firefly Settings");

        xmlWriter.WriteStartElement("MovingON");
        xmlWriter.WriteString(PlayerPrefs.GetInt("isMoving").ToString());
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

        xmlWriter.WriteStartElement("RunNumber");
        xmlWriter.WriteString((PlayerPrefs.GetInt("Run Number")).ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("Setting");
        xmlWriter.WriteAttributeString("Type", "Calibration Settings");

        xmlWriter.WriteStartElement("isAuto");
        xmlWriter.WriteString(PlayerPrefs.GetInt("isAuto").ToString());
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("ignoreInitialSeconds");
        xmlWriter.WriteString(PlayerPrefs.GetFloat("ignoreInitialSeconds").ToString());
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

    //Read csv file for COM task
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

    public void ReadCoord2CSV()
    {
        StreamReader strReader = new StreamReader(path + "\\Config_2FF_2.csv");
        bool endoffile = false;
        while (!endoffile)
        {
            string data_string = strReader.ReadLine();
            if (data_string == null)
            {
                break;
            }
            var data_values = data_string.Split(',');
            Tuple<float, float> New_Coord_Tuple;
            float x = float.Parse(data_values[0], CultureInfo.InvariantCulture.NumberFormat);
            float y = float.Parse(data_values[1], CultureInfo.InvariantCulture.NumberFormat);
            New_Coord_Tuple = new Tuple<float, float>(x, y);
            FF2coordsList.Add(New_Coord_Tuple);
        }
    }

    public void ReadFFCoordDisc()
    {
        StreamReader strReader = new StreamReader(PlayerPrefs.GetString("replay_disc_path"));
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
            print(New_Coord_Tuple);
        }
    }
}