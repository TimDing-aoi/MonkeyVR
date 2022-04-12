using System.Collections;
using UnityEngine;
using PupilLabs;
using System.Text;
using System.IO.Ports;
using UnityEngine.InputSystem;

public class MonkeGaze : MonoBehaviour
{
    public Camera Lcam;
    public Camera Rcam;

    //public Text statusText;
    public GazeVisualizer gazeVisualizer;

    private StringBuilder sb;

    private Matrix4x4 lm;
    private Matrix4x4 rm;

    // left matrix first row
    float lm00;
    float lm01;
    float lm02;
    float lm03;

    // left matrix second row
    float lm10;
    float lm11;
    float lm12;
    float lm13;

    // left matrix third row
    float lm20;
    float lm21;
    float lm22;
    float lm23;

    // left matrix fourth row
    float lm30;
    float lm31;
    float lm32;
    float lm33;

    // right matrix first row
    float rm00;
    float rm01;
    float rm02;
    float rm03;

    // right matrix second row
    float rm10;
    float rm11;
    float rm12;
    float rm13;

    // right matrix third row
    float rm20;
    float rm21;
    float rm22;
    float rm23;

    // right matrix fourth row
    float rm30;
    float rm31;
    float rm32;
    float rm33;

    [Range(-1, 1)]
    public float m00Offset = 0.0f;

    [Range(-1, 1)]
    public float m01Offset = 0.0f;

    [Range (-1, 1)]
    public float m02Offset = -0.317f;

    [Range(-1, 1)]
    public float m03Offset = 0.0f;

    [Range(-1, 1)]
    public float m10Offset = 0.0f;

    [Range(-1, 1)]
    public float m11Offset = 0.0f;

    [Range(-1, 1)]
    public float m12Offset = 0.0f;

    [Range(-1, 1)]
    public float m13Offset = 0.0f;

    [Range(-1, 1)]
    public float m20Offset = 0.0f;

    [Range(-1, 1)]
    public float m21Offset = 0.0f;

    [Range(-1, 1)]
    public float m22Offset = 0.0f;

    [Range(-1, 1)]
    public float m23Offset = 0.0f;

    [Range(-1, 1)]
    public float m30Offset = 0.0f;

    [Range(-1, 1)]
    public float m31Offset = 0.0f;

    [Range(-1, 1)]
    public float m32Offset = 0.0f;

    [Range(-1, 1)]
    public float m33Offset = 0.0f;

    [Header("SET THIS BEFORE PLAYING")]
    [Tooltip("Switch between custom (true) and Vive (false) matrix")]
    public bool toggleProjectionMatrix = true;

    [Tooltip("SerialPort of your device.")]
    [HideInInspector] public string portName = "COM4";

    [Tooltip("Baudrate")]
    [HideInInspector] public int baudRate = 2000000;

    [Tooltip("Timeout")]
    [HideInInspector] public int ReadTimeout = 5000;

    SerialPort juiceBox;
    private bool giveJuice = false;
    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.XR.InputTracking.disablePositionalTracking = true;
        UnityEngine.XR.XRDevice.DisableAutoXRCameraTracking(Lcam, true);
        UnityEngine.XR.XRDevice.DisableAutoXRCameraTracking(Rcam, true);

        if (toggleProjectionMatrix)
        {
            Lcam.ResetProjectionMatrix();
            Rcam.ResetProjectionMatrix();
        }
        lm = Lcam.projectionMatrix; //Lcam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
        lm00 = lm.m00;
        lm01 = lm.m01;
        lm02 = lm.m02;
        lm03 = lm.m03;
        lm10 = lm.m10;
        lm11 = lm.m11;
        lm12 = lm.m12;
        lm13 = lm.m12;
        lm20 = lm.m20;
        lm21 = lm.m21;
        lm22 = lm.m22;
        lm23 = lm.m23;
        lm30 = lm.m30;
        lm31 = lm.m31;
        lm32 = lm.m32;
        lm33 = lm.m33;

        rm = Rcam.projectionMatrix; //Rcam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
        rm00 = rm.m00;
        rm01 = rm.m01;
        rm02 = rm.m02;
        rm03 = rm.m03;
        rm10 = rm.m10;
        rm11 = rm.m11;
        rm12 = rm.m12;
        rm13 = rm.m12;
        rm20 = rm.m20;
        rm21 = rm.m21;
        rm22 = rm.m22;
        rm23 = rm.m23;
        rm30 = rm.m30;
        rm31 = rm.m31;
        rm32 = rm.m32;
        rm33 = rm.m33;

        lm.m02 = lm02 + m02Offset;
        Lcam.SetStereoProjectionMatrix(Camera.StereoscopicEye.Left, lm);
        Lcam.projectionMatrix = lm;

        rm.m02 = rm02 - m02Offset;
        Rcam.SetStereoProjectionMatrix(Camera.StereoscopicEye.Right, rm);
        Rcam.projectionMatrix = rm;
    }

    private void FixedUpdate()
    {
        lm.m00 = lm00 + m00Offset;
        lm.m01 = lm01 + m01Offset;
        lm.m02 = lm02 + m02Offset;
        lm.m03 = lm03 + m03Offset;
        lm.m10 = lm10 + m10Offset;
        lm.m11 = lm11 + m11Offset;
        lm.m12 = lm12 + m12Offset;
        lm.m13 = lm13 + m13Offset;
        lm.m20 = lm20 + m20Offset;
        lm.m21 = lm21 + m21Offset;
        lm.m22 = lm22 + m22Offset;
        lm.m23 = lm23 + m23Offset;
        lm.m30 = lm30 + m30Offset;
        lm.m31 = lm31 + m31Offset;
        lm.m32 = lm32 + m32Offset;
        lm.m33 = lm33 + m33Offset;
        Lcam.SetStereoProjectionMatrix(Camera.StereoscopicEye.Left, lm);

        rm.m00 = rm00 - m00Offset;
        rm.m01 = rm01 - m01Offset;
        rm.m02 = rm02 - m02Offset;
        rm.m03 = rm03 - m03Offset;
        rm.m10 = rm10 - m10Offset;
        rm.m11 = rm11 - m11Offset;
        rm.m12 = rm12 - m12Offset;
        rm.m13 = rm13 - m13Offset;
        rm.m20 = rm20 - m20Offset;
        rm.m21 = rm21 - m21Offset;
        rm.m22 = rm22 - m22Offset;
        rm.m23 = rm23 - m23Offset;
        rm.m30 = rm30 - m30Offset;
        rm.m31 = rm31 - m31Offset;
        rm.m32 = rm32 - m32Offset;
        Rcam.SetStereoProjectionMatrix(Camera.StereoscopicEye.Right, rm);
    }
}
