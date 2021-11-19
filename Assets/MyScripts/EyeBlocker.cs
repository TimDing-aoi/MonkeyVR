using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeBlocker : MonoBehaviour
{
    public Transform target;
    public Camera cam;
    public float offset = 0.15f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        target.position = cam.transform.position + cam.transform.forward * offset;
        target.rotation = new Quaternion(0.0f, cam.transform.rotation.y, 0.0f, cam.transform.rotation.w);
    }
}
