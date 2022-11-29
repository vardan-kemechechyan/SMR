using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using System.Text.RegularExpressions;
using System;
using System.Linq;

public class LocationManager : MonoBehaviour, ISubscribeUnsubscribe
{
    public LevelManager mng_Level;
    GameManager gm;

    [Tooltip( "Vertical placement Correction" )]
    [SerializeField] float Y_position;

    [Tooltip( "This number decides the amount of locations that the level will be contructed of." )]
    [SerializeField] int NumberOfLocationsToGenerate;

    [SerializeField] string levelString;
    [SerializeField] string appearanceString;

    [SerializeField] List<string> locations_string = new List<string>();
    [SerializeField] List<Locations> locations = new List<Locations>();

    [SerializeField] List<string> appearances_string = new List<string>();
    [SerializeField] List<Appearance> appearances = new List<Appearance>();

    [SerializeField] Location StartLocation;
    [SerializeField] Location FinishLocation;

    [SerializeField] List<Location> LocationsForThisLevel = new List<Location>();

    List<Location> FinalGeneratedLevels = new List<Location>();

    [SerializeField] List<GameObject> HintArrows;

    int currentLocationIndex;

	private void Start()
	{
        UnSubscribe();
        Subscribe();
    }

	public void PrepareTheLevel( bool _randomizeLevels )
    {
        print(" Randomizing Levels BEGIN");

        if(gm == null) gm = GameManager.GetInstance();

        NumberOfLocationsToGenerate = gm.GetNumberOfLocationsToGenerate();

        levelString = gm.GetLocationsToGenerate();

        appearanceString = gm.GetClothTypes();

        ReadLevelDataFromString( levelString, appearanceString);

        RandomizeTheLevelsInTheList( _randomizeLevels );

        ConstructLevelBasedOnLocations();

        print(" Randomizing Levels STOP");
    }

    void ConstructLevelBasedOnLocations()
    {
		for ( int i = 0; i < FinalGeneratedLevels.Count; i++ )
		{
            if(i == 0)                                                                      FinalGeneratedLevels[i].EnableLocationVariant(0);
            else if(FinalGeneratedLevels[i].GetLocationType() != Enums.Locations.FINISH)    FinalGeneratedLevels[i].EnableLocationVariant();

            FinalGeneratedLevels[i].LocationIndex = i == 0 ? -1 : FinalGeneratedLevels[i].GetLocationType() == Enums.Locations.FINISH ? -2 : i;

            FinalGeneratedLevels[ i ].transform.position = i == 0 ? new Vector3(0f, Y_position, 0f ) : NewPosition( FinalGeneratedLevels[ i - 1] );

            if ( FinalGeneratedLevels[ i ].GetLocationType() == Enums.Locations.FINISH )
                mng_Level.finish = FinalGeneratedLevels[ i ].transform.position;

            FinalGeneratedLevels[ i ].gameObject.SetActive( true );

            if( i != 0 && i != (FinalGeneratedLevels.Count - 1) )
            {
                if(gm.GetTutorialStatus() )
                {
                    if(i == 1)
                    {
                        FinalGeneratedLevels[i].EnableMainGatesByIndecies(new List<bool> { false, true, false});
                    }
                    else if(i == 2)
                    {
                        FinalGeneratedLevels[i].EnableMainGatesByIndecies(new List<bool> { true, true, false });
                    }
                    else if(i == 3)
                    {
                        FinalGeneratedLevels[i].EnableMainGatesByIndecies(new List<bool> { true, true, false });
                    }
                    else if(i == 4)
                    {
                        FinalGeneratedLevels[i].EnableMainGatesByIndecies(new List<bool> { true, true, false });
                    }
                }
                else
                {
                    if(i == 1)
                    {
                        bool leftOrRight = UnityEngine.Random.value < 0.5f;
                        FinalGeneratedLevels[i].EnableMainGatesByIndecies(new List<bool> { leftOrRight, true, !leftOrRight });
                    }
                    else
                    {
                        FinalGeneratedLevels[i].EnableMainGatesByFormula();
					}
                }

                FinalGeneratedLevels[i].PrepareTheGates(this);
            }
        }

        currentLocationIndex = 0;

        InitializeNextMainGates();

        //FinalGeneratedLevels[1].InitializeTheMainGates( appearances );
    }

    public GameObject GetCorrectGates( int index )
    {
		foreach(var locations in FinalGeneratedLevels)
		{
                    if( index == -2) return null;
            else    if( index == -1) return FinalGeneratedLevels[1].GetTheCorrectGate();
            else    return FinalGeneratedLevels[index+1].GetTheCorrectGate() != null ? FinalGeneratedLevels[index+1].GetTheCorrectGate() : null;
        }

        return null;
    }

