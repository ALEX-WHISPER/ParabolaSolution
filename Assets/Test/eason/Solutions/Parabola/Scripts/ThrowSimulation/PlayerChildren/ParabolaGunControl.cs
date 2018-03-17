using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ParabolaGunControl : MonoBehaviour {

	public GameObject m_MissileObject;
	public string shotSpawnPath = "rocket/FirePoint";

	public event Action EmitMissileMoveEvent;

	private PlayerControl_EmitParabola playerControl;
	private float gunRotOffsetAlongXAxis;
	private Transform shotSpawn;

	public Transform GetShotSpawn {
		get { return shotSpawn; }
	}

	#region Unity Life Cycle
	void Awake() {
		Arrangement ();
	}

	void OnDestroy() {
		EventsDeRegistration ();
	}
	#endregion

	#region Arrangement
	private void Arrangement() {
		ReferenceSetting ();
		VariablesInit ();
		EventsRegistration ();
		FindShotSpawn();
	}

	private void ReferenceSetting() {
		playerControl = GetComponentInParent<PlayerControl_EmitParabola>();
	}

	private void VariablesInit() {
		gunRotOffsetAlongXAxis = playerControl.gunRotOffsetAlongXAxis;
	}

	private void FindShotSpawn() {
		shotSpawn = transform;
	}
	#endregion

	#region Events
	private void EventsRegistration() {
		playerControl.EmitMissileCommandEvent += CreateMissile;
		playerControl.RecvToFireEvent += CreateMissile;
	}

	private void EventsDeRegistration() {
		playerControl.EmitMissileCommandEvent -= CreateMissile;
		playerControl.TargetChangedEvent -= GunRotationFollowByOffset;
	}
	#endregion

	#region Event Listeners
	public void CreateMissile() {
		Instantiate (m_MissileObject, shotSpawn.position, shotSpawn.rotation, this.transform);
	}

	private void GunRotationFollowByOffset() {
		float xRot_Gun = transform.eulerAngles.x;
		xRot_Gun = -1 * gunRotOffsetAlongXAxis;
		transform.localEulerAngles = new Vector3 (xRot_Gun, transform.localEulerAngles.y, transform.localEulerAngles.z);
	}
	#endregion
}
