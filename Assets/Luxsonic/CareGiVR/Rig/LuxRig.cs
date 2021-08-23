using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Caregivr
{
    public class LuxRig : MonoBehaviour
    {
        public LuxInput luxInput;
        public LuxHand luxLeftHand;
        public LuxHand luxRightHand;

        public Transform hmdTransform;

        public LuxHand[] hands { get; private set; }

        public LuxFadeSphere fadeSphere;

        private Vector3 startPos;
		private Quaternion startRot;

        private ObjectRegistrar registrar;

        private ModuleRuntime moduleRuntime;

        //If not changed respawn at start location in scene
        public string customRespawnID = "fill_custom_respawn_location";

        public Vector3 feetPositionGuess
        {
            get
            {
                // TODO LUX
                return transform.position - Vector3.up;
            }
        }

        public Transform trackingOriginTransform
        {
            get
            {
                // TODO LUX
                return transform;
            }
        }

        void Awake()
        {
            hands = new LuxHand[] { luxLeftHand, luxRightHand };
            startPos = transform.position;
			startRot = transform.rotation;
            registrar = GetComponentInParent<ObjectRegistrar>();
            moduleRuntime = GetComponentInParent<ModuleRuntime>();
            moduleRuntime.OnProcedureEvent.AddListener(FadeAndRespawn);
        }

        public LuxHand GetHand(LuxHand.LuxHandDirection handDirection)
        {
            return handDirection == LuxHand.LuxHandDirection.Left ? luxLeftHand : luxRightHand;
        }

        public void FadeAndRespawn(ProcedureEvent p)
        {
			if (p.eventName.Equals(Procedure.EventNames.ModuleUnload))
			{
				fadeSphere.FadeOut(Respawn, 0.5f);
			}
            else if (p.eventName.Equals(Procedure.EventNames.ModuleLoaded))
            {
                fadeSphere.FadeIn(null, 2.0f);
            }
        }

        public void Respawn()
        {
			bool usingCustomRespawnId = false;
			if (!string.IsNullOrEmpty(customRespawnID))
			{
				ObjectIdentity customRespawnLocation = registrar.LookupObject(customRespawnID);
				if (customRespawnLocation != null)
				{
					usingCustomRespawnId = true;
					transform.position = customRespawnLocation.transform.position;
					transform.rotation = customRespawnLocation.transform.rotation;
				}
			}
			if (!usingCustomRespawnId)
			{
                transform.position = startPos;
                transform.rotation = startRot;
			}
        }
    }
}
