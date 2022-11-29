using Enums;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;

public class SkinUnlockedScreen : UIScreen
{
    GameManager gameManager;

    [SerializeField] UIManager ui;

    [SerializeField] ShopManager shopManager;

    [SerializeField] GameObject loseIt;
    
    [SerializeField] GameObject getDefaultSkin;
    [SerializeField] TextMeshProUGUI goToShopText;

    [SerializeField] GameObject getRewardedButton;

    [SerializeField] GiftPositionCorrector giftPositioner;

    [SerializeField] Animator scaleAnimator;
    [SerializeField] Animator rotationAnimator;

    [SerializeField] TextMeshProUGUI boosterDescription;
    [SerializeField] List<BoosterCustomizableObject> allBoosters;

    public override void Open()
    {
        base.Open();

        //IronSourceManager.Instance.skipInterstitial = false;
        AdMob.Instance.skipInterstitial = false;

        if(gameManager == null)
        {
            gameManager = GameManager.GetInstance();
        }

        ui.EnableCamera(2);

        loseIt.SetActive(false);
        getDefaultSkin.SetActive(false);

        giftPositioner.PositionTheItem(gameManager.UnlockedItem);

        CancelInvoke("UpdateRewardButton");

        StopCoroutine(ShowDelayedLoseButton());

        boosterDescription.enabled = false;

        if(gameManager.UnlockedItem.lockInfo.lockStatus == LockStatus.UNLOCKED)
        {
            getRewardedButton.SetActive(false);
            getDefaultSkin.SetActive(true);

            if(gameManager.UnlockedItem.shopSection == ShopSection.Accessory)
            {
                goToShopText.text = "PLAY";
                ShowBoosterDescription();
			}
            else
            {
                goToShopText.text = "TO SHOP";
			}
        }
        else
        {
            InvokeRepeating("UpdateRewardButton", 0, 1);

            StartCoroutine(ShowDelayedLoseButton());
		}

        scaleAnimator.Play("ScaleAnimation", 0, 0);
        rotationAnimator.Play("InfiniteAnimation", 0, 0);
    }

    private void OnDisable()
    {
        AdMob.OnRewarded -= OnRewarded;
        AdMob.OnRewardedFailed -= OnRewardedFailed;

        /*IronSourceManager.OnRewarded -= OnRewarded;
        IronSourceManager.OnRewardedFailed -= OnRewardedFailed;*/
    }

    void ShowBoosterDescription()
    {
        boosterDescription.enabled = true;

		foreach(var booster in allBoosters)
            if(booster.itemName == gameManager.UnlockedItem.itemName)
                boosterDescription.text = booster.description;

    }

    public override void Close()
    {
        base.Close();

        ui.EnableCamera(0);
    }

    private void UpdateRewardButton()
    {
        bool isAdReady = false;

        //isAdReady = IronSourceManager.Instance.IsRewardedReady("Reward_Skin");
        isAdReady = AdMob.Instance.IsReady("Reward_Skin");

        getRewardedButton.SetActive(isAdReady);
    }

    private void OnRewarded()
    {
        AdMob.OnRewarded -= OnRewarded;
        AdMob.OnRewardedFailed -= OnRewardedFailed;

        AdMob.Instance.skipInterstitial = true;

        /*IronSourceManager.OnRewarded -= OnRewarded;
        IronSourceManager.OnRewardedFailed -= OnRewardedFailed;

        IronSourceManager.Instance.skipInterstitial = true;*/

        LockStatus _ls = LockStatus.UNLOCKED;
        SelectionState _ss = gameManager.UnlockedItem.selectionState;
        HighLight _hl = gameManager.UnlockedItem.highlight;
        LockType _lt = LockType.NONE;
        int price = 0;

        var lvl = new Dictionary<string, object>();
        lvl.Add("skin_name", gameManager.UnlockedItem.itemName);

        AnalyticEvents.ReportEvent("skin_reward", lvl);

        AnalyticEvents.ReportEvent("reward_claim");

        shopManager.UnlockItem(gameManager.UnlockedItem, _ls, _ss, _hl, _lt, price);

        NextScreen();
    }

    private void OnRewardedFailed()
    {
        AdMob.OnRewarded -= OnRewarded;
        AdMob.OnRewardedFailed -= OnRewardedFailed;

        /*IronSourceManager.OnRewarded -= OnRewarded;
        IronSourceManager.OnRewardedFailed -= OnRewardedFailed;*/

        LoseGift();
    }

    public void WatchAndGetGift()
    {
        getRewardedButton.SetActive(false);

        CancelInvoke("UpdateRewardButton");

        AdMob.OnRewarded -= OnRewarded;
        AdMob.OnRewardedFailed -= OnRewardedFailed;
        AdMob.OnRewarded += OnRewarded;
        AdMob.OnRewardedFailed += OnRewardedFailed;

        AdMob.Instance.Show("Reward_Skin");

        /*IronSourceManager.OnRewarded -= OnRewarded;
        IronSourceManager.OnRewardedFailed -= OnRewardedFailed;
        IronSourceManager.OnRewarded += OnRewarded;
        IronSourceManager.OnRewardedFailed += OnRewardedFailed;

        IronSourceManager.Instance.ShowRewardedVideo("Reward_Skin");*/
    }

    public void GetDefaultGift()
    {
        CancelInvoke("UpdateRewardButton");

        LockStatus _ls = LockStatus.UNLOCKED;
        SelectionState _ss = gameManager.UnlockedItem.selectionState;
        HighLight _hl = gameManager.UnlockedItem.highlight;
        LockType _lt = LockType.NONE;
        int price = 0;

        shopManager.UnlockItem(gameManager.UnlockedItem, _ls, _ss, _hl, _lt, price);

        if(gameManager.UnlockedItem.shopSection == ShopSection.Accessory)
        {
            gameManager.sse.SyncAndSaveChanges();
            gameManager.UnlockedItem = null;
            gameManager.LoadLevel(false);
        }
        else
        {
            gameManager.LoadLevel(true);
		}
        
        //shopManager.BuyAccessory(gameManager.UnlockedItem);
    }

    public void LoseGift()
    {
        CancelInvoke("UpdateRewardButton");

        LockStatus _ls = gameManager.UnlockedItem.lockInfo.lockStatus;
        SelectionState _ss = gameManager.UnlockedItem.selectionState;
        HighLight _hl = gameManager.UnlockedItem.highlight;
        LockType _lt = gameManager.UnlockedItem.lockInfo.lockType == LockType.NONE ? LockType.NONE : LockType.BUY_FOR_MONEY;
        int price = gameManager.UnlockedItem.lockInfo.priceToUnlock == -1 ? 0 : gameManager.UnlockedItem.lockInfo.priceToUnlock;

        var lvl = new Dictionary<string, object>();
        lvl.Add("skin_name", gameManager.UnlockedItem.itemName);

        AnalyticEvents.ReportEvent("skin_lost", lvl);

        shopManager.UnlockItem(gameManager.UnlockedItem, _ls, _ss, _hl, _lt, price);

        NextScreen();
    }

    void NextScreen()
    {
        AdMob.OnRewarded -= OnRewarded;
        AdMob.OnRewardedFailed -= OnRewardedFailed;

        /*IronSourceManager.OnRewarded -= OnRewarded;
        IronSourceManager.OnRewardedFailed -= OnRewardedFailed;*/

        gameManager.LoadLevel(true);
    }

    IEnumerator ShowDelayedLoseButton()
    {
        yield return new WaitForSeconds(2f);

        loseIt.SetActive(true);
    }
}
