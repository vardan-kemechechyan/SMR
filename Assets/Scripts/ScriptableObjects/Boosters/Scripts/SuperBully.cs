using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Boosters/Bully")]

public class SuperBully : BoosterCustomizableObject
{
	public int interactionCount;

	public override void Apply(GameObject target)
	{
		target.GetComponent<Character>().bullyInteractionCount = interactionCount;
	}
}
