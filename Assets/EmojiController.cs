using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmojiController : MonoBehaviour
{
	[System.Serializable]
	public class EmojiDescription
	{
		public EffectStyle effectName;
		public List<Sprite> emojiVariants;
		public List<GameObject> emojiEffectVariants;
	}

	[SerializeField] List<EmojiDescription> emojiList;
	[SerializeField] SpriteRenderer emojiContainer;
	[SerializeField] Transform youText;

	public void HideEmojis()
	{
		//emojiContainer.enabled = false;

		if(!youText.gameObject.activeSelf)
			YouTextEnabled(true);

		foreach(var emoji in emojiList)
			foreach(var effect in emoji.emojiEffectVariants)
				effect.SetActive(false);
	}

	public void EnableEmojiEffect( EffectStyle _style, float Y_correction = 2f )
	{
		//emojiContainer.enabled = true;

		HideEmojis();

		foreach(var emoji in emojiList)
		{
			if(_style == emoji.effectName)
			{
				//emojiContainer.sprite = emoji.emojiVariants[Random.Range(0, emoji.emojiVariants.Count)];
				emoji.emojiEffectVariants[Random.Range(0, emoji.emojiEffectVariants.Count)].SetActive(true);

				Vector3 pos = emojiContainer.transform.localPosition;
				pos.y = Y_correction;
				emojiContainer.transform.localPosition = pos;

				//youText.localPosition.y = -0.5f;

				break;
			}
		}
	}

	public void YouTextEnabled( bool status) 
	{
		youText.gameObject.SetActive(status);
	}

	public void RecalculatePosition(float Y_correction = 2f)
	{
		Vector3 pos = emojiContainer.transform.localPosition;
		pos.y = Y_correction;
		emojiContainer.transform.localPosition = pos;
	}
}

public enum EffectStyle
{
	POSITIVE,
	NEGATIVE,
	LOST
}
