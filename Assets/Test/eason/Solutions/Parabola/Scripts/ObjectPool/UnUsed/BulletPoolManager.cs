using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPoolManager : ShootingObjPoolManager {

    public override bool GetInputCommand() {
        return true;
    }
}
