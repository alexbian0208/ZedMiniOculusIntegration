using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caregivr
{
	[ExecuteInEditMode]
    public class LuxHandPoser : MonoBehaviour
    {
		public LuxHandPose pose;
		public GameObject skeletonRoot;
		public HandType handType;
		public enum HandType { Left, Right };
		[Tooltip("Used to supply a default argument to SetPose()")]
		public GameObject defaultObjectRoot;

		[Tooltip("If objectRoot is a child of this hand, move the objectRoot to the hand. Otherwise move the hand to the objectRoot.")]
		public GameObject objectRoot;
		Transform[] bones;
		public int boneCount { get { return 31; } }

		Vector3[] boneVelocities;
		Vector3[] boneAngularVelocities;
		Vector3 rootVelocity;
		Vector3 rootAngularVelocity;
		
		public void SetPose(LuxHandPose poseData, GameObject objectRootData = null)
		{
			if (objectRootData == null)
			{
				objectRootData = defaultObjectRoot;
				this.transform.localPosition = Vector3.zero;
				this.transform.localRotation = Quaternion.identity;
			}
			objectRoot = objectRootData;
			pose = poseData;
		}

		void Awake()
		{
			FetchBonesIfNeeded();
		}

#if UNITY_EDITOR
		void OnValidate()
		{
			FetchBonesIfNeeded();
		}
#endif

		void FetchBonesIfNeeded()
		{
			if (bones == null)
			{
				if (skeletonRoot != null)
				{
					bones = skeletonRoot.GetComponentsInChildren<Transform>();
					boneVelocities = new Vector3[bones.Length];
					boneAngularVelocities = new Vector3[bones.Length];
				}
			}
		}

		void Start()
		{
			ApplyPoseToSkeleton();
		}
		
		public void MoveObjectRoot(Vector3 position, Quaternion rotation)
		{
			if (objectRoot != null)
			{
				objectRoot.transform.position = position;
				objectRoot.transform.rotation = rotation;
			}
		}

		public void ApplyPoseToSkeleton()
		{
			if (pose != null)
			{
				LuxHand_Pose_Hand hand = handType == HandType.Left ? pose.leftHand : pose.rightHand;	
				Vector3[] bonePositions = hand.bonePositions;
				Quaternion[] boneRotations = hand.boneRotations;
				Vector3 p = hand.position;
				Quaternion q = hand.rotation;
				
				for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
				{
					if (bones[boneIndex] == null)
						continue;
					
					if (boneIndex == LuxHandEnums.LuxHand_JointIndexes.wrist && LuxHand_Pose_Hand.ignoreWristPoseData)
					{
						bones[boneIndex].localPosition = Vector3.zero;
						bones[boneIndex].localRotation = Quaternion.identity;
					}
					else if (boneIndex == LuxHandEnums.LuxHand_JointIndexes.root && LuxHand_Pose_Hand.ignoreRootPoseData)
					{
						bones[boneIndex].localPosition = Vector3.zero;
						bones[boneIndex].localRotation = Quaternion.identity;
					}
					else
					{
						bones[boneIndex].localPosition = blendToPosition(bones[boneIndex].localPosition, bonePositions[boneIndex], ref boneVelocities[boneIndex]);
						bones[boneIndex].localRotation = blendToRotation(bones[boneIndex].localRotation, boneRotations[boneIndex], ref boneAngularVelocities[boneIndex]);
					}
				}
				
				if (objectRoot != null)
				{
					bool moveObjectToHand = objectRoot.transform.IsChildOf(this.transform);
					if (moveObjectToHand)
					{
						objectRoot.transform.localPosition = blendToPosition(objectRoot.transform.localPosition, p, ref rootVelocity);
						objectRoot.transform.localRotation = blendToRotation(objectRoot.transform.localRotation, q, ref rootAngularVelocity);
					}
					else
					{
						// Object is not a child, we will move the hand to the object
						this.transform.rotation = objectRoot.transform.rotation * Quaternion.Inverse(q);
						this.transform.position = objectRoot.transform.position + ((objectRoot.transform.rotation * Quaternion.Inverse(q)) * -hand.position);
					}
				}
			}
			else
			{
				//Debug.LogWarning("[LuxHandPoser] null pose on LuxHandPoser " + gameObject.name);
			}
		}

		Vector3 blendToPosition(Vector3 current, Vector3 destination, ref Vector3 velocity)
		{
			#if UNITY_EDITOR
			if (Application.isEditor && !Application.isPlaying)
			{
				return destination;
			}
			#endif

			return Vector3.SmoothDamp(current, destination, ref velocity, 0.1f);
		}
		
		Quaternion blendToRotation(Quaternion current, Quaternion destination, ref Vector3 velocity)
		{
			#if UNITY_EDITOR
			if (Application.isEditor && !Application.isPlaying)
			{
				return destination;
			}
			#endif
			
			// How do I do this properly??			
			// ?????
			// Attempt 1
			/*
			Quaternion delta = destination * Quaternion.Inverse(current);
			Vector3 axis = Vector3.zero;
			float angle = 0.0f;
			delta.ToAngleAxis(out angle, out axis);

			return destination;			
			*/

			// Attempt 2
			return Quaternion.Slerp(current, destination, 0.1f);

			// Attempt 3
			//return Quaternion.Euler(blendToPosition(current.eulerAngles, destination.eulerAngles, ref velocity));
		}

		void Update()
		{
			ApplyPoseToSkeleton();
		}

		public Transform GetBone(LuxHandEnums.LuxHand_JointIndexEnum fingerIndexEnum)
		{
			return GetBone((int)fingerIndexEnum);
		}
		public Transform GetBone(int i)
		{
			return bones[i];
		}
    }
}
