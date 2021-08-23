using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.XR;

namespace Caregivr
{
	public class LuxHand : MonoBehaviour
	{
		public enum LuxHandDirection { Left, Right }
		public LuxHandDirection handDirection;

		public LuxInput luxInput { get; private set; }
		public LuxHandHovering hoverer;
		public LuxHandPoser poser;
		public LuxHandPoser ghostPoser;
		public LuxHandCollider handCollider { get; private set; }

		public LuxPickupable attachedPickupable { get; private set; }

		public MaterialOverrider materialOverrider;
		public MaterialOverrider ghostMaterialOverrider;

		public LuxHandPose gripPose;
		public LuxHandPose normalPose;

		public Transform attachmentParent;
		public LuxInputController controller { get { return luxInput == null ? null : luxInput.GetLuxInputController(handDirection); } }
		public bool hasAttachedPickupable { get { return attachedPickupable != null; } }
		private bool wasTeleporting = false;
		private Transform originalParent;
		ModuleRuntime moduleRuntime;
		private bool wasTriggering = false;

		public Color ghostMaterialColor;
		public Color ghostMaterialClearColor
		{
			get
			{
				var c = ghostMaterialColor;
				c.a = 0.0f;
				return c;
			}
		}
		bool ghostIsFadedIn;
		public Material ghostMaterial;
		public SkinnedMeshRenderer ghostHandRenderer;

		// The transform used when detaching a parent
		// Keeps the objects as children of the module
		Transform deattachmentParent { get { return moduleRuntime != null ? moduleRuntime.SceneRoot.transform : null; } }
		public Transform teleportArcTransform;
		public Transform centerHandTransform;
		List<InputDevice> hapticsDevice = new List<InputDevice>();

		public virtual void Awake()
		{
			luxInput = GetComponentInParent<LuxInput>();
			handCollider = GetComponentInChildren<LuxHandCollider>();
			moduleRuntime = GetComponentInParent<ModuleRuntime>();
			if (!teleportArcTransform)
			{
				teleportArcTransform = transform;
			}
			if (!centerHandTransform)
			{
				centerHandTransform = transform;
			}

			if (handDirection == LuxHandDirection.Right)
			{
				InputDeviceCharacteristics rightTrackedControllerFilter = InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Right, rightHandedControllers;
				InputDevices.GetDevicesWithCharacteristics(rightTrackedControllerFilter, hapticsDevice);
			}

			if (handDirection == LuxHandDirection.Left)
			{
				InputDeviceCharacteristics leftTrackedControllerFilter = InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Left, leftHandedControllers;
				InputDevices.GetDevicesWithCharacteristics(leftTrackedControllerFilter, hapticsDevice);
			}
		}

		public virtual void Start()
		{
			ghostMaterialOverrider.OverrideMaterials(ghostMaterial);
			ghostHandRenderer.enabled = false;
		}

		public virtual void SetPose()
		{
			if (attachedPickupable != null)
			{
				if (attachedPickupable.skeletonMainPose != null)
				{
					poser.SetPose(attachedPickupable.skeletonMainPose, poser.objectRoot);
				}
				else
				{
					poser.SetPose(gripPose);
				}
			}
			else if (controller.GripButton())
			{
				poser.SetPose(gripPose);
			}
			else if (hoverer.hoveringPoseOverriders.Count > 0)
			{
				poser.SetPose(hoverer.hoveringPoseOverriders[0].pose);
			}
			else
			{
				poser.SetPose(normalPose);
			}

			ghostPoser.pose = poser.pose;
		}

		public virtual void DetachObject()
		{
			if (attachedPickupable != null)
			{
				if (!attachedPickupable.moveHandToObject)
				{
					if (!attachedPickupable.GetComponent<LuxPickupableStaticParent>())
					{
						attachedPickupable.transform.SetParent(originalParent, true);
					}
					else
					{
						attachedPickupable.GetComponent<LuxPickupableStaticParent>().psuedoHand = null;
					}
				}
				attachedPickupable.OnDetach(this);
				attachedPickupable = null;
			}
		}

		public virtual void Update()
		{
			SetPose();
			float ghostFadeThreshold = 0.05f;
			// Ghost Hands fade in and out
			if (handCollider.distanceFromTargetHand > ghostFadeThreshold)
			{
				if (!ghostIsFadedIn)
				{
					ghostHandRenderer.enabled = true;
					ghostHandRenderer.material.DOColor(ghostMaterialColor, 0.2f);
					ghostIsFadedIn = true;
				}
			}
			else
			{
				if (ghostIsFadedIn)
				{
					ghostHandRenderer.material.DOColor(ghostMaterialClearColor, 0.2f).OnComplete(() =>
					{
						ghostHandRenderer.enabled = false;
					});
					ghostIsFadedIn = false;
				}
			}

			// Teleport
			if (!hasAttachedPickupable)
			{
				bool isTeleport = controller.Teleport();

				if (!isTeleport && wasTeleporting)
				{
					// Do Teleport
				}

				if (isTeleport)
				{
					// Render Teleport Laser
				}

				wasTeleporting = isTeleport;
			}

			// Laser
			if (!hasAttachedPickupable)
			{
				bool isLaser = controller.GripButton();

				if (isLaser)
				{
					// Render Laser
				}
			}

			bool isTrigger = controller.GripButton() || controller.TriggerButton();
			// Pickup and Drop Objects
			if (isTrigger)
			{
				// Only trigger pickups on click down, not on hold
				if (!wasTriggering)
				{
					if (!hasAttachedPickupable)
					{
						if (luxInput.inputMode == LuxInput.InputMode.controllers && hoverer.closestHoveringPickupable != null)
						{
							PickupObject();
						}
					}
				}
			}
			else
			{
				DetachObject();
			}

			wasTriggering = isTrigger;
		}

		public void PickupObject()
		{
			LuxPickupable pickThisUp = hoverer.closestHoveringPickupable;
			if (pickThisUp.isAttached)
			{
				pickThisUp.attachedLuxHand.DetachObject();
			}
			pickThisUp.OnAttached(this);


			if (!pickThisUp.moveHandToObject)
			{
				poser.MoveObjectRoot(pickThisUp.transform.position, pickThisUp.transform.rotation);
				originalParent = pickThisUp.transform.parent;
				LuxPickupableStaticParent doesExist = pickThisUp.gameObject.GetComponent<LuxPickupableStaticParent>();
				if (!doesExist)
				{
					pickThisUp.transform.SetParent(attachmentParent, true);
				}
				else
				{
					doesExist.psuedoHand = attachmentParent;
				}
				pickThisUp.transform.localPosition = Vector3.zero;
				pickThisUp.transform.localRotation = Quaternion.identity;
				poser.objectRoot = poser.defaultObjectRoot;
			}
			else
			{
				poser.objectRoot = pickThisUp.gameObject;
			}

			attachedPickupable = pickThisUp;
		}

		public void PlayHaptics(uint channelIn, float amplitudeIn, float durationIn)
		{
			controller.PlayHaptics(channelIn,amplitudeIn,durationIn);
		}
	}
}
