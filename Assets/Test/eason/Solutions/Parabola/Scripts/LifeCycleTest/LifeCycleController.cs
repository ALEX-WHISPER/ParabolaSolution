using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeCycleController : MonoBehaviour {

//	public LifeCycleConsole consoleScript;
	public GameObject consoleObject;

	void Start() {
//		consoleScript.enabled = true;
		consoleObject.SetActive(true);
	}

	void Update() {
		if (Input.GetKeyUp(KeyCode.Space)) {
//			consoleScript.enabled = !consoleScript.enabled;
			consoleObject.SetActive(!consoleObject.activeSelf);
		}
	}
}
