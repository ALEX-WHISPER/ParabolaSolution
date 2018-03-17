using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InteractableGroundTagList {
	
	public static string[] InteractiveTagNames = new string[] 
	{
		"Ground",
		"Obstacle"
	};

	public static int TargetLayer = 1 << 11 | 1 << 4;	//	11: Target, 4: Water
	public static int GroundLayer = 1 << 9;				//	9: Ground
	public static string ExplodableTagName = "Ground";
}