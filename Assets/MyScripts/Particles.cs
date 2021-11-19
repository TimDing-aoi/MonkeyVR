using UnityEngine;

public class Particles : MonoBehaviour
{
    [HideInInspector] public float lifeSpan;
    [HideInInspector] public float dist;
    [HideInInspector] public float densityLow;
    [HideInInspector] public float densityHigh;
    [HideInInspector] public float densityLowRatio;
    float density;
    float prevDensity;
    [HideInInspector] public float t_height;
    readonly float baseH = 0.0185f;
    uint seed;
    [HideInInspector] public static Particles particles;
    ParticleSystem particleSystem;

    // Start is called before the first frame update
    void OnEnable()
    {
        particles = this;

        seed = (uint)UnityEngine.Random.Range(1, 10000);
        PlayerPrefs.SetInt("Optic Flow Seed", (int)seed);
        lifeSpan = PlayerPrefs.GetFloat("Life Span");
        dist = PlayerPrefs.GetFloat("Draw Distance");
        densityLow = PlayerPrefs.GetFloat("Density Low");
        densityHigh = PlayerPrefs.GetFloat("Density High");
        densityLowRatio = PlayerPrefs.GetFloat("Density Low Ratio");
        t_height = PlayerPrefs.GetFloat("Triangle Height");
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

        prevDensity = density;

        main.maxParticles = Mathf.RoundToInt(Mathf.Pow(dist, 2.0f) * Mathf.PI * density / t_height);

        var densityInCM = Mathf.FloorToInt(main.maxParticles / 10000.0f) < 1f ? main.maxParticles / 10000.0f : Mathf.FloorToInt(main.maxParticles / 10000.0f);
        emission.rateOverTime = Mathf.Floor(densityInCM / lifeSpan * 10000.0f / (t_height));

        shape.randomPositionAmount = dist;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float SwitchDensity()
    {
        var n = Random.Range(0f, 1f);

        var main = particleSystem.main;
        var emission = particleSystem.emission;

        if (n < densityLowRatio)
        {
            //print("low");
            density = densityLow;
        }
        else
        {
            //print("high");
            density = densityHigh;
        }

        if (density != prevDensity)
        {
            //print(density);
            main.maxParticles = Mathf.RoundToInt(Mathf.Pow(dist, 2.0f) * Mathf.PI * density / t_height);
            var densityInCM = Mathf.FloorToInt(main.maxParticles / 10000.0f) < 1f ? main.maxParticles / 10000.0f : Mathf.FloorToInt(main.maxParticles / 10000.0f);
            emission.rateOverTime = Mathf.Floor(densityInCM / lifeSpan * 10000.0f / (t_height));
            particleSystem.Clear();
        }

        prevDensity = density;

        return density;
    }
}
