using System;
using System.Collections.Generic;
using UnityEngine;

namespace Caregivr
{
	public class SequentialProcedure : Procedure
	{
		public static class SequentialProcedureEventNames
		{
			public const string StepCompleted = "sequential_step_completed";
		}

		public event Action OnStepCompleted;
		public List<Procedure> Steps { get; protected set; }

		// currentStep will be null if the procedure is completed
		public int currentStepIndex { get; set; }
		public Procedure currentStep
		{
			get
			{
				return currentStepIndex >= 0 && currentStepIndex < Steps.Count ? Steps[currentStepIndex] : null;
			}
		}

		public SequentialProcedure(
			string id,
			string name,
			string description,
			ProcedureType type,
			VariantDictionary data,
			params Procedure[] steps
		) : base(id, name, description, type, data)
		{
			Steps = new List<Procedure>(steps);
			currentStepIndex = 0;
		}

		public override void Reset()
		{
			base.Reset();
			currentStepIndex = 0;
			foreach(var procedure in Steps)
			{
				procedure.Reset();
			}
		}
		
		public override List<Procedure> SubProcedures()
		{
			return Steps;
		}

		public bool HasCompletedAllSteps()
		{
			return currentStepIndex >= Steps.Count;
		}

		protected void NextStep()
		{
			// Advance step index
			currentStepIndex = Mathf.Min(currentStepIndex + 1, Steps.Count);
			EmitStepCompletionEvent();

			if (!HasCompletedAllSteps())
			{
				currentStep.Activate();
			}
			else
			{
				// Procedure is complete
				EmitCompletionEvent(new VariantDictionary());
			}
		}

		public override void RegisterRuntime(ModuleRuntime _runtime)
		{
			base.RegisterRuntime(_runtime);
			foreach (Procedure step in Steps)
			{
				step.RegisterRuntime(_runtime);
			}

			runtime.OnProcedureEvent.AddListener(OnProcedureEvent);
		}

		public override void Activate()
		{
			base.Activate();
			if (currentStep != null)
			{
				currentStep.Activate();
			}
		}

		protected void OnProcedureEvent(ProcedureEvent procedureEvent)
		{
			if (State != ProcedureState.Active)
			{
				return;
			}
			if (currentStep != null)
			{
				if (procedureEvent.eventName.Equals(EventNames.ProcedureCompleted))
				{
					if (procedureEvent.procedure.ID == currentStep.ID)
					{
						NextStep();
					}
				}
			}

		}

		protected void EmitStepCompletionEvent()
		{
			EmitProcedureEvent(SequentialProcedureEventNames.StepCompleted);
			if (OnStepCompleted != null)
			{
				OnStepCompleted();
			}
		}

		public override List<AtomicProcedure> GetActiveAtomicProcedures()
		{
			if (HasCompletedAllSteps())
			{
				return new List<AtomicProcedure>();
			}
			else
			{
				return new List<AtomicProcedure>(currentStep.GetActiveAtomicProcedures());
			}
		}

		public override int SubProcedureCount()
		{
			int numberOfSubProcedures = 0;
			foreach (Procedure step in Steps)
			{
				numberOfSubProcedures += step.SubProcedureCount();
			}
			return numberOfSubProcedures;
		}
	}
}
