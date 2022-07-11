using UnityEngine;
using static JoystickMonke;

public class Particles : MonoBehaviour
{
    [HideInInspector] public float lifeSpan;
    [HideInInspector] public float dist;
    [HideInInspector] public float density;
    [HideInInspector] public float t_height;
    readonly float baseH = 0.0185f;
    uint seed;
    [HideInInspector]
    public bool changedensityflag = false;
    [HideInInspector] public static Particles particles;
    ParticleSystem particleSystem;
    public int counter = 0;

    // Start is called before the first frame update
    void OnEnable()
    {
        particles = this;

        seed = (uint)UnityEngine.Random.Range(1, 10000);
        PlayerPrefs.SetInt("Optic Flow Seed", (int)seed);
        lifeSpan = PlayerPrefs.GetFloat("lifeSpan");
        dist = PlayerPrefs.GetFloat("Draw Distance");
        density = PlayerPrefs.GetFloat("Density");
        t_height = PlayerPrefs.GetFloat("tHeight");
        particleSystem = GetComponent<ParticleSystem>();

        particleSystem.Stop();

        if (particleSystem.isStopped) particleSystem.randomSeed = seed;

        particleSystem.Play();

        var main = particleSystem.main;
        var emission = particleSystem.emission;
        var shape = particleSystem.shape;

        main.startLifetime = lifeSpan;
        main.startSize = t_height * baseH;

        main.maxParticles = Mathf.RoundToInt(Mathf.Pow(dist, 2.0f) * Mathf.PI * density / t_height);

        var densityInCM = Mathf.FloorToInt(main.maxParticles / 10000.0f) < 1f ? main.maxParticles / 10000.0f : Mathf.FloorToInt(main.maxParticles / 10000.0f);
        emission.rateOverTime = Mathf.Floor(densityInCM / lifeSpan * 10000.0f / (t_height));

        shape.randomPositionAmount = dist;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
