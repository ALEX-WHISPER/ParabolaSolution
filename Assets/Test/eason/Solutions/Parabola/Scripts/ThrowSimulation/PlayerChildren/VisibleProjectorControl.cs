using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibleProjectorControl : MonoBehaviour {

	public float projectHeight = 10f;
	private LocateObstaclePointControl locateTargetControl;

	void OnEnable() {
		ReferenceSetting ();
		EventsRegistration ();
	}

	void OnDisable() {
		EventsDeRegistration ();
	}

	private void ReferenceSetting() {
		locateTargetControl = transform.parent.GetComponentInChildren<LocateObstaclePointControl> ();
	}

	private void EventsRegistration() {
		locateTargetControl.OnFindObstaclePoint += OnFinfObstaclePointCallback;
	}

	private void EventsDeRegistration() {
		locateTargetControl.OnFindObstaclePoint -= OnFinfObstaclePointCallback;
	}

	private void OnFinfObstaclePointCallback(Vector3 hitPoint) {
		transform.position = new Vector3 (hitPoint.x, projectHeight, hitPoint.z);
		Vector3 newEuler = new Vector3(90f, transform.eulerAngles.y, 0f);
		transform.eulerAngles = newEuler;
	}
}
