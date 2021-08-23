using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Caregivr
{
	public class InformationUI : MonoBehaviour
	{
		ObjectIdentity objectID;
		ModuleRuntime moduleRuntime;
		public UnityEngine.UI.Button okayButton;
		public TextMeshProUGUI descriptionLabel;
		public TextMeshProUGUI nameLabel;
		public CanvasGroup canvasGroup;
		public UnityEngine.UI.Image okayButtonBG;
		public TextMeshProUGUI okayButtonLabel;

		private void Awake()
		{
			objectID = GetComponent<ObjectIdentity>();
			moduleRuntime = GetComponentInParent<ModuleRuntime>();
			moduleRuntime.OnProcedureEvent.AddListener(OnProcedureEvent);
			okayButton.onClick.AddListener(EmitOkayEvent);
			HideCanvasGroup(canvasGroup);
		}

		public void OnProcedureEvent(ProcedureEvent procedureEvent)
		{
			bool isMyProcedure = atomicProcedureIsMe(procedureEvent.procedure as AtomicProcedure);
			bool amShowingStuff = procedureDataContainsMyId(procedureEvent.procedure.Data);
			if (isMyProcedure || amShowingStuff)
			{
				// This is about me
				if (procedureEvent.eventName.Equals(Procedure.EventNames.ProcedureActivated))
				{
					ShowData(procedureEvent.procedure);
				}
				else if (procedureEvent.eventName.Equals(Procedure.EventNames.ProcedureDeactivated) || procedureEvent.eventName.Equals(Procedure.EventNames.ProcedureCompleted))
				{
					HideCanvasGroup(canvasGroup);
				}

				// Turn on and off okay button
				okayButton.gameObject.SetActive(isMyProcedure);
			}
		}

		public void ShowData(Procedure procedureIn)
		{
			ShowCanvasGroup(canvasGroup);
			descriptionLabel.SetText(procedureIn.Description);
			nameLabel.SetText(procedureIn.Name);
		}

		public void EmitOkayEvent()
		{
			//Debug.Log("[InformationUI] Emit Okay Event : " + System.DateTime.Now);
			objectID.EmitEvent("okay");
		}

		private bool procedureDataContainsMyId(VariantDictionary procedureData)
		{
			if (procedureData != null)
			{
				if (procedureData.ContainsKey(Procedure.DataKeys.ShowOnInfoUI))
				{
					string showingOnInfoUIIdentifier = procedureData[Procedure.DataKeys.ShowOnInfoUI].AsString();
					if (!string.IsNullOrEmpty(showingOnInfoUIIdentifier))
					{
						return objectID.Identifier.Equals(showingOnInfoUIIdentifier);
					}
				}
			}
			return false;
		}

		private bool atomicProcedureIsMe(AtomicProcedure procedure)
		{
			if (procedure != null)
			{
				if (procedure.ObjectIdentifier.Equals(objectID.Identifier))
				{
					return true;
				}
			}
			return false;
		}

		void HideCanvasGroup(CanvasGroup canvasGroup)
		{
			canvasGroup.alpha = 0.0f;
			canvasGroup.blocksRaycasts = false;
			canvasGroup.interactable = false;
		}

		void ShowCanvasGroup(CanvasGroup canvasGroup)
		{
			canvasGroup.alpha = 1.0f;
			canvasGroup.blocksRaycasts = true;
			canvasGroup.interactable = true;
		}
	}
}
