using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Boosters/SpeedUp")]

public class SpeedPowerUp : BoosterCustomizableObject
{
	public float speedUpdateValue;

	public override void Apply(GameObject target)
	{
		target.GetComponent<Character>().buffSpeedModifier += speedUpdateValue;
	}
}
