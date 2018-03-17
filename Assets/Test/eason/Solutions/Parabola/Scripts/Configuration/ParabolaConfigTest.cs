using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParabolaConfigTest : MonoBehaviour {

	public bool isLocal;
	public Button btn_AddConfig;
	public Button btn_RemoveConfig;

	void Start() {
		AddConfig ();
		ButtonEvents();
    }

	private void ButtonEvents() {
		if (btn_AddConfig == null || btn_RemoveConfig == null) {
			return;
		}

		btn_AddConfig.onClick.AddListener (AddConfig);
		btn_RemoveConfig.onClick.AddListener (RemoveConfig);
	}

	private void AddConfig() {
		ParabolaWeaponConfig config = GetComponent<ParabolaWeaponConfig>();

		if (config == null) {
			config = gameObject.AddComponent<ParabolaWeaponConfig> ();
		}

		config.isEasonUIEnabled = true;
		config.AddParabolaConfig(gameObject, isLocal);
	}

	private void RemoveConfig() {
		ParabolaWeaponConfig config = GetComponent<ParabolaWeaponConfig> ();
		if (config == null) {
			Debug.Log ("Remove FAILED: ParabolaWeaponConfig has not been added!!!");
			return;
		} else {
			config.RemoveParabolaConfig ();
		}
	}

	/// <summary>
	/// Iteration of Add and Remove
	/// </summary>
	/// <returns>The config test.</returns>
    IEnumerator ParaConfigTest() {
        ParabolaWeaponConfig config = gameObject.AddComponent<ParabolaWeaponConfig>();

        config.AddParabolaConfig(gameObject, isLocal);

        yield return new WaitForSeconds(2f);

        for (int i = 0; i < 3; i++) {
            config.AddParabolaConfig(gameObject, isLocal);
        }

        yield return new WaitForSeconds(2f);
        config.RemoveParabolaConfig();

        yield return new WaitForSeconds(2f);
        config.AddParabolaConfig(gameObject, isLocal);
    }
}