    public void InitializeNextMainGates()
    {
        currentLocationIndex++;

        if(currentLocationIndex >= FinalGeneratedLevels.Count - 1) return;

        FinalGeneratedLevels[currentLocationIndex].InitializeTheMainGates(appearances, currentLocationIndex);
    }

    List<(Appearance, string)> appearaneLocationNamePair = new List<(Appearance, string)>();

    void ReadLevelDataFromString( string _levelDescription, string _appearancesDescription )
    {
        locations.Clear();
        appearances.Clear();

        _levelDescription = _levelDescription.Replace("\\", "-");

        locations_string = new List<string>(_levelDescription.Split('-'));

        Locations[] l = Enum.GetValues(typeof(Locations)).OfType<Locations>().ToArray();

        foreach(var _location in locations_string)
            locations.Add(l.First((x) => x.ToString() == _location.Substring(0, _location.Length-3)));
        /////////////////////////////
        _appearancesDescription = _appearancesDescription.Replace("\\", "-");

        appearances_string = new List<string>(_appearancesDescription.Split('-'));

        Appearance[] ap = Enum.GetValues(typeof(Appearance)).OfType<Appearance>().ToArray();

        foreach(var _appearance in appearances_string)
            appearances.Add(ap.First((x) => x.ToString() == _appearance));
    }

    public Appearance FirstAppearingLocation;

