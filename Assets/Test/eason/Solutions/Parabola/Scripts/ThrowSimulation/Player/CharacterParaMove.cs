using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterParaMove : MonoBehaviour {

    public Transform targetTransform;
    public float defaultEmitDegAngle = 45f;
    public float gravity_MutiValue = 1f;
    public float fireInterval = 0.5f;

    private float v0;
    private float gravity;
    private GameObject relativeObj;

    private float lastFire = 0f;

    private void Start() {
        gravity = Mathf.Abs(Physics.gravity.y) * gravity_MutiValue;
        relativeObj = new GameObject("relativeObject");
    }

    private void Update() {
        if (Input.GetAxis("Jump") > 0 && Time.time > lastFire) {
            gravity = Mathf.Abs(Physics.gravity.y) * gravity_MutiValue;

            Emit();
            lastFire = Time.time + fireInterval;
        }
    }

    private void Emit() {
        StartCoroutine(SimulateProjectile());
    }

    IEnumerator SimulateProjectile() {
        // Short delay added before Projectile is thrown
        //yield return new WaitForSeconds(fireDelay);

        float Vx, Vy, flightDuration;
        SimulateCalculation(out Vx, out Vy, out flightDuration);

        // Rotate proje	ctile to face the target.
        transform.rotation = Quaternion.LookRotation(targetTransform.position - transform.position);
        relativeObj.transform.rotation = transform.rotation;
        relativeObj.transform.position = transform.position;

        float elapse_time = 0;
        while (elapse_time < flightDuration) {
            Vector3 p = new Vector3(0, (Vy - (gravity * elapse_time)) * Time.deltaTime, Vx * Time.deltaTime);
            relativeObj.transform.Translate(p);
            transform.localRotation = Quaternion.LookRotation(relativeObj.transform.position - transform.position);

            transform.Translate(p, relativeObj.transform);
            elapse_time += Time.deltaTime;
            yield return null;
        }
    }

    private void SimulateCalculation(out float Vx, out float Vy, out float flightDuration) {
        // Calculate distance to target
        Vector3 targetPos = targetTransform.position;
        Vector3 startPos = transform.position;
        Vector3 startPos_SamePlane = new Vector3(startPos.x, targetPos.y, startPos.z);
        float target_Distance = Vector3.Distance(startPos_SamePlane, targetPos);

        // Calculate the velocity needed to throw the object to the target at specified angle.
        v0 = target_Distance / (Mathf.Sin(2 * defaultEmitDegAngle * Mathf.Deg2Rad) / gravity);

        // Extract the X  Y componenent of the velocity
        Vx = Mathf.Sqrt(v0) * Mathf.Cos(defaultEmitDegAngle * Mathf.Deg2Rad);
        Vy = Mathf.Sqrt(v0) * Mathf.Sin(defaultEmitDegAngle * Mathf.Deg2Rad);

        // Calculate flight time.
        flightDuration = target_Distance / Vx;
    }
}
