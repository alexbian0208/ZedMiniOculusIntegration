using System;
using UnityEngine;
using System.Linq;

namespace Caregivr
{
    public class LuxHandPose : ScriptableObject
    {
        public LuxHand_Pose_Hand leftHand = new LuxHand_Pose_Hand(LuxHand.LuxHandDirection.Left);
        public LuxHand_Pose_Hand rightHand = new LuxHand_Pose_Hand(LuxHand.LuxHandDirection.Right);

        public bool applyToSkeletonRoot = true;

        public LuxHand_Pose_Hand GetHand(LuxHand.LuxHandDirection hand)
        {
            if (hand == LuxHand.LuxHandDirection.Left)
                return leftHand;
            else if (hand == LuxHand.LuxHandDirection.Right)
                return rightHand;
            return null;
        }
    }

    [Serializable]
    public class LuxHand_Pose_Hand
    {
        //public SteamVR_Input_Sources inputSource;
		public LuxHand.LuxHandDirection handDirection;

        public LuxHand_FingerExtensionTypes thumbFingerMovementType = LuxHand_FingerExtensionTypes.Static;
        public LuxHand_FingerExtensionTypes indexFingerMovementType = LuxHand_FingerExtensionTypes.Static;
        public LuxHand_FingerExtensionTypes middleFingerMovementType = LuxHand_FingerExtensionTypes.Static;
        public LuxHand_FingerExtensionTypes ringFingerMovementType = LuxHand_FingerExtensionTypes.Static;
        public LuxHand_FingerExtensionTypes pinkyFingerMovementType = LuxHand_FingerExtensionTypes.Static;

        /// <summary>
        /// Get extension type for a particular finger. Thumb is 0, Index is 1, etc.
        /// </summary>
        public LuxHand_FingerExtensionTypes GetFingerExtensionType(int finger)
        {
            if (finger == 0)
                return thumbFingerMovementType;
            if (finger == 1)
                return indexFingerMovementType;
            if (finger == 2)
                return middleFingerMovementType;
            if (finger == 3)
                return ringFingerMovementType;
            if (finger == 4)
                return pinkyFingerMovementType;

            //default to static
            Debug.LogWarning("Finger not in range!");
            return LuxHand_FingerExtensionTypes.Static;
        }

        public const bool ignoreRootPoseData = true;
        public const bool ignoreWristPoseData = true;

        public Vector3 position;
        public Quaternion rotation;

        public Vector3[] bonePositions;
        public Quaternion[] boneRotations;

        public LuxHand_Pose_Hand(LuxHand.LuxHandDirection direction)
        {
            handDirection = direction;
        }

        public LuxHand_FingerExtensionTypes GetMovementTypeForBone(int boneIndex)
        {
            int fingerIndex = LuxHandEnums.LuxHand_JointIndexes.GetFingerForBone(boneIndex);

            switch (fingerIndex)
            {
                case LuxHandEnums.LuxHand_FingerIndexes.thumb:
                    return thumbFingerMovementType;

                case LuxHandEnums.LuxHand_FingerIndexes.index:
                    return indexFingerMovementType;

                case LuxHandEnums.LuxHand_FingerIndexes.middle:
                    return middleFingerMovementType;

                case LuxHandEnums.LuxHand_FingerIndexes.ring:
                    return ringFingerMovementType;

                case LuxHandEnums.LuxHand_FingerIndexes.pinky:
                    return pinkyFingerMovementType;
            }

            return LuxHand_FingerExtensionTypes.Static;
        }
    }

    public enum LuxHand_FingerExtensionTypes
    {
        Static,
        Free,
        Extend,
        Contract,
    }

    public class LuxHand_FingerExtensionTypeLists
    {
        private LuxHand_FingerExtensionTypes[] _enumList;
        public LuxHand_FingerExtensionTypes[] enumList
        {
            get
            {
                if (_enumList == null)
                    _enumList = (LuxHand_FingerExtensionTypes[])System.Enum.GetValues(typeof(LuxHand_FingerExtensionTypes));
                return _enumList;
            }
        }

        private string[] _stringList;
        public string[] stringList
        {
            get
            {
                if (_stringList == null)
                    _stringList = enumList.Select(element => element.ToString()).ToArray();
                return _stringList;
            }
        }
    }
}
