using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class CharacterCollisionManager : MonoBehaviour
{
	[SerializeField] CollisionAnimation[] coliisionAnimations;
	[HideInInspector] public Animator animator;

	LaneControl lc;
	[HideInInspector] public Character mychar;
	List<CharacterCollisionManager> collidedPlayers = new List<CharacterCollisionManager>();

	[HideInInspector] public List<Appearance> areaAppearance;

	[HideInInspector] public CollisionRoles role;
	[HideInInspector] public bool collisionInProgress = false;

	bool canRegisterCollisions = true;
	
	public void Init( Character _char )
	{
		areaAppearance.Clear();

		collidedPlayers.Clear();

		canRegisterCollisions = true;
		collisionInProgress = false;

		role = CollisionRoles.NEUTRAL;

		if(lc == null)
			lc = GetComponent<LaneControl>();

		if(mychar == null)
			mychar = _char;

	}

	public void StopRegisteringColiisions()
	{
		canRegisterCollisions = false;
		lc.GoBackToOriginalLane();
	}

	public void AddCollidedOpponent(CharacterCollisionManager _opponent)
	{
		if(!collidedPlayers.Contains(_opponent))
			collidedPlayers.Add(_opponent);
	}

	public void DefineRoles(CharacterCollisionManager _opponent1, CharacterCollisionManager _opponent2)
	{
		_opponent1.collisionInProgress = true;
		_opponent2.collisionInProgress = true;

		LaneControl.ManeuverInformation opponent1Manuever = _opponent1.GetManeuverInformation();
		LaneControl.ManeuverInformation opponent2Manuever = _opponent2.GetManeuverInformation();

		if(_opponent1.mychar.control == Control.Player || _opponent2.mychar.control == Control.Player)
		{
			var player = _opponent1.mychar.control == Control.Player ? _opponent1 : _opponent2;

			if(player.mychar.bullyInteractionCount > 0)
			{
				_opponent1.role = CollisionRoles.VICTIM;
				_opponent2.role = CollisionRoles.VICTIM;

				player.role = CollisionRoles.BULLY;

				/*_opponent1.collisionInProgress = false;
				_opponent2.collisionInProgress = false;*/

				player.mychar.bullyInteractionCount--;

				return;
			}
		}

		if(_opponent1.role == CollisionRoles.NEUTRAL && _opponent2.role == CollisionRoles.NEUTRAL)
		{
			if(opponent1Manuever.startedManuever && !opponent2Manuever.startedManuever)
			{
				_opponent1.role = CollisionRoles.BULLY;
				_opponent2.role = CollisionRoles.VICTIM;
			}
			else if(!opponent1Manuever.startedManuever && opponent2Manuever.startedManuever)
			{
				_opponent1.role = CollisionRoles.VICTIM;
				_opponent2.role = CollisionRoles.BULLY;
			}
			else if(opponent1Manuever.startedManuever && opponent2Manuever.startedManuever)
			{
				if(opponent1Manuever.maneuverStartTime < opponent2Manuever.maneuverStartTime)
				{
					_opponent1.role = CollisionRoles.BULLY;
					_opponent2.role = CollisionRoles.VICTIM;
				}
				else if(opponent1Manuever.maneuverStartTime > opponent2Manuever.maneuverStartTime)
				{
					_opponent1.role = CollisionRoles.VICTIM;
					_opponent2.role = CollisionRoles.BULLY;
				}
				else
				{
					if(_opponent1.gameObject.name == "Player")
					{
						_opponent1.role = CollisionRoles.BULLY;
						_opponent2.role = CollisionRoles.VICTIM;
					}
					else if(_opponent2.gameObject.name == "Player")
					{
						_opponent1.role = CollisionRoles.VICTIM;
						_opponent2.role = CollisionRoles.BULLY;
					}
					else
					{
						if(Random.value < .5f)
						{
							_opponent1.role = CollisionRoles.BULLY;
							_opponent2.role = CollisionRoles.VICTIM;
						}
						else
						{
							_opponent1.role = CollisionRoles.VICTIM;
							_opponent2.role = CollisionRoles.BULLY;
						}
					}
				}
			}
			else if(!opponent1Manuever.startedManuever && !opponent2Manuever.startedManuever)
			{
				if(_opponent1.gameObject.transform.position.z <= _opponent2.gameObject.transform.position.z)
				{
					_opponent1.role = CollisionRoles.BULLY;
					_opponent2.role = CollisionRoles.VICTIM;
				}
				else
				{
					_opponent1.role = CollisionRoles.VICTIM;
					_opponent2.role = CollisionRoles.BULLY;
				}
			}
		}
		else if(_opponent1.role == CollisionRoles.VICTIM && _opponent2.role == CollisionRoles.VICTIM )
		{
			if(_opponent1.gameObject.transform.position.z <= _opponent2.gameObject.transform.position.z)
			{
				_opponent1.role = CollisionRoles.BULLY;
				_opponent2.role = CollisionRoles.VICTIM;
			}
			else
			{
				_opponent1.role = CollisionRoles.VICTIM;
				_opponent2.role = CollisionRoles.BULLY;
			}
		}
		else if(_opponent1.role == CollisionRoles.VICTIM || _opponent2.role == CollisionRoles.VICTIM)
		{
			var bully = _opponent1.role == CollisionRoles.VICTIM ? _opponent2 : _opponent1;

			bully.role = CollisionRoles.BULLY;
		}
		else
		{
			if(_opponent1.gameObject.transform.position.z <= _opponent2.gameObject.transform.position.z)
			{
				_opponent1.role = CollisionRoles.BULLY;
				_opponent2.role = CollisionRoles.VICTIM;
			}
			else
			{
				_opponent1.role = CollisionRoles.VICTIM;
				_opponent2.role = CollisionRoles.BULLY;
			}
		}

		/*_opponent1.collisionInProgress = false;
		_opponent2.collisionInProgress = false;*/
	}

	public void RemoveCollidedOpponent(CharacterCollisionManager _opponent)
	{
		if(collidedPlayers.Contains(_opponent))
			collidedPlayers.Remove(_opponent);

		if(collidedPlayers.Count == 0) collisionInProgress = false;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if(!(collision.gameObject.layer == 7 || collision.gameObject.layer == 8)) return;

		if( canRegisterCollisions && !collisionInProgress)
		{
			var opponent = collision.gameObject.GetComponent<CharacterCollisionManager>();

			AddCollidedOpponent(opponent);
			opponent.AddCollidedOpponent(this);

			//if( !collisionInProgress )
			//{
				DefineRoles(this, opponent);
			//}

			PlayCollisionAnimation();
			opponent.PlayCollisionAnimation();
		}

		collisionInProgress = false;
	}

	public void PlayCollisionAnimation()
	{
		StopCoroutine(Play());
		StartCoroutine(Play());
	}

	IEnumerator Play()
	{
		PlayAnimation();

		if(role == CollisionRoles.BULLY)
		{
			mychar.EnablePositiveSmiles();
			role = CollisionRoles.NEUTRAL;
		}
		else
		{
			lc.supressed = true;
			mychar.collisionModifier = -0.5f;

			lc.CancelTheManeuver();

			mychar.EnableNegativeSmiles();
			mychar.DisableWind();
		}

		var maneuver = GetManeuverInformation();

		maneuver.startedManuever = false;
		maneuver.maneuverStartTime = 0f;

		yield return new WaitForSeconds(0.5f);

		role = CollisionRoles.NEUTRAL;

		lc.supressed = false;

		mychar.collisionModifier = 1f;

		mychar.UpdateAnimation();
	}

	public LaneControl.ManeuverInformation GetManeuverInformation()
	{
		return lc.maneuverInformation;
	}

	void PlayAnimation()
	{
		int _animationIndex = 0;

		if(areaAppearance[0] == Appearance.SWIMMING)	_animationIndex = 1;
		else											_animationIndex = 0;

		animator.Play(role == CollisionRoles.BULLY ?
			coliisionAnimations[_animationIndex].bully[Random.Range(0, coliisionAnimations[_animationIndex].bully.Length)].name :
			coliisionAnimations[_animationIndex].victim[Random.Range(0, coliisionAnimations[_animationIndex].victim.Length)].name, 0, 0);
	}

	private void OnCollisionExit(Collision collision)
	{
		if(!(collision.gameObject.layer == 7 || collision.gameObject.layer == 8)) return;

		var opponent = collision.gameObject.GetComponent<CharacterCollisionManager>();

		RemoveCollidedOpponent(opponent);
		opponent.RemoveCollidedOpponent(this);
	}

	[System.Serializable]
	public class CollisionAnimation
	{
		public AnimationClip[] bully;
		public AnimationClip[] victim;
	}
}
