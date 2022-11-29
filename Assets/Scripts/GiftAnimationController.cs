using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GiftAnimationController : MonoBehaviour
{
    GameManager gm;

	[SerializeField] Image loadingIMage;
	[SerializeField] TextMeshProUGUI progressText;
	[SerializeField] TextMeshProUGUI commingSoon;

    float skinProgressValue;

    Coroutine specialEffectCoroutine;

    public void UpdateGiftProgress()
	{
        if(gm == null) gm = GameManager.GetInstance();

        float rate = gm.Config.unlockSkinRate;

        int beginningRate = gm.CurrentLevelIndex - 1;

        skinProgressValue = ( beginningRate % rate) / rate;

        if( skinProgressValue != 0f)
            loadingIMage.fillAmount = (beginningRate % rate) / rate - 1 / rate;

        UpdateSkinSilhouetteProgress(skinProgressValue);
    }

    void UpdateSkinSilhouetteProgress(float percentage)
    {
        if(specialEffectCoroutine != null) StopCoroutine(specialEffectCoroutine);

        if(gm.CheckIfItemUnlockeAvailable())
        {
            loadingIMage.transform.parent.gameObject.SetActive(true);
            progressText.enabled = true;

            if(loadingIMage.fillAmount == 1) loadingIMage.fillAmount = 0f;

            if(percentage == 0) { percentage = 1; skinProgressValue = 1f; }

            progressText.text = $"{percentage * 100}%";

            specialEffectCoroutine = StartCoroutine(SilhouetteSpecialEffect(percentage));
        }
        else
        {
            commingSoon.enabled = true;

            loadingIMage.transform.parent.gameObject.SetActive(false);
            progressText.enabled = false;
        }
    }

    IEnumerator SilhouetteSpecialEffect(float _newFillPercentage)
    {
        float currentFillAmount = loadingIMage.fillAmount;
        float TargetFillAmount = _newFillPercentage;

        float fillTime = 0.4f;
        float fillAmountStep = (TargetFillAmount - currentFillAmount) / (fillTime * 100f);
        float yieldTime = fillTime / fillAmountStep;

        yield return new WaitForSeconds(0.25f);

        while(currentFillAmount < TargetFillAmount)
        {
            currentFillAmount += fillAmountStep;
            loadingIMage.fillAmount = currentFillAmount;

            yield return null;
        }

        loadingIMage.fillAmount = TargetFillAmount;
    }

    public float GetSkinProgressValue() { return skinProgressValue; }
}
