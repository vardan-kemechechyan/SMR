using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class Location : MonoBehaviour, ISubscribeUnsubscribe
{
    Transform planeTransorm;

    LocationManager locationManager;

    [SerializeField] Locations location;
    [SerializeField] List<Appearance> appearancesForLocation;
    [SerializeField] List<Appearance> appearancesForLevel;

    [SerializeField] List<GameObject> locationVariants;

    [SerializeField] Transform startPosition;

    [SerializeField] Transform endPosition;

    [SerializeField] GateManager mainGates;
    
    [SerializeField] List<GateManager> innerGateManager;

    public GameObject CorrectGate;

    bool crossedByAtLeastOnePlayer = false;

    int locationIndex = 0;
    public int LocationIndex { get => locationIndex; set { locationIndex = value; } }

    public int SubscribedTimes { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

	private void OnEnable()
	{
        UnSubscribe();
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

    public GameObject ReturnCorrectGate()
    {
        return CorrectGate;
    }

    public void EnableLocationVariant(int index = -1) //-1 - Random, 0 - small, 1 - medium, 2 - large
    {
        if(index == -1)
            index = Random.Range(0, locationVariants.Count);

        foreach(var loc in locationVariants)
            loc?.SetActive(false);
        
        if( locationVariants.Count != 0 )
        {
            locationVariants[index].SetActive(true);
            startPosition = locationVariants[index].transform.Find("Start");
            endPosition = locationVariants[index].transform.Find("End");
		}
        else
        {
            startPosition = transform.Find("Start");
            endPosition = transform.Find("End");
        }
    }

    public void PrepareTheGates(LocationManager lm)
    {
        crossedByAtLeastOnePlayer = false;

        locationManager = lm;

        mainGates.PrepareTheGates();
        mainGates.gameObject.SetActive(false);

		foreach(var innerGates in innerGateManager)
		{
            innerGates.PrepareTheGates();
            innerGates.gameObject.SetActive(false);
        }
    }

    public void EnableMainGatesByFormula()
    {
        mainGates.EnableMainGatesByFormula();
    }

    public void EnableMainGatesByIndecies(List<bool> indecies)
    {
        mainGates.EnableMainGatesByIndecies(indecies);
    }

    public void InitializeTheMainGates( List<Appearance> dressesForTheLevel, int currentLocationIndex )
    {
        appearancesForLevel = dressesForTheLevel;

        mainGates.InitializeGates(appearancesForLocation[0], new List<Appearance>(appearancesForLevel), currentLocationIndex);

        mainGates.gameObject.SetActive(true);
    }

    public void RevealTheNextMainGates()
    {
        if(crossedByAtLeastOnePlayer) return;

        crossedByAtLeastOnePlayer = true;

        locationManager?.InitializeNextMainGates();
    }

    public void HighlightGateInMainGate( bool _isHighlighted)
    {
        GetTheCorrectGate().GetComponent<GateScript>().HintArrowManagement(_isHighlighted);
    }

    public void HideAllGates()
    {
        mainGates.HideAllGates();

        foreach(var innerGates in innerGateManager)
        {
            innerGates.HideAllGates();
        }
    }

    public GameObject GetTheCorrectGate()
    {
        return mainGates != null ? mainGates.GetTheCorrectGate() : null;
	}

    public Vector3 StartPositionCoordinate() { return startPosition.position; }
    public Vector3 EndPositionCoordinate() { return endPosition.position; }

    public Locations GetLocationType() { return location; }
    public List<Appearance> GetLocationDressTypes() { return appearancesForLocation; }

    public Vector3 ReturnPlaneLocalScale() { return planeTransorm.localScale; }

    void OnChangeGameState( GameState _newState )
    {
         if(_newState == GameState.LoadLevel && ( gameObject.name != "Start" && gameObject.name != "Finish"))
       {
            //print("Trying to return to pool the object " + gameObject.name);
            PoolingScript.GetInstance().ReturnToPool( gameObject );
	   }
    }

    public void Subscribe()
	{
        CustomGameEventList.OnChangeGameState += OnChangeGameState;
    }

	public void UnSubscribe()
	{
        CustomGameEventList.OnChangeGameState -= OnChangeGameState;
    }
}
