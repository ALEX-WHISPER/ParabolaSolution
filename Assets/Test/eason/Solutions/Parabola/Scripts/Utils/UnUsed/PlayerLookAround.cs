using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLookAround : MonoBehaviour {

	public float lookSensitivity = 2f;
	public float smoothTimeValue = 0.2f;

    //  xRot range: (rot_MIN.x, rot_MAX.x) 
    //  yRot range: (rot_MIN.y, rot_MAX.y)
	public Vector2 rot_MAX;
	public Vector2 rot_MIN;

	private float xRot;
	private float yRot;
	private float xRotValue;
	private float yRotValue;
	private float xRotRef;
	private float yRotRef;
	private bool isGameOver = false;

	void Start() {
		#if !UNITY_EDITOR
		this.enabled = false;
		#endif

		this.HideCursor ();
	}

	void Update() {

		if (isGameOver) {
			return;
		}

		//	Get Mouse Axis
		xRot -= Input.GetAxis ("Mouse Y") * lookSensitivity;
		yRot += Input.GetAxis ("Mouse X") * lookSensitivity;

		//	Clamp
		xRot = Mathf.Clamp(xRot, rot_MIN.x, rot_MAX.x);
		yRot = Mathf.Clamp(yRot, rot_MIN.y, rot_MAX.y);

		//	Smooth
		xRotValue = Mathf.SmoothDamp(xRotValue, xRot, ref xRotRef, smoothTimeValue);
		yRotValue = Mathf.SmoothDamp(yRotValue, yRot, ref yRotRef, smoothTimeValue);

		//	Set EulerAngles
		transform.localEulerAngles = new Vector3 (xRotValue, yRotValue, 0f);
	}

	public void GameOver() {
		this.isGameOver = true;
		this.ShowCursor ();
	}

	private void HideCursor() {
		if (!Cursor.visible) {
			return;
		}
		Cursor.visible = false;
	}

	private void ShowCursor() {
		if (Cursor.visible) {
			return;
		}
		Cursor.visible = true;
	}
}
