using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Caregivr
{
	public class LuxLaserable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		
		private ObjectIdentity objectIdentity;
		public HighlightRenderer highlightRenderer;
		public bool disabled;
		public UnityEvent OnLaserEnter;
		public UnityEvent OnLaserExit;
		public UnityEvent OnLaserClick;
		
		public float highlightThickness = 0.005f;
		public bool highlightControlOnOff = false;
		public Color highlightColor;
		private float originalHighlightThickness;
		private Color originalHighlightColor;

		void Awake()
		{
			objectIdentity = GetComponent<ObjectIdentity>();
			if (highlightRenderer != null)
			{
				originalHighlightColor = highlightRenderer.highlightColor;
				originalHighlightThickness = highlightRenderer.highlightWidth;
			}
		}

		
		public void OnLaserClicked()
		{
			if (disabled)
			{
				return;
			}
			if (objectIdentity != null)
			{
				objectIdentity.EmitEvent("clicked");
			}
			
			OnLaserClick.Invoke();
		}

		public void OnPointerEnter(PointerEventData pointerEventData)
		{
			if (disabled)
			{
				return;
			}
			//Debug.Log("[LuxLaserAble] OnPointerEnter");
			if (objectIdentity != null)
			{
				objectIdentity.EmitEvent("pointer_enter");	
			}
			if (highlightRenderer != null)
			{
				if (highlightControlOnOff)
					highlightRenderer.highlightEnabled = true;
				highlightRenderer.highlightWidth = highlightThickness;
				highlightRenderer.highlightColor = highlightColor;
			}

			OnLaserEnter.Invoke();
		}
		
		public void OnPointerExit(PointerEventData pointerEventData)
		{
			if (disabled)
			{
				return;
			}
			//Debug.Log("[LuxLaserAble] OnPointerExit");
			if (objectIdentity != null)
			{
				objectIdentity.EmitEvent("pointer_exit");
			}
			if (highlightRenderer != null)
			{
				if (highlightControlOnOff)
					highlightRenderer.highlightEnabled = false;
				highlightRenderer.highlightWidth = originalHighlightThickness;
				highlightRenderer.highlightColor = originalHighlightColor;
			}

			OnLaserExit.Invoke();
		}

		public void Disable()
		{
			Debug.Log("[LuxLaserable] Disable()");
			disabled = true;
		}
		public void Enable()
		{

			Debug.Log("[LuxLaserable] Enable()");
			disabled = false;
		}
	}
}
