using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PupilLabs
{
    [RequireComponent(typeof(CalibrationController))]
    public class CalibrationStatusText : MonoBehaviour
    {
        public SubscriptionsController subsCtrl;
        public Text statusText;

        private CalibrationController calibrationController;

        void Awake()
        {
            SetStatusText("Not connected");
            calibrationController = GetComponent<CalibrationController>();
        }

        void OnEnable()
        {
            subsCtrl.requestCtrl.OnConnected += OnConnected;
            calibrationController.OnCalibrationStarted += OnCalibrationStarted;
            calibrationController.OnCalibrationRoutineDone += OnCalibrationRoutineDone;
            calibrationController.OnCalibrationSucceeded += CalibrationSucceeded;
            calibrationController.OnCalibrationFailed += CalibrationFailed;
            calibrationController.OnFuseTestStarted += FuseTestStarted;
            calibrationController.OnFuseTestComplete += FuseTestComplete;
            calibrationController.OnMicroSimuStarted += MicroSimuStarted;
            calibrationController.OnMicroSimuComplete += MicroSimuComplete;
        }

        void OnDisable()
        {
            subsCtrl.requestCtrl.OnConnected -= OnConnected;
            calibrationController.OnCalibrationStarted -= OnCalibrationStarted;
            calibrationController.OnCalibrationRoutineDone -= OnCalibrationRoutineDone;
            calibrationController.OnCalibrationSucceeded -= CalibrationSucceeded;
            calibrationController.OnCalibrationFailed -= CalibrationFailed;
            calibrationController.OnFuseTestStarted -= FuseTestStarted;
            calibrationController.OnFuseTestComplete -= FuseTestComplete;
            calibrationController.OnMicroSimuStarted -= MicroSimuStarted;
            calibrationController.OnMicroSimuComplete -= MicroSimuComplete;
        }

        private void OnConnected()
        {
            string text = "Connected";
            text += "\n\nPlease warm up your eyes and press 'C' to start the calibration or 'P' to preview the calibration targets.\n\n" +
                "You may also press 'N' to start the Firefly Task (only if calibration has already been done).\n" +
                "Or, you may press 'F' to start the Fusing Test, or M for the micro simulation.";
            SetStatusText(text);
        }

        private void OnCalibrationStarted()
        {
            statusText.enabled = false;
        }

        private void OnCalibrationRoutineDone()
        {
            statusText.enabled = true;
            SetStatusText("Calibration routine is done. Waiting for results ...");
        }

        private void CalibrationSucceeded()
        {
            statusText.enabled = true;
            SetStatusText("Calibration succeeded.");

            StartCoroutine(ChangeTextAfter(1.0f, "Press 'N' to start the Firefly Task.\nPress 'F' to start the Fusing Test."));
        }

        private void CalibrationFailed()
        {
            statusText.enabled = true;
            SetStatusText("Calibration failed.");

            StartCoroutine(DisableTextAfter(1));
        }

        private void FuseTestStarted()
        {
            SetStatusText("Press 'Enter' to move on to next target when routine is finished.");
        }

        private void FuseTestComplete()
        {
            SetStatusText("Fuse Test complete.");

            StartCoroutine(ChangeTextAfter(1.0f, "Press 'N' to start the Firefly Task.\nPress 'F' to start the Fusing Test."));
        }

        private void MicroSimuStarted()
        {
            SetStatusText("Micro Simulation in progress.");
        }

        private void MicroSimuComplete()
        {
            SetStatusText("Micro Simulation complete.");

            StartCoroutine(ChangeTextAfter(1.0f, "Please warm up your eyes and press 'C' to start the calibration or 'P' to preview the calibration targets.\n\n" +
                "You may also press 'N' to start the Firefly Task (only if calibration has already been done).\n" +
                "Or, you may press 'F' to start the Fusing Test, or M for the micro simulation."));
        }

        private void SetStatusText(string text)
        {
            if (statusText != null)
            {
                statusText.text = text;
            }
        }

        IEnumerator DisableTextAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            statusText.enabled = false;
        }

        IEnumerator ChangeTextAfter(float delay, string text)
        {
            yield return new WaitForSeconds(delay);
            SetStatusText(text);
        }
    }
}
