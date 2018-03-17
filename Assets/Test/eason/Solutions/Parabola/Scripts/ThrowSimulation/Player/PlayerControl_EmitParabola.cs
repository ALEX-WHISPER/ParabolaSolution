using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerControl_EmitParabola : MonoBehaviour {

	public event Action TargetChangedEvent;
	public event Action EmitMissileCommandEvent;
	public event Action RecvToFireEvent;
	public event Action OnReCharge;		//	Reset charge process on the beginning of charging(chargeValue equals to 0 or 1)
	public event Action<float> OnChargingProcess;	//	During charging

    #region For AngleRotate and ViewRotate
    public string joystickName_RotateView = "Joystick_FrontView";
	public List<string> groundTagList = new List<string>();
	public float angleIncrement = 2f;
	public float gunRotOffsetAlongXAxis = 45f;
	public float gravity_MultiValue = 0.5f;

	public float viewRangeYAxis_Min = -180f;
	public float viewRangeYAxis_Max = 180f;
	public float viewRangeXAxis_Min = -20f;
	public float viewRangeXAxis_Max = 20f;
	#endregion

	#region For Charging
	public float chargeFillSpeed = 0.25f;
	private float holdDuration;
	private float imgFillAmount_Simu;
	private bool isCharging;
	private ChargedBtnControl chargeControl;
	#endregion

	#region Variables For Reference
	private Vector3 targetPos;
    private float curFlightDuration;
    private Vector3 curFlightHighestPoint;

	//	For Missile Motion
	private float v0, vx, vy, maxDistance_H;

	private float xRot, xRotValue, xRotRef;
	private float yRot, yRotValue, yRotRef;
	private float curZPos, tarZPos;

	private bool frontViewTrigger = false;
	private bool isMissileMoving = false;
	private Vector2 frontViewInputValueVec;
	#endregion

	#region Public Properties
	public bool IsParabolaActive {get; set;}
	public bool Missile_Moving {get { return isMissileMoving; } set { isMissileMoving = value; }}
	public float Missile_V0 { get { return this.v0; } set { this.v0 = value; }}
	public float Missile_Vx { get { return this.vx; } set { this.vx = value; }}
	public float Missile_Vy { get { return this.vy; } set { this.vy = value; }}
	public float Missile_HorizontalDistance {get { return this.maxDistance_H; } set { this.maxDistance_H = value; }}

	public bool IsCharging { 
		get { 
			if (FindObjectOfType<ChargedBtnControl> () == null) {
				return false;
			} else {
				return FindObjectOfType<ChargedBtnControl> ().IsCharging;
			}
		} 
	}

	public float GetFiringAngle { get { return -1 * transform.eulerAngles.x + gunRotOffsetAlongXAxis; } }

	public Vector3 FireTarget { get { return this.targetPos; } set { targetPos = value; } }

    public float CurFlightDuration { get { return this.curFlightDuration; } set { this.curFlightDuration = value; } }
    
	public Vector3 CurFlightHighestPoint { get { return this.curFlightHighestPoint; } set { this.curFlightHighestPoint = value; } }

    public Vector2 GetFrontViewInputValue { get { return this.frontViewInputValueVec;} }

	public float GetXRotaValue { get { return this.xRotValue; }}

	public bool FrontViewInput { get { return this.frontViewTrigger; } set { this.frontViewTrigger = value; }}

	public void CallTargetChangedEvent() {
		if (TargetChangedEvent != null) {
			Debug.Log ("CALL TargetChangedEvent: OnCallTargetChangedEvent");
			TargetChangedEvent ();
		}
	}

	public void CallRecvToTargetEvent() {
		if (RecvToFireEvent != null) {
			RecvToFireEvent ();
		}
	}
#endregion

	#region UNITY LIFE CYCLE

    private void OnEnable() {
		Arrangement ();
//		StartCoroutine (CallTargetEventOnStart());
	}

	private void Start() {
		StartCoroutine (CallTargetEventOnStart());
	}

	private void Update() {
		InputResponseOperation ();
	}

	private void OnDisable() {
		EventsDeRegistration ();
	}
#endregion

	#region Arrangement
	private void Arrangement() {
		ReferenceSetting ();
		InitGroundTagList ();
		TransformSetting ();
		EventsRegistration ();

		//if (GetComponent<Rigidbody>() != null) { 
		//	GetComponent<Rigidbody> ().isKinematic = true; 
		//}
	}

	private void ReferenceSetting() {
		chargeControl = FindObjectOfType<ChargedBtnControl> ();
	}

	private void InitGroundTagList() {
		string[] tagNameList = InteractableGroundTagList.InteractiveTagNames;

		foreach(string tag in tagNameList) {
			groundTagList.Add (tag);
		}

		foreach (string tagName in groundTagList) {
			Debug.Log (tagName);
		}
	}

	private void TransformSetting() {
		Vector3 newEuler = new Vector3 (transform.eulerAngles.x, transform.eulerAngles.y, 0f);
		transform.eulerAngles = newEuler;
	}
#endregion

	#region Call TargetChangedEvent on Start
	IEnumerator CallTargetEventOnStart() {
		yield return new WaitForSeconds (0.2f);
		CallTargetChangedEvent ();
	}
#endregion

	#region Events
	private void EventsRegistration() {
		EasyJoystick.On_JoystickMove += OnFrontViewJoystickMove;
		EasyJoystick.On_JoystickMove += OnFrontViewChanged;
		EasyJoystick.On_JoystickMoveEnd += OnFrontViewChangedEnd;

		//	Charging invoked by btn
		if (chargeControl != null) {
			chargeControl.OnChargingProcess += RotateFronViewAlongXAxis;
			chargeControl.OnReCharge += ResetFrontView;
		}
	}

	private void EventsDeRegistration() {
		EasyJoystick.On_JoystickMove -= OnFrontViewJoystickMove;
		EasyJoystick.On_JoystickMoveStart -= OnFrontViewChanged;
		EasyJoystick.On_JoystickMoveEnd -= OnFrontViewChangedEnd;

		//	Charging invoked by btn
		if (chargeControl != null) {
			chargeControl.OnChargingProcess -= RotateFronViewAlongXAxis;
			chargeControl.OnReCharge -= ResetFrontView;
		}
    }
#endregion

	#region INPUT
	/// <summary>
	/// Get Input that will causes target changing
	/// </summary>
	/// <returns><c>true</c>, if changed input was targeted, <c>false</c> otherwise.</returns>
	private bool TargetChangedInput() {
		return (IsCharging || FrontViewInput);
	}

	/// <summary>
	/// Called on JoystickMove, to rotate player's front view
	/// </summary>
	/// <param name="move">Move.</param>
	private void OnFrontViewJoystickMove(MovingJoystick move) {
		if (Missile_Moving) {
			Debug.Log ("Missile is still moving");
			return;
		}

		if (move.joystickName.ToString().Equals(joystickName_RotateView)) {
			
			//	摇杆的控制 小于0.1的移动就取消
			if (Mathf.Abs (move.joystickAxis.y) > 0.1f || Mathf.Abs (move.joystickAxis.x) > 0.1f) {

				//	不限制人物沿 y 轴旋转的范围（joystickAxis.x）, 但限制其沿 x 轴的旋转范围(joysyickAxis.y: viewRangeXAxis_Min ~ viewRangeXAxis_Max)
				Vector3 rot = transform.localEulerAngles;
				rot += new Vector3 (0f, move.joystickAxis.x);

				xRot -= move.joystickAxis.y * angleIncrement;
				xRot = Mathf.Clamp (xRot, viewRangeXAxis_Min, viewRangeXAxis_Max);
				xRotValue = Mathf.SmoothDamp (xRotValue, xRot, ref xRotRef, 0.05f);
				xRotValue = Mathf.Clamp (xRotValue, viewRangeXAxis_Min, viewRangeXAxis_Max);

				rot = new Vector3 (xRotValue, rot.y, 0f);

				if (move.joystickName.ToString().Equals(joystickName_RotateView)) {
					transform.localEulerAngles = rot;
				}
			}
			this.frontViewInputValueVec = move.joystickAxis;
		}

		if (move.joystickName.ToString().Equals("Joystick2")) {
			this.frontViewInputValueVec = move.joystickAxis;
		}
	}

	private void OnFrontViewChanged(MovingJoystick move) {
		if (Missile_Moving) {
			return;
		}

		FrontViewInput = true;
	}

	private void OnFrontViewChangedEnd(MovingJoystick move) {
		if (Missile_Moving) {
			return;
		}

		FrontViewInput = false;
	}
#endregion

	#region INPUT Response
	private void InputResponseOperation() {
		//	If the input which will cause target changing happens, call the TargetChangedEvent
		if (TargetChangedInput()) {
			if(TargetChangedEvent != null) {
				Debug.Log ("CALL TargetChangedEvent: OnInputResponseOperation");
				TargetChangedEvent ();
			}
		}
	}

	#region Fire
	public void CommandEmitMissile() {
		if (EmitMissileCommandEvent != null) {
			EmitMissileCommandEvent ();
		}
	}
	#endregion

	#region Rotate Front View of Player, Invoke by Joystick move input
	public void RotateFrontViewAlongYAxis(float inputValue) {
		Debug.Log ("Move Along Y Axis: " + inputValue);

		yRot += inputValue * angleIncrement;
		yRot = Mathf.Clamp (yRot, viewRangeYAxis_Min, viewRangeYAxis_Max);
		yRotValue = Mathf.SmoothDamp (yRotValue, yRot, ref yRotRef, 0.05f);
		yRotValue = Mathf.Clamp (yRotValue, viewRangeYAxis_Min, viewRangeYAxis_Max);

		transform.localEulerAngles = new Vector3 (transform.eulerAngles.x, yRotValue, transform.eulerAngles.z);
	}

	public void RotateFronViewAlongXAxis(float inputValue) {
		Debug.Log ("Move Along X Axis: " + inputValue);

		xRot -= inputValue * angleIncrement;
		xRot = Mathf.Clamp (xRot, viewRangeXAxis_Min, viewRangeXAxis_Max);
		xRotValue = Mathf.SmoothDamp (xRotValue, xRot, ref xRotRef, 0.05f);
		xRotValue = Mathf.Clamp (xRotValue, viewRangeXAxis_Min, viewRangeXAxis_Max);

		transform.localEulerAngles = new Vector3 (xRotValue, transform.eulerAngles.y, transform.eulerAngles.z);
	}

	private void ResetFrontView() {
		xRot = xRotRef = xRotValue = 0f;
	}
	#endregion

	#region Charging
	private void OnChargeToAimTarget(float chargeValue) {
		if (OnChargingProcess != null) {
			OnChargingProcess (chargeValue);
			Debug.Log ("CALL TargetChangedEvent: OnChargeToAimTarget");
			TargetChangedEvent ();
		}
	}
   	#endregion
#endregion

	#region Recv Net Message
	public void CommandAimAtTarget(ParabolaData data) {
		FireTarget = data.targetPos;
		CallTargetChangedEvent ();
	}

	public void CommandFire(ParabolaData data) {
		if (isMissileMoving) {
			return;
		}

		FireTarget = data.targetPos;
		CallRecvToTargetEvent ();
	}
#endregion
}
