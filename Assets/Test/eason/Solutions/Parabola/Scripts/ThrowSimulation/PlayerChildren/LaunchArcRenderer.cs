using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaunchArcRenderer : MonoBehaviour {
	public int elemCount = 10;
    public bool isHighestVisual = true;
    public GameObject highestVisualPrefab;
    private GameObject m_HightestPoint;

	private float v0;
	private float h0;
	private float angle;

    private LineRenderer lr;
    private float g;
    private float radianAngle;
	private PlayerControl_EmitParabola playerControl;

	#region Public Properties
	public float GetV0 {get { return v0;}}
	public float GetVx {get { return v0 * Mathf.Cos (playerControl.GetFiringAngle * Mathf.Deg2Rad);}}
	public float GetVy {get { return v0 * Mathf.Sin (playerControl.GetFiringAngle * Mathf.Deg2Rad);}}
	public float GetRadAngle {get { return radianAngle;}}

	//	Get distance between taget and shotSpawn along x-z plane
	public float GetTargetDistance {
		get {
			Vector3 targetPos = playerControl.FireTarget;
			Vector3 startPos = transform.position;
			Vector3 startPos_SamePlane = new Vector3 (startPos.x, targetPos.y, startPos.z);

			float target_Distance = Vector3.Distance (startPos_SamePlane, targetPos);
			return target_Distance;
		}
	}
	public float GetFlightDuration {get { return GetTargetDistance / GetVx;}}
    public Vector3 GetFlightHighestPoint {
        get {
            float t = Mathf.Abs(GetVy / g) / GetFlightDuration;
            Vector3 point_Local = CalculateArcPoint(t, GetTargetDistance);
            Vector3 point_WorldSpace = transform.TransformPoint(point_Local);
            return point_WorldSpace;
        }
    }
	#endregion

	#region Unity Life Cycle

	private void OnEnable() {
		Arrangement ();
		TransformSetting ();
	}

	private void Start() {
		transform.localRotation = Quaternion.identity;
	}

	private void OnDestroy() {
		EventsDeRegistration ();
	}
	#endregion

	#region Arrangement for reference and so on
	private void Arrangement() {
		ReferenceSetting ();
		EventsRegistration ();
	}

	private void ReferenceSetting() {
		playerControl = GetComponentInParent<PlayerControl_EmitParabola>();

		lr = GetComponent<LineRenderer>();
		g = Mathf.Abs(Physics.gravity.y) * playerControl.gravity_MultiValue;
	}


	//	The global scale of this object must always be Vector3.one, cause the LineRenderer component has disabled "Use World Space"
	private void TransformSetting() {
		transform.localScale = Vector3.one;
		Vector3 multiValue = new Vector3 (1f / playerControl.transform.localScale.x, 1f / playerControl.transform.localScale.y, 1f / playerControl.transform.localScale.z);
		Debug.Log ("player.name: " + playerControl.name);
		Debug.Log ("player.localScale: " + playerControl.transform.lossyScale);
		Debug.Log ("multiValue : " + multiValue);
		Vector3 newScaleVec = new Vector3 (transform.localScale.x * multiValue.x, transform.localScale.y * multiValue.y, transform.localScale.z * multiValue.z);
		transform.localScale = newScaleVec;
	}
	#endregion

	#region Events
	private void EventsRegistration() {
		playerControl.TargetChangedEvent += GenParabolaTrajectory;
	}

	private void EventsDeRegistration() {
		playerControl.TargetChangedEvent -= GenParabolaTrajectory;
	}
	#endregion

	#region Render Funcs
	private void GenParabolaTrajectory(float inputValue) {
		this.GenParabolaTrajectory ();
	}

	private void GenParabolaTrajectory() {
		PropertiesUpdate ();
		RenderArc ();
	}

	/// <summary>
	/// Update v0 and angle, cause target has been changed
	/// </summary>
	private void PropertiesUpdate() {
//		transform.eulerAngles = new Vector3 (0f, transform.eulerAngles.y, 0f);

		g = Mathf.Abs(Physics.gravity.y) * playerControl.gravity_MultiValue;

		//	Face to target, in order to keep the parabola trajectory the same as missile's motion trajectory when emit height is larger than zero
//		transform.rotation = Quaternion.LookRotation(playerControl.FireTarget - transform.position);
		transform.LookAt(playerControl.FireTarget);

		//	Calculate fire angle in radian
		radianAngle = playerControl.GetFiringAngle * Mathf.Deg2Rad;

        float diff_Y = transform.position.y - playerControl.FireTarget.y;

		//	Calculate the initial velocity, by given d(max distance along horizontal), h0(the initial height when missile is emitted) and shootAngle
		v0 = CalculateInitialVelocityByMaxHDistance(GetTargetDistance, diff_Y, radianAngle);

		UpdateToPlayerControl ();
    }

	private void UpdateToPlayerControl() {
		//  Update properties in playerControl
		playerControl.Missile_V0 = GetV0;
		playerControl.Missile_Vx = GetVx;
		playerControl.Missile_Vy = GetVy;
		playerControl.Missile_HorizontalDistance = GetTargetDistance;

		playerControl.CurFlightDuration = GetFlightDuration;
		playerControl.CurFlightHighestPoint = GetFlightHighestPoint;
	}

	/// <summary>
	/// Set Positions to LineRenderer, which means, draw the parabola trajectory
	/// </summary>
    private void RenderArc() {
		if (!lr.enabled) {
			Debug.Log ("LineRenderer is disabled");
			return;
		}

        lr.positionCount = elemCount + 1;
        lr.SetPositions(CalculateArcArray());
    }
	#endregion

	#region Calculate Points
	/// <summary>
	/// Get Vector3[] array, which is consist of the points of that parabola line
	/// </summary>
	/// <returns>The arc array.</returns>
    private Vector3[] CalculateArcArray() {
        Vector3[] arcArray = new Vector3[elemCount + 1];

		#region	从原点抛出, 水平方向上运动的最大距离
//        float maxDistance = (Mathf.Pow(v0, 2) * Mathf.Sin(2 * radianAngle)) / g;
		#endregion


        for (int i = 0; i <= elemCount; i++) {
			if (i == 0) {
				arcArray [0] = Vector3.zero;
			} else {
				float t = (float)i / (float)elemCount;
				arcArray [i] = CalculateArcPoint (t, GetTargetDistance);
			}
        }

        return arcArray;
    }

	/// <summary>
	/// Calculate a single point position, by given the ratio and d(total distance along x-z plane)
	/// </summary>
	/// <returns>The arc point.</returns>
	/// <param name="t">T.</param>
	/// <param name="maxDistance">Max distance.</param>
    private Vector3 CalculateArcPoint(float t, float maxDistance) {
        float x = t * maxDistance;
		float y = x * Mathf.Tan(radianAngle) - (g * x * x) / (2 * Mathf.Pow(v0 * Mathf.Cos(radianAngle), 2));

		if (float.IsNaN (y)) {
			return new Vector3 (0, 0, x);
		} else {
			return new Vector3 (0, y, x);
		}
    }
	#endregion

	#region	Math
	/// <summary>
	/// 从 (0, h0) 以初速度 v0(与水平面夹角为 radAngle)抛出，水平方向上运动的最大距离
	/// </summary>
	/// <returns>The max distance.</returns>
	/// <param name="v0">V0.</param>
	/// <param name="radAngle">RAD angle.</param>
	/// <param name="h0">H0.</param>
	public float GetMaxDistance(float v0, float radAngle, float h0) {
		return 
//			(
//				(Mathf.Pow(v0, 2) * Mathf.Sin(2 * radAngle)) / (2 * g)
//			)
//
//			* 
//
//			(
//				Mathf.Sqrt
//				(
//					1 + 
//					(2 * g * h0) / (Mathf.Pow(v0 * Mathf.Sin(angle), 2))
//				)
//
//				+ 1
//			);

			(
		    	(v0 * Mathf.Cos (radAngle)) / g
			)

			*

			(
			    (v0 * Mathf.Sin (radAngle))
			    
				+
			    
				(
			        Mathf.Sqrt
					(
				        Mathf.Pow (v0 * Mathf.Sin (radAngle), 2) + 2 * g * h0
			        )
			    )
			);
	}

	/// <summary>
	/// 从 (0, h0) 处以 与水平面呈 radAngle 角度抛出，水平方向最远运动 d 个单位，所需要的初速度 v0
	/// </summary>
	private float CalculateInitialVelocityByMaxHDistance(float d, float h0, float radAngle) {
		float m = (Mathf.Sqrt (2 * g * h0) * Mathf.Cos (radAngle)) / (g * Mathf.Sin (radAngle));
		float molecular = (d - m) * g;
		float denoMinator = Mathf.Sin (2 * radAngle);

		return 
			Mathf.Sqrt
			(
				molecular / denoMinator
			);
	}
	#endregion
}
