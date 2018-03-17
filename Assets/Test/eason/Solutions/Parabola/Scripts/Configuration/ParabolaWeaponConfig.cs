using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ParabolaWeaponConfig : MonoBehaviour {

	public string gunName = "AimAtStartPoint";
	public string fireSpawnName = "FirePoint";
    public bool isEasonUIEnabled = false;

	public event Action OnReCharge;		//	Reset charge process on the beginning of charging(chargeValue equals to 0 or 1)
	public event Action<float> OnChargingProcess;	//	During charging

	private float holdDuration;
	private float imgFillAmount_Simu;
	private bool isCharging;

	private GameObject m_ParaUI;
	private GameObject m_ChargeController;
	private GameObject m_ParaGun;
	private GameObject m_TargetDetector;
	private GameObject m_LaunchArcRenderer;
	private PlayerControl_EmitParabola playerControl;
    private List<GameObject> featureObjList = new List<GameObject>();

	#region	ADD
	#region PUBLIC
	public void AddParabolaConfig(GameObject go, bool isLocal) {
		AddPlayerComponent ();
		AddFeatureObjects (isLocal);
		playerControl.IsParabolaActive = true;
	}
	#endregion

	#region PRIVATE
	private void AddPlayerComponent() {
        if (playerControl != null) {
            if (!playerControl.enabled) {
                Debug.Log("AddComponent FAILED, Enable SUCCESS: PlayerControl_EmitParabola had been added before, now enable it.");
                playerControl.enabled = true;
                return;
            } else {
                Debug.Log("AddComponent FAILED, Enable FAILED: PlayerControl_EmitParabola is active now, you're trying to re-add it.");
                return;
            }
        } else {
            Debug.Log("AddComponent SUCCESS: PlayerControl_EmitParabola been added");
            playerControl = gameObject.AddComponent<PlayerControl_EmitParabola>();
        }
    }

	private void AddFeatureObjects(bool isLocal) {
		
		string folderPath = "eason/ParabolaWeaponInit";
		string path_UI = folderPath + "/ParabolaUI";
		string path_Charge = folderPath + "/ChargeController";
		string path_Gun = folderPath + "/Gun";
//		string path_TargetDetector = folderPath + "/TargetDetector";
		string path_TargetDetector = folderPath + "/Projector";
		string path_LaunchArc = folderPath + "/LaunchArc";

        if (isLocal && isEasonUIEnabled) {
            if (IsLegalToCreate(m_ParaUI)) {
                m_ParaUI = Instantiate(Resources.Load(path_UI, typeof(GameObject)), transform.position, Quaternion.identity, FindObjectOfType<Canvas>().transform) as GameObject;
                featureObjList.Add(m_ParaUI);
            }
        }

        if (IsLegalToCreate(m_ParaGun)) {
			m_ParaGun = Instantiate(Resources.Load(path_Gun, typeof(GameObject)), transform.Find(gunName).position, transform.Find(gunName).rotation, transform.Find(gunName)) as GameObject;
//            featureObjList.Add(m_ParaGun);
        }

        if (isLocal) {
            if (IsLegalToCreate(m_TargetDetector)) {
//                m_TargetDetector = Instantiate(Resources.Load(path_TargetDetector, typeof(GameObject)), transform.position + new Vector3(0f, 0f, 5f), Quaternion.identity, transform.Find(gunName)) as GameObject;
				GameObject projector = Resources.Load(path_TargetDetector, typeof(GameObject)) as GameObject;
				m_TargetDetector = Instantiate(Resources.Load(path_TargetDetector, typeof(GameObject)), transform.position + new Vector3(0f, 0f, 10f), projector.transform.rotation, transform.Find(gunName)) as GameObject;

                featureObjList.Add(m_TargetDetector);
            }
        }

        if (IsLegalToCreate(m_LaunchArcRenderer)) {
			m_LaunchArcRenderer = Instantiate(Resources.Load(path_LaunchArc, typeof(GameObject)), transform.Find(gunName).position, transform.Find(gunName).rotation, transform.Find(gunName)) as GameObject;
            featureObjList.Add(m_LaunchArcRenderer);
        }
    }

    private bool IsLegalToCreate(GameObject m_GO) {
        if (m_GO != null) {
            if (!m_GO.activeSelf) {
                Debug.Log(string.Format("{0} had been created before, now activate it", m_GO.name));
                m_GO.SetActive(true);
                return false;
            } else {
                Debug.Log(string.Format("{0} is active now, and you're trying to re-creaete it", m_GO.name));
                return false;
            }
        }
        return true;
    }

    #endregion
    #endregion

    #region REMOVE
    public void RemoveParabolaConfig() {
        if (playerControl == null)
            return;
		//	Disable Player Component
        if (playerControl.enabled) {
            Debug.Log("Remove SUCCESS: Remove PlayerControl_EmitParabola");
            playerControl.enabled = false;
        }

		//	Disable feature objects
		if (featureObjList.Count == 0) {
			Debug.Log ("Remove FAILED: There's nothing to be removed!");
			return;
		} else {
			foreach (GameObject item in featureObjList) {
				if (item.activeSelf) {
					item.SetActive (false);
					playerControl.IsParabolaActive = false;
					Debug.Log (string.Format ("Remove SUCCESS: Remove {0}", item.name));
				} else {
					Debug.Log (string.Format("Remove FAILED: {0} has been removed already", item.name));
				}
			}
		}
    }
    #endregion

#region PUBLIC interfaces
	public void CmdAimAtTarget(ParabolaData data) {
		if (!playerControl.enabled) {
			Debug.Log ("Command TargetChanged FAILED: PlayerControl is disabled now");
			return;
		}

		playerControl.FireTarget = data.targetPos;
		playerControl.CallTargetChangedEvent ();
	}

	public void CmdNormalFire(ParabolaData data) {
		if (!playerControl.enabled) {
			Debug.Log ("Command Normal Fire FAILED: PlayerControl is disabled now");
			return;
		}

		if (playerControl.Missile_Moving) {
			Debug.Log ("Can not fire yet. The last missile is still moving!!!");
			return;
		}

		playerControl.FireTarget = data.targetPos;
		playerControl.CallRecvToTargetEvent ();
	}
#endregion
}

public struct ParabolaData {
    public Vector3 targetPos;
    public ParabolaData(Vector3 targetPos) {
        this.targetPos = targetPos;
    }
}