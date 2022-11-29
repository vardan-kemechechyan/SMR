using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Boosters/GateHighLight")]

public class GateHighLight : BoosterCustomizableObject
{
	public int gateHighlightCount;

	public override void Apply(GameObject target)
	{
		target.GetComponent<Character>().gateHighlightCount = gateHighlightCount;
	}
}
