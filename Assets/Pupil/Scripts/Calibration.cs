using System;
using System.Collections.Generic;
using UnityEngine;
using NetMQ.Sockets;
using NetMQ;
using MessagePack;
using System.Text;
using System.Linq;
using System.IO;

namespace PupilLabs
{
    public class Calibration
    {
        //events
        public event Action OnCalibrationStarted;
        public event Action OnCalibrationSucceeded;
        public event Action OnCalibrationFailed;

        //members
        SubscriptionsController subsCtrl;
        RequestController requestCtrl;
        Publisher publisher;
        CalibrationSettings settings;

        List<Dictionary<string, object>> calibrationData = new List<Dictionary<string, object>>();
        float[] rightEyeTranslation;
        float[] leftEyeTranslation;

        public bool IsCalibrating { get; set; }

        public void StartCalibration(CalibrationSettings settings, SubscriptionsController subsCtrl)
        {
            this.settings = settings;
            this.subsCtrl = subsCtrl;
            this.requestCtrl = subsCtrl.requestCtrl;

            if (OnCalibrationStarted != null)
            {
                OnCalibrationStarted();
            }

            IsCalibrating = true;

            subsCtrl.SubscribeTo("notify.calibration.successful", ReceiveSuccess);
            subsCtrl.SubscribeTo("notify.calibration.failed", ReceiveFailure);
            subsCtrl.SubscribeTo("notify.calibration.", ReceiveCalibrationData);
            //subsCtrl.SubscribeTo("logs.", ReceiveCalibrationData);

            requestCtrl.StartPlugin(settings.PluginName);
            publisher = new Publisher(requestCtrl);

            UpdateEyesTranslation();

            requestCtrl.Send(new Dictionary<string, object> {
                { "subject","calibration.should_start" },
                {
                    "translation_eye0",
                    rightEyeTranslation
                },
                {
                    "translation_eye1",
                    leftEyeTranslation
                },
                {
                    "record",
                    true
                }
            });

            Debug.Log("Calibration Started");

            calibrationData.Clear();
        }

        public void AddCalibrationPointReferencePosition(float[] position, double timestamp)
        {
            calibrationData.Add(new Dictionary<string, object>() {
                { settings.PositionKey, position },
                { "timestamp", timestamp },
            });
        }

        public void SendCalibrationReferenceData()
        {
            Debug.Log("Send CalibrationReferenceData");

            Send(new Dictionary<string, object> {
                { "subject","calibration.add_ref_data" },
                {
                    "ref_data",
                    calibrationData.ToArray ()
                },
                {
                    "record",
                    true
                }
            });

            //Clear the current calibration data, so we can proceed to the next point if there is any.
            calibrationData.Clear();
        }

        public void StopCalibration()
        {
            Debug.Log("Calibration should stop");

            IsCalibrating = false;

            Send(new Dictionary<string, object> {
                {
                    "subject",
                    "calibration.should_stop"
                },
                {
                    "record",
                    true
                }
            });
        }

        public void Destroy()
        {
            if (publisher != null)
            {
                publisher.Destroy();
            }
        }

        private void Send(Dictionary<string, object> data)
        {
            string topic = "notify." + data["subject"];
            publisher.Send(topic, data);
        }

