using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caregivr
{
	// LuxHandCollider
	// Connects LuxHand to the instantiated-on-startup SteamVR managed handCollider prefab.
	[ExecuteInEditMode]
	public class LuxHandCollider : MonoBehaviour
	{
		public LuxHand.LuxHandDirection handDirection;
		public GameObject targetTransform;
		//public CGPlayer luxPlayer { get; private set; }
		public LuxHand luxHand;
		public Rigidbody body;
		public LuxHandPoser poser
		{
			get
			{
				return luxHand == null ? null : luxHand.poser;
			}
		}
		public bool doPhysics;
		public Transform thumb;
		
		public Transform index2;
		public Transform index1;
		public Transform index0;

		public Transform middle2;
		public Transform middle1;
		public Transform middle0;

		public Transform ring2;
		public Transform ring1;

		public Transform pinky2;
		public Transform pinky1;

		public float distanceFromTargetHand { get {
			return Vector3.Distance(body.position, targetPosition);
		}}

		//public GameObject center;
		
		private Vector3 bonePosition(LuxHandEnums.LuxHand_JointIndexEnum jointIndexEnum)
		{
			Vector3 worldPosition = poser.GetBone(jointIndexEnum).transform.position;
			return poser.skeletonRoot.transform.InverseTransformPoint(worldPosition);
		}

		public virtual void Update()
		{
			if (poser == null)
			{
				return;
			}

			thumb.transform.localPosition = bonePosition(LuxHandEnums.LuxHand_JointIndexEnum.thumbTip);
			index2.transform.localPosition = bonePosition(LuxHandEnums.LuxHand_JointIndexEnum.indexTip);
			index1.transform.localPosition = bonePosition(LuxHandEnums.LuxHand_JointIndexEnum.indexDistal);
			index0.transform.localPosition = bonePosition(LuxHandEnums.LuxHand_JointIndexEnum.indexMiddle);

			middle2.transform.localPosition = bonePosition(LuxHandEnums.LuxHand_JointIndexEnum.middleTip);
			middle1.transform.localPosition = bonePosition(LuxHandEnums.LuxHand_JointIndexEnum.middleDistal);
			middle0.transform.localPosition = bonePosition(LuxHandEnums.LuxHand_JointIndexEnum.middleMiddle);
			
			ring2.transform.localPosition = bonePosition(LuxHandEnums.LuxHand_JointIndexEnum.ringTip);
			ring1.transform.localPosition = bonePosition(LuxHandEnums.LuxHand_JointIndexEnum.ringDistal);

			pinky2.transform.localPosition = bonePosition(LuxHandEnums.LuxHand_JointIndexEnum.pinkyTip);
			pinky1.transform.localPosition = bonePosition(LuxHandEnums.LuxHand_JointIndexEnum.pinkyDistal);
		}

		public void FixedUpdate()
		{
			/*
			Vector3 force 				= forceScalar * -1.0f * transform.localPosition;
			Vector3 currentVelocity 	= body.velocity;
			
			force - currentVelocity;
			
			body.AddForce(force);
			*/

			//transform.localPosition = Vector3.zero;
			//Quaternion angularForce = Quaternion.Inverse(transform.localRotation);
			//body.AddTorque(angularForce.eulerAngles);
			#if UNITY_EDITOR
			if (!Application.isPlaying) {
				return;
			}
			#endif
			ExecuteFixedUpdate();
		}

		void LateUpdate()
		{
			if (!doPhysics)
			{
				ExecuteFixedUpdate();
			}
		}
		
		public bool collidersInRadius;
        protected void ExecuteFixedUpdate()
        {
            //collidersInRadius = Physics.CheckSphere(center.transform.localPosition, 0.2f, 1);
			collidersInRadius = distanceFromTargetHand > 0.05f;
            if (!doPhysics || collidersInRadius == false)
            {
                //keep updating velocity, just in case. Otherwise you get jitter
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                /*
                rigidbody.velocity = (targetPosition - rigidbody.localPosition) / Time.fixedDeltaTime;
                float angle; Vector3 axis;
                (targetRotation * Quaternion.Inverse(rigidbody.rotation)).ToAngleAxis(out angle, out axis);
                rigidbody.angularVelocity = axis.normalized * angle / Time.fixedDeltaTime;
                */

                body.MovePosition(targetPosition);
                body.MoveRotation(targetRotation);
            }
            else
            {
                Vector3 velocityTarget, angularTarget;
                bool success = GetTargetVelocities(out velocityTarget, out angularTarget);
                if (success)
                {
					float scale = 0.1f;
                    float maxAngularVelocityChange = MaxAngularVelocityChange * scale;
                    float maxVelocityChange = MaxVelocityChange * scale;

                    body.velocity = Vector3.MoveTowards(body.velocity, velocityTarget, maxVelocityChange);
                    body.angularVelocity = Vector3.MoveTowards(body.angularVelocity, angularTarget, maxAngularVelocityChange);
                }
            }
        }


		private Vector3 targetPosition { get { return targetTransform.transform.position; }}
        private Quaternion targetRotation { get { return targetTransform.transform.rotation; }}

        protected const float MaxVelocityChange = 10f;
		protected const float MaxVelocity = 3f;
        protected const float VelocityMagic = 6000f;
        protected const float AngularVelocityMagic = 50f;
        protected const float MaxAngularVelocityChange = 20f;

		protected bool GetTargetVelocities(out Vector3 velocityTarget, out Vector3 angularTarget)
        {
            bool realNumbers = false;

            float velocityMagic = VelocityMagic;
            float angularVelocityMagic = AngularVelocityMagic;

            Vector3 positionDelta = (targetPosition - body.position);
            velocityTarget = (positionDelta * velocityMagic * Time.deltaTime);
			velocityTarget = Vector3.ClampMagnitude(velocityTarget, MaxVelocity);

            if (float.IsNaN(velocityTarget.x) == false && float.IsInfinity(velocityTarget.x) == false)
            {
                realNumbers = true;
            }
            else
                velocityTarget = Vector3.zero;


            Quaternion rotationDelta = targetRotation * Quaternion.Inverse(GetComponent<Rigidbody>().rotation);


            float angle;
            Vector3 axis;
            rotationDelta.ToAngleAxis(out angle, out axis);

            if (angle > 180)
                angle -= 360;

            if (angle != 0 && float.IsNaN(axis.x) == false && float.IsInfinity(axis.x) == false)
            {
                angularTarget = angle * axis * angularVelocityMagic * Time.deltaTime;

                realNumbers &= true;
            }
            else
                angularTarget = Vector3.zero;

            return realNumbers;
        }

	}
}


