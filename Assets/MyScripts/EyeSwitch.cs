using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeSwitch : MonoBehaviour
{
    public Camera Mcam;
    public Camera Lcam;
    public Camera Rcam;
    public GameObject Lmask;
    public GameObject Rmask;

    //private int seed;
    //private System.Random rand;

    private int mode;

    void Awake()
    {
        //seed = UnityEngine.Random.Range(1, 10000);
        //rand = new System.Random(seed);
        mode = PlayerPrefs.GetInt("Eye Mode");
        //UnityEngine.Debug.Log("Eyemode " + mode);
        switch (mode)
        {
            case 0:
                Lmask.SetActive(false);
                Rmask.SetActive(true);
                //Camera.main.stereoTargetEye = StereoTargetEyeMask.Left;
                //Rcam.gameObject.SetActive(false);
                break;
            case 1:
                Lmask.SetActive(true);
                Rmask.SetActive(false);
                //Camera.main.stereoTargetEye = StereoTargetEyeMask.Right;
                //Lcam.gameObject.SetActive(false);
                break;
            case 2:
                Lmask.SetActive(false);
                Rmask.SetActive(false);
                //Camera.main.stereoTargetEye = StereoTargetEyeMask.None;
                //Lcam.gameObject.SetActive(false);
                //Rcam.gameObject.SetActive(false);
                break;
        }
        Camera.main.stereoTargetEye = StereoTargetEyeMask.None;
        UnityEngine.XR.XRDevice.DisableAutoXRCameraTracking(Mcam, true);
        UnityEngine.XR.XRDevice.DisableAutoXRCameraTracking(Lcam, true);
        UnityEngine.XR.XRDevice.DisableAutoXRCameraTracking(Rcam, true);
        UnityEngine.XR.InputTracking.disablePositionalTracking = true;
        Mcam.transform.localPosition = new Vector3(0f, 0.0f, 0f);
        Mcam.transform.localRotation = Quaternion.identity;
        Lcam.transform.localPosition = new Vector3(0f, 0.0f, 0f);
        Lcam.transform.localRotation = Quaternion.identity;
        Rcam.transform.localPosition = new Vector3(0f, 0.0f, 0f);
        Rcam.transform.localRotation = Quaternion.identity;
    }

    // Start is called before the first frame update
    void Start()
    {
        //UnityEngine.Debug.Log(Camera.main.stereoTargetEye);
    }
    void FixedUpdate()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Mcam.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        Lcam.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        Rcam.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        //Lmask.transform.position = Lcam.transform.position + (Vector3.forward * 0.1f);
        //Lmask.transform.rotation = new Quaternion(0.0f, Lcam.transform.rotation.y, 0.0f, Lcam.transform.rotation.w);
        //Rmask.transform.position = Rcam.transform.position + (Vector3.forward * 0.1f);
        //Rmask.transform.rotation = new Quaternion(0.0f, Rcam.transform.rotation.y, 0.0f, Rcam.transform.rotation.w);
    }
    
}
