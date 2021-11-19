using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fade : MonoBehaviour
{
    // Start is called before the first frame update
    private Renderer rend;
    private Color color;
    public GameObject player;

    void Start()
    {
        rend = gameObject.GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        color.a = 5f / Vector3.Distance(player.transform.position, gameObject.transform.position);
        rend.material.color = color;
    }
}
