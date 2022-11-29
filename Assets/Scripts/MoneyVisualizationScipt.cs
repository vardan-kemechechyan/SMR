using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoneyVisualizationScipt : MonoBehaviour
{
	SaveSystemExperimental sse;
	GameManager gm;
	MoneyManagementSystem money_Manager;
	[SerializeField] TextMeshProUGUI moneyText;

	private void Start()
	{
		/*if(sse == null) sse = SaveSystemExperimental.GetInstance();
		if(gm == null) gm = GameManager.GetInstance();

		//if(sse.GetCurrentLevel() == 0)
		if(gm.CurrentLevelIndex + 1 == 0)
		{
			gameObject.SetActive(false);
		}
		else
		{
			gameObject.SetActive(true);
		}*/

		money_Manager = GameManager.GetInstance().GetComponent<MoneyManagementSystem>();
		UpdateMoneyCounter(money_Manager.AddMoneyUI(this));
	}

	public void UpdateMoneyCounter(float money) { moneyText.text = money.ToString(); }
}
