using UnityEngine;
using static JoystickMonke;

public class Particles : MonoBehaviour
{
    [HideInInspector] public float Life_Span;
    [HideInInspector] public float Draw_Distance;
    [HideInInspector] public float Density_Low;
    [HideInInspector] public float Density_High;
    [HideInInspector] public float Density_Low_Ratio;
    [HideInInspector] public float ObsDensityRatio;
    private bool isObsNoise;
    public float TauValueR = 0.0F;
    public float TauValueG = 0.0F;
    public float TauValueB = 0.0F;
    public float TauValueA = 1.0F;
    float density;
    float prevDensity;
    [HideInInspector] public float Floor_Height;
    readonly float baseH = 0.0185f;
    int seed;
    [HideInInspector]
    public bool changedensityflag = false;
    [HideInInspector] public static Particles particles;
    ParticleSystem particleSystem;
    public int counter = 0;

    // Start is called before the first frame update
    void OnEnable()
    {
        particles = this;

        seed = Random.Range(1, 10000);
        bool replay = PlayerPrefs.GetInt("isReplay") == 1;
        if (replay)
        {
            seed = (int)PlayerPrefs.GetFloat("replay_seed");
        }
        print(seed);
        PlayerPrefs.SetInt("Optic Flow Seed", seed);
        Life_Span = PlayerPrefs.GetFloat("Life_Span");
        Draw_Distance = PlayerPrefs.GetFloat("Draw_Distance");
        Density_Low = PlayerPrefs.GetFloat("Density_Low");
        Density_High = PlayerPrefs.GetFloat("Density_High");
        Density_Low_Ratio = PlayerPrefs.GetFloat("Density_Low_Ratio");
        Floor_Height = PlayerPrefs.GetFloat("Floor_Height");
        ObsDensityRatio = PlayerPrefs.GetFloat("ObsDensityRatio");
        isObsNoise = PlayerPrefs.GetInt("isObsNoise") == 1;
        particleSystem = GetComponent<ParticleSystem>();

        particleSystem.Stop();
        
        particleSystem.randomSeed = (uint)seed;

        particleSystem.Play();

        var main = particleSystem.main;
        var emission = particleSystem.emission;
        var shape = particleSystem.shape;

        main.startLifetime = Life_Span;
        main.startSize = Floor_Height * baseH;

        var n = Random.Range(0f, 1f);

        if (n < Density_Low_Ratio)
        {
            density = Density_Low;
        }
        else
        {
            density = Density_High;
        }

        if (isObsNoise)
        {
            if(name == "Dots")
            {
                density *= 1 - ObsDensityRatio;
                Density_High *= 1 - ObsDensityRatio;
                Density_Low *= 1 - ObsDensityRatio;
            }
            else
            {
                density *= ObsDensityRatio;
                Density_High *= ObsDensityRatio;
                Density_Low *= ObsDensityRatio;
            }
        }

        prevDensity = density;

        main.maxParticles = Mathf.RoundToInt(Mathf.Pow(Draw_Distance, 2.0f) * Mathf.PI * density / Floor_Height);

        var densityInCM = Mathf.FloorToInt(main.maxParticles / 10000.0f) < 1f ? main.maxParticles / 10000.0f : Mathf.FloorToInt(main.maxParticles / 10000.0f);
        emission.rateOverTime = Mathf.Floor(densityInCM / Life_Span * 10000.0f / (Floor_Height));

        shape.randomPositionAmount = Draw_Distance;
    }

    // Update is called once per frame
    void Update()
    {
        if((int)PlayerPrefs.GetFloat("PTBType") != 0 && (int) PlayerPrefs.GetFloat("TauColoredFloor") == 1)
        {
            counter++;
            var main = particleSystem.main;
            var TauValue = SharedJoystick.savedTau/3;
            if(TauValue > 1)
            {
                TauValue = 1;
            }
            TauValueR = 1;
            TauValueG = TauValue;
            TauValueB = 0;
            main.startColor = new Color(TauValueR, TauValueG, TauValueB, TauValueA);
        }
    }

    public float SwitchDensity()
    {
        var n = Random.Range(0f, 1f);

        var main = particleSystem.main;
        var emission = particleSystem.emission;

        if (n < Density_Low_Ratio)
        {
            //print("low");
            density = Density_Low;
        }
        else
        {
            //print("high");
            density = Density_High;
        }

        if (density != prevDensity)
        {
            //print(density);
            changedensityflag = true;
            main.maxParticles = Mathf.RoundToInt(Mathf.Pow(Draw_Distance, 2.0f) * Mathf.PI * density / Floor_Height);
            var densityInCM = Mathf.FloorToInt(main.maxParticles / 10000.0f) < 1f ? main.maxParticles / 10000.0f : Mathf.FloorToInt(main.maxParticles / 10000.0f);
            emission.rateOverTime = Mathf.Floor(densityInCM / Life_Span * 10000.0f / (Floor_Height));
            particleSystem.Clear();
        }
        else
        {
            changedensityflag = false;
        }

        prevDensity = density;

        return density;
    }

    public void SetDensity(float density)
    {
        var main = particleSystem.main;
        var emission = particleSystem.emission;
        main.maxParticles = Mathf.RoundToInt(Mathf.Pow(Draw_Distance, 2.0f) * Mathf.PI * density / Floor_Height);
        var densityInCM = Mathf.FloorToInt(main.maxParticles / 10000.0f) < 1f ? main.maxParticles / 10000.0f : Mathf.FloorToInt(main.maxParticles / 10000.0f);
        emission.rateOverTime = Mathf.Floor(densityInCM / Life_Span * 10000.0f / (Floor_Height));
        particleSystem.Clear();
    }
}
