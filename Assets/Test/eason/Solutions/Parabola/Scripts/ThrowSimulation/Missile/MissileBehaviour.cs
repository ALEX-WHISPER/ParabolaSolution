using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#region For Object Pool, please DO NOT remove it
//public class MissileBehaviour : PoolObjectBehaviour, IMissileControl {
#endregion

public class MissileBehaviour: MonoBehaviour {

	#region Public Variables
    public float fireDelay = 0f;		//	delay before missile's movement
	public float nextShotDelay = 1.5f;	//	Player is allowed to fire again after "nextShotDelay" seconds after missile's movement is over

	[Header ("Raycast for physics to the ground layer in horizontal axis")]
	public float hitRadius_H = 1f;	//	maxDistance for horizontal RaycastHit detection

	[Header ("Raycast for physics to the ground layer in vertical axis")]
	public float hitRadius_V = 1f;	//	maxDistance for vertical RaycastHit detection

	public event Action OnHitGroundEvent;		//	Event on missile hitting ground

	//	Layermasks
	public LayerMask layer_Target;
	public LayerMask layer_Ground;
	#endregion

	#region Private Variables
	private PlayerControl_EmitParabola playerControl;
	private float gravity;
	private float v0;
	private bool moveTrigger = false;	//	force to stop missile's movement and enable physics when it get hit by obstacle
	private bool physicsTrigger = false;	//	flag of physics
	private bool hitGroundCalled = false;
    private GameObject relativeObj; //  For missile's local rotation (look at next point) duing moving
	private GameObject m_BulletHolder;
    #endregion

    #region For Object Pool, please DO NOT remove it
    //    public override void OnObjectReuse() {
    //		moveTrigger = true;
    //		DisablePhysicsDetection ();
    //        StartCoroutine(SimulateProjectile());
    //    }
    #endregion

    #region UNITY LIFE CYCLE
    private void Awake() {
		Arrangement ();
    }

	private void Start() {
		StartCoroutine (SimulateProjectile());
	}

	private void FixedUpdate() {
		if (RaycastForHitGroundDetection() && !hitGroundCalled) {
			Debug.Log ("Hit Ground: Stop flying to enable physics");
			if (OnHitGroundEvent != null) {
				OnHitGroundEvent ();
			}
			hitGroundCalled = true;
		}
	}

	private void OnDisable() {
		EventsDeRegistration ();
	}
	#endregion

	#region Arrangement, for reference, varaibles initilization and events registration stuff
	private void Arrangement() {
		ReferenceSetting ();
		TransformSetting ();
		VariablesInitialization ();
		EventsRegistration ();
	}

	private void ReferenceSetting() {
		playerControl = GetComponentInParent<PlayerControl_EmitParabola>();
	}

	private void VariablesInitialization() {
		gravity = Mathf.Abs(Physics.gravity.y) * playerControl.gravity_MultiValue;
	}

	private void TransformSetting() {
		Vector3 multiValue = new Vector3 (1f / playerControl.transform.localScale.x, 1f / playerControl.transform.localScale.y, 1f / playerControl.transform.localScale.z);
		Vector3 newScaleVec = new Vector3 (transform.localScale.x * multiValue.x, transform.localScale.y * multiValue.y, transform.localScale.z * multiValue.z);
		transform.localScale = newScaleVec;

		if (GameObject.Find ("relativeObject") == null) {
			relativeObj = new GameObject ("relativeObject");
		} else {
			relativeObj = GameObject.Find ("relativeObject");
		}
    }

    private void EventsRegistration() {
		OnHitGroundEvent += MissileOnHitGroundCallBack;

//		if (GetComponent<MissileDestruction>() != null) {
//			GetComponent<MissileDestruction> ().missileDestroyGroundEvent += playerControl.CallTargetChangedEvent;
//		}
	}

	private void EventsDeRegistration() {
		OnHitGroundEvent -= MissileOnHitGroundCallBack;

//		if (GetComponent<MissileDestruction>() != null) {
//			GetComponent<MissileDestruction> ().missileDestroyGroundEvent -= playerControl.CallTargetChangedEvent;
//		}
	}
	#endregion

	#region Raycast for Hit GROUND and Hit OBSTACLE
	//	射线检测判断：导弹发出沿水平与垂直方向上的射线，若该射线与 "Ground" 或 "Water" 层相交，则说明已落地
	//	之所以采用射线检测判断与地面相交，是由于导弹在飞行过程中需始终保持 IK 开启，而 IK 开启时无法使用物理碰撞检测
	private bool RaycastForHitGroundDetection() {
		RaycastHit hitInfo_H;
		RaycastHit hitInfo_V;
		int layer = InteractableGroundTagList.GroundLayer | InteractableGroundTagList.TargetLayer;

		if (Physics.Raycast(transform.position, -1 * Vector3.up, out hitInfo_V, hitRadius_V, layer)) {
			moveTrigger = false;
			return true;
		}

		if (Physics.Raycast(transform.position, Vector3.forward, out hitInfo_H, hitRadius_H, layer)) {
			moveTrigger = false;
			return true;
		}

		return false;
	}
	#endregion

