using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//[RequireComponent(typeof(MissileBehaviour))]
[RequireComponent(typeof(Rigidbody))]
public class MissileDestruction : MonoBehaviour {

	[Header("有参/无参击打")]
	public bool destroyWithArgs = false;

	[Header("是否锁死目标点, true: 始终击打指定位置(即下个变量所表示的位置)，false: 击打小球碰撞位置")]
	public bool lockDestroyTarget = false;

	[Header("击打目标点")]
	public Vector3 craterTargetPos;

	[Header("弹坑半径，由于原贴图为32*32, 故该值需为2的幂，否则弹坑变形")]
	public int craterRadius = 16;

	[Header("弹坑物理深度")]
	public float craterPhysicsDepth = 5f;

	[Header("弹坑贴图颜色深度")]
	public float craterColorDepth = 2f;

	[Header("Rigidbody 爆炸力")]
	public float rbExplosionForce = 50f;

	[Header("射线检测(检测是否与Terrain相撞)的距离")]
	public float hitDistanceValue = 0.5f;

	[Header("爆炸效果预制体")]
	public GameObject explosion;
//	public float influenceRadiusValue = 6f;
//	public float explosionForceValue = 15f;

	[Header("以下两个 Texture 用于显示弹坑形态，按照示例场景中进行设置即可")]
	public Texture2D craterTexture;
	public Texture2D groundTexture;

//	[Header("Terrain 物体的 tag 名称，用于射线检测判断")]
//	public string groundTagName = "Ground";

	public event Action missileDestroyGroundEvent;
	public event Action<Vector3, int, float, float> missileDestroyGroundWithArgsEvent;

	private List<string> groundTagList;
	private float hitDistance;
	private float influenceRadius;
	private float explosionForce;

	private int xRes;
	private int yRes;

	private int xRes_Ground;
	private int yRes_Ground;

	private int layers;
	private Terrain terrainObject;
	private TerrainData terrainData;
	private Color[] craterData;
	private Color[] craterGroundData;

	private bool isStop = false;
	private bool b_HasDestroyed = false;
	private PlayerControl_EmitParabola playerControl;

	private void Start() {
		Arrangement ();
	}

//	private void FixedUpdate() {
//		HitGround ();
//	}

	private void OnCollisionEnter(Collision col) {
		if (col.transform.tag.Equals (InteractableGroundTagList.ExplodableTagName) && !b_HasDestroyed) {
			Debug.Log ("!!!BOOM!!!");

			//	有参击打
			if (destroyWithArgs) {
				if (missileDestroyGroundWithArgsEvent != null) {
					missileDestroyGroundWithArgsEvent (craterTargetPos, craterRadius, craterPhysicsDepth, craterColorDepth);
				}
			} 

			//	无参击打
			else {
				if (missileDestroyGroundEvent != null) {
					missileDestroyGroundEvent ();
				}
			}

			b_HasDestroyed = true;	//	已完成对地面的击打

		} else {
			Debug.Log ("Collide: " + col.transform.name);
		}
	}

	private void OnDisable() {
		EventsDeRegistration ();
	}

	#region Arrangment
	private void Arrangement() {
		ReferenceSetting ();
		TerrainDataInit ();		//	For create crater and change the texture in Terrain
		HitSettings ();			//	For bomb's raycast hit and explosion
		EventsRegistration();
	}

	private void ReferenceSetting() {
		playerControl = GetComponentInParent<PlayerControl_EmitParabola> ();

		if (playerControl != null) {
			groundTagList = playerControl.groundTagList;
		}
	}

	private void TerrainDataInit() {
		terrainObject = FindObjectOfType<Terrain>();
		if (terrainObject == null) {
			return;
		}

		terrainData = terrainObject.terrainData;

		xRes = terrainData.heightmapWidth;
		yRes = terrainData.heightmapHeight;
		xRes_Ground = terrainData.alphamapWidth;
		yRes_Ground = terrainData.alphamapHeight;

		layers = terrainData.alphamapLayers;

		craterData = craterTexture.GetPixels ();
		craterGroundData = groundTexture.GetPixels ();
	}

	private void HitSettings() {
		hitDistance = TransformationForARWorld(hitDistanceValue);
	}

	private float TransformationForARWorld(float originValue) {
		if (transform.parent == null) {
			return originValue;
		}
		return originValue * transform.parent.localScale.magnitude;
	}
	#endregion

	#region Events
	private void EventsRegistration() {
		missileDestroyGroundEvent += OnMissileDestGround;
		missileDestroyGroundWithArgsEvent += OnMissileDestGroundWithArgs;
	}

