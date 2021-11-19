using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixationPoint : MonoBehaviour
{
    public GameObject FP;
    public GameObject player;
    public GameObject FF;
    private int mode = 0;
    private int eye = 0;
    private int nFF = 1;
    private float radius = 0.0f;
    private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        mode = PlayerPrefs.GetInt("FP Mode");
        eye = PlayerPrefs.GetInt("Eye Mode");
        radius = PlayerPrefs.GetFloat("Size");
        nFF = (int)PlayerPrefs.GetFloat("Number of Fireflies");

        FP = player.transform.Find("Fixation Point").gameObject;

        if (mode == 0 || nFF > 1)
        {
            FP.SetActive(false);
        }
        else
        {
            switch (eye)
            {
                case 0:
                    // Left Eye
                    cam = player.transform.Find("L [CameraRig]").Find("L Camera").gameObject.GetComponent<Camera>();
                    break;
                case 1:
                    // Right Eye
                    cam = player.transform.Find("R [CameraRig]").Find("R Camera").gameObject.GetComponent<Camera>();
                    break;
                case 2:
                    // Both Eyes
                    //UnityEngine.Debug.Log("both");
                    cam = player.transform.Find("[CameraRig]").Find("Camera").gameObject.GetComponent<Camera>();
                    //UnityEngine.Debug.Log(cam == null);
                    break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (mode == 2 && nFF < 2)
        {
            try
            {
                Vector3 screenPos = cam.WorldToScreenPoint(FF.transform.position);
                float x = Mathf.Clamp(((screenPos.y / cam.pixelHeight) - 0.5f) * 13.4f, -6.7f, 6.7f);
                //UnityEngine.Debug.Log(x);
                FP.transform.localPosition = new Vector3(0.0f, x, 10.0f);
                if ((FF.transform.position.z - player.transform.position.z) > 0.0f && (Vector3.Distance(FF.transform.position, player.transform.position) > radius))
                {
                    FP.GetComponent<SpriteRenderer>().enabled = true;
                }
                else
                {
                    FP.GetComponent<SpriteRenderer>().enabled = false;
                }
            }
            catch (UnityEngine.UnityException e)
            {
                UnityEngine.Debug.Log(e);
            }
        }
    }
}
