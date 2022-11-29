using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneControl : MonoBehaviour, ISubscribeUnsubscribe
{
    [System.Serializable] public enum LANE_POSITION
    {
        LEFT = -1,
        CENTER = 0,
        RIGHT = 1
	}

    GameManager gm;

    public LANE_POSITION startLanePosition;
    LANE_POSITION currentLanePosition;
    LANE_POSITION previousLane;

    [SerializeField] float minimumSpeedMultiplier;
    [SerializeField] float mamimumSpeedMultiplier;
    
    float moveSpeed;
    bool inSafeArea = false;
    [HideInInspector] public bool supressed = false;

    public ManeuverInformation maneuverInformation;

    void Start()
    {
        UnSubscribe();

        if(gm == null)
            gm = GameManager.GetInstance();

        Subscribe();
    }

    private void OnDestroy()
    {
        UnSubscribe();
    }

    private void OnDisable()
    {
        UnSubscribe();
    }

	private void Update()
	{
        transform.position = Vector3.Lerp(transform.position, new Vector3((float)currentLanePosition, transform.position.y, transform.position.z), moveSpeed * Time.deltaTime);

        if(Mathf.Abs(transform.position.x - (float)currentLanePosition) <= 0.01f)
            transform.position = new Vector3((float)currentLanePosition, transform.position.y, transform.position.z);

        if( maneuverInformation.startedManuever )
        {
            if( transform.position.x == (float) currentLanePosition )
            {
                maneuverInformation.startedManuever = false;
                maneuverInformation.maneuverStartTime = 0;
                previousLane = currentLanePosition;
            }
		}
    }

	public void Init()
	{
        if(gm == null)
            gm = GameManager.GetInstance();

        inSafeArea = false;
        supressed = false;
        moveSpeed = gm.Config.characterSpeeds.default_speed * minimumSpeedMultiplier;
        previousLane = currentLanePosition = startLanePosition;

        maneuverInformation = new ManeuverInformation(false, 0f);
    }

    public void CancelTheManeuver()
    {
        currentLanePosition = previousLane;
    }

	public void ChangeLane( LANE_POSITION moveToNewPosition )
    {
        if( inSafeArea || supressed ) return;

        previousLane = currentLanePosition;

        maneuverInformation.startedManuever = true;
        maneuverInformation.maneuverStartTime = Time.time;

        switch(moveToNewPosition)
        {
            case LANE_POSITION.LEFT:
                    currentLanePosition--;
                if((int)currentLanePosition <= -1) 
                    currentLanePosition = (LANE_POSITION)(-1);
                break;
            case LANE_POSITION.RIGHT:
                currentLanePosition++;
                if((int)currentLanePosition >= 1)
                    currentLanePosition = (LANE_POSITION)1;
                break;
		}
    }

    public void ChangeTheSpeed(bool _changeOutcome)
    {
        moveSpeed = _changeOutcome ? gm.Config.characterSpeeds.max_speed * mamimumSpeedMultiplier : gm.Config.characterSpeeds.default_speed * minimumSpeedMultiplier;
    }

    public void SafeAreaStatusUpdate(bool _status, int id) 
    {
        if(id == gameObject.GetInstanceID() && gm.CurrentLevelIndex != 0)
            inSafeArea = _status;
    }

    public void GoBackToOriginalLane()
    {
        currentLanePosition = startLanePosition;
	}

    public int GetLanePosition() { return (int) currentLanePosition; }

    public void Subscribe()
    {
        CustomGameEventList.DressChangeCorrectState += ChangeTheSpeed;
        CustomGameEventList.CharacterInSafeArea += SafeAreaStatusUpdate;
    }

    public void UnSubscribe()
    {
        CustomGameEventList.DressChangeCorrectState -= ChangeTheSpeed;
        CustomGameEventList.CharacterInSafeArea -= SafeAreaStatusUpdate;
    }

    [System.Serializable]
    public class ManeuverInformation
    {
        public bool startedManuever;
        public float maneuverStartTime;

        public ManeuverInformation(bool startedManuever, float maneuverStartTime)
		{
			this.startedManuever = startedManuever;
			this.maneuverStartTime = maneuverStartTime;
		}
	}
}
