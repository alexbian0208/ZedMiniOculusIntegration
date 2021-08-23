using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Caregivr
{
	public class SimpleLaserPointer : MonoBehaviour
	{
		public LuxInput luxInput;
		public LuxHand.LuxHandDirection handDirection;
		public LineRenderer lineRenderer;
		public GameObject reticle;
		public LayerMask layerMask;

		private bool lastWasClicking = false;

		RaycastHit raycastHit;
		const float raycastMaxLength = 10.0f;
		const float distanceToFullAlpha = 1.0f;

		GameObject lastHoveringObject = null;
		public LuxInputController controller { get { return luxInput.GetLuxInputController(handDirection); }}

		bool inputClicking()
		{
			return controller.AButton() || controller.BButton() || controller.TriggerButton();
		}

		bool inputLasering()
		{
			return controller.GripButton();
		}

		GameObject CastRay()
		{
			Ray raycast = new Ray(lineRenderer.transform.position, lineRenderer.transform.forward);

			if (Physics.Raycast(raycast, out raycastHit, raycastMaxLength, layerMask.value, QueryTriggerInteraction.Collide))
			{
				//Debug.Log("[SimpleLaserPointer] ray hit: " + raycastHit.collider.gameObject.name);
				return raycastHit.collider.gameObject;
			}
			return null;
		}

		void Update()
		{
			// Show & Hide LineRenderer
			if (inputLasering())
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
					lineRenderer.SetPosition(1, new Vector3(0.0f, 0.0f, lineDistance));
				}

				// Click buttons
				isClicking = inputClicking();
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
