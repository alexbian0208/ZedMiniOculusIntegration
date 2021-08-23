using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caregivr
{
    public abstract class LuxInputController : MonoBehaviour
    {
        public static LuxInputController AddPlatformComponent(GameObject gameObject, LuxHand.LuxHandDirection handDirection)
        {
            LuxInputController platformComponent = null;

			LuxInput luxInput = FindObjectOfType<LuxInput>();

            if (luxInput.inputMode == LuxInput.InputMode.controllers)
            {

                platformComponent = gameObject.AddComponent<LuxInputController_UnityXR>();

            }
           
            platformComponent.handDirection = handDirection;
            return platformComponent;
        }

        public LuxHand.LuxHandDirection handDirection { get; protected set; }

        public abstract Vector2 Joystick();
        public abstract float Grip();
        public abstract bool GripButton();
        public abstract float Trigger();
        public abstract bool TriggerButton();
        public abstract bool Teleport();
        public abstract bool AButton();
        public abstract bool BButton();
		public virtual void PlayHaptics(uint channelIn, float amplitudeIn, float durationIn) {}
    }
}
