using Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AILaneBehavior : MonoBehaviour, ISubscribeUnsubscribe
{
    LaneControl lc;

	float aiMoveDelay;
    float BasicDecisionTime;
    AIDifficulty ai_difficulty;
    AI_behavior ai;

    List<bool> nextGateInfo = new List<bool>();

    void Start()
    {
        UnSubscribe();

        lc = gameObject.GetComponent<LaneControl>();

        Subscribe();
    }

	private void OnDisable()
	{
        UnSubscribe();
    }

	private void OnDestroy()
	{
        UnSubscribe();
    }

    public void SetAI(AI_behavior _ai_from_config)
    {
        ai = _ai_from_config;
        ai_difficulty = ai.difficulty_type;
        BasicDecisionTime = ai.lane_pick_delay[1];
    }

    public int SubscribedTimes { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

	IEnumerator AISelectLane()
    {
        var waitTime = new WaitForSeconds(BasicDecisionTime);

        aiMoveDelay = Random.Range( ai.lane_pick_delay[0], ai.lane_pick_delay[1]);

        yield return new WaitForSeconds(aiMoveDelay);

        bool isCorrect = Random.value > ai.wrong_lane_pick_percent;

        int currentLanePosition = lc.GetLanePosition();
        int correctLanePosition = currentLanePosition;
        List<int> wrongLanes = new List<int>();

        for(int i = 0; i < nextGateInfo.Count; i++)
        {
            if(nextGateInfo[i])
            {
                switch(i)
                {
                    case 0: correctLanePosition = -1; break;
                    case 1: correctLanePosition = 0; break;
                    case 2: correctLanePosition = 1; break;
                }
            }
            else
            {
                switch(i)
                {
                    case 0: wrongLanes.Add(-1); break;
                    case 1: wrongLanes.Add(0); break;
                    case 2: wrongLanes.Add(1); break;
                }
            }
        }

        if(isCorrect)
        {
            while(currentLanePosition != correctLanePosition)
            {
                if(currentLanePosition < correctLanePosition)
                    lc.ChangeLane(LaneControl.LANE_POSITION.RIGHT);
                else if(currentLanePosition > correctLanePosition)
                    lc.ChangeLane(LaneControl.LANE_POSITION.LEFT);

                currentLanePosition = lc.GetLanePosition();

                yield return waitTime;
			}

            yield break;
        }
        else
        {
            lc.ChangeLane((LaneControl.LANE_POSITION) wrongLanes[Random.Range(0, wrongLanes.Count)]);

            currentLanePosition = lc.GetLanePosition();

            yield return waitTime;

            while(currentLanePosition != correctLanePosition)
            {
                if(currentLanePosition < correctLanePosition)
                    lc.ChangeLane(LaneControl.LANE_POSITION.RIGHT);
                else if(currentLanePosition > correctLanePosition)
                    lc.ChangeLane(LaneControl.LANE_POSITION.LEFT);

                currentLanePosition = lc.GetLanePosition();

                yield return waitTime;
            }

            yield break;
        }
    }

    void GetNextGateInfo(List<bool> _gateInfo)
    {
        nextGateInfo?.Clear();

        nextGateInfo = _gateInfo;

        StopCoroutine("AISelectLane");
        StartCoroutine("AISelectLane");
    }

    public void Subscribe()
	{
        CustomGameEventList.NextMainGatesInfo += GetNextGateInfo;
    }

	public void UnSubscribe()
	{
        CustomGameEventList.NextMainGatesInfo -= GetNextGateInfo;
    }
}
