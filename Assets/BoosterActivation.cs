using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoosterActivation : MonoBehaviour
{
    [SerializeField] BoosterActivationManager boosterManager;
    [SerializeField] FakeInactivity fakeButtons;
    [SerializeField] GameObject checkMark;
    [SerializeField] GameObject GetButton;
    [SerializeField] GameObject RandomizeButton;
    [SerializeField] GameObject MaskObject;

    [SerializeField] Image boosterImage;
    [SerializeField] Image shadowImage;

    [SerializeField] Sprite simpleRectangle;
    [SerializeField] Sprite framedRectangle;
    [SerializeField] TextMeshProUGUI description;

    [SerializeField] int boosterIndex, clickedIndex;

    List<Image> AllIconImages = new List<Image>();

    float finalYcoordinate = -256;
    float deltaY = 512 + 64;
    float gap = 5f;
    public void Init(Sprite icon, string _description, List<Sprite> _allIcons)
	{
        if(AllIconImages.Count == 0)
            for(int i = 0; i < MaskObject.transform.childCount; i++)
                AllIconImages.Add(MaskObject.transform.GetChild(i).GetComponent<Image>());

        for(int i = 0; i < MaskObject.transform.childCount; i++)
		{ 
            RectTransform r = AllIconImages[i].transform as RectTransform;
            r.anchoredPosition = Vector2.up * ( finalYcoordinate + deltaY * i);
        }

        boosterImage = AllIconImages[AllIconImages.Count - 1];

        PopulateMask(icon, _allIcons);

        description.text = _description;
        description.enabled = false;

        clickedIndex = -99;

        GetButton.SetActive(false);
        RandomizeButton.SetActive(true);
        fakeButtons.FadeIn(true);
        EnableCheckMark(false);

        shadowImage.sprite = simpleRectangle;

        CancelInvoke("UpdateRewardButton");
        InvokeRepeating("UpdateRewardButton", 0, 1);
    }

    void PopulateMask(Sprite correctIcon, List<Sprite> _allIcons)
    {
        List<Sprite> AllIcons = new List<Sprite>(_allIcons);

        AllIcons.Remove(correctIcon);
        Extensions.Shuffle(AllIcons);

        int iconIndex = 0;

		for(int i = 0; i < AllIconImages.Count; i++)
		{
            AllIconImages[i].sprite = AllIcons[iconIndex];
            iconIndex++;

            if(iconIndex == AllIcons.Count) iconIndex = 0;
        }

        boosterImage.sprite = correctIcon;
    }

    public void ChangeYPosition(  float deltaPosition = -2.304f)
    {
        for(int i = 0; i < AllIconImages.Count; i++)
        {
            RectTransform r = AllIconImages[i].transform as RectTransform;
            r.anchoredPosition += Vector2.up * deltaPosition;
        }
    }

    public void EnableDescription() 
    { 
        description.enabled = true;

        for(int i = 0; i < AllIconImages.Count; i++)
        {
            RectTransform r = AllIconImages[i].transform as RectTransform;
            r.anchoredPosition = Vector2.up * (finalYcoordinate - ( deltaY * ( AllIconImages.Count - 1 - i)));
        }
    }

	public void EnableCheckMark( bool status)
    {
        checkMark.SetActive(status);
    }

    public void WatchAddGetBooster( bool _skipAd = false )
    {
        clickedIndex = boosterIndex;

        RandomizeButton.SetActive(false);

        CancelInvoke("UpdateRewardButton");

        AdMob.OnRewarded -= OnRewarded;
        AdMob.OnRewardedFailed -= OnRewardedFailed;
        AdMob.OnRewarded += OnRewarded;
        AdMob.OnRewardedFailed += OnRewardedFailed;

        if(_skipAd)
        {
            GetButton.SetActive(true);
        }
        else
        {
            AdMob.Instance.Show("reward_booster");

            /*IronSourceManager.OnRewarded -= OnRewarded;
            IronSourceManager.OnRewardedFailed -= OnRewardedFailed;
            IronSourceManager.OnRewarded += OnRewarded;
            IronSourceManager.OnRewardedFailed += OnRewardedFailed;

            IronSourceManager.Instance.ShowRewardedVideo("Reward_Skin");*/
		}
    }

    public void GetBooster()
    {
        if(clickedIndex != boosterIndex) return;
        else clickedIndex = -99;

        CancelInvoke("UpdateRewardButton");

        GetButton.SetActive(false);
        shadowImage.sprite = framedRectangle;
        boosterManager.ActivateBooster(boosterIndex);
        EnableCheckMark(true);
    }

    private void OnDisable()
    {
        AdMob.OnRewarded -= OnRewarded;
        AdMob.OnRewardedFailed -= OnRewardedFailed;
    }

    private void UpdateRewardButton()
    {
        bool isAdReady = false;

        isAdReady = AdMob.Instance.IsReady("reward_booster");

        fakeButtons.FadeIn(!isAdReady);

        //RandomizeButton.SetActive(isAdReady);
    }

    private void OnRewarded()
    {
        if(clickedIndex != boosterIndex) return;

        AdMob.OnRewarded -= OnRewarded;
        AdMob.OnRewardedFailed -= OnRewardedFailed;

        RandomizeButton.SetActive(false);
        GetButton.SetActive(true);
        GetBooster();
    }

    private void OnRewardedFailed()
    {
        if(clickedIndex != boosterIndex) return;

        AdMob.OnRewarded -= OnRewarded;
        AdMob.OnRewardedFailed -= OnRewardedFailed;
    }
}
