using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Boosters/Compass")]

public class ShowCompass : BoosterCustomizableObject
{
	public override void Apply(GameObject target)
	{
		target.GetComponent<Character>().EnableCompass();
	}
}
