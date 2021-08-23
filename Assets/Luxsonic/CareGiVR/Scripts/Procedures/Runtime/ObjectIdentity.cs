using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Caregivr
{
	public class ObjectIdentity : MonoBehaviour
	{
		public string Identifier;

		// ObjectEvent
		// Arguments are (Identity, Event)
		public class ObjectEvent : UnityEvent<string, string, VariantDictionary> {};

		[HideInInspector]
		public ObjectEvent OnEvent = new ObjectEvent();
		public static ObjectEvent OnEventGlobal = new ObjectEvent();

		protected ObjectRegistrar registrar;

		protected void Awake()
		{
			registrar = GetComponentInParent<ObjectRegistrar>();
			Debug.AssertFormat(
				registrar != null,
				"[ObjectIdentity] Awake() gameObject {0} could not find registrar.", gameObject.name
			);
			registrar.RegisterObject(this);
		}

		protected void OnDestroy()
		{
			registrar.UnregisterObject(this);
		}

		public void EmitEvent(string eventName) { EmitEvent(eventName, new VariantDictionary()); }
		public void EmitEvent(string eventName, VariantDictionary data)
		{
			OnEvent.Invoke(Identifier, eventName, data);
		}

	}
}
