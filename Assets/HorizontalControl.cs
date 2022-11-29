using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using static Configuration;

public class HorizontalControl : MonoBehaviour
{
    LaneControl lc;

    #region Touch Mechanic

    bool fingerOnScreen = false;

    #endregion

    #region Swipe Mechanic

    bool tap, swipeLeft, swipeRight, swipeUp, swipeDown;
    Vector2 swipeDelta, startTouch;

    const float DEADZONE = 10f;

    #endregion

    bool isTutorial = false;

    private void Start()
	{
        lc = gameObject.GetComponent<LaneControl>();

        isTutorial = GameManager.GetInstance().GetTutorialStatus() && GameManager.GetInstance().CurrentLevelIndex == 0;
    }

	void Update()
    {
        tap = swipeLeft = swipeRight = swipeDown = swipeUp = false;

        if(Input.touchCount > 0)
        {
            // The screen has been touched so store the touch
            Touch touch = Input.GetTouch(0);

            if(touch.phase == TouchPhase.Began)
            {
                tap = true;

                startTouch = Input.mousePosition;
			}
            else if(touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                startTouch = swipeDelta = Vector3.zero;
            }
        }
        else if(Input.touchCount == 0)
        {
            if(fingerOnScreen) fingerOnScreen = false;

            startTouch = swipeDelta = Vector2.zero;
        }

        swipeDelta = Vector2.zero;  

        if(startTouch != Vector2.zero)
        {
            if(Input.touches.Length != 0 )
            {
                swipeDelta = Input.touches[0].position - startTouch;
			}
		}

        if(swipeDelta.magnitude > DEADZONE)
        {
            float x = swipeDelta.x;
            float y = swipeDelta.y;

            if(Mathf.Abs(x) > Mathf.Abs(y))
            {
                if(x < 0)
                    swipeLeft = true;
				else
                    swipeRight = true;
			}
            else
            {
                if(y < 0)
                    swipeDown = true;
                else
                    swipeUp = true;
			}

            print(swipeDelta.magnitude);

            startTouch = swipeDelta = Vector2.zero;
		}

        if(swipeLeft)
        {
            if(!isTutorial)
                lc.ChangeLane(LaneControl.LANE_POSITION.LEFT);
            else if(CustomGameEventList.TUTORIAL_TO_ENABLE == 4 || CustomGameEventList.TUTORIAL_TO_ENABLE == 8 || CustomGameEventList.TUTORIAL_TO_ENABLE == 0)
            {
                lc.ChangeLane(LaneControl.LANE_POSITION.LEFT);

                if(CustomGameEventList.TUTORIAL_TO_ENABLE != 0)
                    CustomGameEventList.NextTutorial();
            }
		}
        else if(swipeRight)
        {
            if(!isTutorial)
                lc.ChangeLane(LaneControl.LANE_POSITION.RIGHT);
            else if(CustomGameEventList.TUTORIAL_TO_ENABLE == 6 || CustomGameEventList.TUTORIAL_TO_ENABLE == 0)
            {
                lc.ChangeLane(LaneControl.LANE_POSITION.RIGHT);
                
                if(CustomGameEventList.TUTORIAL_TO_ENABLE != 0)
                    CustomGameEventList.NextTutorial();
            }
        }
    }
}

[System.Serializable]
public enum MovementType
{
    GOTO_TOUCH,
    TOUCH_DRESS_UP,
    TOUCH_LIMITED,
    SWIPE_LANE
}
