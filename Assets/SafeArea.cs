using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeArea : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		CustomGameEventList.CharacterInSafeArea(true, other.gameObject.GetInstanceID());
	}

	private void OnTriggerExit(Collider other)
	{
		CustomGameEventList.CharacterInSafeArea(false, other.gameObject.GetInstanceID());
	}
}
