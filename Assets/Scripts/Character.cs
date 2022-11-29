using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using System;
using System.Linq;
using DG.Tweening;

public class Character : MonoBehaviour, ISubscribeUnsubscribe
{
    [HideInInspector] public GameManager gameManager;
    SaveSystemExperimental sse;
    SoundManager sm;
    CharacterCollisionManager charCollisionManager;
    LaneControl charLaneControl;

    [HideInInspector] public GameScreen ui_GameScreen;

    public Control control;
    public AIDifficulty ai_difficulty;
    public AI_behavior ai;

    public float BasicDecisionTime;

    [SerializeField] Animator animator;

    [SerializeField] LocationManager loc_Manager;

    Dictionary<Appearance, GameObject> PrefabsForTheLevel = new Dictionary<Appearance, GameObject>();
    GameObject CurrentActivePrefab;
    [SerializeField] GameObject modelHolder;

    [SerializeField] List<Appearance> areaAppearance;

    Appearance tempAppearance;
    public Appearance appearance;

    [SerializeField] List<Appearance> LevelAppearance;

    MovementSpeedInfo speedInfo;

    public Vector3 finish_values;

    [SerializeField] float moveSpeed;
    [HideInInspector] public float collisionModifier;

    public bool move;
    bool tempMove;
    bool crossedTheFinishLine = false;
    public bool CrossedTheFinishLine
    {
        get { return crossedTheFinishLine; }
        set { crossedTheFinishLine = value; }
    }
    bool stoppedAtTheStopLine = false;
    public bool StoppedAtTheStopLine
    {
        get { return stoppedAtTheStopLine; }
        set { stoppedAtTheStopLine = value; }
    }

    float aiMoveDelay;

    bool correctDressEnabled = true;
    float successRateValue = 0f;

    [SerializeField] float timeAfterCrossingLocation = 0f;

    List<float> TutorialEnablePathRatio = new List<float>();
    int tutorialToEnable = -1;
    bool isTutorial = false;
    List<Vector3> locationStartEndPositions = new List<Vector3>();

    bool levelSkipped;

    [SerializeField] ParticleSystem goodParticle;
    [SerializeField] ParticleSystem badParticle;

    public GameObject HintArrow;
    public GameObject GatesToLookAt;

    private void OnDisable() { UnSubscribe(); }

	private void OnDestroy() { UnSubscribe(); }

    float timeInLevel = 0f;

    EmojiController myEmojies;
    WindController myWind;

    #region BOOSTERS
    public float buffSpeedModifier = 1f;
    public int gateHighlightCount = 0;
    public int bullyInteractionCount = 0;
    #endregion

    DressCrossState dressChangeState;
    int levelIndex = 0;
    int locationIndex = 0;
    string locationName = "";

