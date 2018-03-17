using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestObject : PoolObjectBehaviour {

    private void Awake() {
        
    }

    private void Update() {
        transform.Translate(Vector3.forward * Time.deltaTime * 20);
        transform.localScale += Vector3.one * Time.deltaTime * 3;
    }

    public override void OnObjectReuse() {
        transform.localScale = Vector3.one;
    }
}