        void ReceiveCalibrationData(string topic, Dictionary<string, object> dictionary, byte[] thirdFrame = null)
        {
            foreach (KeyValuePair<string, object> entry in dictionary)
            {
                if (entry.Key == "calib_data")
                {
                    Dictionary<object, object> dict = (Dictionary<object, object>)dictionary["calib_data"];
                    object[] refList = (object[])dict["ref_list"];
                    object[] pupilList = (object[])dict["pupil_list"];

                    foreach (object refEntry in refList)
                    {
                        Debug.Log(refEntry.ToString());
                    }

                    foreach (object pupilEntry in pupilList)
                    {
                        Debug.Log(pupilEntry.ToString());
                    }
                }

                if (entry.Key == "params")
                {
                    Dictionary<object, object> secDict = (Dictionary<object, object>)dictionary["params"];

                    Dictionary<object, object> leftDict = (Dictionary<object, object>)secDict["left_model"];
                    Dictionary<object, object> rightDict = (Dictionary<object, object>)secDict["right_model"];
                    Dictionary<object, object> biDict = (Dictionary<object, object>)secDict["binocular_model"];

                    object[] leftMatrixDict = (object[])leftDict["eye_camera_to_world_matrix"];

                    object gaze_distance_L = (object)leftDict["gaze_distance"];

                    Debug.Log(gaze_distance_L.ToString());

                    if (File.Exists("C:\\Users\\Lab\\Desktop\\LeftMatrix.txt"))
                    {
                        File.Delete("C:\\Users\\Lab\\Desktop\\LeftMatrix.txt");
                    }

                    if (File.Exists("C:\\Users\\Lab\\Desktop\\RightMatrix.txt"))
                    {
                        File.Delete("C:\\Users\\Lab\\Desktop\\RightMatrix.txt");
                    }

                    if (File.Exists("C:\\Users\\Lab\\Desktop\\BiMatrix0.txt"))
                    {
                        File.Delete("C:\\Users\\Lab\\Desktop\\BiMatrix0.txt");
                    }

                    if (File.Exists("C:\\Users\\Lab\\Desktop\\BiMatrix1.txt"))
                    {
                        File.Delete("C:\\Users\\Lab\\Desktop\\BiMatrix1.txt");
                    }

                    StringBuilder sb = new StringBuilder();

                    foreach (object leftMatrixEntry in leftMatrixDict)
                    {
                        object[] leftMatrixList = (object[])leftMatrixEntry;

                        //Debug.Log(leftMatrixList.Length);

                        foreach (object leftListEntry in leftMatrixList)
                        {
                            //Debug.Log(leftListEntry.ToString());
                            sb.AppendLine(leftListEntry.ToString());
                        }
                        //Debug.Log(leftMatrixEntry.ToString());
                    }

                    File.AppendAllText("C:\\Users\\Lab\\Desktop\\LeftMatrix.txt", sb.ToString());

                    sb.Clear();

                    object[] rightMatrixDict = (object[])rightDict["eye_camera_to_world_matrix"];

                    foreach (object rightMatrixEntry in rightMatrixDict)
                    {
                        object[] rightMatrixList = (object[])rightMatrixEntry;

                        foreach (object rightListEntry in rightMatrixList)
                        {
                            //Debug.Log(rightListEntry.ToString());
                            sb.AppendLine(rightListEntry.ToString());
                        }
                    }

                    File.AppendAllText("C:\\Users\\Lab\\Desktop\\RightMatrix.txt", sb.ToString());

                    sb.Clear();

                    object[] biLeftMatrixDict = (object[])biDict["eye_camera_to_world_matrix0"];
                    object[] biRightMatrixDict = (object[])biDict["eye_camera_to_world_matrix1"];

                    foreach (object biLeftMatrixEntry in biLeftMatrixDict)
                    {
                        object[] biLeftMatrixList = (object[])biLeftMatrixEntry;

                        foreach (object biLeftListEntry in biLeftMatrixList)
                        {
                            //Debug.Log(biLeftListEntry.ToString());
                            sb.AppendLine(biLeftListEntry.ToString());
                        }
                    }

                    File.AppendAllText("C:\\Users\\Lab\\Desktop\\BiMatrix0.txt", sb.ToString());

                    sb.Clear();

                    foreach (object biRightMatrixEntry in biRightMatrixDict)
                    {
                        object[] biRightMatrixList = (object[])biRightMatrixEntry;

                        foreach (object biRightListEntry in biRightMatrixList)
                        {
                            //Debug.Log(biRightListEntry.ToString());
                            sb.AppendLine(biRightListEntry.ToString());
                        }
                    }

                    File.AppendAllText("C:\\Users\\Lab\\Desktop\\BiMatrix1.txt", sb.ToString());

                    sb.Clear();
                }
            }
        }

        private void UpdateEyesTranslation()
        {
            Vector3 leftEye = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.LeftEye);
            Vector3 rightEye = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.RightEye);
            Vector3 centerEye = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye);
            Quaternion centerRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.CenterEye);

            //convert local coords into center eye coordinates
            Vector3 globalCenterPos = Quaternion.Inverse(centerRotation) * centerEye;
            Vector3 globalLeftEyePos = Quaternion.Inverse(centerRotation) * leftEye;
            Vector3 globalRightEyePos = Quaternion.Inverse(centerRotation) * rightEye;

            //right
            var relativeRightEyePosition = globalRightEyePos - globalCenterPos;
            relativeRightEyePosition *= Helpers.PupilUnitScalingFactor;
            MonoBehaviour.print(relativeRightEyePosition.x);
            rightEyeTranslation = new float[] { relativeRightEyePosition.x - 13.1f, relativeRightEyePosition.y, relativeRightEyePosition.z };

            //left
            var relativeLeftEyePosition = globalLeftEyePos - globalCenterPos;
            relativeLeftEyePosition *= Helpers.PupilUnitScalingFactor;
            MonoBehaviour.print(relativeLeftEyePosition.x);
            leftEyeTranslation = new float[] { relativeLeftEyePosition.x + 13.1f, relativeLeftEyePosition.y, relativeLeftEyePosition.z };
        }

        private void ReceiveSuccess(string topic, Dictionary<string, object> dictionary, byte[] thirdFrame)
        {
            if (OnCalibrationSucceeded != null)
            {
                OnCalibrationSucceeded();
            }

            CalibrationEnded(topic, dictionary);
        }

        private void ReceiveFailure(string topic, Dictionary<string, object> dictionary, byte[] thirdFrame)
        {
            if (OnCalibrationFailed != null)
            {
                OnCalibrationFailed();
            }

            CalibrationEnded(topic, dictionary);
        }

        private void CalibrationEnded(string topic, Dictionary<string, object> dictionary)
        {
            Debug.Log($"Calibration response: {topic}");

            foreach (KeyValuePair<string, object> entry in dictionary)
            {
                Debug.Log(entry.ToString());
            }

            subsCtrl.UnsubscribeFrom("notify.calibration.successful", ReceiveSuccess);
            subsCtrl.UnsubscribeFrom("notify.calibration.failed", ReceiveFailure);
            subsCtrl.UnsubscribeFrom("notify.calibration.", ReceiveCalibrationData);
        }
    }
}