using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  Caregivr
{
	public class ActiveObjectHighlightRenderer : MonoBehaviour
	{
		ModuleRuntime moduleRuntime;
		Dictionary<string, HighlightRenderer> instantiatedInWorldUIs;
		
		public Color highlightColor = new Color(0.462745f, 0.7450981f, 0.8156863f);
		
		void Awake()
		{
			moduleRuntime = GetComponentInParent<ModuleRuntime>();
			instantiatedInWorldUIs = new Dictionary<string, HighlightRenderer>();
			moduleRuntime.OnProcedureEvent.AddListener(OnProcedureEvent);

		}

		void Start()
		{
		}

		void OnProcedureEvent(ProcedureEvent procedureEvent)
		{
			if (procedureEvent.eventName.Equals(Procedure.EventNames.ProcedureActivated)||
				procedureEvent.eventName.Equals(Procedure.EventNames.ProcedureCompleted))
			{
				DestroyUnneededUIs(moduleRuntime.procedure);
				HighlightActiveObjects(moduleRuntime.procedure);
			}
		}

		void HighlightActiveObjects(Procedure procedure)
		{
			var activeProcedures = procedure.GetActiveAtomicProcedures();
			foreach (var activeProcedure in activeProcedures)
			{
				if (!instantiatedInWorldUIs.ContainsKey(activeProcedure.ID))
				{
					var activeObject = moduleRuntime.registrar.LookupObject(activeProcedure.ObjectIdentifier);
					var highlightRenderer = activeObject.gameObject.AddComponent<HighlightRenderer>();
					highlightRenderer.highlightEnabled = true;
					highlightRenderer.overrideColor = true;
					highlightRenderer.highlightColor = highlightColor;
					highlightRenderer.highlightWidth = 0.0025f;
					instantiatedInWorldUIs.Add(activeProcedure.ID, highlightRenderer);
				}
			}
		}

		void DestroyUnneededUIs(Procedure procedure)
		{
			List<AtomicProcedure> activeAtomics = procedure.GetActiveAtomicProcedures();
			
			// Map activeAtomics -> activeProcedureIds
			List<string> activeProcedureIDs = new List<string>();
			foreach (AtomicProcedure atomicProcedure in activeAtomics)
			{
				activeProcedureIDs.Add(atomicProcedure.ID);
			}

			// Filter instantiatedInWorldUIs for keys not in activeProcedureIds
			List<string> keysToRemove = new List<string>();
			var enumerator = instantiatedInWorldUIs.GetEnumerator();
			while (enumerator.MoveNext())
			{
				string procedureID = enumerator.Current.Key;
				if (!activeProcedureIDs.Contains(procedureID))
				{
					keysToRemove.Add(procedureID);
				}
			}

			// Remove UIs in keysToRemove
			foreach(string procedureIDToRemove in keysToRemove)
			{
				var inWorldInstance = instantiatedInWorldUIs[procedureIDToRemove];
				Destroy(inWorldInstance);
				instantiatedInWorldUIs.Remove(procedureIDToRemove);
				
			}
		}
	}
}
