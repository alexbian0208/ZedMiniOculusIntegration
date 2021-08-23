using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXR = UnityEngine.XR;

namespace Caregivr
{
	public class LuxInputController_UnityXR : LuxInputController
	{
		private UXR.InputDevice uxrInputDevice;
		
		public override Vector2 Joystick() {
			Vector2 joystickValue;
			if (uxrInputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out joystickValue)) {
				return joystickValue;
			}
			return new Vector2(0.0f, 0.0f);
		}

		public override float Grip()
		{
			float gripValue;
			if (uxrInputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out gripValue))
			{
				return gripValue;
			}

			//Debug.LogError("[LuxInputController_UnityXR] Grip() no grip");
			return 0.0f;
		}
		
		public override bool GripButton()
		{
			/*
			bool gripValue;
			if (uxrInputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out gripValue))
			{
				return gripValue;
			}

			Debug.LogError("[LuxInputController_UnityXR] GripButton() no grip");
			return false;
			*/
			return Grip() > 0.5f;
		}

		public override float Trigger()
		{
			float triggerValue;
			if (uxrInputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out triggerValue))
			{
				return triggerValue;
			}

			//Debug.LogError("[LuxInputController_UnityXR] Trigger() no trigger");
			return 0.0f;
		}
		
		public override bool TriggerButton()
		{
			/*
			bool triggerValue;
			if (uxrInputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerValue))
			{
				return triggerValue;
			}

			Debug.LogError("[LuxInputController_UnityXR] TriggerButton() no trigger");
			return false;
			*/
			return Trigger() > 0.5f;
		}

		public override bool AButton()
		{
			bool buttonValue;
			if (uxrInputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonValue))
			{
				return buttonValue;
			}

			//Debug.LogError("[LuxInputController_UnityXR] AButton() no a button");
			return false;
		}

		public override bool BButton()
		{
			bool buttonValue;
			if (uxrInputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out buttonValue))
			{
				return buttonValue;
			}

			//Debug.LogError("[LuxInputController_UnityXR] AButton() no a button");
			return false;
		}

		public override bool Teleport()
		{
			Vector2 axis;
			if (uxrInputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out axis))
			{
				return Vector2.Dot(axis, Vector2.up) > 0.9f;
			}

			return false;
		}

		void Awake()
		{
			UXR.InputDevices.deviceConnected += OnDeviceConnected;
		}
		
		void OnDestroy()
		{
			UXR.InputDevices.deviceConnected -= OnDeviceConnected;
		}

		void Start()
		{
			FetchUXRInputDevice();
		}

		void FetchUXRInputDevice()
		{
			List<UXR.InputDevice> uxrInputDevices = new List<UXR.InputDevice>();
			UXR.InputDevices.GetDevicesAtXRNode(GetXRNode(), uxrInputDevices);

			if(uxrInputDevices.Count == 1)
			{
				uxrInputDevice = uxrInputDevices[0];
			}
			else if(uxrInputDevices.Count > 1)
			{
				Debug.LogWarning("[LuxInputController_UnityXR] Awake() Found more than one hand for handDirection: " + handDirection.ToString());
				uxrInputDevice = uxrInputDevices[0];
			}
			else if (uxrInputDevices.Count == 0)
			{
				Debug.LogError("[LuxInputController_UnityXR] Awake() Found no input devices for handDirection: " + handDirection.ToString());
			}
		}

		void OnDeviceConnected(UXR.InputDevice device)
		{
			FetchUXRInputDevice();
		}

		private UnityEngine.XR.XRNode GetXRNode()
		{
			if (handDirection == LuxHand.LuxHandDirection.Left)
			{
				return UXR.XRNode.LeftHand;
			}
			return UXR.XRNode.RightHand;
		}

		public override void PlayHaptics(uint channelIn, float amplitudeIn, float durationIn)
		{
			uxrInputDevice.SendHapticImpulse(channelIn, amplitudeIn, durationIn);
		}
	}
}
