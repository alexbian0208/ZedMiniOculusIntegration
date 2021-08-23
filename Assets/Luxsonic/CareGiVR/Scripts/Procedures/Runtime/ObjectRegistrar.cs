using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Caregivr
{
	public class ObjectRegistrar : MonoBehaviour
	{
		public int Count { get { return registrar.Count; }}

		public List<ObjectIdentity> registeredObjects { get { return registrar.Values.ToList(); }}

		private Dictionary<string, ObjectIdentity> registrar = new Dictionary<string, ObjectIdentity>();

		public void RegisterObject(ObjectIdentity identity)
		{
			Debug.AssertFormat(
				!registrar.ContainsKey(identity.Identifier),
				"[ObjectRegistrar] RegisterObject() identity '{0}' is already registered. Identifiers must be unique. The offending gameobject is: {1}",
				identity.Identifier,
				identity.gameObject.name
			);

			registrar.Add(identity.Identifier, identity);
		}

		public void UnregisterObject(ObjectIdentity identity)
		{
			Debug.AssertFormat(
				registrar.ContainsKey(identity.Identifier),
				"[ObjectRegistrar] LookupObject() identity '{0}' is not registered.",
				identity.Identifier,
				identity.gameObject.name
			);

			registrar.Remove(identity.Identifier);
		}

		public ObjectIdentity LookupObject(string identifier)
		{
			Debug.AssertFormat(
				registrar.ContainsKey(identifier),
				"[ObjectRegistrar] LookupObject() identity '{0}' is not registered.",
				identifier
			);

			return registrar[identifier];
		}
	}
}
