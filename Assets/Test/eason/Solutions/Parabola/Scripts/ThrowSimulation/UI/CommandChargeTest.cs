using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandChargeTest : MonoBehaviour {

	public Button btn_CmdCharge;
	public InputField if_InputChargeValue;

	private PlayerControl_EmitParabola chargeController;

	void Start() {
		StartCoroutine (Arrangement());
	}

	private IEnumerator Arrangement() {
		yield return new WaitForSeconds (0.5f);
		ReferenceSetting ();
		EventsRegistration ();
	}

	private void ReferenceSetting() {
		chargeController = FindObjectOfType<PlayerControl_EmitParabola> ();
	}

	private void EventsRegistration() {
		btn_CmdCharge.onClick.AddListener (CommandCharge);
	}

	private void CommandCharge() {
		float chargeTargetValue = float.Parse (if_InputChargeValue.text.ToString ().Trim());
	}
}
