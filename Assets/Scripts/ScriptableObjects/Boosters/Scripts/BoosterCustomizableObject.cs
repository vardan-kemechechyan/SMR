using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public abstract class BoosterCustomizableObject : ScriptableObject
{
	public string itemName;
	public string itemTypeToString;
	public string skinTypeToString;
	public BoosterTypes boosterType;
	public string description;

	public abstract void Apply(GameObject target);
}
