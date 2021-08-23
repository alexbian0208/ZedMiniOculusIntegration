using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Caregivr
{
	public class LuxTriggerTracker : MonoBehaviour
	{
		public List<Collider> overlapping {get; private set;}

		public class LuxTriggerEvent: UnityEvent<GameObject> {}
		
		[System.NonSerialized]
		public LuxTriggerEvent OnTriggerEnterEvent = new LuxTriggerEvent();
		public LuxTriggerEvent OnTriggerExitEvent = new LuxTriggerEvent();
		List<Collider> objectsToRemove = new List<Collider>();

		void Awake()
		{
			overlapping = new List<Collider>();
		}

		void OnTriggerEnter(Collider collider)
		{
			if (collider.gameObject.name.Equals("LymphFingerGhostRight"))
			{
				Debug.Log("[LuxTriggerTracker] LymphFingerGhostRight ENTER");
			}
			overlapping.Add(collider);
			OnTriggerEnterEvent.Invoke(collider.gameObject);
		}

		void OnTriggerExit(Collider collider)
		{
			if (collider.gameObject.name.Equals("LymphFingerGhostRight"))
			{
				Debug.Log("[LuxTriggerTracker] LymphFingerGhostRight EXIT");
			}
			overlapping.Remove(collider);
			OnTriggerExitEvent.Invoke(collider.gameObject);
		}

		void Update()
		{
			foreach (Collider hoveringCollider in overlapping)
			{
				if (hoveringCollider.gameObject.name.Equals("LymphFingerGhostRight"))
				{
					Debug.LogFormat("[LuxTriggerTracker] LymphFingerGhostRight active: {0} enabled: {1}", hoveringCollider.gameObject.activeInHierarchy, hoveringCollider.enabled);
					int breaker = 0;
					if (!hoveringCollider.gameObject.activeInHierarchy || !hoveringCollider.enabled)
					{
						breaker++;
					}
				}

				if (!hoveringCollider.gameObject.activeInHierarchy || !hoveringCollider.enabled)
				{
					objectsToRemove.Add(hoveringCollider);
				}
			}

			if (objectsToRemove.Count > 0)
			{
				foreach (Collider hoveringCollider in objectsToRemove)
				{
					overlapping.Remove(hoveringCollider);
					OnTriggerExitEvent.Invoke(hoveringCollider.gameObject);
				}
				objectsToRemove.Clear();
			}
		}
	}
}
