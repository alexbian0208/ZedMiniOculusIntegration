using System.Collections.Generic;
using UnityEngine;

namespace Caregivr
{
	public class AtomicProcedure : Procedure
	{
		public string ObjectIdentifier { get; protected set; }
		public string CompletionEvent { get; protected set; }

		public AtomicProcedure(
			string id,
			string name,
			string description,
			ProcedureType type,
			VariantDictionary data,
			string objectIdentifier,
			string completionEvent
		) : base(id, name, description, type, data)
		{
			ObjectIdentifier = objectIdentifier;
			CompletionEvent = completionEvent;
		}

		public override List<Procedure> SubProcedures()
		{
			return new List<Procedure>();
		}

		public override List<AtomicProcedure> GetActiveAtomicProcedures()
		{
			if (State == ProcedureState.Active)
			{
				return new List<AtomicProcedure> { this };
			}
			return new List<AtomicProcedure>();
		}
		
		public override int SubProcedureCount()
		{
			return 1;
		}

		public override void RegisterRuntime(ModuleRuntime _runtime)
		{
			base.RegisterRuntime(_runtime);
			ObjectIdentity worldObject = _runtime.registrar.LookupObject(ObjectIdentifier);
			Debug.AssertFormat(
				worldObject != null, 
				"[AtomicProcedure] RegisterObjects() could not find object with identity: {0}",
				ObjectIdentifier
			);
			worldObject.OnEvent.AddListener(OnObjectEvent);
		}
		
		protected void OnObjectEvent(string identity, string eventName, VariantDictionary data)
		{
			//Debug.LogFormat("[AtomicProcedure] OnObjectEvent({0}, {1}) caught by {2}", identity, eventName, ID);
			if (State == ProcedureState.Active)
			{
				if (eventName.Equals(CompletionEvent))
				{
					EmitCompletionEvent(data);
				}
			}
		}
	}
}
