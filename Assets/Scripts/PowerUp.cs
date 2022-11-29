using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
	public BoosterCustomizableObject myPowerUp;

	public void ActivatePowerUp(GameObject target)
	{
		myPowerUp.Apply(target);
	}
}
