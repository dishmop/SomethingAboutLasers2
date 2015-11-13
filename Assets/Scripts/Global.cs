using UnityEngine;
using System.Collections;

public static class Global {
	
	public enum BoxType {
        None, Wall, PowerSource, BooleanSource, CosineSource, Mirror, Splitter, Combiner, Transistor, Limiter, PowerMeter,
        Button, Walkway, WalkwayButton,
	};
	public const int BoxCount = 14;
	public const float LinePower = 8.0f, SignalPower = 1.0f, UnitBeamRadius = 0.075f;
	/*public const int[] Cost = new int[TowerCount] {
		10, 100, 150, 250
	};*/
	
	public static readonly Plane groundPlane = new Plane (Vector3.up, Vector3.zero);

    public const float LevelFinishHangTime = 0.75f;
}
