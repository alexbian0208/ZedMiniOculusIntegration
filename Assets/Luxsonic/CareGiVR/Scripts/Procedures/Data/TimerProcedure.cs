using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Caregivr
{
	public class TimerProcedure : Procedure
	{
		public TimerProcedure(
			string id,
			string name,
			string description,
			ProcedureType type,
			VariantDictionary data
		) : base(id, name, description, type, data)
		{
			duration = GetDuration(data);
		}

		const float defaultDuration = 5.0f;
		public float duration { get; private set; }

		public override List<Procedure> SubProcedures()
		{
			return new List<Procedure> { this };
		}

		public override List<AtomicProcedure> GetActiveAtomicProcedures()
		{
			return new List<AtomicProcedure>();
		}

		public override int SubProcedureCount()
		{
			return 1;
		}
		
		public override void Activate()
		{
			base.Activate();
			State = ProcedureState.Completed;
			runtime.EmitDelayedProcedureEvent(new ProcedureEvent(this, EventNames.ProcedureCompleted, null), duration);
		}

		float GetDuration(VariantDictionary data)
		{
			if (data.ContainsKey(DataKeys.TimerDuration))
			{
				var durationVariant = data[DataKeys.TimerDuration];
				if (durationVariant.type == Variant.VariantType.Float)
				{
					return durationVariant.AsFloat();
				}
				else
				{
					Debug.LogErrorFormat("[TimerProcedure] procedure: {0} wrong type {1} for {2} data key", ID, durationVariant.type, DataKeys.TimerDuration);
				}
			}
			return defaultDuration;
		}
	}
}
