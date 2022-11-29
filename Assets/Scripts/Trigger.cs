using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class Trigger : MonoBehaviour
{
    [SerializeField] bool stopLine;
    [SerializeField] bool finishLine;
    [SerializeField] int stopLineID;
    [SerializeField] List<Appearance> appearance;
    [SerializeField] Location attachedLocation;

    [SerializeField] ParticleSystem[] RainbowGun;
    [SerializeField] ParticleSystem LocationBasedConfetti;

    GameManager gm;
    bool isTutorial = false;

	private void Start()
	{
        if(gm == null) gm = GameManager.GetInstance();
    }

	void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            if(gm == null)
                gm = GameManager.GetInstance();

			Character charScript = other.GetComponent<Character>();

            if(finishLine)
            {
                charScript.CharacterCrossedTheFinishLine();
                //CustomGameEventList.CharacterCrossedTheFinishLine.Invoke(charScript.control, charScript.gameObject.GetInstanceID());
            }

            if(finishLine && charScript.control == Control.Player)
            {
                //if(gm == null) gm = GameManager.GetInstance();

                if(gm.IsPlayerFirstToFinish() && RainbowGun.Length != 0)
                    foreach(var gun in RainbowGun)
                        gun.Play();

                if(gm.IsPlayerFirstToFinish() && LocationBasedConfetti != null) 
                    LocationBasedConfetti.Play();

                CustomGameEventList.OnChangeGameState.Invoke(GameState.FinishLine);

                if(attachedLocation == null)
                    attachedLocation = transform.parent.GetComponent<Location>();

                appearance = attachedLocation?.GetLocationDressTypes();

                if(finishLine)
                    appearance = new List<Appearance>() { Appearance.None };

                CustomGameEventList.OnEnterArea.Invoke(appearance, transform.parent.GetComponent<TutorialScript>()?.ReturnStartEndPositions(), charScript.gameObject.GetInstanceID(), attachedLocation == null ? -2 : attachedLocation.LocationIndex, attachedLocation.GetLocationType());
            }
            else if(stopLine) 
                CustomGameEventList.OnEnterStopLine.Invoke(stopLineID, charScript.gameObject.GetInstanceID());
            else
            {
                if(attachedLocation == null)
                    attachedLocation = transform.parent.GetComponent<Location>();
                
                if( attachedLocation != null )
                    appearance = attachedLocation.GetLocationDressTypes();
                
                if(finishLine)
                    appearance = new List<Appearance>() { Appearance.None };

                if( !stopLine && !finishLine )
                {
                    attachedLocation.RevealTheNextMainGates();

                    //if(CustomGameEventList.TUTORIAL_TO_ENABLE == 9 && appearance[0] == Appearance.OFFICE && gm.GetTutorialStatus())
                        //CustomGameEventList.NextTutorial();
                }

                CustomGameEventList.OnEnterArea.Invoke(appearance, transform.parent.GetComponent<TutorialScript>()?.ReturnStartEndPositions(), charScript.gameObject.GetInstanceID(), attachedLocation == null ? -2 : attachedLocation.LocationIndex, attachedLocation.GetLocationType());
            }
        }
    }
}
