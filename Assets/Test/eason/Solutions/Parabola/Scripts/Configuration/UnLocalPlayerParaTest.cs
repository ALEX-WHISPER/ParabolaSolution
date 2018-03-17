using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnLocalPlayerParaTest : MonoBehaviour {

	public Vector3 targetPosVec;

	void Update() {
		if (Input.GetKeyUp(KeyCode.R)) {
			CommandAimAtTarget ();
		}

		if (Input.GetKeyUp(KeyCode.Space)) {
			CommandFire ();
		}
	}

	private void CommandAimAtTarget() {
		if (GetComponent<ParabolaWeaponConfig>() == null) {
			return;
		}

		ParabolaData paraData = new ParabolaData (targetPosVec);
		GetComponent<ParabolaWeaponConfig> ().CmdAimAtTarget (paraData);
	}

	private void CommandFire() {
		if (GetComponent<ParabolaWeaponConfig>() == null) {
			return;
		}

		ParabolaData paraData = new ParabolaData (targetPosVec);
		GetComponent<ParabolaWeaponConfig> ().CmdNormalFire (paraData);
	}
}
