using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Caregivr
{
	public class LuxLaserPointer : MonoBehaviour
	{
		public LuxInput luxInput;
		public LineRenderer lineRenderer;
		public GameObject reticle;
		public LuxHand luxHand;

		private bool lastWasClicking = false;

		RaycastHit raycastHit;
		const float raycastMaxLength = 10.0f;
		const float distanceToFullAlpha = 1.0f;
		public LuxHand.LuxHandDirection handDirection;

		GameObject lastHoveringObject = null;

		GameObject CastRay()
		{
			Ray raycast = new Ray(lineRenderer.transform.position, lineRenderer.transform.forward);

			if (Physics.Raycast(raycast, out raycastHit, raycastMaxLength, 1, QueryTriggerInteraction.Collide))
			{
				return raycastHit.collider.gameObject;
			}
			return null;
		}

		void Update()
		{
			// Show & Hide LineRenderer
			if (luxInput.GetLuxInputController(handDirection).GripButton() && luxHand.attachedPickupable == null)
			{
				lineRenderer.enabled = true;
			}
			else
			{
				lineRenderer.enabled = false;
			}

			bool isClicking = false;
			GameObject hitgameObject = null;
			bool showReticle = false;
			if (lineRenderer.enabled)
			{
				//Debug.Log("[LuxLaserPointer] Casting Ray");
				hitgameObject = CastRay();
				float lineDistance = hitgameObject != null ? raycastHit.distance : raycastMaxLength;
				string hitStr = hitgameObject == null ? "null" : hitgameObject.ToString();
				//Debug.Log("[LuxLaserPointer] hitGameObject:  " + hitStr + " lineDistance " + lineDistance);
				float lengthToFullAlpha = Mathf.Min(distanceToFullAlpha, lineDistance);

				// Set the length of the line to match the ray cast
				{
					lineRenderer.SetPosition(1, new Vector3(0.0f, 0.0f, lengthToFullAlpha));
					lineRenderer.SetPosition(2, new Vector3(0.0f, 0.0f, lineDistance));
				}

				// Set the color gradient to be consistently sized
				{
					lineRenderer.colorGradient.alphaKeys[1].time = distanceToFullAlpha / lineDistance;

				}

				// Click buttons
				isClicking = (luxInput.GetLuxInputController(handDirection).AButton() || luxInput.GetLuxInputController(handDirection).TriggerButton());
				if (hitgameObject != null)
				{
					
					/*
					if (hoveringButton)
					{
						Debug.Log("[LuxLaserPointer] ray hit button: " + currentHoveringObject.gameObject.name);
					}
					*/
					Button hoveringButton = hitgameObject.GetComponentInParent<Button>();
					LuxLaserable hoveringLaserable = hitgameObject.GetComponentInParent<LuxLaserable>();

					showReticle = hoveringButton != null || hoveringLaserable != null;
					
					// Handle Clicks
					if (isClicking && !lastWasClicking)
					{
						//Debug.Log("[LuxLaserPointer] Ray Hit: " + hitgameObject);
						
						if (hoveringButton != null)
						{
							hoveringButton.onClick.Invoke();
						}
						// Click LuxClickables
						if (hoveringLaserable != null)
						{
							hoveringLaserable.OnLaserClicked();
						}
					}
				}
				
			}

			// Reticle
			if (reticle)
			{
				if (showReticle)
				{
					// Show reticle
					reticle.gameObject.SetActive(true);
					reticle.transform.position = raycastHit.point;
					reticle.transform.forward = raycastHit.normal;
				}
				else
				{
					// Hide Reticle
					reticle.gameObject.SetActive(false);
				}
			}
			// Pointer Enter/Exit
			PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
			if (hitgameObject != lastHoveringObject)
			{
				if (ExecuteEvents.CanHandleEvent<IPointerEnterHandler>(hitgameObject))
				{
					ExecuteEvents.Execute(hitgameObject, pointerEventData, ExecuteEvents.pointerEnterHandler);
				}
				if (lastHoveringObject != null)
				{
					if (ExecuteEvents.CanHandleEvent<IPointerExitHandler>(lastHoveringObject))
					{
						ExecuteEvents.Execute(lastHoveringObject, pointerEventData, ExecuteEvents.pointerExitHandler);
					}
				}
			}
			lastHoveringObject = hitgameObject;
			lastWasClicking = isClicking;
		}
	}
}
