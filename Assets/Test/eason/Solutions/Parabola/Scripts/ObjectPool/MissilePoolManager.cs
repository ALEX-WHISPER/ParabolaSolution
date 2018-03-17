using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissilePoolManager : ShootingObjPoolManager {

    public override bool GetInputCommand() {
		return Input.GetKeyDown(KeyCode.E);
    }

	public new void ShootByCharged(float chargedValue) {
//		base.ShootByCharged (chargedValue);
		base.ShootByInput();
	}
}
