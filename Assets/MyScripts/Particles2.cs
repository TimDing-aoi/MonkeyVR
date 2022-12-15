using UnityEngine;
using static JoystickMonke;

public class Particles2 : MonoBehaviour
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
    [HideInInspector] public float Floor_Height;
    readonly float baseH = 0.0185f;
    uint seed;
    [HideInInspector] public static Particles2 particles2;
    ParticleSystem particleSystem;
    public int counter = 0;

    // Start is called before the first frame update
    void OnEnable()
    {
        particles2 = this;

        seed = (uint)UnityEngine.Random.Range(10000, 20000);
        PlayerPrefs.SetInt("Optic Flow Seed", (int)seed);
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

        particleSystem.randomSeed = seed;

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
            if (name == "Dots")
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

        main.maxParticles = Mathf.RoundToInt(Mathf.Pow(Draw_Distance, 2.0f) * Mathf.PI * density / Floor_Height);

        var densityInCM = Mathf.FloorToInt(main.maxParticles / 10000.0f) < 1f ? main.maxParticles / 10000.0f : Mathf.FloorToInt(main.maxParticles / 10000.0f);
        emission.rateOverTime = Mathf.Floor(densityInCM / Life_Span * 10000.0f / (Floor_Height));

        shape.randomPositionAmount = Draw_Distance;
    }

    // Update is called once per frame
    void Update()
    {
        if ((int)PlayerPrefs.GetFloat("PTBType") != 0 && (int)PlayerPrefs.GetFloat("TauColoredFloor") == 1)
        {
            counter++;
            var main = particleSystem.main;
            var TauValue = SharedJoystick.savedTau / 3;
            if (TauValue > 1)
            {
                TauValue = 1;
            }
            TauValueR = 1;
            TauValueG = TauValue;
            TauValueB = 0;
            main.startColor = new Color(TauValueR, TauValueG, TauValueB, TauValueA);
        }
    }

    public void SwitchDensity2()
    {
        var main = particleSystem.main;
        var emission = particleSystem.emission;

        if (density == Density_High)
        {
            //print("low");
            density = Density_Low;
        }
        else
        {
            //print("high");
            density = Density_High;
        }

        print(density);
        main.maxParticles = Mathf.RoundToInt(Mathf.Pow(Draw_Distance, 2.0f) * Mathf.PI * density / Floor_Height);
        var densityInCM = Mathf.FloorToInt(main.maxParticles / 10000.0f) < 1f ? main.maxParticles / 10000.0f : Mathf.FloorToInt(main.maxParticles / 10000.0f);
        emission.rateOverTime = Mathf.Floor(densityInCM / Life_Span * 10000.0f / (Floor_Height));
        particleSystem.Clear();
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
