using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Caregivr
{
	public class LuxFadeSphere : MonoBehaviour
	{
		MeshRenderer meshRenderer;
		private const float fadeDuration = 0.1f;

		void Start()
		{
			meshRenderer = GetComponent<MeshRenderer>();
			
			Debug.AssertFormat(
				meshRenderer != null,
				"[LuxFadeSphere] Couldn't find mesh renderer."
			);
		}
		
		public void Fade(TweenCallback callback = null, float duration = fadeDuration)
		{	
			Sequence tweenSequence = FadeOut(callback, duration);
			FadeIn(tweenSequence, duration);
		}

		public Sequence FadeOut(TweenCallback callback = null, float duration = fadeDuration)
		{
			Sequence tweenSequence = DOTween.Sequence()
				.Append(meshRenderer.material
					.DOColor(Color.black, duration)
					.From(Color.clear)
					.OnStart(EnableGameObject));
			
			if (callback != null)
			{
				tweenSequence.AppendCallback(callback);
			}
			return tweenSequence;
		}

		public void FadeIn(Sequence tweenSequence = null, float duration = fadeDuration)
		{
			if (tweenSequence == null)
			{
				tweenSequence = DOTween.Sequence();
			}

			tweenSequence.Append(meshRenderer.material
				.DOColor(Color.clear, duration)
				.From(Color.black)
				.OnComplete(DisableGameObject));
		}

		void DisableGameObject()
		{
			this.meshRenderer.gameObject.SetActive(false);
		}
		void EnableGameObject()
		{
			this.meshRenderer.gameObject.SetActive(true);
		}
	}
}
