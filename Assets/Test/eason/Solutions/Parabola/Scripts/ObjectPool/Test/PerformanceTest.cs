using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformanceTest : MonoBehaviour {

    public GameObject m_Prefab;
    public int iterations;
    public bool usePool;
    public int poolSize;

    private void Start() {
        if (usePool) {
            PoolTest();
        } else {
            RegularTest();
        }
    }

    private void RegularTest() {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        for (int i = 0; i < iterations; i++) {
            Destroy(Instantiate(m_Prefab, Vector3.zero, Quaternion.identity) as GameObject);
        }

        sw.Stop();

        Debug.Log(string.Format("Regular instantiate / Destroy {0} objects takes {1} ms", iterations, sw.ElapsedMilliseconds));
    }

    private void PoolTest() {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        PoolManager.GetInstance.CreatePool(m_Prefab, poolSize);
        for (int i = 0; i < iterations; i++) {
            PoolManager.GetInstance.Reuse(m_Prefab, Vector3.zero, Quaternion.identity);
        }

        sw.Stop();

        Debug.Log(string.Format("Pool reuse {0} objects takes {1} ms", iterations, sw.ElapsedMilliseconds));
    }
}
