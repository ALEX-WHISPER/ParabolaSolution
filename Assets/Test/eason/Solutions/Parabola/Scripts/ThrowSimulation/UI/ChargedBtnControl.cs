using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class ChargedBtnControl: MonoBehaviour {
	public float fillingSpeed = 0.3f;
	public float holdingTriggerDuration = 0.3f;
	public float clickInterval = 1f;
	public event Action OnReCharge;		//	Reset charge process on the beginning of charging(chargeValue equals to 0 or 1)
	public event Action<float> OnChargingProcess;	//	During charging

    private bool isUp;
	private bool isWake;
	private bool isCharging;
    private Image img;
	private float chargedValue;
	private float holdDuration;
	private float imgFillAmount_Simu;
	private MissilePoolManager missileManager;
	private PlayerControl_EmitParabola playerControl;

	private bool canClick = true;

	public bool IsCharging { get { return isCharging; } }

	public bool IsLoosen {get { return isWake && isUp;}}

	#region Unity Life Cycle
	void OnEnable() {
		Arrangement ();
	}

	void OnDestroy() {
		EventsDeRegistration ();
	}
	#endregion

	#region Arrangement
	private void Arrangement() {
		playerControl = FindObjectOfType<PlayerControl_EmitParabola>();
		img = GetComponent<Image>();
		EventsRegistration ();
	}
	#endregion

	#region Events
	private void EventsRegistration() {
		EventTriggerManager.Get(gameObject).onPressDown += OnClickDown;
		EventTriggerManager.Get(gameObject).onPressUp += OnClickUp;
	}

	private void EventsDeRegistration() {
		EventTriggerManager.Get (gameObject).onPressDown -= OnClickDown;
		EventTriggerManager.Get (gameObject).onPressUp -= OnClickUp;
	}

    void OnClickDown(GameObject go)
    {
		if (!isWake) {
			isWake = true;
		}

		isUp = false;
		OnChargeHolding ();
    }

    void OnClickUp(GameObject go)
    {
        isUp = true;

		if (canClick) {
			OnChargeLoosen ();
			StartCoroutine (NextShootDuration());
		}
    }
	#endregion

	private void OnChargeHolding() {
		StartCoroutine(HoldEnoughTimeToGrow());
	}

	private void OnChargeLoosen() {
		img.fillAmount = 0f;

		chargedValue = 0f;
		ParabolaData data = new ParabolaData (playerControl.FireTarget);
		playerControl.CommandFire (data);	
	}

	private IEnumerator HoldEnoughTimeToGrow() {
		float holdingTime = 0f;
		bool isEnough = true;	//	Assume the user holds the charge btn long enough to start charge operation, or the charge operation would be regarded as a normal shoot operation

		//	In this while loop, if the user loose the btn when the holdingTime is not enough, the flag of "isEnough" will set to be false.
		//	So that the "Grow()" function will not be called.
		//	Which means this operation will be regarded as a normal shoot operation
		while(holdingTime < holdingTriggerDuration) {
			if (isUp) {
				isEnough = false;
				break;
			} else {
				holdingTime += Time.deltaTime;
				yield return null;
			}
		}

		//	If the holdTime is not enough to trigger charging shoot, do not enter in the "Grow()" function
		if (!isEnough) {
			yield break;
		}

		//	Else the holdTime would be enough as assumption, then start to charge
		StartCoroutine (Grow());
	}

	private IEnumerator Grow()
	{
		if (OnReCharge != null) {
			OnReCharge ();
		}

		while (true)
		{
			isCharging = true;
			if (isUp) {
				holdDuration = 0;
				break;
			} else if (Mathf.Abs (img.fillAmount - 1) <= float.Epsilon) {
				ChargeFull ();

				if (OnReCharge != null) {
					OnReCharge ();
				}

				holdDuration = 0;
				continue;
			} else {
				ChargeGrowing ();
				holdDuration += Time.deltaTime;

				if (OnChargingProcess != null) {
					OnChargingProcess (fillingSpeed * holdDuration);
				}
			}

			yield return null;
		}
		isCharging = false;
	}

	private void ChargeFull() {	
		img.fillAmount = 0f;
	}

	private void ChargeGrowing() {
		img.fillAmount += fillingSpeed * Time.deltaTime;
		chargedValue = img.fillAmount;
		chargedValue = Mathf.Clamp (chargedValue, 0f, img.fillAmount);
	}

	private IEnumerator NextShootDuration() {
		canClick = false;
		yield return new WaitForSeconds (clickInterval);
		canClick = true;
	}

	#region For interface test
	public void StartCharging(float targetChargeValue) {
		StartCoroutine (Grow(targetChargeValue));
	}

	private IEnumerator Grow(float targetChargeValue)
	{
		if (isCharging) {
			Debug.Log ("Charging is excuting...");
			yield break;
		}

		if (targetChargeValue <= 0 || targetChargeValue > 1f) {
			Debug.Log (string.Format("TargetChargeValue is {0}, which is out of range!!! ", targetChargeValue));
			yield break;
		}

		if (OnReCharge != null) {
			OnReCharge ();
		}

		holdDuration = 0f;
		imgFillAmount_Simu = 0f;
		while (true) {
			isCharging = true;
			if (Mathf.Abs (imgFillAmount_Simu - targetChargeValue) <= 0.002f) {
				break;
			} else {
//				ChargeGrowing ();
				holdDuration += Time.deltaTime;
				imgFillAmount_Simu += fillingSpeed * Time.deltaTime;

				if (OnChargingProcess != null) {
					OnChargingProcess (fillingSpeed * holdDuration);
				}
				yield return null;
			}
		}
		isCharging = false;
		holdDuration = 0;
		imgFillAmount_Simu = 0;

		OnChargeLoosen ();
	}
	#endregion
}
