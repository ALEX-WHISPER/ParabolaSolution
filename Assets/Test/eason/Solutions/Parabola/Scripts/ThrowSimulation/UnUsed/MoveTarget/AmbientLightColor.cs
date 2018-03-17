using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientLightColor : MonoBehaviour {

	void Start() {
		RenderSettings.ambientSkyColor = Color.red;
	}
}
