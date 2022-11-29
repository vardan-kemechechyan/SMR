using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Boosters/MoneyMultiplier")]

public class MoneyMultiplier : BoosterCustomizableObject
{
	public float moneyMultiplierValue;

	public override void Apply(GameObject target)
	{
		target.GetComponent<Character>().gameManager.moneyBoosterModifier = moneyMultiplierValue;
	}
}
