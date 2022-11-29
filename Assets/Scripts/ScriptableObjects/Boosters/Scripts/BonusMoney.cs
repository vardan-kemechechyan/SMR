using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Boosters/AdditionalMoney")]

public class BonusMoney : BoosterCustomizableObject
{
	public float bonusAmount;
	public override void Apply(GameObject target)
	{
		target.GetComponent<Character>().gameManager.bonusMoney = bonusAmount;
	}
}