	public int SubscribedTimes { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

	void RandomizeTheLevelsInTheList( bool _randomizeLevels ) 
    {
        //Create list of paired values (dress type, Location Name)

        if( _randomizeLevels )
        {
            int _randomIndex= -1;

            string lastLocationLoaded = "";

            Extensions.Shuffle(locations_string);
            List<string> newLocationString = new List<string>(locations_string);

            Dictionary<Appearance, List<Location>> LocationsBasedOnType = new Dictionary<Appearance, List<Location>>();

            for(int i = 0; i < NumberOfLocationsToGenerate; i++)
            {
                if(newLocationString.Count == 0)
                {
                    int minimumLength = 999;
                    KeyValuePair<Appearance, List<Location>> locType = new KeyValuePair<Appearance, List<Location>>();

					foreach(var keyValuePair in LocationsBasedOnType)
					{
                        if( minimumLength >= keyValuePair.Value.Count )
                        {
                            minimumLength = keyValuePair.Value.Count;
                            locType = keyValuePair;
                        }
					}

                    string notRepeatedLocation = locations_string.First((x) => locType.Value.Any( loc => loc.gameObject.name == x));

                    LocationsForThisLevel.Add(PoolingScript.GetInstance().Pull(notRepeatedLocation).GetComponent<Location>());

                    LocationsBasedOnType[LocationsForThisLevel.Last().GetLocationDressTypes()[0]].Add(LocationsForThisLevel.Last());

                    lastLocationLoaded = notRepeatedLocation;

                    Extensions.Shuffle(locations_string);
                }
                else
                {
                    //if(newLocationString.Count == locations_string.Count)
                    //{
                        _randomIndex = UnityEngine.Random.Range(0, newLocationString.Count);
                        lastLocationLoaded = newLocationString[_randomIndex];
					//}
                    //else
                    //{
                        //lastLocationLoaded = newLocationString.First(x => LocationsForThisLevel.Last().GetLocationDressTypes()[0] != LocationsBasedOnType.FirstOrDefault(locArr => locArr.Value.FirstOrDefault(loc => loc.gameObject.name == x)).Key);
                        //_randomIndex = newLocationString.IndexOf(lastLocationLoaded);
                    //}

                    LocationsForThisLevel.Add(PoolingScript.GetInstance().Pull(lastLocationLoaded).GetComponent<Location>());

                    newLocationString.RemoveAt(_randomIndex);

                    if(LocationsBasedOnType.ContainsKey(LocationsForThisLevel.Last().GetLocationDressTypes()[0]))
                        LocationsBasedOnType[LocationsForThisLevel.Last().GetLocationDressTypes()[0]].Add(LocationsForThisLevel.Last());
                    else
                        LocationsBasedOnType.Add(LocationsForThisLevel.Last().GetLocationDressTypes()[0], new List<Location>() { LocationsForThisLevel.Last() });
                }
            }

            int numberOfOccurences = -1;

            string newlocationstr = "";

            while(numberOfOccurences != 0)
            {
                numberOfOccurences = 0;

                for(int i = 0; i < LocationsForThisLevel.Count; i++)
			    {
                    if(i != 0 && i != LocationsForThisLevel.Count - 1 )
                    {
                        if( LocationsForThisLevel[i].GetLocationDressTypes()[0] == LocationsForThisLevel[i - 1].GetLocationDressTypes()[0] )
                        {
                            numberOfOccurences++;
                            Extensions.Move<Location>(LocationsForThisLevel, i, i + 1);
						}
                        else if(LocationsForThisLevel[i].GetLocationDressTypes()[0] == LocationsForThisLevel[i + 1].GetLocationDressTypes()[0])
                        {
                            numberOfOccurences++;
                            Extensions.Move<Location>(LocationsForThisLevel, i, i - 1);
						}
                    }
                    
                    newlocationstr += LocationsForThisLevel[i].gameObject.name + "/";
			    }

                print(numberOfOccurences + " : " + newlocationstr);
                newlocationstr = "";
            }
		}
        else
        {
            foreach(var _location in locations_string)
                LocationsForThisLevel.Add(PoolingScript.GetInstance().Pull(_location).GetComponent<Location>());
        }

        FirstAppearingLocation = LocationsForThisLevel[0].GetLocationDressTypes()[0];

        FinalGeneratedLevels.Add( StartLocation );
        FinalGeneratedLevels.AddRange( LocationsForThisLevel );
        FinalGeneratedLevels.Add( FinishLocation );
    }

    public List<Appearance> ReturnLevelAppearanceList() { return appearances; }

    Vector3 NewPosition( Location PreviouseBlock, Location CurrentBlock )
    {
        Vector3 PosA = new Vector3( PreviouseBlock.transform.position.x, Y_position, PreviouseBlock.transform.position.z );
        Vector3 PosB = new Vector3( PosA.x, Y_position, PosA.z + ( PreviouseBlock.ReturnPlaneLocalScale().z + CurrentBlock.ReturnPlaneLocalScale().z ) / 2f );

        return PosB;
    }

    Vector3 NewPosition( Location PreviouseBlock )
    {
        return PreviouseBlock.EndPositionCoordinate();
    }

    void HintManagement(int hintIndex)
    {
        if(hintIndex == 0)
            foreach(var arrow in HintArrows)
                arrow.SetActive(true);

        if(hintIndex == 1)
        {
            LocationsForThisLevel[0].HighlightGateInMainGate(true);

            GetComponent<Animator>().Play("YellowArrows", -1, 0.5f);
		}

        if(hintIndex == 2)
        {
            foreach(var arrow in HintArrows)
                arrow.SetActive(false);

            CustomGameEventList.TurnOnHint(false);

            LocationsForThisLevel[0].HighlightGateInMainGate(false);
        }

        if(hintIndex == 4)
        {
            //mng_Level.player.HintArrow.SetActive(true);
            mng_Level.player.GatesToLookAt = LocationsForThisLevel[1].GetTheCorrectGate();

            LocationsForThisLevel[0].GetTheCorrectGate().SetActive(false);
            LocationsForThisLevel[1].HighlightGateInMainGate( true );
            mng_Level.player.GetComponent<HorizontalControl>().enabled = true;
        }

        /*if(hintIndex == 5 || hintIndex == 7 || hintIndex == 9)
        {
            mng_Level.player.GetComponent<HorizontalControl>().enabled = true;
        }*/

        if( hintIndex == 6 )
        {
            //mng_Level.player.GetComponent<HorizontalControl>().enabled = false;
            mng_Level.player.GatesToLookAt = LocationsForThisLevel[2].GetTheCorrectGate();

            LocationsForThisLevel[1].GetTheCorrectGate().SetActive(false);
            LocationsForThisLevel[2].HighlightGateInMainGate(true);
        }

        if(hintIndex == 8)
        {
            //mng_Level.player.GetComponent<HorizontalControl>().enabled = false;
            mng_Level.player.GatesToLookAt = LocationsForThisLevel[3].GetTheCorrectGate();

            LocationsForThisLevel[2].GetTheCorrectGate().SetActive(false);
            LocationsForThisLevel[3].HighlightGateInMainGate(true);
        }

        if(hintIndex == 10)
        {
            //LocationsForThisLevel[3].HideAllGates();
            mng_Level.player.HintArrow.SetActive(false);

            CustomGameEventList.TUTORIAL_TO_ENABLE = 0;
        }
    }

    public void Subscribe()
	{
        CustomGameEventList.TutorialCheckpoinReached += HintManagement;
    }

	public void UnSubscribe()
	{
        CustomGameEventList.TutorialCheckpoinReached += HintManagement;
    }
}