	#region Enable and Disable PhysicsHit
	private void EnablePhysicsDetection() {
		if (GetComponent<Rigidbody>() == null) {
			return;
		}

		GetComponent<Rigidbody> ().useGravity = true;
		GetComponent<Rigidbody> ().isKinematic = false;

		physicsTrigger = true;
		UnbindPlayerParent ();
		Destroy (gameObject, 0.2f);
	}

	private void DisablePhysicsDetection() {
		if (GetComponent<Rigidbody>() == null) {
			return;
		}

		GetComponent<Rigidbody> ().useGravity = false;
		GetComponent<Rigidbody> ().isKinematic = true;
		GetComponent<Rigidbody> ().detectCollisions = true;

		physicsTrigger = false;
	}

	//	Once the missile has hitted the ground and enabled physics, which means the parabolic flight has ended.
	//	It should be out of the control of its parent's transform
	//	导弹结束飞行并落地开启物理引擎后，应该脱离当前父物体(Player)的控制，以避免人物移动使得已落地导弹随之移动的现象
	private void UnbindPlayerParent() {
		m_BulletHolder = (GameObject.Find ("BulletHolder") == null) ? new GameObject ("BulletHolder") : GameObject.Find ("BulletHolder");
		this.transform.parent = m_BulletHolder.transform;
	}
	#endregion

	#region Missile Movement
    IEnumerator SimulateProjectile() {
		playerControl.Missile_Moving = true;
		moveTrigger = true;
		DisablePhysicsDetection ();

		// Short delay added before Projectile is thrown
		yield return new WaitForSeconds (fireDelay);

		float V0, Vx, Vy, flightDuration, maxDistance;
//		SimulateCalculation (out Vx, out Vy, out flightDuration);

		V0 = playerControl.Missile_V0;
		Vx = playerControl.Missile_Vx;
		Vy = playerControl.Missile_Vy;
		flightDuration = playerControl.CurFlightDuration;
		maxDistance = playerControl.Missile_HorizontalDistance;

        // Rotate proje	ctile to face the target.
		transform.rotation = Quaternion.LookRotation(playerControl.FireTarget - transform.position);
        relativeObj.transform.rotation = transform.rotation;
        relativeObj.transform.position = transform.position;

        float elapse_time = 0;
		playerControl.Missile_Moving = true;

        while (elapse_time < flightDuration) {
			if (!moveTrigger)
				break;
			
            Vector3 p = new Vector3(0, (Vy - (gravity * elapse_time)) * Time.deltaTime, Vx * Time.deltaTime);
            relativeObj.transform.Translate(p);
            transform.localRotation = Quaternion.LookRotation(relativeObj.transform.position - transform.position);
			transform.localEulerAngles = new Vector3 (transform.localEulerAngles.x, 0f, transform.localEulerAngles.z);	//	adjust the orientation of missile, to make it face to the target point straightforward

            transform.Translate(p, relativeObj.transform);
            elapse_time += Time.deltaTime;
            yield return null;
        }
		playerControl.Missile_Moving = false;

		EnablePhysicsDetection ();
    }
	#endregion

	#region CallBacks
	/// <summary>
	/// Call on missile hit ground
	/// </summary>
	private void MissileOnHitGroundCallBack() {
		moveTrigger = false;
		EnablePhysicsDetection ();
	}

	/// <summary>
	/// Call on missile hit obstacle
	/// </summary>
	private void MissileOnHitObstacleCallBack() {
		moveTrigger = false;
		EnablePhysicsDetection ();
	}
	#endregion

	#region Unused so far
	public void SimulateCalculation(out float Vx, out float Vy, out float flightDuration) {
		// Calculate distance to target
//		float target_Distance = Vector3.Distance(transform.position, playerControl.FireTarget);
		Vector3 targetPos = playerControl.FireTarget;
		Vector3 startPos = transform.position;
		Vector3 startPos_SamePlane = new Vector3 (startPos.x, targetPos.y, startPos.z);
		float target_Distance = Vector3.Distance (startPos_SamePlane, targetPos);

		Debug.Log ("missile.maxDistance: " + target_Distance);

		// Calculate the velocity needed to throw the object to the target at specified angle.
		v0 = target_Distance / (Mathf.Sin(2 * playerControl.GetFiringAngle * Mathf.Deg2Rad) / gravity);
		Debug.Log ("missile.V0: " + Mathf.Sqrt(v0));

		// Extract the X  Y componenent of the velocity
		Vx = Mathf.Sqrt(v0) * Mathf.Cos(playerControl.GetFiringAngle * Mathf.Deg2Rad);
		Vy = Mathf.Sqrt(v0) * Mathf.Sin(playerControl.GetFiringAngle * Mathf.Deg2Rad);

		// Calculate flight time.
		flightDuration = target_Distance / Vx;
	}
	#endregion
}
