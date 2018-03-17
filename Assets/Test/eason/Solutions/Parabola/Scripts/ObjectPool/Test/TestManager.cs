using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestManager : MonoBehaviour {
    public GameObject m_PoolPrefab;
    public int poolSize;

    private void Start() {
        PoolManager.GetInstance.CreatePool(m_PoolPrefab, poolSize);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Q)) {
            PoolManager.GetInstance.Reuse(m_PoolPrefab, Vector3.zero, Quaternion.identity);
        }
    }
}
