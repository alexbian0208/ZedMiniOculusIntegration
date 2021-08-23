using System;
using System.Collections.Generic;

namespace Caregivr
{
	public abstract class Procedure
	{
		public static class EventNames
		{
			public const string ModuleUnload 			= "module_unload";
			public const string ModuleLoaded 			= "module_loaded";
			public const string ProcedureActivated 		= "procedure_activated";
			public const string ProcedureDeactivated 	= "procedure_deactivated";
			public const string ProcedureCompleted 		= "procedure_completed";
		}

		public static class DataKeys
		{
			public const string DisableFloatingUI 	= "disable_floating_ui";
			public const string FloatingUIPosition 	= "floating_ui_position";
			public const string FloatingUIDisableLookAtPlayer = "floating_ui_disable_look_at_player";
			public const string FloatingUIDisableMinHeight = "floating_ui_disable_min_height";
			public const string ShowOnInfoUI		= "show_on_info_ui";
			public const string ShowProgress		= "show_progress";
			public const string Progress			= "progress";
			public const string GazeDuration		= "gaze_duration";
			public const string VoiceOverDisable	= "voice_over_disable";
			public const string VoiceOverText		= "voice_over_text";
			public const string TimerDuration		= "timer_duration";
		}

		public enum ProcedureType { Atomic, Sequential, Branching, Timer }
		public enum ProcedureState { InActive, Active, Completed }

		public string ID { get; protected set; }
		public string Name { get; protected set; }
		public string Description { get; protected set; }
		public ProcedureType Type { get; protected set; }
		public VariantDictionary Data { get; protected set; }
		public event Action OnCompletion;
		public ProcedureState State = ProcedureState.InActive;
		public ModuleRuntime runtime { get; private set; }

		public Procedure(
			string id,
			string name,
			string description,
			ProcedureType type,
			VariantDictionary data
		)
		{
			ID = id;
			Name = name;
			Description = description;
			Type = type;
			Data = data;
			State = ProcedureState.InActive;
		}

		public virtual void Activate()
		{
			if (State == ProcedureState.InActive)
			{
				State = ProcedureState.Active;
				EmitProcedureEvent(Procedure.EventNames.ProcedureActivated);
			}
		}

		public virtual void Deactivate()
		{
			if (State == ProcedureState.Active)
			{
				State = ProcedureState.InActive;
				EmitProcedureEvent(Procedure.EventNames.ProcedureDeactivated);
			}
		}

		public virtual void ForceComplete()
		{
			if (State != ProcedureState.Completed)
			{
				State = ProcedureState.Completed;
				EmitProcedureEvent(Procedure.EventNames.ProcedureCompleted);
			}
		}

		public virtual void ForceDeactivate()
		{
			if (State != ProcedureState.InActive)
			{
				State = ProcedureState.InActive;
				EmitProcedureEvent(Procedure.EventNames.ProcedureDeactivated);
			}
		}

		public abstract List<Procedure> SubProcedures();
		public abstract List<AtomicProcedure> GetActiveAtomicProcedures();
		public abstract int SubProcedureCount();

		public virtual void RegisterRuntime(ModuleRuntime _runtime) {
			this.runtime = _runtime;
		}

		protected void EmitCompletionEvent(VariantDictionary data = null)
		{
			State = ProcedureState.Completed;

			if (OnCompletion != null)
			{
				OnCompletion();
			}
			EmitProcedureEvent(EventNames.ProcedureCompleted, data);
		}

		protected void EmitProcedureEvent(string eventName, VariantDictionary data = null)
		{
			this.runtime.EmitProcedureEvent(new ProcedureEvent(this, eventName, data));
		}

		public virtual void Reset()
		{
			// Total hack for clearing progress
			// Would be so much better with value semantics on these procedures
			if (Data.ContainsKey(DataKeys.Progress))
			{
				Data.Remove(DataKeys.Progress);
			}
			ForceDeactivate();
		}
	}
}
