using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;

public class RFPlot : MonoBehaviour
{
    public GameObject dot;
    public GameObject fixationPoint;
    public int size;
    public float density;
    public float speed;

    [Tooltip("SerialPort of your device.")]
    [HideInInspector] public string portName = "COM3";

    [Tooltip("Baudrate")]
    [HideInInspector] public int baudRate = 2000000;

    [Tooltip("Timeout")]
    [HideInInspector] public int ReadTimeout = 5000;

    [Tooltip("QueueLength")]
    [HideInInspector] public int QueueLength = 1;

    SerialPort juiceBox;

    [HideInInspector] public float juiceTime = 75.0f;

    [Tooltip("Trial timeout (how much time player can stand still before trial ends")]
    [HideInInspector] public float timeout;
    [Tooltip("Minimum x value to plug into exponential distribution from which time to wait before new trial is pulled")]
    [HideInInspector] public float interMax;
    [Tooltip("Maximum x value to plug into exponential distribution from which time to wait before new trial is pulled")]
    [HideInInspector] public float interMin;
    [Tooltip("Player height")]
    [HideInInspector] public float p_height;
    // 1 / Mean for intertrial time exponential distribution
    private float i_lambda;
    private float i_min;
    private float i_max;

    private float wait;

    private int nPatch;
    private float height;
    private float width;

    private float tPrev;
    private float delay;

    private int nSwitch = 0;

    private bool fixating = true;

    private Vector3 dxz;

    public enum Phases
    {
        begin = 0,
        trial = 1,
        juice = 2,
        ITI = 3,
        none = 9
    }
    public Phases phase;

    public struct holderProperties
    {
        public float left;
        public float right;
        public float top;
        public float bottom;
    };

    private List<GameObject> dots = new List<GameObject>();
    private List<GameObject> holders = new List<GameObject>();
    private List<holderProperties> properties = new List<holderProperties>();
    private List<Vector3> directions = new List<Vector3>();

    private int seed;
    private System.Random rand;

