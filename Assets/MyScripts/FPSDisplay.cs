using UnityEngine;
using System.Collections;
using static Monkey2D;
using static JoystickMonke;
using UnityEngine.InputSystem;
using PupilLabs;

public class FPSDisplay : MonoBehaviour
{
	public static FPSDisplay FPScounter;

	float deltaTime = 0.0f;
	public Transform target;
	public Camera cam;
	public float offset = 0.15f;
	public float fps;

	bool toggle = false;
	string toggleText = "Open";
	public GameObject OF2;

	public Texture texture;

	void Awake()
	{
		FPScounter = this;
	}

	void Update()
	{
		deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
		target.position = cam.transform.position + cam.transform.forward * offset;
		target.rotation = new Quaternion(0.0f, cam.transform.rotation.y, 0.0f, cam.transform.rotation.w);
		//print(Vector3.forward * offset);
	}

	void OnGUI()
	{
		var keyboard = Keyboard.current;

		int w = Screen.width, h = Screen.height;

		GUIStyle style = new GUIStyle();

		Rect rect = new Rect(0, 0, w, h * 2 / 50);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = h * 2 / 50;
		style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
		float msec = deltaTime * 1000.0f;
		fps = 1.0f / deltaTime;
		string text;
		text = string.Format("{0:0.0} ms ({1:0.} fps)\nGood Trials / Total Trials: {2}/{3}\n Phase: {4}", msec, fps, SharedMonkey.points, SharedMonkey.trialNum, SharedMonkey.GFFPhaseFlag);
		GUI.Label(rect, text, style);
		if (keyboard.spaceKey.isPressed)
		{
			GUI.contentColor = Color.red;
		}
		else
		{
			GUI.contentColor = Color.black;
		}
		GUI.Box(new Rect(0f, 120f, 50f, 50f), texture);
	}

	public float GetFPS()
	{
		return fps;
	}
}