using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour {

    private static PoolManager _instance;

    //  int: poolID, Queue: the queue of objectInstance in this pool
    private Dictionary<int, Queue<ObjectInstance>> poolDictionary = new Dictionary<int, Queue<ObjectInstance>>();
    private GameObject poolElemPrefab;
    private int poolSizeValue;

    public static PoolManager GetInstance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<PoolManager>();
            }
            return _instance;
        }
    }

	public List<ObjectInstance> CreatePool(GameObject m_Prefab, int poolSize) {
        int poolID = m_Prefab.GetInstanceID();

        if (poolDictionary.ContainsKey(poolID)) {
            Debug.LogError("Pool_" + m_Prefab.name + " already exsists!!!");
			return null;
        }

        poolDictionary.Add(poolID, new Queue<ObjectInstance>());
        Transform elemsHolder = InspectorWrapper(m_Prefab.name);

		List<ObjectInstance> OIList = new List<ObjectInstance> ();
        for (int i = 0; i < poolSize; i++) {
            GameObject poolElemPrefab = Instantiate(m_Prefab) as GameObject;
            ObjectInstance poolElement = new ObjectInstance(poolElemPrefab);
			OIList.Add (poolElement);
            poolDictionary[poolID].Enqueue(poolElement);
            poolElement.SetXformParent(elemsHolder);
        }
		return OIList;
    }

    public void Reuse(GameObject m_Prefab, Vector3 position, Quaternion rotation) {
        if (!poolDictionary.ContainsKey(m_Prefab.GetInstanceID())) {
            Debug.LogError("No such pool!!!");
            return;
        }

        ObjectInstance lastElemForReusing = poolDictionary[m_Prefab.GetInstanceID()].Dequeue();
        poolDictionary[m_Prefab.GetInstanceID()].Enqueue(lastElemForReusing);

        lastElemForReusing.Reuse(position, rotation);
    }

    public class ObjectInstance {
		private GameObject go;
		private Transform xform;

        private bool hasObjectBehaviourComponent;
        private PoolObjectBehaviour objectBehaviourComponent;

        public ObjectInstance(GameObject obInstance) {
            go = obInstance;
            xform = go.transform;
            go.SetActive(false);

            if (go.GetComponent<PoolObjectBehaviour>() != null) {
                hasObjectBehaviourComponent = true;
                objectBehaviourComponent = go.GetComponent<PoolObjectBehaviour>();
            }
        }

        public void Reuse(Vector3 position, Quaternion rotation) {
            
            xform.position = position;
            xform.rotation = rotation;
            go.SetActive(true);

            if (hasObjectBehaviourComponent) {
                objectBehaviourComponent.OnObjectReuse();
            }
        }

        public void SetXformParent(Transform parent) {
            xform.parent = parent;
        }

		public GameObject GetGameObject {get {return this.go;}}
		public Transform GetXform {get { return this.xform;}}
    }

    private Transform InspectorWrapper(string goName) {
        GameObject poolObjHolder = new GameObject("POOL_" + goName);
        poolObjHolder.transform.parent = this.transform;
        return poolObjHolder.transform;
    }
}
