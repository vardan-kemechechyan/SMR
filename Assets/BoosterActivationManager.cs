using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoosterActivationManager : MonoBehaviour
{
    [SerializeField] GameObject continueButton;
    [SerializeField] GameManager gameManager;
    [SerializeField] List<BoosterActivation> boosterButtons;
    [SerializeField] List<BoosterCustomizableObject> allBoosters;

    List<BoosterCustomizableObject> setOfRandomBoosters = new List<BoosterCustomizableObject>();
    List<BoosterCustomizableObject> activatedBoosters = new List<BoosterCustomizableObject>();
    SaveSystemExperimental SSP;

    private void OnEnable()
	{
        if(SSP == null) SSP = SaveSystemExperimental.GetInstance();

        InstantiateRouletteButtons();
	}

    void InstantiateRouletteButtons()
    {
        activatedBoosters.Clear();
        setOfRandomBoosters.Clear();

        List<Customizable_SO> itemsArrivedFromSaveFile = new List<Customizable_SO>();
        List<Sprite> boosterIcons = new List<Sprite>();

        itemsArrivedFromSaveFile = SSP.ReturnItemsInSection(Enums.ShopSection.Accessory).FindAll( item => item.lockInfo.lockStatus == Enums.LockStatus.UNLOCKED);

        BoosterCustomizableObject showNow;

        int boosterButtonIndex = 0;

		foreach(var item in itemsArrivedFromSaveFile)
            boosterIcons.Add(item.shopIcon);

		for(int i = 0; i < itemsArrivedFromSaveFile.Count; i++)
		{
            if(itemsArrivedFromSaveFile[i].lockInfo.levelToUnlock == (gameManager.CurrentLevelIndex - 1))
            {
                Customizable_SO accs = itemsArrivedFromSaveFile[i];

                showNow = allBoosters.Find(b => b.itemName == accs.itemName);
                boosterButtons[0].Init(accs.shopIcon, showNow.description, boosterIcons);
                setOfRandomBoosters.Add(showNow);

                itemsArrivedFromSaveFile.RemoveAll(b => b.itemTypeToString == accs.itemTypeToString);
                boosterButtonIndex++;
                break;
            }
		}

        for(int i = boosterButtonIndex; i < boosterButtons.Count; i++)
        {
            var booster = boosterButtons[i];

            Customizable_SO accs = itemsArrivedFromSaveFile[Random.Range(0, itemsArrivedFromSaveFile.Count)];
            BoosterCustomizableObject boost = allBoosters.Find(b => b.itemName == accs.itemName);

            booster.Init(accs.shopIcon, boost.description, boosterIcons);
            setOfRandomBoosters.Add(boost);

            itemsArrivedFromSaveFile.RemoveAll(b => b.itemTypeToString == accs.itemTypeToString);
        }

        if(gameManager.CurrentLevelIndex == 1)
        {
            boosterButtons[0].WatchAddGetBooster(true);
        }

        StopAllCoroutines();
        StartCoroutine(SlotMachine());
    }

    IEnumerator SlotMachine()
    {
        float startingValue = 4516+64*10;
        float delta = -92.16f;
        var timeD = new WaitForSeconds(0.02f); //4608 / (2000/40) -(startingValue / (2000 / 20));

        while(startingValue > 0)
		{
            yield return timeD;

            foreach(var boosterButton in boosterButtons)
                boosterButton.ChangeYPosition(delta * 3);

            startingValue += delta * 3;
        }

        foreach(var boosterButton in boosterButtons)
            boosterButton.EnableDescription();

        ShowContinueButton();
    }

	public void ActivateBooster( int boosterIndex )
    {
        activatedBoosters.Add(setOfRandomBoosters[boosterIndex]);

        continueButton.SetActive( true );
    }
    public void SendBoostersToGameManager()
    {
        StopCoroutine(SlotMachine());

        gameManager.LoadBoosters(activatedBoosters);
    }
    void ShowContinueButton()
    {
        continueButton.SetActive(true);
    }
}
