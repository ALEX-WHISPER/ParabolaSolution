using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class LocateObstaclePointControl : MonoBehaviour {

	public float hitDistance = 1f;
	public GameObject hitVisualPrefab;
	public float projectorHeight = 10f;
	public event Action<Vector3> OnFindObstaclePoint;

	private GameObject intersectedHitPoint;
	private LineRenderer lr;
	private PlayerControl_EmitParabola playerControl;

	void OnEnable() {
		Arrangement ();
	}

	void Start() {
		intersectedHitPoint = Instantiate(hitVisualPrefab) as GameObject;
		intersectedHitPoint.GetComponent<Collider> ().isTrigger = true;

		intersectedHitPoint.SetActive (false);

		playerControl.TargetChangedEvent += FindObstaclePoint;
	}

	private void Arrangement() {
		lr = GetComponent<LineRenderer> ();
		playerControl = GetComponentInParent<PlayerControl_EmitParabola> ();
	}

	//	For every point in LineRenderer, emit a ray to the next point, check whether the hitInfo's tag equals to "Ground".
	private void FindObstaclePoint() {
		Stopwatch sw = new Stopwatch ();
		int iteration = 0;

		for (int i = 0; i < lr.positionCount - 1; i++) {
			RaycastHit hitInfo;
			Vector3 startPos = transform.TransformPoint (lr.GetPosition(i));
			Vector3 hitDir = (transform.TransformPoint (lr.GetPosition(i + 1)) - transform.TransformPoint (lr.GetPosition(i))).normalized;

			if (Physics.Raycast (startPos, hitDir, out hitInfo, hitDistance)) {
				if (hitInfo.transform.tag.ToString().Equals("Ground")) {
					iteration = i;
					//UnityEngine.Debug.Log ("FindObstacle: " + hitInfo.point);
					if (OnFindObstaclePoint != null) {
						OnFindObstaclePoint (hitInfo.point);
					}
				}
				break;
			} else {
				continue;
			}
		}
	}
}
