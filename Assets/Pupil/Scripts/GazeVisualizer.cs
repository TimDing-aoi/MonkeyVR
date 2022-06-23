using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using static PupilLabs.DataController;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace PupilLabs
{
    public class GazeVisualizer : MonoBehaviour
    {
        public Transform gazeOrigin;
        public GazeController gazeController;
        public CalibrationController calibrationController;
        public TimeSync timeSync;

        StringBuilder sb = new StringBuilder();

        [Header("Settings")]
        [Range(0f, 1f)]
        public float confidenceThreshold = 0.0f;
        public bool binocularOnly = false;
        public bool simulateGaze;

        [Header("Projected Visualization")]
        public Transform projectionMarker;
        public Transform gazeDirectionMarker;
        [Range(0.01f, 0.1f)]
        public float sphereCastRadius = 0.05f;

        public Vector3 localGazeDirection;
        bool isGazing = false;

        bool errorAngleBasedMarkerRadius = true;
        float angleErrorEstimate = 2f;

        Vector3 origMarkerScale;
        MeshRenderer targetRenderer;
        float minAlpha = 0.2f;
        float maxAlpha = 0.8f;

        float lastConfidence;

        float timer = 0.0f;
        float GazeMarkerSize = 0.0f;

        System.Random random;

        void OnEnable()
        {
            random = new System.Random();

            bool allReferencesValid = true;
            if (projectionMarker == null)
            {
                UnityEngine.Debug.LogError("ProjectionMarker reference missing!");
                allReferencesValid = false;
            }
            if (gazeDirectionMarker == null)
            {
                UnityEngine.Debug.LogError("GazeDirectionMarker reference missing!");
                allReferencesValid = false;
            }
            if (gazeOrigin == null)
            {
                UnityEngine.Debug.LogError("GazeOrigin reference missing!");
                allReferencesValid = false;
            }
            if (gazeController == null)
            {
                UnityEngine.Debug.LogError("GazeController reference missing!");
                allReferencesValid = false;
            }
            if (!allReferencesValid)
            {
                UnityEngine.Debug.LogError("GazeVisualizer is missing required references to other components. Please connect the references, or the component won't work correctly.");
                enabled = false;
                return;
            }

            gazeDirectionMarker.localScale = Vector3.one * 0.01f;

            origMarkerScale = gazeDirectionMarker.localScale;
            targetRenderer = gazeDirectionMarker.GetComponent<MeshRenderer>();

            projectionMarker.localScale = Vector3.one * 0.025f;
#if UNITY_EDITOR
            EditorSceneManager.activeSceneChanged += ChangedActiveScene;
#else
            SceneManager.activeSceneChanged += ChangedActiveScene;       
#endif

            StartVisualizing();
        }

        void OnDisable()
        {
            if (gazeDirectionMarker != null)
            {
                gazeDirectionMarker.localScale = origMarkerScale;
            }

            sb.Clear();

            StopVisualizing();
        }

        void Update()
        {
            projectionMarker.localScale = Vector3.one * GazeMarkerSize * calibrationController.scale;

            if (!isGazing)
            {
                return;
            }

            localGazeDirection = dataController.gazeDataNow.GazeDirection;
            lastConfidence = dataController.gazeDataNow.Confidence;

            VisualizeConfidence();

            ShowProjected();
        }

        public void StartVisualizing()
        {
            if (!enabled)
            {
                UnityEngine.Debug.LogWarning("Component not enabled.");
                return;
            }

            if (isGazing)
            {
                UnityEngine.Debug.Log("Already gazing!");
                return;
            }

            UnityEngine.Debug.Log("Start Visualizing Gaze");

            //gazeController.OnReceive3dGaze += ReceiveGaze;

            projectionMarker.gameObject.SetActive(true);
            //gazeDirectionMarker.gameObject.SetActive(true);
            isGazing = true;
        }

        void ChangedActiveScene(Scene current, Scene next)
        {
            if (next.name == "Monkey2D")
            {
                gazeOrigin = Camera.main.transform;
                GazeMarkerSize = 0.25f;

                timeSync.UpdateTimeSync();
            }
            else if (next.name == "MainMenu")
            {
                StopVisualizing();
            }
            else if (next.name == "MonkeyGaze")
            {
                gazeOrigin = Camera.main.transform;
                GazeMarkerSize = 0.025f;

                if (!isGazing)
                {
                    StartVisualizing();
                }

                timeSync.UpdateTimeSync();
            }
        }

        public void StopVisualizing()
        {
            if (!isGazing || !enabled)
            {
                UnityEngine.Debug.Log("Nothing to stop.");
                return;
            }

            if (projectionMarker != null)
            {
                projectionMarker.gameObject.SetActive(false);
            }
            if (gazeDirectionMarker != null)
            {
                gazeDirectionMarker.gameObject.SetActive(false);
            }

            isGazing = false;

            //gazeController.OnReceive3dGaze -= ReceiveGaze;
        }

        //void ReceiveGaze(GazeData gazeData)
        //{
        //    if (binocularOnly && gazeData.MappingContext != GazeData.GazeMappingContext.Binocular)
        //    {
        //        return;
        //    }

        //    lastConfidence = gazeData.Confidence;

        //    localGazeDirection = gazeData.GazeDirection;
        //}

        void VisualizeConfidence()
        {
            if (targetRenderer != null)
            {
                Color c = targetRenderer.material.color;
                c.a = MapConfidence(lastConfidence);
                targetRenderer.material.color = c;
            }
        }

        void ShowProjected()
        {
            gazeDirectionMarker.localScale = origMarkerScale;

            Vector3 origin = gazeOrigin.position;
            Vector3 direction = gazeOrigin.TransformDirection(localGazeDirection);

            if (Physics.SphereCast(origin, sphereCastRadius, direction, out RaycastHit hit, Mathf.Infinity))
            {
                float xScale = dataController.xScale;
                float yScale = dataController.yScale;
                float xOffset = dataController.xOffset;
                float yOffset = dataController.yOffset;

                if (simulateGaze)
                {

                    if (Time.time - timer > 0.0f)
                    {
                        //localGazeDirection = calibrationController.pos + new Vector3((((float)random.NextDouble() - 0.5f) / 10f) * xScale + xOffset, (((float)random.NextDouble() - 0.5f) / 10f) * yScale + yOffset, -0.001f);
                        projectionMarker.position = calibrationController.pos + new Vector3(calibrationController.xThreshold * xScale + xOffset, calibrationController.yThreshold * yScale + yOffset, -0.001f);

                        timer = Time.time;
                    }
                }
                else
                {
                    // hit -> hit point of vector from enter of eyes to ground plane
                    UnityEngine.Debug.DrawRay(origin, direction * hit.distance, Color.yellow);

                    projectionMarker.position = new Vector3(hit.point.x * xScale + xOffset, hit.point.y * yScale + yOffset, hit.point.z);

                    gazeDirectionMarker.position = origin + direction * hit.distance;
                    gazeDirectionMarker.LookAt(origin);

                    if (errorAngleBasedMarkerRadius)
                    {
                        gazeDirectionMarker.localScale = GetErrorAngleBasedScale(origMarkerScale, hit.distance, angleErrorEstimate);
                    }
                }
            }
            else
            {
                UnityEngine.Debug.DrawRay(origin, direction * 10, Color.white);
            }
        }

        Vector3 GetErrorAngleBasedScale(Vector3 origScale, float distance, float errorAngle)
        {
            Vector3 scale = origScale;
            float scaleXY = distance * Mathf.Tan(Mathf.Deg2Rad * angleErrorEstimate) * 2;
            scale.x = scaleXY;
            scale.y = scaleXY;
            return scale;
        }

        float MapConfidence(float confidence)
        {
            return Mathf.Lerp(minAlpha, maxAlpha, confidence);
        }
    }
}