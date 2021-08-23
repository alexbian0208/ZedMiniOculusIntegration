using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caregivr
{
	public abstract class LuxTeleportMarkerBase : MonoBehaviour
	{
		public bool locked = false;
		public bool markerActive = true;
		public bool onlyActiveOnProcedure = true;

		private ObjectIdentity objectIdentity;
		private ModuleRuntime moduleRuntime;

		protected virtual void Awake()
		{
			moduleRuntime = GetComponentInParent<ModuleRuntime>();
			objectIdentity = GetComponent<ObjectIdentity>();
			if (moduleRuntime != null)
			{
				moduleRuntime.OnProcedureEvent.AddListener(OnProcedureEvent);
			}
		}
		
		protected virtual void Start() {}

		protected virtual void OnProcedureEvent(ProcedureEvent procedureEvent)
		{
			if (onlyActiveOnProcedure)
			{
				if (objectIdentity != null)
				{
					if (procedureEvent.eventName.Equals(Procedure.EventNames.ModuleLoaded))
					{
						// Deactivate all on start
						gameObject.SetActive(false);
					}

					// Turn this Teleporter On or Off
					AtomicProcedure atomicProcedure = procedureEvent.procedure as AtomicProcedure;
					if (atomicProcedure != null)
					{	
						bool isThisTeleporter = atomicProcedure.ObjectIdentifier.Equals(objectIdentity.Identifier);
						if (isThisTeleporter)
						{
							bool isActivationEvent = procedureEvent.eventName.Equals(Procedure.EventNames.ProcedureActivated);
							if (isActivationEvent)
							{
								// Activate this active teleporter
								gameObject.SetActive(true);
							}

							bool isDeactivationEvent = procedureEvent.eventName.Equals(Procedure.EventNames.ProcedureCompleted);
							if (isDeactivationEvent)
							{
								// Deactivate this active teleporter
								gameObject.SetActive(false);
							}
						}
					}
				}
			}
		}

		//-------------------------------------------------
		public virtual bool showReticle
		{
			get
			{
				return true;
			}
		}


		//-------------------------------------------------
		public void SetLocked(bool locked)
		{
			this.locked = locked;

			UpdateVisuals();
		}


		//-------------------------------------------------
		public virtual void TeleportPlayer(Vector3 pointedAtPosition)
		{
		}

		public void OnTeleport()
		{
			if (objectIdentity != null)
			{
				objectIdentity.EmitEvent("teleport");
			}
			else
			{
				//Debug.LogWarning("[LuxTeleportMarkerBase] there is no identity for teleport: " + gameObject.name);
			}
		}

		//-------------------------------------------------
		public abstract void UpdateVisuals();

		//-------------------------------------------------
		public abstract void Highlight(bool highlight);

		//-------------------------------------------------
		public abstract void SetAlpha(float tintAlpha, float alphaPercent);

		//-------------------------------------------------
		public abstract bool ShouldActivate(Vector3 playerPosition);

		//-------------------------------------------------
		public abstract bool ShouldMovePlayer();
	}
}
