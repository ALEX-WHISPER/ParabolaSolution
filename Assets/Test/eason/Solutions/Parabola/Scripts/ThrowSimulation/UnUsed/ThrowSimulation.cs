using UnityEngine;
using System.Collections;

public class ThrowSimulation : MonoBehaviour {
    public Transform Target;

    public float angleIncrement;
    public float gravity = 9.8f;
    public float shootDelay = 1.5f;
    public float fireAngle_Min = 20f;   
    public float fireAngle_Max = 80f;

    public GameObject projectilePrefab;
    public Vector3 spawnOffset;

    private float firingAngle = 45.0f;
    private Transform myTransform;
    private Transform angleController;


    void Awake() {
        myTransform = transform;
        angleController = transform;
    }

    void Start() {
        //StartCoroutine(SimulateProjectile());
    }

    private void Update() {
        firingAngle += GetAngleInput() * angleIncrement;
        float angleClamp = Mathf.Clamp(firingAngle, fireAngle_Min, fireAngle_Max);

        angleController.eulerAngles = new Vector3(
            angleController.eulerAngles.x, 
            angleController.eulerAngles.y, 
            angleClamp);

        if (Input.GetButtonDown("Jump")) {
            Debug.Log("Shoot");
            StartCoroutine(SimulateProjectile());
        }
    }

    private float GetAngleInput() {
        return Input.GetAxis("Vertical");
    }

    IEnumerator SimulateProjectile() {
        // Short delay added before Projectile is thrown
        yield return new WaitForSeconds(shootDelay);

        Transform projectTile = Instantiate(projectilePrefab, transform.position, transform.rotation).transform;

        // Move projectile to the position of throwing object + add some offset if needed.
        projectTile.position = myTransform.position + spawnOffset;

        // Calculate distance to target
        float target_Distance = Vector3.Distance(projectTile.position, Target.position);

        // Calculate the velocity needed to throw the object to the target at specified angle.
        float projectile_Velocity = target_Distance / (Mathf.Sin(2 * GetFiringAngle() * Mathf.Deg2Rad) / gravity);

        // Extract the X  Y componenent of the velocity
        float Vx = Mathf.Sqrt(projectile_Velocity) * Mathf.Cos(GetFiringAngle() * Mathf.Deg2Rad);
        float Vy = Mathf.Sqrt(projectile_Velocity) * Mathf.Sin(GetFiringAngle() * Mathf.Deg2Rad);

        // Calculate flight time.
        float flightDuration = target_Distance / Vx;

        // Rotate projectile to face the target.
        projectTile.rotation = Quaternion.LookRotation(Target.position - projectTile.position);

        float elapse_time = 0;

        while (elapse_time < flightDuration) {
            projectTile.Translate(0, (Vy - (gravity * elapse_time)) * Time.deltaTime, Vx * Time.deltaTime);
            elapse_time += Time.deltaTime;
            yield return null;
        }
    }

    private float GetFiringAngle() {
        return firingAngle;
    }
}