using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caregivr
{
	public static class LuxHandEnums
	{
		 public const int numBones = 31;
		 
		/// <summary>The order of the joints that SteamVR Skeleton Input is expecting.</summary>
		public static class LuxHand_JointIndexes
		{
			public const int root = 0;
			public const int wrist = 1;
			public const int thumbMetacarpal = 2;
			public const int thumbProximal = 2;
			public const int thumbMiddle = 3;
			public const int thumbDistal = 4;
			public const int thumbTip = 5;
			public const int indexMetacarpal = 6;
			public const int indexProximal = 7;
			public const int indexMiddle = 8;
			public const int indexDistal = 9;
			public const int indexTip = 10;
			public const int middleMetacarpal = 11;
			public const int middleProximal = 12;
			public const int middleMiddle = 13;
			public const int middleDistal = 14;
			public const int middleTip = 15;
			public const int ringMetacarpal = 16;
			public const int ringProximal = 17;
			public const int ringMiddle = 18;
			public const int ringDistal = 19;
			public const int ringTip = 20;
			public const int pinkyMetacarpal = 21;
			public const int pinkyProximal = 22;
			public const int pinkyMiddle = 23;
			public const int pinkyDistal = 24;
			public const int pinkyTip = 25;
			public const int thumbAux = 26;
			public const int indexAux = 27;
			public const int middleAux = 28;
			public const int ringAux = 29;
			public const int pinkyAux = 30;

			public static int GetFingerForBone(int boneIndex)
			{
				switch (boneIndex)
				{
					case root:
					case wrist:
						return -1;

					case thumbMetacarpal:
					case thumbMiddle:
					case thumbDistal:
					case thumbTip:
					case thumbAux:
						return 0;

					case indexMetacarpal:
					case indexProximal:
					case indexMiddle:
					case indexDistal:
					case indexTip:
					case indexAux:
						return 1;

					case middleMetacarpal:
					case middleProximal:
					case middleMiddle:
					case middleDistal:
					case middleTip:
					case middleAux:
						return 2;

					case ringMetacarpal:
					case ringProximal:
					case ringMiddle:
					case ringDistal:
					case ringTip:
					case ringAux:
						return 3;

					case pinkyMetacarpal:
					case pinkyProximal:
					case pinkyMiddle:
					case pinkyDistal:
					case pinkyTip:
					case pinkyAux:
						return 4;

					default:
						return -1;
				}
			}

			public static int GetBoneForFingerTip(int fingerIndex)
			{
				switch (fingerIndex)
				{
					case LuxHand_FingerIndexes.thumb:
						return thumbTip;
					case LuxHand_FingerIndexes.index:
						return indexTip;
					case LuxHand_FingerIndexes.middle:
						return middleTip;
					case LuxHand_FingerIndexes.ring:
						return ringTip;
					case LuxHand_FingerIndexes.pinky:
						return pinkyTip;
					default:
						return indexTip;
				}
			}
		}

		public enum LuxHand_JointIndexEnum
		{
			root = LuxHand_JointIndexes.root,
			wrist = LuxHand_JointIndexes.wrist,
			thumbMetacarpal = LuxHand_JointIndexes.thumbMetacarpal,
			thumbProximal = LuxHand_JointIndexes.thumbProximal,
			thumbMiddle = LuxHand_JointIndexes.thumbMiddle,
			thumbDistal = LuxHand_JointIndexes.thumbDistal,
			thumbTip = LuxHand_JointIndexes.thumbTip,
			indexMetacarpal = LuxHand_JointIndexes.indexMetacarpal,
			indexProximal = LuxHand_JointIndexes.indexProximal,
			indexMiddle = LuxHand_JointIndexes.indexMiddle,
			indexDistal = LuxHand_JointIndexes.indexDistal,
			indexTip = LuxHand_JointIndexes.indexTip,
			middleMetacarpal = LuxHand_JointIndexes.middleMetacarpal,
			middleProximal = LuxHand_JointIndexes.middleProximal,
			middleMiddle = LuxHand_JointIndexes.middleMiddle,
			middleDistal = LuxHand_JointIndexes.middleDistal,
			middleTip = LuxHand_JointIndexes.middleTip,
			ringMetacarpal = LuxHand_JointIndexes.ringMetacarpal,
			ringProximal = LuxHand_JointIndexes.ringProximal,
			ringMiddle = LuxHand_JointIndexes.ringMiddle,
			ringDistal = LuxHand_JointIndexes.ringDistal,
			ringTip = LuxHand_JointIndexes.ringTip,
			pinkyMetacarpal = LuxHand_JointIndexes.pinkyMetacarpal,
			pinkyProximal = LuxHand_JointIndexes.pinkyProximal,
			pinkyMiddle = LuxHand_JointIndexes.pinkyMiddle,
			pinkyDistal = LuxHand_JointIndexes.pinkyDistal,
			pinkyTip = LuxHand_JointIndexes.pinkyTip,
			thumbAux = LuxHand_JointIndexes.thumbAux,
			indexAux = LuxHand_JointIndexes.indexAux,
			middleAux = LuxHand_JointIndexes.middleAux,
			ringAux = LuxHand_JointIndexes.ringAux,
			pinkyAux = LuxHand_JointIndexes.pinkyAux,
		}


		/// <summary>The order of the fingers that SteamVR Skeleton Input outputs</summary>
		public class LuxHand_FingerIndexes
		{
			public const int thumb = 0;
			public const int index = 1;
			public const int middle = 2;
			public const int ring = 3;
			public const int pinky = 4;

			public static LuxHand_FingerIndexEnum[] enumArray = (LuxHand_FingerIndexEnum[])System.Enum.GetValues(typeof(LuxHand_FingerIndexEnum));
		}

		/// <summary>The order of the fingerSplays that SteamVR Skeleton Input outputs</summary>
		public class LuxHand_FingerSplayIndexes
		{
			public const int thumbIndex = 0;
			public const int indexMiddle = 1;
			public const int middleRing = 2;
			public const int ringPinky = 3;

			public static LuxHand_FingerSplayIndexEnum[] enumArray = (LuxHand_FingerSplayIndexEnum[])System.Enum.GetValues(typeof(LuxHand_FingerSplayIndexEnum));
		}

		public enum LuxHand_FingerSplayIndexEnum
		{
			thumbIndex = LuxHand_FingerSplayIndexes.thumbIndex,
			indexMiddle = LuxHand_FingerSplayIndexes.indexMiddle,
			middleRing = LuxHand_FingerSplayIndexes.middleRing,
			ringPinky = LuxHand_FingerSplayIndexes.ringPinky,
		}

		public enum LuxHand_FingerIndexEnum
		{
			thumb = LuxHand_FingerIndexes.thumb,
			index = LuxHand_FingerIndexes.index,
			middle = LuxHand_FingerIndexes.middle,
			ring = LuxHand_FingerIndexes.ring,
			pinky = LuxHand_FingerIndexes.pinky,
		}
	}
}