    // Start is called before the first frame update
    void Start()
    {
        seed = UnityEngine.Random.Range(1, 10000);
        rand = new System.Random(seed);

        portName = PlayerPrefs.GetString("Port");
        juiceBox = new SerialPort(portName, baudRate);
        //juiceBox.Open();
        //juiceBox.ReadTimeout = ReadTimeout;

        //SendMarker("f", 0.0f);

        juiceTime = PlayerPrefs.GetFloat("Juice Time");

        i_lambda = 1.0f / PlayerPrefs.GetFloat("Mean 2");
        interMin = PlayerPrefs.GetFloat("Minimum Intertrial Wait");
        interMax = PlayerPrefs.GetFloat("Maximum Intertrial Wait");
        i_min = Tcalc(interMin, i_lambda);
        i_max = Tcalc(interMax, i_lambda);

        nPatch = (int)Mathf.Pow(size, 2.0f);
        height = 2.0f / size;
        width = 2.0f / size;

        for (int i = 0; i < nPatch; i++)
        {
            string name = "Holder" + i.ToString();
            holders.Add(new GameObject(name));
            properties.Add(new holderProperties());
            directions.Add(Vector3.zero);
        }

        int nDots = Mathf.RoundToInt(height * width * density);
        for (int i = 0; i < nPatch; i++)
        {
            for (int k = 0; k < nDots; k++)
            {
                dots.Add(Instantiate(dot));
                dots[dots.Count - 1].transform.parent = holders[i].transform;
            }
        }

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float x = width * i - 1.0f;
                float y = height * j - 1.0f + height;
                holderProperties prop = properties[size * i + j];
                prop.left = x;
                prop.top = y;
                prop.right = x + width;
                prop.bottom = y - height;
                properties[size * i + j] = prop;

                GameObject holder = holders[size * i + j];
                holder.transform.position = new Vector3(prop.left, 0.0f, prop.top);
                foreach (Transform child in holder.transform)
                {
                    child.transform.position = new Vector3(UnityEngine.Random.value * width, 0.0f, UnityEngine.Random.value * height);
                    child.gameObject.SetActive(false);
                }

                int direction = UnityEngine.Random.Range(0, 8);

                switch (direction)
                {
                    case 0:
                        // N
                        directions[size * i + j] = new Vector3(1.0f, 0.0f, 0.0f) * speed * Time.deltaTime;
                        break;
                    case 1:
                        // NE
                        directions[size * i + j] = new Vector3(1.0f, 0.0f, 1.0f) * speed * Time.deltaTime;
                        break;
                    case 2:
                        // E
                        directions[size * i + j] = new Vector3(0.0f, 0.0f, 1.0f) * speed * Time.deltaTime;
                        break;
                    case 3:
                        // SE
                        directions[size * i + j] = new Vector3(-1.0f, 0.0f, -1.0f) * speed * Time.deltaTime;
                        break;
                    case 4:
                        // S
                        directions[size * i + j] = new Vector3(-1.0f, 0.0f, 0.0f) * speed * Time.deltaTime;
                        break;
                    case 5:
                        // SW
                        directions[size * i + j] = new Vector3(-1.0f, 0.0f, -1.0f) * speed * Time.deltaTime;
                        break;
                    case 6:
                        // W
                        directions[size * i + j] = new Vector3(0.0f, 0.0f, -1.0f) * speed * Time.deltaTime;
                        break;
                    case 7:
                        // NW
                        directions[size * i + j] = new Vector3(1.0f, 0.0f, -1.0f) * speed * Time.deltaTime;
                        break;
                }
            }
        }

        int position = UnityEngine.Random.Range(0, 3);

        switch (position)
        {
            case 0:
                fixationPoint.transform.position = new Vector3(-2.0f * width, 0.0f, 0.0f);
                break;
            case 1:
                fixationPoint.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
                break;
            case 2:
                fixationPoint.transform.position = new Vector3(2.0f * width, 0.0f, 0.0f);
                break;
        }

        //ToggleStimulus();

        Debug.Log("begin");

        phase = Phases.begin;

        tPrev = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        float tNow = Time.time;

        switch ((int)phase)
        {
            case 0: // begin
                //SendMarker("s", 100.0f);

                fixationPoint.SetActive(true);
                
                if (tNow - tPrev > 0.2f)
                {
                    Debug.Log("trial");
                    ToggleStimulus();
                    phase = Phases.trial;
                    tPrev = tNow;
                }

                break;

            case 1: // trial
                if (tNow - tPrev > 0.1f)
                {
                    UpdateStimulus(true);
                    tPrev = tNow;
                    nSwitch++;
                }
                else
                {
                    UpdateStimulus(false);
                }

                if (fixating == false && nSwitch < 20)
                {
                    ToggleStimulus();
                    fixationPoint.SetActive(false);
                    phase = Phases.ITI;
                    tPrev = tNow;
                }
                else if (fixating && nSwitch >= 20)
                {
                    ToggleStimulus();
                    fixationPoint.SetActive(false);
                    Debug.Log("juice");
                    phase = Phases.juice;
                    tPrev = tNow;
                }
                break;

            case 2: // juice
                //SendMarker("j", juiceTime);
                if (tNow - tPrev > juiceTime / 1000.0f)
                {
                    wait = i_lambda * Mathf.Exp(-i_lambda * ((float)rand.NextDouble() * (i_max - i_min) + i_min));

                    Debug.Log("ITI: " + wait + "s");
                    phase = Phases.ITI;
                    tPrev = tNow;
                }
                break;

            case 3: // ITI

                int position = UnityEngine.Random.Range(0, 3);

                switch (position)
                {
                    case 0:
                        fixationPoint.transform.position = new Vector3(-2.0f * width, 0.0f, 0.0f);
                        break;
                    case 1:
                        fixationPoint.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
                        break;
                    case 2:
                        fixationPoint.transform.position = new Vector3(2.0f * width, 0.0f, 0.0f);
                        break;
                }
                //SendMarker("e", wait * 1000.0f);
                if (tNow - tPrev > wait)
                {
                    nSwitch = 0;
                    Debug.Log("begin");
                    tPrev = tNow;
                    phase = Phases.begin;
                }
                break;

            case 9: // none

                break;

            default:

                break;
        }
    }

    public async void ToggleStimulus()
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                holderProperties prop = properties[size * i + j];

                GameObject holder = holders[size * i + j];
                foreach (Transform child in holder.transform)
                {
                    child.gameObject.SetActive(!child.gameObject.activeInHierarchy);
                }
            }
        }

        await new WaitForUpdate();
    }

    public async void UpdateStimulus(bool change)
    {
        if (change)
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    int direction = UnityEngine.Random.Range(0, 8);

                    switch (direction)
                    {
                        case 0:
                            // N
                            directions[size * i + j] = new Vector3(1.0f, 0.0f, 0.0f) * speed * Time.deltaTime;
                            break;
                        case 1:
                            // NE
                            directions[size * i + j] = new Vector3(1.0f, 0.0f, 1.0f) * speed * Time.deltaTime;
                            break;
                        case 2:
                            // E
                            directions[size * i + j] = new Vector3(0.0f, 0.0f, 1.0f) * speed * Time.deltaTime;
                            break;
                        case 3:
                            // SE
                            directions[size * i + j] = new Vector3(-1.0f, 0.0f, -1.0f) * speed * Time.deltaTime;
                            break;
                        case 4:
                            // S
                            directions[size * i + j] = new Vector3(-1.0f, 0.0f, 0.0f) * speed * Time.deltaTime;
                            break;
                        case 5:
                            // SW
                            directions[size * i + j] = new Vector3(-1.0f, 0.0f, -1.0f) * speed * Time.deltaTime;
                            break;
                        case 6:
                            // W
                            directions[size * i + j] = new Vector3(0.0f, 0.0f, -1.0f) * speed * Time.deltaTime;
                            break;
                        case 7:
                            // NW
                            directions[size * i + j] = new Vector3(1.0f, 0.0f, -1.0f) * speed * Time.deltaTime;
                            break;
                    }
                }
            }
        }

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {

                holderProperties prop = properties[size * i + j];

                GameObject holder = holders[size * i + j];
                foreach (Transform child in holder.transform)
                {
                    child.position += directions[size * i + j];

                    if (child.position.x > prop.right)
                    {
                        child.position -= new Vector3(width, 0.0f, 0.0f);
                    }
                    else if (child.position.x < prop.left)
                    {
                        child.position += new Vector3(width, 0.0f, 0.0f);
                    }

                    if (child.position.z > prop.top)
                    {
                        child.position -= new Vector3(0.0f, 0.0f, height);
                    }
                    else if (child.position.z < prop.bottom)
                    {
                        child.position += new Vector3(0.0f, 0.0f, height);
                    }
                }
            }
        }

        await new WaitForUpdate();
    }

    public async void SendMarker(string mark, float time)
    {
        string toSend = "i" + mark + time.ToString();

        juiceBox.Write(toSend);

        await new WaitForSeconds(time / 1000.0f);
    }

    private float Tcalc(float t, float lambda)
    {
        return -1.0f / lambda * Mathf.Log(t / lambda);
    }

    private void OnDisable()
    {
        juiceBox.Close();
    }

    private void OnApplicationQuit()
    {
        juiceBox.Close();
    }
}
