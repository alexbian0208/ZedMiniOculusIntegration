using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caregivr
{
	public class BranchingProcedure : Procedure
	{
		public Dictionary<Procedure, Procedure> branches;
		public Procedure selectedBranch = null;
		public bool hasSelectedBranch { get { return selectedBranch != null; } }

		public BranchingProcedure(
			string id,
			string name,
			string description,
			ProcedureType type,
			VariantDictionary data,
			Dictionary<Procedure, Procedure> branches
		) : base(id, name, description, type, data)
		{
			this.branches = branches;
		}

		public override void Reset()
		{
			base.Reset();
			selectedBranch = null;
			foreach(var branchKVP in branches)
			{
				branchKVP.Key.Reset();
				branchKVP.Value.Reset();
			}
		}

		public override void RegisterRuntime(ModuleRuntime _runtime)
		{
			base.RegisterRuntime(_runtime);
			foreach (var procedureTuple in branches)
			{
				procedureTuple.Key.RegisterRuntime(_runtime);
				procedureTuple.Value.RegisterRuntime(_runtime);
			}
			runtime.OnProcedureEvent.AddListener(OnProcedureEvent);
		}

		// TODO remove this, its not used and is incorrect
		public override int SubProcedureCount()
		{
			return branches.Count;
		}

		public override List<Procedure> SubProcedures()
		{
			// My sub procedures are both all my starting procedures
			// And all my branches
			List<Procedure> subProcedures = new List<Procedure>();
			foreach (var procedureTuple in branches)
			{
				subProcedures.Add(procedureTuple.Key);
				subProcedures.Add(procedureTuple.Value);
			}
			return subProcedures;
		}

		public override List<AtomicProcedure> GetActiveAtomicProcedures()
		{
			if (hasSelectedBranch)
			{
				// A branch has been selected
				// Return the branch's active atomics
				return selectedBranch.GetActiveAtomicProcedures();
			}
			else
			{
				// A branch has not been selected,
				// Return the active atomics for the starting procedures
				List<AtomicProcedure> activeAtomics = new List<AtomicProcedure>();
				foreach (var procedureTuple in branches)
				{
					activeAtomics.AddRange(procedureTuple.Key.GetActiveAtomicProcedures());
				}
				return activeAtomics;
			}
		}

		public override void Activate()
		{
			base.Activate();
			if (selectedBranch != null)
			{
				// A branch has been selected,
				// Activate the selected branch
				selectedBranch.Activate();
			}
			else
			{
				// A branch has not been selected
				// Activate the branch starting procedures
				ActivateBranchStarters();
			}
		}

		void OnProcedureEvent(ProcedureEvent procedureEvent)
		{
			if (selectedBranch == null)
			{
				if (State != ProcedureState.Active)
				{
					return;
				}
				// A branch has not been selected,
				// See if one of our active branch starters has been completed
				foreach (var procedureTuple in branches)
				{
					Procedure startingProcedure = procedureTuple.Key;
					Procedure branchProcedure = procedureTuple.Value;
					// Check if this event is about one of our branch starters
					if (procedureEvent.procedure.ID.Equals(startingProcedure.ID))
					{
						// Check if this is a completion event
						if (procedureEvent.eventName.Equals(Procedure.EventNames.ProcedureCompleted))
						{
							// This branch starter has been completed
							// Select the branch associated with this branch starter
							SelectBranch(branchProcedure);
							break;
						}
					}
				}
			}
			else
			{
				// A branch has been selected
				// Check if this event is about our branch
				if (procedureEvent.procedure.ID.Equals(selectedBranch.ID))
				{
					// Check if this is a completion event
					if (procedureEvent.eventName.Equals(Procedure.EventNames.ProcedureCompleted))
					{
						// Our branch has been completed
						// Unselect it, and see if all of our branch starters are completed
						selectedBranch = null;
						if (HasCompletedAllBranchStarters())
						{
							EmitCompletionEvent();
						}
						else
						{
							Activate();
						}
					}
				}
			}
		}

		void SelectBranch(Procedure selectedBranch)
		{
			// should I deactivate the branching node when branch is selected?
			Deactivate();
			this.selectedBranch = selectedBranch;
			Debug.Log("[BranchingProcedure] selecting branch: " + selectedBranch.ID);
			DeactivateBranchStarters();
			selectedBranch.Activate();
		}

		bool HasCompletedAllBranchStarters()
		{
			foreach (var procedureTuple in branches)
			{
				Procedure startingProcedure = procedureTuple.Key;
				if (startingProcedure.State != ProcedureState.Completed)
				{
					return false;
				}
			}
			return true;
		}

		void ActivateBranchStarters()
		{
			int availableBranchCount = 0;
			Procedure onlyOneBranchLeft = null;
			Procedure onlyOneBranchStarterLeft = null;
			foreach (var procedureTuple in branches)
			{
				if (procedureTuple.Key.State != ProcedureState.Completed)
				{
					availableBranchCount++;
					onlyOneBranchLeft = procedureTuple.Value;
					onlyOneBranchStarterLeft = procedureTuple.Key;
				}
			}

			if (availableBranchCount == 1)
			{
				onlyOneBranchStarterLeft.ForceComplete();
				SelectBranch(onlyOneBranchLeft);
			}
			else
			{
				foreach (var procedureTuple in branches)
				{
					procedureTuple.Key.Activate();
				}
			}
		}

		void DeactivateBranchStarters()
		{
			foreach (var procedureTuple in branches)
			{
				procedureTuple.Key.Deactivate();
			}
		}
	}
}