    public void OnStart( List<(string, string)> boosters = null )
    {
        //appearance = Appearance.CASUAL;

        if(control == Control.Player && boosters != null )
        {
            GatesToLookAt = loc_Manager.GetCorrectGates(0);

            foreach(var booster in boosters)
                DressUpBoosters((booster.Item1, booster.Item2));

            HighlightTheCorrectDoors();

        }

        UpdateAppearance();

        var pos = transform.position;
        transform.position = new Vector3(pos.x, pos.y, 0);

        timeInLevel = 0f;

        move = true;

        levelSkipped = false;

        if(control == Control.Player)
        {
            if(gameManager.GetTutorialStatus() && gameManager.CurrentLevelIndex == 0)
                GetComponent<HorizontalControl>().enabled = false;
            else
                GetComponent<HorizontalControl>().enabled = true;
        }
    }
    private void UpdateAppearance(bool _checkAnimation = false) 
    {
        UpdateReferences(appearance );

        UpdateAnimation(_checkAnimation);

        if ((areaAppearance.Contains(Appearance.None) || areaAppearance.Count == 0) && !_checkAnimation) 
        {
            moveSpeed = speedInfo.default_speed;
            correctDressEnabled = true;
        }
        else if (areaAppearance.Contains(appearance) || _checkAnimation)
        {
            moveSpeed = speedInfo.max_speed;
            correctDressEnabled = true;

            if(control == Control.Player && gameManager.levelStarted && CustomGameEventList.DressChangeCorrectState != null )
                CustomGameEventList.DressChangeCorrectState(true);
        }
        else 
        {
            moveSpeed = speedInfo.min_speed;
            correctDressEnabled = false;

            if(control == Control.Player && CustomGameEventList.DressChangeCorrectState != null)
                CustomGameEventList.DressChangeCorrectState(false);
        }
    }
    public void UpdateAnimation(bool _checkAnimation = false) 
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (move)
        {
            int _animationIndex = 0;

            if( areaAppearance == null || areaAppearance.Count == 0 /*|| areaAppearance[0] == Appearance.None*/) _animationIndex = 0;
            else if(areaAppearance[0] == Appearance.SWIMMING)   _animationIndex = 2;
            else /*if(areaAppearance[0] != Appearance.None ) */ _animationIndex = 1;

            if(_checkAnimation)
            {
                animator.Play( gameManager.Config.appearanceAnimations[_animationIndex].correct[UnityEngine.Random.Range(0, gameManager.Config.appearanceAnimations[_animationIndex].correct.Length)].name);
            }
            else
            {
                animator.Play( areaAppearance.Contains(appearance) || areaAppearance[0] == Appearance.None ?
                gameManager.Config.appearanceAnimations[_animationIndex].correct[UnityEngine.Random.Range(0, gameManager.Config.appearanceAnimations[_animationIndex].correct.Length)].name :
                gameManager.Config.appearanceAnimations[_animationIndex].wrong[UnityEngine.Random.Range(0, gameManager.Config.appearanceAnimations[_animationIndex].wrong.Length)].name, 0, 0);
			}
        }
        else 
        {
            if( !StoppedAtTheStopLine )
            {
                //animator.Play("Idle", 0, -1);
                animator.Play(gameManager.Config.appearanceAnimations[0].correct[UnityEngine.Random.Range(0, gameManager.Config.appearanceAnimations[0].correct.Length)].name, 0, 0);
    		}
        }
    }

    public void Init(GameManager gameManager)
    {
        this.gameManager = gameManager;

        if(control == Control.Player)
        {
            if(myEmojies == null)
                myEmojies = GetComponent<EmojiController>();

            if(myWind == null)
                myWind = GetComponent<WindController>();

            myEmojies.HideEmojis();
            myWind.DisableAllWinds();
        }

        dressChangeState = DressCrossState.NOCHANGE;

        levelIndex = gameManager.CurrentLevelIndex;

        if(sm == null) sm = SoundManager.GetInstance();

        TutorialEnablePathRatio = gameManager.Config.TutorialEnablePathRatio;

        Subscribe();

        correctDressEnabled = true;

        successRateValue = 0f;

        speedInfo = gameManager.GetSpeedInfo();
        
        moveSpeed = speedInfo.default_speed;

        collisionModifier = 1f;

        if(HintArrow != null)
            HintArrow.SetActive(false);

        BasicDecisionTime = gameManager.GetBasicDecisionTime();

        isTutorial = gameManager.GetTutorialStatus() && gameManager.CurrentLevelIndex == 0;

        tutorialToEnable = isTutorial  ? - 1 : 5;

        if(LevelAppearance == null || LevelAppearance.Count == 0)
            LevelAppearance = new List<Appearance>(loc_Manager.ReturnLevelAppearanceList());

        foreach(var pref in PrefabsForTheLevel)
            Destroy(pref.Value);

        PrefabsForTheLevel.Clear();

        LoadApearancesForTheLevel();

        UpdateAppearance();

        layerId = control == Control.Player ? 7 : 8;

        if(gameManager.theGameIsFinished)
            gameObject.SetActive(false);

        if(charCollisionManager == null )
            charCollisionManager = GetComponent<CharacterCollisionManager>();

        if(charLaneControl == null)
            charLaneControl = GetComponent<LaneControl>();

        charCollisionManager.Init(this);
        charLaneControl.Init();
        charCollisionManager.animator = animator;
    }

    public void UpdateAppearanceAndAccessories()
    {
		foreach(var pref in PrefabsForTheLevel)
            Destroy(pref.Value);

        PrefabsForTheLevel.Clear();

        LoadApearancesForTheLevel();

        UpdateAppearance();
    }


    //TODO: NEED TO OPTIMIZE: do pooling
    void LoadApearancesForTheLevel()
    {
        if(sse == null) sse = SaveSystemExperimental.GetInstance();

        Dictionary<Appearance, List<string>> CostumeNamesForTheLevel = sse.ReturnNamesOfSelectedCostumes(LevelAppearance, control);

		foreach(var costName in CostumeNamesForTheLevel)
		{
            GameObject GO;

            if(control == Control.AI)
                GO = Instantiate(Resources.Load<GameObject>("Costume/" + costName.Value[UnityEngine.Random.Range(0, costName.Value.Count)]), modelHolder.transform);
            else
                GO = Instantiate(Resources.Load<GameObject>("Costume/" + costName.Value[0]), modelHolder.transform);

            PrefabsForTheLevel.Add(costName.Key, GO);
        }

        foreach(var item in PrefabsForTheLevel)
        {
            if(item.Key != loc_Manager.FirstAppearingLocation)
            {
                item.Value.SetActive(true);
                appearance = item.Key;
                CurrentActivePrefab = item.Value;
                animator = CurrentActivePrefab.GetComponent<Animator>();
                break;
            }
        }

        DressUp();
    }

    void UpdateReferences( Appearance _toactivate)
    {
		foreach(var item in PrefabsForTheLevel)
		{
            if(item.Key == _toactivate)
            {
                CurrentActivePrefab = item.Value;
                item.Value.SetActive(true);
            }
            else
                item.Value.SetActive(false);
		}
        
        animator = CurrentActivePrefab.GetComponent<Animator>();
        
        if(charCollisionManager != null)
            charCollisionManager.animator = animator;
    }

    void DressUp()
    {
        if(sse == null) sse = SaveSystemExperimental.GetInstance();

        List<(string, string)> AccessoriesToWear = sse.ReturnListOfAccessories(control);

        string hairColor = sse.GetHairColor(control == Control.Player);

        foreach(var item in PrefabsForTheLevel)
        {
            item.Value.GetComponent<AccessoruManagement>().LoadAccessory(AccessoriesToWear);
            item.Value.GetComponent<AccessoruManagement>().ApplyDye("#"+hairColor);
		}
    }

    void DressUpBoosters( (string, string) accessoriesToWear )
    {
        GameObject GO = null;

        foreach(var item in PrefabsForTheLevel)
            GO = item.Value.GetComponent<AccessoruManagement>().LoadAccessory(accessoriesToWear);

        GO?.GetComponent<PowerUp>().ActivatePowerUp(this.gameObject);
    }

    void HighlightTheCorrectDoors()
    {
        if(control == Control.Player)
        {
            if(gateHighlightCount > 0)
            {
                GatesToLookAt?.GetComponent<GateScript>().EnableParticleSystem();
                gateHighlightCount--;
			}
		}
	}

    public void EnableCompass()
    {
        HintArrow.SetActive(true);
    }

    private void OnAreaEnter(List<Appearance> appearance, List<Vector3> StartEndPositions, int id, int locationid, Locations _locName)
    {
        if(id != gameObject.GetInstanceID()) return;

        if(control == Control.Player)
            if(!(areaAppearance.Contains(Appearance.None) || areaAppearance.Count == 0))
            {
                if(dressChangeState == DressCrossState.NOCHANGE)
                {
                    var lvl = new Dictionary<string, object>();
                    //var loc = new Dictionary<string, object>();

                    lvl.Add("Level", levelIndex);
                    lvl.Add("Location", locationIndex + "_" + locationName);
                    lvl.Add("DressStatus", dressChangeState.ToString());

                    AnalyticEvents.ReportEvent("ClothesStatus_Level", lvl, false);

                    var lvl_json = new Dictionary<string, object>();
                    var locIndex_json = new Dictionary<string, object>();
                    //var locName_json = new Dictionary<string, object>();
                    //var dress_json = new Dictionary<string, object>();

                    //dress_json.Add(locationIndex + "_" + locationName, dressChangeState.ToString());

                    //locIndex_json.Add(levelIndex.ToString(), dress_json);
                    locIndex_json.Add(levelIndex.ToString(), dressChangeState.ToString());
                    //locName_json.Add(locationName.ToString(), dressChangeState.ToString());

                    lvl_json.Add("level", locIndex_json);

                    var str_json = YMMJSONUtils.JSONEncoder.Encode(lvl_json);
                    AnalyticEvents.ReportEvent("clothes_change", str_json);



                    /*loc.Add("LocationName", locationName);
                    loc.Add("DressStatus", dressChangeState.ToString());
                    AnalyticEvents.ReportEvent("Change_clothes_location_based", loc, false);*/




                    /*var loc_json = new Dictionary<string, object>();
                    loc_json.Add(dressChangeState.ToString(), locationName);

                    var str_loc_json = YMMJSONUtils.JSONEncoder.Encode(loc_json);
                    AnalyticEvents.ReportEvent("ClothesStatus_Dress_Location", str_loc_json);



                    var loc_dress_json = new Dictionary<string, object>();
                    loc_dress_json.Add(locationName, dressChangeState.ToString());

                    var str_loc_dress_json = YMMJSONUtils.JSONEncoder.Encode(loc_dress_json);
                    AnalyticEvents.ReportEvent("ClothesStatus_Location_Dress", str_loc_dress_json);*/
                }
            }

        dressChangeState = DressCrossState.NOCHANGE;

        areaAppearance = new List<Appearance>( appearance );
        locationIndex = locationid;
        locationName = _locName.ToString();

        charCollisionManager.areaAppearance = areaAppearance;

        if(myEmojies != null)
            //myEmojies.RecalculatePosition(areaAppearance.Contains(Appearance.SWIMMING) ? 1f : areaAppearance.Contains(Appearance.None) ? 1.8f : 2f);
            myEmojies.RecalculatePosition(areaAppearance.Contains(Appearance.SWIMMING) ? 1f : 2f);

        if(control == Control.Player)
        {
            if((gateHighlightCount > 0 || HintArrow.activeSelf) && gameManager.CurrentLevelIndex != 0)
            {
                GatesToLookAt = loc_Manager.GetCorrectGates(locationid);

                HintArrow.SetActive(GatesToLookAt != null);

                HighlightTheCorrectDoors();
			}

            if( move )
            {
                tutorialToEnable++;
                locationStartEndPositions = StartEndPositions;

                if( isTutorial )    tutorialToEnable = Mathf.Clamp(tutorialToEnable, 0, 3);
                else                tutorialToEnable = Mathf.Clamp(tutorialToEnable, 4, 5);
            }

            if( !isTutorial )
                CustomGameEventList.TurnOnHint.Invoke(false);
        }

        UpdateAppearance();

        if( control == Control.Player )
        {
            StopCoroutine("IncreaseTimeAfterCrossingTheLocation");
            StartCoroutine("IncreaseTimeAfterCrossingTheLocation");
        }

        /*if(control == Control.AI && !areaAppearance.Contains(Appearance.None) ) 
        {
            StopCoroutine("AISelectApearance");
            StartCoroutine("AISelectApearance");
        }*/
    }

    private void OnStopLine( int finishLineIndex, int id )
    {
        //UpdateAnimation();

        if(id != gameObject.GetInstanceID()) return;

        if(control == Control.Player)
        {
            if( ( finishLineIndex == 0 && gameManager.playersOnFinish.Count > 2) || (finishLineIndex == gameManager.PlayerSuccessRate))
            {
                StoppedAtTheStopLine = true;

                move = false;

                animator.transform.Rotate(Vector3.up, 180);

                bool PlayerIsTheWinner = gameManager.IsPlayerFirstToFinish();

                animator.Play( PlayerIsTheWinner ?
                    gameManager.Config.appearanceAnimations[3].correct[UnityEngine.Random.Range(0, gameManager.Config.appearanceAnimations[3].correct.Length)].name :       // PLAY HAPPY ANIMATION
                    gameManager.Config.appearanceAnimations[3].wrong[UnityEngine.Random.Range(0, gameManager.Config.appearanceAnimations[3].wrong.Length)].name, 0, 0);  // PLAY SAD ANIMATION

                print(" ON STOP LINE "); 

                gameManager.CompleteLevel(); //FAIL

                if(myEmojies != null && !PlayerIsTheWinner)
                    myEmojies.EnableEmojiEffect(EffectStyle.LOST, 1.9f);    
            }
        }
        else
        {
            StoppedAtTheStopLine = true;
            move = false;

            if( finishLineIndex == 0 )
            {
                animator.transform.Rotate(Vector3.up, 180);

                animator.Play(!gameManager.IsPlayerFirstToFinish() ?
                    gameManager.Config.appearanceAnimations[3].correct[UnityEngine.Random.Range(0, gameManager.Config.appearanceAnimations[3].correct.Length)].name :       // PLAY SAD ANIMATION
                    gameManager.Config.appearanceAnimations[3].wrong[UnityEngine.Random.Range(0, gameManager.Config.appearanceAnimations[3].wrong.Length)].name, 0, 0);  // PLAY HAPPY ANIMATION
            }
        }
    }

    void PlayerStopped( bool _playerWinStatus )
    {
        if(gameObject.activeSelf && control == Control.AI)
        {
            animator.transform.LookAt(gameManager.Player.transform);
            animator.Play(gameManager.Config.appearanceAnimations[4].wrong[0].name, 0, 0);
        }
    }

    public void SkipToFinishLines( float _zPositionStopLine )
    {
        if( StoppedAtTheStopLine ) return;

        if( control == Control.AI )
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, _zPositionStopLine);
            gameManager.playersOnFinish.Add(this);
        }
        else
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, _zPositionStopLine);
        }
	}

    int currentHintIndex = 3;

    void Update()
    {
        if(levelSkipped) return;

        if(tempAppearance != appearance) 
        {
            tempAppearance = appearance;
            //UpdateAppearance();
        }

        if (tempMove != move) 
        {
            tempMove = move;
            UpdateAnimation();
        }

        //if(isTutorial)
        //{
        if(control == Control.Player)
            if(HintArrow.activeSelf && GatesToLookAt != null)
                HintArrow.transform.GetChild(0).GetComponent<CompassAnimation>().SetEndPosition(GatesToLookAt.transform.position);
                    //HintArrow.transform.LookAt(GatesToLookAt.transform, Vector3.up);
        //}

        if (move)
        {
            //if(control != Control.Player)
                transform.Translate(Vector3.forward * moveSpeed * collisionModifier * buffSpeedModifier *Time.deltaTime);

            if(control == Control.Player )
            {
                if( !CrossedTheFinishLine )
                    timeInLevel += Time.deltaTime;

                if( correctDressEnabled )
                {
                    successRateValue += 1f / (finish_values.z / ( moveSpeed * collisionModifier * buffSpeedModifier * Time.deltaTime) );
                
                    ui_GameScreen.UpdateLevelProgress(successRateValue);
                }

                if( isTutorial )
                {
                   /* if(HintArrow.activeSelf)
                        HintArrow.transform.LookAt(GatesToLookAt.transform);*/

                    if(tutorialToEnable > -1 )
                    {
                        if(locationStartEndPositions != null && locationStartEndPositions.Count != 0 )
                            if( ( ( transform.position.z - locationStartEndPositions[0].z ))/ ( locationStartEndPositions[1].z - locationStartEndPositions[0].z ) >= TutorialEnablePathRatio[tutorialToEnable])
                            {
                                //if(areaAppearance[0] != appearance )
                                //{
                                    //CustomGameEventList.TutorialCheckpoinReached.Invoke(areaAppearance[0]);
                                    //CustomGameEventList.TutorialCheckpoinReached.Invoke(tutorialToEnable);
                                    
                                    if(currentHintIndex != CustomGameEventList.TUTORIAL_TO_ENABLE && currentHintIndex != 8)
                                    {
                                        CustomGameEventList.NextTutorial();
                                        CustomGameEventList.TurnOnHint.Invoke( true );
                                        locationStartEndPositions.Clear();
                                        currentHintIndex = CustomGameEventList.TUTORIAL_TO_ENABLE;
                                    }
                                    /*else if (CustomGameEventList.TUTORIAL_TO_ENABLE == 8)
                                             CustomGameEventList.NextTutorial();*/
                                //}
                            }
					}
                }
            }
        }
    }

    void Rotat180()
    {
        modelHolder.transform.DOLocalRotate( Vector3.up * 359, 0.2f, RotateMode.FastBeyond360).SetEase(Ease.Linear).OnComplete(delegate () {
            modelHolder.transform.localEulerAngles = Vector3.zero;
        });
	}

    public void PlayerChangedDress(Appearance _appearance, bool isThisCorrectOption)
    {
        appearance = _appearance;

        UpdateAppearance(isThisCorrectOption);

        if( /*areaAppearance.Contains(appearance) ||*/ isThisCorrectOption && control == Control.Player)
        {
            //CustomGameEventList.PlayerChangedDressTime.Invoke(timeAfterCrossingLocation);
            //CustomGameEventList.TurnOnHint.Invoke(false);
            CustomGameEventList.PlayerChangedDressTime.Invoke(-1f);

            goodParticle.Play();

            dressChangeState = DressCrossState.CORRECT;

            sm.ChangeDressSound(true);

            if(myEmojies != null)
                if(areaAppearance.Contains(Appearance.SWIMMING))
                    myEmojies.EnableEmojiEffect(EffectStyle.POSITIVE, 1f);
                else
                    myEmojies.EnableEmojiEffect(EffectStyle.POSITIVE);

            if(myWind != null)
                myWind.PlayRandomWind();

            /*var lvl = new Dictionary<string, object>();
            lvl.Add(gameManager.CurrentLevelIndex.ToString(), "correct");

            AnalyticEvents.ReportEvent("clothes_change", lvl);*/
        }
        else
        {
            if(!areaAppearance.Contains(Appearance.None))
            {
                /*var lvl = new Dictionary<string, object>();
                lvl.Add(gameManager.CurrentLevelIndex.ToString(), "incorrect");

                AnalyticEvents.ReportEvent("clothes_change", lvl);*/

                sm.ChangeDressSound(false);

                dressChangeState = DressCrossState.INCORRECT;

                if(myEmojies != null)
                    if(areaAppearance.Contains(Appearance.SWIMMING))
                        myEmojies.EnableEmojiEffect(EffectStyle.NEGATIVE, 1f);
                    else
                        myEmojies.EnableEmojiEffect(EffectStyle.NEGATIVE);

                //badParticle.Play();
            }
        }

        if(control == Control.Player)
            if(!(areaAppearance.Contains(Appearance.None) ))
            {
                var lvl = new Dictionary<string, object>();
                //var loc = new Dictionary<string, object>();

                lvl.Add("Level", levelIndex);
                lvl.Add("Location", locationIndex + "_" + locationName);
                lvl.Add("DressStatus", dressChangeState.ToString());

                AnalyticEvents.ReportEvent("ClothesStatus_Level", lvl, false);

                var lvl_json = new Dictionary<string, object>();
                var locIndex_json = new Dictionary<string, object>();
                //var locName_json = new Dictionary<string, object>();
                //var dress_json = new Dictionary<string, object>();

                //dress_json.Add(locationIndex + "_" + locationName, dressChangeState.ToString());

                //locIndex_json.Add(levelIndex.ToString(), dress_json);
                locIndex_json.Add(levelIndex.ToString(), dressChangeState.ToString());
                //locName_json.Add(locationName.ToString(), dressChangeState.ToString());

                lvl_json.Add("level", locIndex_json);

                var str_json = YMMJSONUtils.JSONEncoder.Encode(lvl_json);
                AnalyticEvents.ReportEvent("clothes_change", str_json);



                /*loc.Add("LocationName", locationName);
                loc.Add("DressStatus", dressChangeState.ToString());
                AnalyticEvents.ReportEvent("Change_clothes_location_based", loc, false);*/




                /*var loc_json = new Dictionary<string, object>();
                loc_json.Add(dressChangeState.ToString(), locationName);

                var str_loc_json = YMMJSONUtils.JSONEncoder.Encode(loc_json);
                AnalyticEvents.ReportEvent("ClothesStatus_Dress_Location", str_loc_json);



                var loc_dress_json = new Dictionary<string, object>();
                loc_dress_json.Add(locationName, dressChangeState.ToString());

                var str_loc_dress_json = YMMJSONUtils.JSONEncoder.Encode(loc_dress_json);
                AnalyticEvents.ReportEvent("ClothesStatus_Location_Dress", str_loc_dress_json);*/
                
            }

        Rotat180();

        /*if( isTutorial && !move )
        {
            CustomGameEventList.TurnOnHint.Invoke(false);
        }   */
    }

    int layerId = -1;

    public void EnableNegativeSmiles()
    {
        if(myEmojies != null)
            if(areaAppearance.Contains(Appearance.SWIMMING))
                myEmojies.EnableEmojiEffect(EffectStyle.NEGATIVE, 1f);
            else
                myEmojies.EnableEmojiEffect(EffectStyle.NEGATIVE);
    }

    public void EnablePositiveSmiles()
    {
        if(myEmojies != null)
            if(areaAppearance.Contains(Appearance.SWIMMING))
                myEmojies.EnableEmojiEffect(EffectStyle.POSITIVE, 1f);
            else
                myEmojies.EnableEmojiEffect(EffectStyle.POSITIVE);
    }

    public void DisableWind()
    {
        if(myWind != null)
            myWind.DisableAllWinds();
    }

    public void TemporaryChangeLayer()
    {
        gameObject.layer = 9;

        Invoke("ReturnOriginalLayer", 0.1f);
    }

    void ReturnOriginalLayer() 
    {
        gameObject.layer = layerId;
    }

    IEnumerator IncreaseTimeAfterCrossingTheLocation()
    {
        timeAfterCrossingLocation = 0f;

        while ( move == true )
        {
            yield return 0;

            if(!isTutorial || ( isTutorial && move ) )
                timeAfterCrossingLocation += Time.deltaTime;
        }
    }

    public void SetAI(AI_behavior _ai_from_config)
    {
        ai = _ai_from_config;
        ai_difficulty = ai.difficulty_type;
    }

    public void CharacterCrossedTheFinishLine()
    {
        CrossedTheFinishLine = true;

        collisionModifier = 1f;

        buffSpeedModifier = 1f;
        gateHighlightCount = 0;
        bullyInteractionCount = 0;

        charCollisionManager.StopRegisteringColiisions();

        gameManager.playersOnFinish.Add(this);

        if( control == Control.Player)
        {
            myEmojies.YouTextEnabled(false);

            GetComponent<HorizontalControl>().enabled = false;
            
            if(gameManager.IsPlayerFirstToFinish())
            {
                /*var lvl = new Dictionary<string, object>();
                lvl.Add(gameManager.CurrentLevelIndex.ToString(), timeInLevel);

                AnalyticEvents.ReportEvent("lvl_X_time", lvl);*/

                sm.PlayerCrossedTheFinishLine(true);
            }
            else
            {
                sm.PlayerCrossedTheFinishLine(false);
            }

            gameManager.PlayerSuccessRate = successRateValue;
		}
    }

    void EnableHInt( bool _hintStatus)
    {
        if( isTutorial /*&& !move*/)
        {
            move = !_hintStatus;

            if(control == Control.AI )
                if(_hintStatus)
                    StopCoroutine("AISelectApearance");
                else
                    StartCoroutine("AISelectApearance");
		}
    }

    public void SkipLevel() { levelSkipped = true; }

    public int SubscribedTimes { get; set; }

    enum DressCrossState
    {
        CORRECT,
        INCORRECT,
        NOCHANGE
    }

    public void Subscribe()
	{
        ++SubscribedTimes;

        //print($"Character: {SubscribedTimes}");

        CustomGameEventList.OnEnterArea += OnAreaEnter;
        CustomGameEventList.OnEnterStopLine += OnStopLine;
        CustomGameEventList.TurnOnHint += EnableHInt;
        CustomGameEventList.PlayerCrossedTheFinishLine += PlayerStopped;
        CustomGameEventList.SkipLevel += SkipLevel;
    }

	public void UnSubscribe()
	{
        SubscribedTimes--;

        SubscribedTimes = Mathf.Clamp(SubscribedTimes, 0, 99999);

        CustomGameEventList.OnEnterArea -= OnAreaEnter;
        CustomGameEventList.OnEnterStopLine -= OnStopLine;
        CustomGameEventList.TurnOnHint -= EnableHInt;
        CustomGameEventList.PlayerCrossedTheFinishLine -= PlayerStopped;
        CustomGameEventList.SkipLevel -= SkipLevel;
    }
}