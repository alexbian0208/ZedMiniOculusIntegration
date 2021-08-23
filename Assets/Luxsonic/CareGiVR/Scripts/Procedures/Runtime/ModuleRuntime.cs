using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Caregivr
{
	[RequireComponent(typeof (ObjectRegistrar))]
	public class ModuleRuntime : MonoBehaviour
	{
		public GameObject SceneRoot;
		public GameObject ScenePrefab;
		public TextAsset InitialModuleText;

		public class ProcedureUnityEvent : UnityEvent<ProcedureEvent> {}
		[HideInInspector]
		[System.NonSerialized]
		public ProcedureUnityEvent OnProcedureEvent = new ProcedureUnityEvent();
	
		public Procedure procedure { get; protected set; }
		public ObjectRegistrar registrar { get; protected set; }
		

		private Queue<ProcedureEvent> deferredProcedureEvents;
		public class DelayedProcedureEvent : IComparable<DelayedProcedureEvent>
		{
			public DelayedProcedureEvent(float t, ProcedureEvent p) {this.time = t; this.ProcedureEvent = p;}
			public float time {get; private set;}
			public ProcedureEvent ProcedureEvent {get; private set;}
			// Sortable by time
			public int CompareTo(DelayedProcedureEvent other) { return time.CompareTo(other.time); }
		}
		private List<DelayedProcedureEvent> delayedProcedureEvents;

		protected void Awake()
		{
			deferredProcedureEvents = new Queue<ProcedureEvent>();
			delayedProcedureEvents = new List<DelayedProcedureEvent>();
			registrar = GetComponent<ObjectRegistrar>();
			
		}

		void Start()
		{
			if (InitialModuleText != null)
			{
				Procedure procedure = JsonParser.ParseProcedureString(InitialModuleText.text);
				if (procedure != null)
				{
					LoadProcedure(procedure);
				}
			}

		}

		public void LoadProcedure(Procedure p)
		{
			StartCoroutine(loadProcedureCoroutine(p));
		}

		private IEnumerator loadProcedureCoroutine(Procedure p)
		{
			if (procedure == null)
			{
				procedure = new AtomicProcedure("null", "null", "null", Procedure.ProcedureType.Atomic, new VariantDictionary(), "", "");
			}
			
			EmitProcedureEvent(new ProcedureEvent(procedure, Procedure.EventNames.ModuleUnload));
			yield return new WaitForSeconds(1.0f);
			procedure.Reset();
			if (SceneRoot != null)
			{
				DestroyImmediate(SceneRoot);
			}

			if (ScenePrefab != null)
			{
				SceneRoot = Instantiate(ScenePrefab, Vector3.zero, Quaternion.identity, this.transform) as GameObject;
			}
			else
			{
				Debug.LogWarning("[ModuleRuntime] Has no ScenePrefab. Not instantiating SceneRoot.");
			}

			procedure = p;
		
			yield return new WaitForSeconds(0.1f);
			procedure.RegisterRuntime(this);
			EmitProcedureEvent(new ProcedureEvent(procedure, Procedure.EventNames.ModuleLoaded));
			yield return new WaitForSeconds(0.5f);
			procedure.Activate();

		}

		public void EmitProcedureEvent(ProcedureEvent procedureEvent)
		{
			Debug.Log("[ModuleRuntime] EmitProcedureEvent " + procedureEvent);
			deferredProcedureEvents.Enqueue(procedureEvent);
			// Restart the procedure at the end
			if (procedureEvent.procedure.ID.Equals("basic_procedure") && procedureEvent.eventName.Equals(Procedure.EventNames.ProcedureCompleted))
			{
				Invoke("Start", 0.5f);
			}
		}

		public void EmitDelayedProcedureEvent(ProcedureEvent procedureEvent, float delay)
		{
			delayedProcedureEvents.Add(new DelayedProcedureEvent(Time.time + delay, procedureEvent));
			delayedProcedureEvents.Sort();
		}
		
		void Update()
		{
			// ProcedureEvents are enqueued and drained at the end of a frame
			// So that user actions (eg clicking a button) are completely consumed
			// and finished before the Procedure system evaluates the effects of this.
			// This fixes a bug where a single user action can complete multiple Procedures,
			// when it should have been consumed by the first one.
			while (deferredProcedureEvents.Count > 0)
			{
				ProcedureEvent procedureEvent = deferredProcedureEvents.Dequeue();
				OnProcedureEvent.Invoke(procedureEvent);
			}

			if (delayedProcedureEvents.Count > 0)
			{
				DelayedProcedureEvent delayedProcedureEvent = delayedProcedureEvents[0];
				if (Time.time >= delayedProcedureEvent.time)
				{
					delayedProcedureEvents.RemoveAt(0);
					OnProcedureEvent.Invoke(delayedProcedureEvent.ProcedureEvent);
				}
			}
		}
	}
}
