using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
	[SerializeField] GameObject defaultButton;
	[SerializeField] GameObject extendedButton;
	SaveSystemExperimental sse;
	GameManager gm;

	private void OnEnable()
	{
		/*if(sse == null) sse = SaveSystemExperimental.GetInstance();
		if(gm == null) gm = GameManager.GetInstance();

		if(gm.CurrentLevelIndex == 0)
		{
			gameObject.SetActive(false);
		}
		else
		{
			gameObject.SetActive(true);
		}*/

		defaultButton.SetActive(true);
		extendedButton.SetActive(false);
	}

	/*private void Start()
	{
		if(sse == null) sse = SaveSystemExperimental.GetInstance();
		if(gm == null) gm = GameManager.GetInstance(); 

		//if(sse.GetCurrentLevel() == 0)
		if(gm.CurrentLevelIndex == 0)
		{
			gameObject.SetActive(false);
		}
		else
		{
			gameObject.SetActive(true);
		}
	}*/
}
