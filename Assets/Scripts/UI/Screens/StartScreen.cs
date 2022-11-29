using UnityEngine;
using UnityEngine.UI;
using UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

public class StartScreen : UIScreen
{
    [SerializeField] bool showProgressBar;

    GameManager gm;
    SaveSystemExperimental sse;

    [SerializeField] UIManager ui;
    [SerializeField] ProgressMapManager progressBarManager;
    [SerializeField] GameObject TapToStart;
    [SerializeField] GameObject Shop;
    [SerializeField] GameObject BoosterPanel;
    [SerializeField] GameObject moneyContainer;
    [SerializeField] GameObject tapToStart;

    int unlockSkinRate = 0;

    public override void Open()
    {
        base.Open();

        print(" SHOWING TO SHOW START SCREEN ");

        if(gm == null) gm = GameManager.GetInstance();
        if(sse == null) sse = SaveSystemExperimental.GetInstance();

        int currentLevelIndex = sse.GetCurrentLevel();

        if(unlockSkinRate == 0)
            unlockSkinRate = gm.GetUnlockSkinRate();

        progressBarManager.transform.parent.gameObject.SetActive( currentLevelIndex != 0 && showProgressBar);

        if(currentLevelIndex == 0) moneyContainer.SetActive(false);

        if( currentLevelIndex != 0 )
        {
            moneyContainer.SetActive(true);

            BoosterPanel.SetActive( false );

            SetInterfaceOn(true);

            int beginningNumber = ( currentLevelIndex  - currentLevelIndex % unlockSkinRate ) + 1;

            if( currentLevelIndex % unlockSkinRate == 0 ) beginningNumber = currentLevelIndex - ( unlockSkinRate - 1 );

            if(gm.disableLoop && currentLevelIndex >= gm.Config.csvData.Count)
            {
                TapToStart.SetActive(false);

                //currentLevelIndex = gm.Config.csvData.Count - 1;
                //beginningNumber = currentLevelIndex;
            }

            progressBarManager.SetLevelNumberInCircles( beginningNumber, currentLevelIndex, ( gm.disableLoop && currentLevelIndex >= gm.Config.csvData.Count ));
		}
    }

    public void OpenShop()
    {
        AnalyticEvents.ReportEvent("shop_open");

        ui.ShowScreen<ShopScreen>();
    }

    public void SetInterfaceOn( bool _status )
    {
        Shop.SetActive(_status);
        tapToStart.SetActive(_status);
    }
}
