using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caregivr
{
	public class LuxHandHovering : MonoBehaviour
	{
		public LuxHand luxHand;
		LuxTriggerTracker triggerTracker;

		public List<LuxPickupable> hoveringPickupables { get; private set; }
		public List<HandPoseRadius> hoveringPoseOverriders { get; private set; }
		public LuxPickupable closestHoveringPickupable { get; private set; }

		void Awake()
		{
			triggerTracker = GetComponentInChildren<LuxTriggerTracker>();
			hoveringPickupables = new List<LuxPickupable>();
			hoveringPoseOverriders = new List<HandPoseRadius>();
		}
		
		void Start()
		{
			triggerTracker.OnTriggerEnterEvent.AddListener(OnTriggerEnterEvent);
			triggerTracker.OnTriggerExitEvent.AddListener(OnTriggerExitEvent);
		}

		void OnTriggerEnterEvent(GameObject hoveredGameObject)
		{	
			// Hand Posers
			{
				HandPoseRadius handPoseRadius = hoveredGameObject.GetComponentInParent<HandPoseRadius>();
				if (handPoseRadius != null)
				{
					if (handPoseRadius.allowedHands.AllowedHandsCompare(luxHand.handDirection))
					{
						if (!hoveringPoseOverriders.Contains(handPoseRadius))
						{
							hoveringPoseOverriders.Add(handPoseRadius);
						}
					}
				}
			}

			// Pickupables
			{
				LuxPickupable pickupable = hoveredGameObject.GetComponentInParent<LuxPickupable>();
				if (pickupable != null)
				{
					if (!hoveringPickupables.Contains(pickupable))
					{
						hoveringPickupables.Add(pickupable);
					}
				}
			}
		}

		void OnTriggerExitEvent(GameObject exitHoverGameObject)
		{
			// Pickupables
			{
				LuxPickupable pickupable = exitHoverGameObject.GetComponentInParent<LuxPickupable>();
				if (pickupable != null)
				{
					if (hoveringPickupables.Contains(pickupable))
					{
						hoveringPickupables.Remove(pickupable);
						pickupable.EndHighlight();
					}
				}
			}

			// Hand Posers
			{
				HandPoseRadius handPoseRadius = exitHoverGameObject.GetComponentInParent<HandPoseRadius>();
				if (handPoseRadius != null)
				{
					if (hoveringPoseOverriders.Contains(handPoseRadius))
					{
						hoveringPoseOverriders.Remove(handPoseRadius);
					}
				}
			}
		}

		void Update()
		{
			// Remove inactive pickupables
			hoveringPickupables.RemoveAll( (LuxPickupable hoveringPickupable) => {
				return !hoveringPickupable.gameObject.activeInHierarchy;
			});

			// Remove inactive pose overriders
			hoveringPoseOverriders.RemoveAll( (HandPoseRadius hoveringPoseOverrider) => {
				return !hoveringPoseOverrider.gameObject.activeInHierarchy;
			});

			// Sort hoverers
			hoveringPoseOverriders.Sort((HandPoseRadius one, HandPoseRadius two) => {
				float distanceOne = Vector3.Distance(one.transform.position, this.transform.position);
				float distanceTwo = Vector3.Distance(two.transform.position, this.transform.position);
				return distanceOne.CompareTo(distanceTwo);
			});

			// Find Closest valid pickupable
			closestHoveringPickupable = null;
			float closestDistance = float.PositiveInfinity;
			foreach(LuxPickupable hoveringPickupable in hoveringPickupables)
			{
				if (hoveringPickupable.isGrabbable && !hoveringPickupable.isAttached)
				{
					float distance = Vector3.Distance(hoveringPickupable.transform.position, transform.position);
					if (distance < closestDistance)
					{
						closestHoveringPickupable = hoveringPickupable;
						closestDistance = distance;
					}
				}
			}

			// Highlight Closest
			foreach(LuxPickupable hoveringPickupable in hoveringPickupables)
			{
				if (hoveringPickupable == closestHoveringPickupable)
				{
					hoveringPickupable.StartHighlight();
				}
				else 
				{
					hoveringPickupable.EndHighlight();
				}
			}
		}
	}
}
