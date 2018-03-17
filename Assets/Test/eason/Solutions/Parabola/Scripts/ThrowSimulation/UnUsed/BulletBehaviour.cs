using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehaviour : PoolObjectBehaviour {

    public float shootForce = 100f;
    public float shootSpeed;

    private bool isHit = false;
	private PlayerControl_EmitParabola playerControl;

    public bool IsHit {
        get { return this.isHit; }
    }

    public override void OnObjectReuse() {
        isHit = false;
        Emit();
    }

    private void Awake() {
		playerControl = GameObject.Find("Player").GetComponent<PlayerControl_EmitParabola>();
    }

    private void Start() {
        Emit();
    }

    private void OnTriggerEnter(Collider col) {
        if (col.transform.tag == "Ground" && !isHit) {
            //  Hit the ground, set this be the fire target
            playerControl.FireTarget = transform.position;
            Debug.Log("hit");
        }
    }

    private void Emit() {
        GetComponent<Rigidbody>().velocity = transform.forward * shootSpeed;
    }
}
