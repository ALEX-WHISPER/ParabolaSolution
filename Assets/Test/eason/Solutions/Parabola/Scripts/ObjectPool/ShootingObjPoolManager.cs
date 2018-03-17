using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ShootingObjPoolManager : MonoBehaviour{

    public GameObject m_ShootingObjPrefab;
    public int poolSize;
    public float shootInterval;

    protected float lastShoot;
	protected PlayerControl_EmitParabola playerControl;
	private List<PoolManager.ObjectInstance> OIList = new List<PoolManager.ObjectInstance>();

    protected void Awake() {
		playerControl = GameObject.FindWithTag("Player").GetComponent<PlayerControl_EmitParabola>();
    }

    protected void Start() {
		OIList = PoolManager.GetInstance.CreatePool(m_ShootingObjPrefab, poolSize);
    }

    protected void Update() {
        if (GetInputCommand()) {
			ShootByInput ();
        }
    }

	protected virtual void ShootByInput() {
		if (Time.time > lastShoot + shootInterval) {
//			PoolManager.GetInstance.Reuse(m_ShootingObjPrefab, playerControl.shotSpawn.position, playerControl.shotSpawn.rotation);
		}
	}

	protected virtual void ShootByCharged(float chargedValue) {
		if (OIList.Count < 1) {
			Debug.LogError ("Pool is empty!!!");
			return;
		}

		foreach (PoolManager.ObjectInstance oi in OIList) {
			if (oi.GetXform.GetComponent<MissileBehaviour> () != null) {
				MissileBehaviour missileBehaviour = oi.GetXform.GetComponent<MissileBehaviour> ();
//				missileBehaviour.gravity = Mathf.Abs(Physics.gravity.y) * chargedValue * 5f;
			}
		}
		ShootByInput ();
	}

    public abstract bool GetInputCommand();
}
