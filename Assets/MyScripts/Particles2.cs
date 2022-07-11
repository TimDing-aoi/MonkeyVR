using UnityEngine;
using static JoystickMonke;

public class Particles2 : MonoBehaviour
{
    [HideInInspector] public float lifeSpan;
    [HideInInspector] public float dist;
    [HideInInspector] public float densityLow;
    [HideInInspector] public float densityHigh;
    [HideInInspector] public float densityLowRatio;
    [HideInInspector] public float ObsDensityRatio;
    private bool isObsNoise;
    public float TauValueR = 0.0F;
    public float TauValueG = 0.0F;
    public float TauValueB = 0.0F;
    public float TauValueA = 1.0F;
    float density;
    [HideInInspector] public float t_height;
    readonly float baseH = 0.0185f;
    uint seed;
    [HideInInspector] public static Particles2 particles2;
    ParticleSystem particleSystem;
    public int counter = 0;

    // Start is called before the first frame update
    void OnEnable()
    {
        particles2 = this;

        seed = (uint)UnityEngine.Random.Range(1, 10000);
        PlayerPrefs.SetInt("Optic Flow Seed", (int)seed);
        lifeSpan = PlayerPrefs.GetFloat("Life Span");
        dist = PlayerPrefs.GetFloat("Draw Distance");
        densityLow = PlayerPrefs.GetFloat("Density Low");
        densityHigh = PlayerPrefs.GetFloat("Density High");
        densityLowRatio = PlayerPrefs.GetFloat("Density Low Ratio");
        t_height = PlayerPrefs.GetFloat("Triangle Height");
        ObsDensityRatio = PlayerPrefs.GetFloat("ObsDensityRatio");
        isObsNoise = PlayerPrefs.GetInt("isObsNoise") == 1;
        particleSystem = GetComponent<ParticleSystem>();

        particleSystem.Stop();

        if (particleSystem.isStopped) particleSystem.randomSeed = seed;

        particleSystem.Play();

        var main = particleSystem.main;
        var emission = particleSystem.emission;
        var shape = particleSystem.shape;

        main.startLifetime = lifeSpan;
        main.startSize = t_height * baseH;

        var n = Random.Range(0f, 1f);

        if (n < densityLowRatio)
        {
            density = densityLow;
        }
        else
        {
            density = densityHigh;
        }

        if (isObsNoise)
        {
            if (name == "Dots")
            {
                density *= 1 - ObsDensityRatio;
                densityHigh *= 1 - ObsDensityRatio;
                densityLow *= 1 - ObsDensityRatio;
            }
            else
            {
                density *= ObsDensityRatio;
                densityHigh *= ObsDensityRatio;
                densityLow *= ObsDensityRatio;
            }
        }

        main.maxParticles = Mathf.RoundToInt(Mathf.Pow(dist, 2.0f) * Mathf.PI * density / t_height);

        var densityInCM = Mathf.FloorToInt(main.maxParticles / 10000.0f) < 1f ? main.maxParticles / 10000.0f : Mathf.FloorToInt(main.maxParticles / 10000.0f);
        emission.rateOverTime = Mathf.Floor(densityInCM / lifeSpan * 10000.0f / (t_height));

        shape.randomPositionAmount = dist;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SwitchDensity2()
    {
        var main = particleSystem.main;
        var emission = particleSystem.emission;

        if (density == densityHigh)
        {
            //print("low");
            density = densityLow;
        }
        else
        {
            //print("high");
            density = densityHigh;
        }

        print(density);
        main.maxParticles = Mathf.RoundToInt(Mathf.Pow(dist, 2.0f) * Mathf.PI * density / t_height);
        var densityInCM = Mathf.FloorToInt(main.maxParticles / 10000.0f) < 1f ? main.maxParticles / 10000.0f : Mathf.FloorToInt(main.maxParticles / 10000.0f);
        emission.rateOverTime = Mathf.Floor(densityInCM / lifeSpan * 10000.0f / (t_height));
        particleSystem.Clear();
    }

    public void SetDensity(float density)
    {
        var main = particleSystem.main;
        var emission = particleSystem.emission;
        main.maxParticles = Mathf.RoundToInt(Mathf.Pow(dist, 2.0f) * Mathf.PI * density / t_height);
        var densityInCM = Mathf.FloorToInt(main.maxParticles / 10000.0f) < 1f ? main.maxParticles / 10000.0f : Mathf.FloorToInt(main.maxParticles / 10000.0f);
        emission.rateOverTime = Mathf.Floor(densityInCM / lifeSpan * 10000.0f / (t_height));
        particleSystem.Clear();
    }
}
