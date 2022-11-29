using Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateManager : MonoBehaviour
{
    [SerializeField] List<GateScript> allGates;
    [SerializeField] List<float> gateRevealPossibility;
    List<GateScript> allActiveGates;
	GameManager gm;

	public void PrepareTheGates()
    {
		allActiveGates?.Clear();
		allActiveGates = new List<GateScript>();	

		foreach(var gate in allGates)
		{
			if(gate.gameObject.activeSelf)
				allActiveGates.Add(gate);

			gate.PassGateManagerReference(this);
		}
    }

	public void HideAllGates()
	{
		foreach(var gate in allGates)
		{
			gate.gameObject.SetActive(false);
		}
	}

	public void EnableMainGatesByIndecies(List<bool> indecies)
	{
		for(int i = 0; i < allGates.Count; i++)
		{
			allGates[i].gameObject.SetActive(indecies[i]);
		}
	}

	public void EnableMainGatesByFormula()
	{
		float randomNumber = Random.value;
		int numberOfGatesToActivate = 3;
		int numberOfActivatedGates = 0;

		HideAllGates();

		if(randomNumber <= gateRevealPossibility[0])
			numberOfGatesToActivate = 1;
		else if(randomNumber <= gateRevealPossibility[1])
			numberOfGatesToActivate = 2;

		List<bool> indeciesList = new List<bool> { false, false, false};

		for(int i = 0; i < 3; ++i)
		{
			if(numberOfActivatedGates == numberOfGatesToActivate)
				break;

			bool gateState = numberOfGatesToActivate == 3 || (i == 2 && numberOfActivatedGates <= numberOfGatesToActivate - 1) ? true : Random.value < 0.5f;

			indeciesList[i] = gateState;

			if( gateState ) numberOfActivatedGates++;

			if( i == 2 )
			{
				if( numberOfActivatedGates < numberOfGatesToActivate )
				{
					int index = indeciesList.FindIndex(_state => _state == false);
					indeciesList[index] = true;
				}
			}
		}

		for(int i = 0; i < allGates.Count; i++)
		{
			allGates[i].gameObject.SetActive(indeciesList[i]);
		}
	}

	public void InitializeGates(Appearance correctDress, List<Appearance> dressesForTheLevel, int currentLocationIndex)
    {
		if(gm == null) gm = GameManager.GetInstance();

		if(gm.GetTutorialStatus() )
		{
			dressesForTheLevel.Remove(correctDress);
			Extensions.Shuffle(dressesForTheLevel);

			int dressIndexer = 0;

			foreach(var gate in allActiveGates)
			{
				gate.InitializeGate(dressesForTheLevel[dressIndexer], false);
				dressIndexer++;

				if(allActiveGates.Count >= dressIndexer) break;
			}

			if(CustomGameEventList.TUTORIAL_TO_ENABLE == 5 || ( gm.CurrentLevelIndex == 1 && (currentLocationIndex == 3 || currentLocationIndex == 5))) 
			{
				allActiveGates[0].InitializeGate(dressesForTheLevel[0], false);
				allActiveGates[1].InitializeGate(correctDress, true);
			}
			else if(CustomGameEventList.TUTORIAL_TO_ENABLE == 7 || (gm.CurrentLevelIndex == 1 && (currentLocationIndex == 2 || currentLocationIndex == 4)))
			{
				allActiveGates[0].InitializeGate(correctDress, true);
				allActiveGates[1].InitializeGate(dressesForTheLevel[0], false);

				//CustomGameEventList.NextTutorial();
			}
			else
			{
				allActiveGates[Random.Range(0, allActiveGates.Count - 1)].InitializeGate(correctDress, true);
			}
		}
		else
		{
			bool correctDressIncluded = false;

			int numberOfDresses = dressesForTheLevel.Count;

			for(int i = 0; i < allActiveGates.Count; ++i )
			{
				var gate = allActiveGates[i];

				if(i >= numberOfDresses)
				{
					gate.gameObject.SetActive(false);
					break;
				}

				int dressIndexer = Random.Range(0, dressesForTheLevel.Count);

				if(currentLocationIndex == 1 )
				{
					dressIndexer = dressesForTheLevel.IndexOf(correctDress);
					
					if( gate.GetIndex() != 1 )
					{
						dressIndexer++;

						if(dressIndexer >= dressesForTheLevel.Count)
							dressIndexer = 0;
					}
				}

				gate.InitializeGate(dressesForTheLevel[dressIndexer], correctDress.Equals(dressesForTheLevel[dressIndexer]));

				if(correctDressIncluded == false) 
					correctDressIncluded = correctDress.Equals(dressesForTheLevel[dressIndexer]);

				dressesForTheLevel.RemoveAt(dressIndexer);

				if( i == allActiveGates.Count - 1 && !correctDressIncluded)
				{
					allActiveGates[Random.Range(0, allActiveGates.Count - 1)].InitializeGate(correctDress, true);
				}
			}
		}

		List<bool> upcomingGateInfo = new List<bool>();

		foreach(var gate in allGates)
		{
			upcomingGateInfo.Add(gate.IsThisCorrectGate);
		}

		CustomGameEventList.NextMainGatesInfo?.Invoke(upcomingGateInfo);
	}

	public void NotifyAllGatesToIgnoreCollisionWithThisPlayer(int instanceId)
	{
		foreach(var gate in allActiveGates)
		{
			gate.IgnorCollisionWithThisPlayer(instanceId);
		}
	}
	public GameObject GetTheCorrectGate()
	{
		foreach(var gate in allActiveGates)
		{
			if(gate.IsThisCorrectGate)
				return gate.gameObject;
		}

		return null;
	}
}