	private void EventsDeRegistration() {
		missileDestroyGroundEvent -= OnMissileDestGround;
		missileDestroyGroundWithArgsEvent -= OnMissileDestGroundWithArgs;
	}
	#endregion

	#region CallBacks
	private void OnMissileDestGround() {
		CreateExplosion ();
		CreateCrater ();
		CreateCraterTexture ();

		if (playerControl != null) {
			playerControl.CallTargetChangedEvent ();
		}
	}

	private void OnMissileDestGroundWithArgs(Vector3 target, int radius, float physicsDepth, float colorDepth) {
		CreateCraterWithArgs(target, radius, physicsDepth);
		CreateCraterTextureWithArgs(target, radius, colorDepth);
		CreateExplosion();

		if (playerControl != null) {
			playerControl.CallTargetChangedEvent ();
		}
	}
	#endregion

	#region Destruction
	private void CreateExplosion() {
		if (explosion != null) {
			GameObject explosionPS = Instantiate (explosion, transform.position, Quaternion.identity);
			ParticleSystem.MainModule psMain = explosionPS.GetComponent<ParticleSystem> ().main;
			psMain.startSize = new ParticleSystem.MinMaxCurve(psMain.startSize.constant * transform.localScale.magnitude);
		}

		GetComponent<Rigidbody> ().isKinematic = false;
		GetComponent<Rigidbody> ().useGravity = true;
		GetComponent<Rigidbody> ().AddExplosionForce (rbExplosionForce, transform.position, influenceRadius);
		isStop = true;
	}

	private void CreateCrater() {
		if (terrainObject == null) {
			return;
		}
			
		float xMin = terrainObject.transform.position.x;
		float xMax = xMin + terrainData.size.x;

		float zMin = terrainObject.transform.position.z;
		float zMax = zMin + terrainData.size.z;

		int x = (int)Mathf.Lerp(0, xRes, Mathf.InverseLerp(xMin, xMax, transform.position.x));
		int z = (int)Mathf.Lerp (0, yRes, Mathf.InverseLerp(zMin, zMax, transform.position.z));

		x = Mathf.Clamp (x, craterTexture.width / 2, xRes - craterTexture.width / 2);
		z = Mathf.Clamp (z, craterTexture.height / 2, yRes - craterTexture.height / 2);

		float[,] areaT = terrainData.GetHeights (x - craterTexture.width / 2, z - craterTexture.height / 2, craterTexture.width, craterTexture.height);

		for (int i = 0; i < craterTexture.height; i++) {
			for (int j = 0; j < craterTexture.width; j++) {
				areaT [i, j] = areaT [i, j] - craterData [i * craterTexture.width + j].a * 0.01f * transform.localScale.magnitude;
			}
		}
		terrainData.SetHeights (x - craterTexture.width / 2, z - craterTexture.height / 2 , areaT);
	}

	private void CreateCraterTexture() {
		if (terrainObject == null) {
			return;
		}

		float xMin = terrainObject.transform.position.x;
		float xMax = xMin + terrainData.size.x;

		float zMin = terrainObject.transform.position.z;
		float zMax = zMin + terrainData.size.z;

		int g = (int)Mathf.Lerp (0, xRes_Ground, Mathf.InverseLerp(xMin, xMax, transform.position.x));
		int b = (int)Mathf.Lerp (0, yRes_Ground, Mathf.InverseLerp(zMin, zMax, transform.position.z));

		g = Mathf.Clamp (g, groundTexture.width / 2, xRes_Ground - groundTexture.width / 2);
		b = Mathf.Clamp (b, groundTexture.height / 2, yRes_Ground - groundTexture.height / 2);

		//	A 3D array of floats, where the 3rd dimension represents the mixing weight of each splatmap at each x, y coordinate
		float[,,] area = terrainData.GetAlphamaps (g - groundTexture.width / 2, b - groundTexture.height / 2, groundTexture.width, groundTexture.height);

		for (int i = 0; i < groundTexture.height; i++) {
			for (int j = 0; j < groundTexture.width; j++) {
				for (int k = 0; k < layers; k++) {
					if (k == 1) {
						area [i, j, k] += craterGroundData [i * groundTexture.width + j].a * transform.localScale.magnitude;
					} else {
						area[i,j,k] -= craterGroundData[i * groundTexture.width + j].a * transform.localScale.magnitude;
					}
				}
			}
		}
		terrainData.SetAlphamaps (g - groundTexture.width / 2, b - groundTexture.height / 2, area);
	}

