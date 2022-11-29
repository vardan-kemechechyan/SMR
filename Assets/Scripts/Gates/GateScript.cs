using Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateScript : MonoBehaviour, ISubscribeUnsubscribe
{
    [SerializeField] GameObject hintArrow;
    [SerializeField] int gateIndex;
	[SerializeField] Appearance gateAppearance;
	[SerializeField] SpriteRenderer gateSprite;
	[SerializeField] GateManager myGateManager;
	[SerializeField] GameObject myParticleSystem;

	List<int> ObjectsToIgnoreCollisionWith = new List<int>();

	public bool IsThisCorrectGate = false;


	public int SubscribedTimes { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

	public void InitializeGate( Appearance _gateAppearance, bool _thisIsCorrectGate )
	{
		UnSubscribe();

		myParticleSystem.SetActive(false);

		ObjectsToIgnoreCollisionWith.Clear();

		gateAppearance = _gateAppearance;
		gateSprite.sprite = Resources.Load<Sprite>("Card Button Dress Images/" + _gateAppearance.ToString());

		IsThisCorrectGate = _thisIsCorrectGate;

		HintArrowManagement(false);

		Subscribe();
	}

	public int GetIndex() { return gateIndex; }

	public void GateAppear()
	{
		gameObject.SetActive(true);
	}

	public void GateDisappear()
	{
		gameObject.SetActive(false);
	}

	public void EnableParticleSystem()
	{
		myParticleSystem.SetActive(true);
	}

	public void PassGateManagerReference(GateManager _gateManager)
	{
		myGateManager = _gateManager;
	}

	private void OnTriggerEnter(Collider other)
	{
		Character charScript = other.GetComponent<Character>();

		if(ObjectsToIgnoreCollisionWith.Contains(charScript.gameObject.GetInstanceID())) return;

		charScript.PlayerChangedDress(gateAppearance, IsThisCorrectGate);

		if(CustomGameEventList.TUTORIAL_TO_ENABLE == 9)
			CustomGameEventList.NextTutorial();

		myGateManager.NotifyAllGatesToIgnoreCollisionWithThisPlayer(charScript.gameObject.GetInstanceID());
	}

	public void IgnorCollisionWithThisPlayer(int instanceId)
	{
		if(!ObjectsToIgnoreCollisionWith.Contains(instanceId))
			ObjectsToIgnoreCollisionWith.Add(instanceId);
	}

	public void OnChangeGameState( GameState gameSate )
	{
		
	}

	public void HintArrowManagement( int enableHintIndex)
	{
		if( enableHintIndex == 1 || enableHintIndex == 3)
			hintArrow.SetActive(true);
		if(enableHintIndex == 2)
			hintArrow.SetActive(false);
	}

	public void HintArrowManagement(bool enableHintIndex)
	{
		hintArrow.SetActive(enableHintIndex);
	}

	public void Subscribe()
	{
		CustomGameEventList.OnChangeGameState += OnChangeGameState;
		//CustomGameEventList.TurnOnHint += HintArrowManagement;

		/*if(isTutorial)
		{
			CustomGameEventList.TutorialCheckpoinReached += HintArrowManagement;
		}*/
	}

	public void UnSubscribe()
	{
		CustomGameEventList.OnChangeGameState -= OnChangeGameState;
		//CustomGameEventList.TurnOnHint -= HintArrowManagement;
		//CustomGameEventList.TutorialCheckpoinReached -= HintArrowManagement;
	}
}
