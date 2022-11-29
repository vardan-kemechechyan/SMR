using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompassAnimation : MonoBehaviour
{
	[SerializeField] Material arrowMaterial;
	[SerializeField] Transform player;
	[SerializeField] LineRenderer lineRenderer;

	private void Update()
	{
		lineRenderer.SetPosition(0, new Vector3(player.position.x, 0.1f, player.position.z));
		arrowMaterial.mainTextureOffset += Vector2.left*0.02f;
	}

	public void SetEndPosition(Vector3 targetObject)
	{
		lineRenderer.SetPosition(1, new Vector3(targetObject.x, 0.1f, targetObject.z));
	}
}