	#region With Args
	private void CreateCraterWithArgs(Vector3 targetPosVec, int radius,  float physicsDepth) {
		if (terrainObject == null) {
			return;
		}

		float xMin = terrainObject.transform.position.x;
		float xMax = xMin + terrainData.size.x;

		float zMin = terrainObject.transform.position.z;
		float zMax = zMin + terrainData.size.z;

		Vector3 targetPos = lockDestroyTarget ? targetPosVec : transform.position;

		int x = (int)Mathf.Lerp(0, xRes, Mathf.InverseLerp(xMin, xMax, targetPos.x));
		int z = (int)Mathf.Lerp (0, yRes, Mathf.InverseLerp(zMin, zMax, targetPos.z));

		x = Mathf.Clamp (x, radius / 2, xRes - radius / 2);
		z = Mathf.Clamp (z, radius / 2, yRes - radius / 2);

		float[,] areaT = terrainData.GetHeights (x - radius / 2, z - radius / 2, radius, radius);
		for (int i = 0; i < radius; i++) {
			for (int j = 0; j < radius; j++) {
				float ratio_i = craterTexture.width / radius;
				float ratio_j = craterTexture.height / radius;
				int index_i = (int)(i * ratio_i * craterTexture.width);
				int index_j = (int)(ratio_j * j);
				areaT [i, j] = areaT [i, j] - craterData [index_i + index_j].a * 0.01f * transform.localScale.magnitude * 0.1f * physicsDepth;
			}
		}
		terrainData.SetHeights (x - radius / 2, z - radius / 2 , areaT);
	}

	private void CreateCraterTextureWithArgs(Vector3 targetPosVec, int radius, float colorDepth) {
		if (terrainObject == null) {
			return;
		}

		float xMin = terrainObject.transform.position.x;
		float xMax = xMin + terrainData.size.x;

		float zMin = terrainObject.transform.position.z;
		float zMax = zMin + terrainData.size.z;

		Vector3 targetPos = lockDestroyTarget ? targetPosVec : transform.position;

		int g = (int)Mathf.Lerp (0, xRes_Ground, Mathf.InverseLerp(xMin, xMax, targetPos.x));
		int b = (int)Mathf.Lerp (0, yRes_Ground, Mathf.InverseLerp(zMin, zMax, targetPos.z));

		g = Mathf.Clamp (g, radius / 2, xRes_Ground - radius / 2);
		b = Mathf.Clamp (b, radius / 2, yRes_Ground - radius / 2);

		//	A 3D array of floats, where the 3rd dimension represents the mixing weight of each splatmap at each x, y coordinate
		float[,,] area = terrainData.GetAlphamaps (g - radius / 2, b - radius / 2, radius, radius);

		for (int i = 0; i < radius; i++) {
			for (int j = 0; j < radius; j++) {
				float ratio_i = groundTexture.width / radius;
				float ratio_j = groundTexture.height / radius;
				int index_i = (int)(i * ratio_i * groundTexture.width);
				int index_j = (int)(ratio_j * j);

				for (int k = 0; k < layers; k++) {
					if (k == 1) {
						area [i, j, k] += craterGroundData [index_i + index_j].a * transform.localScale.magnitude * colorDepth;
					} else {
						area[i, j, k] -= craterGroundData[index_i + index_j].a * transform.localScale.magnitude * colorDepth;
					}
				}
			}
		}
		terrainData.SetAlphamaps (g - radius / 2, b - radius / 2, area);
	}
	#endregion
	#endregion

	#region If use raycast hit to detect ground hitting
	public void HitGround() {
		Ray ray = new Ray (transform.position, -1 * Vector3.up);
		Debug.DrawRay (transform.position, -1 * Vector3.up);
		RaycastHit hitInfo;

//		int layer = 1 << 9 | 1 << 4;	//	Ground Layer or Water Layer
		int layer = InteractableGroundTagList.GroundLayer | InteractableGroundTagList.TargetLayer;	//	Ground Layer or Water Layer

		if (Physics.Raycast(ray, out hitInfo, hitDistance, layer) && !isStop) {
//			if (groundTagList.Contains(hitInfo.transform.tag.ToString())) {
				if (destroyWithArgs) {
					if (missileDestroyGroundWithArgsEvent != null) {
						missileDestroyGroundWithArgsEvent (craterTargetPos, craterRadius, craterPhysicsDepth, craterColorDepth);
					}
				} else {
					if (missileDestroyGroundEvent != null) {
						missileDestroyGroundEvent ();
					}
				}
				isStop = true;
//			}
		}
	}
	#endregion
}
