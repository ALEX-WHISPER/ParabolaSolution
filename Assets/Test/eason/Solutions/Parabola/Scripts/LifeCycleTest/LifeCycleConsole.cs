using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeCycleConsole : MonoBehaviour {

	void Awake() {
		Console ("Awake");
	}

	void Start() {
		Console ("Start");
	}

	void OnEnable() {
		Console ("Enable");
	}

	void OnDisable() {
		Console ("Disable");
	}

	void Console(string msg) {
		Debug.Log (msg);
	}
}
