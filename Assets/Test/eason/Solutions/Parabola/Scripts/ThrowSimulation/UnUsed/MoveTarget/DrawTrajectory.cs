using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DrawTrajectory : MonoBehaviour {

	public GameObject m_VisualPointPrefab;

	private float gravity;
	private PlayerControl_EmitParabola playerControl;
	private LineRenderer lr;
	private float Vx, Vy, flightDuration;

	void Awake() {
		lr = GetComponent<LineRenderer> ();
		gravity = Physics.gravity.y;
	}

	void Start() {
		playerControl = GameObject.FindWithTag ("Player").GetComponent<PlayerControl_EmitParabola>();
	}

	void Update() {
		if (Input.GetKey(KeyCode.A)) {
			StartCoroutine(SimulateCalculation ());
			StartCoroutine(SimulateProjectile());
		}
	}

	IEnumerator SimulateCalculation() {

		Debug.Log ("FireTarget: " + playerControl.FireTarget);
		Debug.Log ("FireAngle: " + playerControl.GetFiringAngle);

		yield return new WaitForSeconds (0.2F);
		// Calculate distance to target
		float target_Distance = Vector3.Distance(transform.position, playerControl.FireTarget);

		// Calculate the velocity needed to throw the object to the target at specified angle.
		float projectile_Velocity = target_Distance / (Mathf.Sin(2 * playerControl.GetFiringAngle * Mathf.Deg2Rad) / gravity);
		Debug.Log ("V = " + projectile_Velocity);

		// Extract the X Y componenent of the velocity
		Vx = Mathf.Sqrt(projectile_Velocity) * Mathf.Cos(playerControl.GetFiringAngle * Mathf.Deg2Rad);
		Vy = Mathf.Sqrt(projectile_Velocity) * Mathf.Sin(playerControl.GetFiringAngle * Mathf.Deg2Rad);

		// Calculate flight time.
		flightDuration = target_Distance / Vx;
	}

	private void DrawMotionTrajectory() {
		
	}

	IEnumerator SimulateProjectile() {
//		yield return new WaitForSeconds (2F);

		Debug.Log (string.Format("Vx: {0}, Vy: {1}, flightDuration: {2}", Vx, Vy, flightDuration));
		// Rotate proje	ctile to face the target.
		transform.rotation = Quaternion.LookRotation(playerControl.FireTarget - transform.position);

		float elapse_time = 0;
		while (elapse_time < flightDuration) {
			Vector3 p = new Vector3 (0, (Vy - (gravity * elapse_time)) * Time.deltaTime, Vx * Time.deltaTime);
			Instantiate (m_VisualPointPrefab, p, Quaternion.identity);
			elapse_time += Time.deltaTime;
			yield return null;
		}
	}
}
