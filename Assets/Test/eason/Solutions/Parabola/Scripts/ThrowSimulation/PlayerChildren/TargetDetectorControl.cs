using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetDetectorControl : MonoBehaviour {

	public GameObject m_TargetPrefab;
	public float projectorHeight = 10f;
	public float targetHeightBase_Close = 5f;
	public float targetHeightBase_Far = 40f;
	public float targetMoveSpeed = 1f;
	public Vector3 initPosOffset;

	private PlayerControl_EmitParabola playerControl;
	private LocateObstaclePointControl obstaclePointControl;
	private ChargedBtnControl chargeControl;
	private List<string> groundTagList;
	private GameObject m_TargetHitPoint;
	private Vector3 initPos;

	private float xRot;
	private float xRotRef;
	private float xRotValue;
	private float tarZPos;

	#region UNITY LIFE CYCLE
	void OnEnable() {
		ReferenceSetting ();
		EventsRegistration ();
//		VariablesInit ();
	}

	void Start() {
		VariablesInit ();
	}

	void Update() {
//		if (obstaclePointControl == null) {
//			obstaclePointControl = playerControl.transform.GetComponentInChildren<LocateObstaclePointControl> ();
//			if (obstaclePointControl != null) {
//				obstaclePointControl.OnFindObstaclePoint += OnFindObstaclePointCallBack;
//			}
//		}
	}

	void OnDestroy() {
		EventsDeRegistration ();
	}
	#endregion

	#region Arrangement
	private void ReferenceSetting() {
		playerControl = GetComponentInParent<PlayerControl_EmitParabola>();
		chargeControl = FindObjectOfType<ChargedBtnControl> ();
    }

	private void VariablesInit() {
//		transform.localPosition = new Vector3 (transform.localPosition.x, transform.localPosition.y, targetHeightBase_Close);
//		transform.position = new Vector3 (transform.position.x, projectorHeight, transform.position.z);

		transform.position = playerControl.transform.position;
		transform.localEulerAngles = new Vector3 (90f, 0f, 0f);
		transform.Translate (new Vector3(0f, 10f, 0f), Space.Self);

		initPos = transform.localPosition;
		LocateTargetPoint ();
	}
	#endregion

	#region Events
	private void EventsRegistration() {
		playerControl.TargetChangedEvent += TargetDetectorMovementFollower;
		playerControl.TargetChangedEvent += LocateTargetPoint;
		playerControl.OnChargingProcess += TargetDetectorChargeMovement;

		if (chargeControl != null) {
			chargeControl.OnChargingProcess += TargetDetectorChargeMovement;
			chargeControl.OnReCharge += ResetTransform;
		}
	}

	private void EventsDeRegistration() {
		playerControl.TargetChangedEvent -= LocateTargetPoint;
		playerControl.TargetChangedEvent -= TargetDetectorMovementFollower;
		playerControl.OnChargingProcess -= TargetDetectorChargeMovement;

		if (chargeControl != null) {
			chargeControl.OnChargingProcess -= TargetDetectorChargeMovement;
			chargeControl.OnReCharge -= ResetTransform;
		}
	}
	#endregion

	#region Event Listeners
	/// <summary>
	/// Called on playerControl.TargetChangedEvent
	/// </summary>
	private void LocateTargetPoint() {
		RaycastHit hitInfo;
		int hitLayer = InteractableGroundTagList.TargetLayer;	//	Layer 

		if (Physics.Raycast (transform.position, -1 * Vector3.up, out hitInfo, 100f, hitLayer)) {
			Debug.DrawRay (transform.position, -1 * Vector3.up, Color.red);
			playerControl.FireTarget = hitInfo.point;
//			Debug.Log ("Target Founded: " + hitInfo.point);
		}
	}

	/// <summary>
	/// Called on playerControl.TargetChangedEvent, which is, when user is moving the front view(or shooting angle)
	/// </summary>
	private void TargetDetectorMovementFollower() {
		float curZPos = transform.localPosition.z;
		float targetHeightBase = playerControl.GetFrontViewInputValue.y > 0 ? targetHeightBase_Far : targetHeightBase_Close;

//		float xRot = playerControl.transform.localEulerAngles.x > 180 ? 360 - playerControl.transform.localEulerAngles.x : playerControl.transform.localEulerAngles.x;
		float tarZPos = targetHeightBase;

		transform.localPosition = new Vector3 (transform.localPosition.x, transform.localPosition.y, Mathf.Lerp (curZPos, tarZPos, targetMoveSpeed * Mathf.Abs(playerControl.GetFrontViewInputValue.y) * Time.deltaTime));
		transform.position = new Vector3 (transform.position.x, projectorHeight, transform.position.z);

		Vector3 newEuler = new Vector3(90f, transform.eulerAngles.y, 0f);
		transform.eulerAngles = newEuler;
	}

	private void TargetDetectorChargeMovement(float inputValue) {
//		if (inputValue < 0.2f) {
//			ResetTransform ();
//		}

		float tarZPos = targetHeightBase_Close + (targetHeightBase_Far - targetHeightBase_Close) * inputValue * 0.5f;

		transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, tarZPos);
		transform.position = new Vector3 (transform.position.x, projectorHeight, transform.position.z);

		Vector3 newEuler = new Vector3(90f, transform.eulerAngles.y, 0f);
		transform.eulerAngles = newEuler;
	}

	private void OnFindObstaclePointCallBack(float zPos) {
		transform.position = new Vector3 (transform.position.x, projectorHeight, zPos);
	}

	/// <summary>
	/// Called on ChargeBtnControl.ReCharge
	/// </summary>
	public void ResetTransform() {
//		transform.localPosition = initPos;
//		transform.eulerAngles = new Vector3(90f, transform.eulerAngles.y, transform.eulerAngles.z);

//		transform.position = playerControl.transform.position;
//		transform.localEulerAngles = new Vector3 (90f, 0f, 0f);
//		transform.Translate (new Vector3(0f, 100, 0f), Space.Self);
//		LocateTargetPoint ();

		Debug.Log ("ResetTransform: First or Re-Charge");
	}
	#endregion

	private float GetXRotValue(float inputValue) {
		xRot -= inputValue * 1f;
		xRot = Mathf.Clamp (xRot, playerControl.viewRangeXAxis_Min, playerControl.viewRangeXAxis_Max);
		xRotValue = Mathf.SmoothDamp (xRotValue, xRot, ref xRotRef, 0.05f);
		xRotValue = Mathf.Clamp (xRotValue, playerControl.viewRangeXAxis_Min, playerControl.viewRangeXAxis_Max);

		return xRotValue;
	}
}
