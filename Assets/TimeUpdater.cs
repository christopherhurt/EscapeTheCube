using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeUpdater : MonoBehaviour {

	public static bool running = false;

	private static float currentTime = 0;

	public Text txt;

	private static Text txtCopy;

	void Start () {
		txtCopy = txt;
	}
	
	public static void ResetCounter () {
		currentTime = 0;
		running = false;
		txtCopy.text = "Time - 00:00";
	}

	void Update () {
		if (running) {
			currentTime += Time.deltaTime;

			int minutes = (int)currentTime / 60;
			int seconds = (int)currentTime % 60;

			string minutesZeros = minutes < 10 ? "0" : "";
			string secondsZeros = seconds < 10 ? "0" : "";

			txtCopy.text = "Time - " + minutesZeros + minutes + ":" + secondsZeros + seconds;
		}
	}
}
