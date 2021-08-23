﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEditor;
using UnityEngine;

namespace Caregivr
{

	[CustomEditor(typeof(LuxPickupable))]
	public class LuxPickupableEditor : Editor
	{
		private SerializedProperty skeletonMainPoseProperty;
		private SerializedProperty skeletonAdditionalPosesProperty;

		private SerializedProperty showLeftPreviewProperty;
		private SerializedProperty showRightPreviewProperty;

		private SerializedProperty previewLeftInstanceProperty;
		private SerializedProperty previewRightInstanceProperty;

		private SerializedProperty previewLeftHandPrefab;
		private SerializedProperty previewRightHandPrefab;

		private SerializedProperty previewPoseSelection;

		private SerializedProperty poseEditorExpanded;
		private SerializedProperty blendEditorExpanded;

		private SerializedProperty poserScale;



		private SerializedProperty blendingBehaviourArray;


		Texture handTexL;
		Texture handTexR;


		private LuxPickupable poser;

		private bool PoseChanged = false;

		protected void OnEnable()
		{
			skeletonMainPoseProperty = serializedObject.FindProperty("skeletonMainPose");
			skeletonAdditionalPosesProperty = serializedObject.FindProperty("skeletonAdditionalPoses");

			showLeftPreviewProperty = serializedObject.FindProperty("showLeftPreview");
			showRightPreviewProperty = serializedObject.FindProperty("showRightPreview");

			previewLeftInstanceProperty = serializedObject.FindProperty("previewLeftInstance");
			previewRightInstanceProperty = serializedObject.FindProperty("previewRightInstance");

			previewLeftHandPrefab = serializedObject.FindProperty("overridePreviewLeftHandPrefab");
			previewRightHandPrefab = serializedObject.FindProperty("overridePreviewRightHandPrefab");

			previewPoseSelection = serializedObject.FindProperty("previewPoseSelection");

			poseEditorExpanded = serializedObject.FindProperty("poseEditorExpanded");
			blendEditorExpanded = serializedObject.FindProperty("blendEditorExpanded");

			poserScale = serializedObject.FindProperty("scale");


			blendingBehaviourArray = serializedObject.FindProperty("blendingBehaviours");


			poser = (LuxPickupable)target;
		}

		protected void LoadDefaultPreviewHands()
		{
			/*
            if (previewLeftHandPrefab.objectReferenceValue == null)
            {
                previewLeftHandPrefab.objectReferenceValue = SteamVR_Settings.instance.previewHandLeft;
            }

            if (previewRightHandPrefab.objectReferenceValue == null)
            {
                previewRightHandPrefab.objectReferenceValue = SteamVR_Settings.instance.previewHandRight;
            }
			*/
		}

		protected void UpdatePreviewHand(SerializedProperty instanceProperty, SerializedProperty showPreviewProperty, GameObject previewPrefab, LuxHand_Pose_Hand handData, LuxHandPose sourcePose, bool forceUpdate)
		{
			GameObject preview = instanceProperty.objectReferenceValue as GameObject;
			//EditorGUILayout.PropertyField(showPreviewProperty);

			if (showPreviewProperty.boolValue)
			{
				if (forceUpdate && preview != null)
				{
					DestroyImmediate(preview);
				}

				if (preview == null)
				{
					preview = GameObject.Instantiate<GameObject>(previewPrefab);
					preview.transform.localScale = Vector3.one * poserScale.floatValue;

					preview.transform.parent = poser.transform;
					preview.transform.localPosition = Vector3.zero;
					preview.transform.localRotation = Quaternion.identity;

					LuxHandPoser previewSkeleton = null;

					if (preview != null)
						previewSkeleton = preview.GetComponentInChildren<LuxHandPoser>();

					if (previewSkeleton != null)
					{

						if (handData.bonePositions == null || handData.bonePositions.Length == 0)
						{
							LuxHandPose poseResource = (LuxHandPose)Resources.Load("Luxsonic_RelaxedPose");
							DeepCopyPose(poseResource, sourcePose);
							EditorUtility.SetDirty(sourcePose);
						}


						preview.transform.localPosition = Vector3.zero;
						preview.transform.localRotation = Quaternion.identity;
						preview.transform.parent = null;
						preview.transform.localScale = Vector3.one * poserScale.floatValue;



						preview.transform.parent = poser.transform;

						preview.transform.localRotation = Quaternion.Inverse(handData.rotation);
						preview.transform.position = preview.transform.TransformPoint(-handData.position);

						for (int boneIndex = 0; boneIndex < handData.bonePositions.Length; boneIndex++)
						{
							Transform bone = previewSkeleton.GetBone(boneIndex);
							
							if (boneIndex == LuxHandEnums.LuxHand_JointIndexes.wrist && LuxHand_Pose_Hand.ignoreWristPoseData)
							{
								bone.localPosition = Vector3.zero;
								bone.localRotation = Quaternion.identity;
							}
							else if (boneIndex == LuxHandEnums.LuxHand_JointIndexes.root && LuxHand_Pose_Hand.ignoreRootPoseData)
							{
								bone.localPosition = Vector3.zero;
								bone.localRotation = Quaternion.identity;
							}
							else
							{
								bone.localPosition = handData.bonePositions[boneIndex];
								bone.localRotation = handData.boneRotations[boneIndex];
							}
						}


						//previewSkeleton.pose = sourcePose;
					}
					SceneView.RepaintAll();
					instanceProperty.objectReferenceValue = preview;
				}
			}
			else
			{
				if (preview != null)
				{
					DestroyImmediate(preview);
					SceneView.RepaintAll();
				}
			}
		}

		protected void ZeroTransformParents(Transform toZero, Transform stopAt)
		{
			if (toZero == null)
				return;

			toZero.localPosition = Vector3.zero;
			toZero.localRotation = Quaternion.identity;

			if (toZero == stopAt)
				return;

			ZeroTransformParents(toZero.parent, stopAt);
		}

		//protected EVRSkeletalReferencePose defaultReferencePose = EVRSkeletalReferencePose.OpenHand;
		//protected EVRSkeletalReferencePose forceToReferencePose = EVRSkeletalReferencePose.OpenHand;


		protected void SaveHandData(LuxHand_Pose_Hand handData, LuxHandPoser thisSkeleton)
		{
			handData.position = thisSkeleton.transform.InverseTransformPoint(poser.transform.position);
			//handData.position = thisSkeleton.transform.localPosition;

			handData.rotation = Quaternion.Inverse(thisSkeleton.transform.localRotation);

			handData.bonePositions = new Vector3[thisSkeleton.boneCount];
			handData.boneRotations = new Quaternion[thisSkeleton.boneCount];

			for (int boneIndex = 0; boneIndex < thisSkeleton.boneCount; boneIndex++)
			{
				Transform bone = thisSkeleton.GetBone(boneIndex);
				handData.bonePositions[boneIndex] = bone.localPosition;
				handData.boneRotations[boneIndex] = bone.localRotation;
			}

			EditorUtility.SetDirty(activePose);
		}

		protected void DrawHand(bool showHand, LuxHand_Pose_Hand handData, LuxHand_Pose_Hand otherData, bool getFromOpposite, SerializedProperty showPreviewProperty)
		{
			LuxHandPoser thisSkeleton;
			LuxHandPoser oppositeSkeleton;
			string thisSourceString;
			string oppositeSourceString;

			if (handData.handDirection == LuxHand.LuxHandDirection.Left)
			{
				thisSkeleton = leftSkeleton;
				thisSourceString = "Left Hand";
				oppositeSourceString = "Right Hand";
				oppositeSkeleton = rightSkeleton;
			}
			else
			{
				thisSkeleton = rightSkeleton;
				thisSourceString = "Right Hand";
				oppositeSourceString = "Left Hand";
				oppositeSkeleton = leftSkeleton;
			}


			if (showHand)
			{
				if (getFromOpposite)
				{
					bool confirm = EditorUtility.DisplayDialog("SteamVR", string.Format("This will overwrite your current {0} skeleton data. (with data from the {1} skeleton)", thisSourceString, oppositeSourceString), "Overwrite", "Cancel");
					if (confirm)
					{
						Vector3 reflectedPosition = new Vector3(-oppositeSkeleton.transform.localPosition.x, oppositeSkeleton.transform.localPosition.y, oppositeSkeleton.transform.localPosition.z);
						thisSkeleton.transform.localPosition = reflectedPosition;

						Quaternion oppositeRotation = oppositeSkeleton.transform.localRotation;
						Quaternion reflectedRotation = new Quaternion(-oppositeRotation.x, oppositeRotation.y, oppositeRotation.z, -oppositeRotation.w);
						thisSkeleton.transform.localRotation = reflectedRotation;


						for (int boneIndex = 0; boneIndex < thisSkeleton.boneCount; boneIndex++)
						{
							Transform boneThis = thisSkeleton.GetBone(boneIndex);
							Transform boneOpposite = oppositeSkeleton.GetBone(boneIndex);

							boneThis.localPosition = boneOpposite.localPosition;
							boneThis.localRotation = boneOpposite.localRotation;

						}

						handData.thumbFingerMovementType = otherData.thumbFingerMovementType;
						handData.indexFingerMovementType = otherData.indexFingerMovementType;
						handData.middleFingerMovementType = otherData.middleFingerMovementType;
						handData.ringFingerMovementType = otherData.ringFingerMovementType;
						handData.pinkyFingerMovementType = otherData.pinkyFingerMovementType;

						EditorUtility.SetDirty(poser.skeletonMainPose);
					}
				}

			}

			EditorGUIUtility.labelWidth = 120;
			LuxHand_FingerExtensionTypes newThumb = (LuxHand_FingerExtensionTypes)EditorGUILayout.EnumPopup("Thumb movement", handData.thumbFingerMovementType);
			LuxHand_FingerExtensionTypes newIndex = (LuxHand_FingerExtensionTypes)EditorGUILayout.EnumPopup("Index movement", handData.indexFingerMovementType);
			LuxHand_FingerExtensionTypes newMiddle = (LuxHand_FingerExtensionTypes)EditorGUILayout.EnumPopup("Middle movement", handData.middleFingerMovementType);
			LuxHand_FingerExtensionTypes newRing = (LuxHand_FingerExtensionTypes)EditorGUILayout.EnumPopup("Ring movement", handData.ringFingerMovementType);
			LuxHand_FingerExtensionTypes newPinky = (LuxHand_FingerExtensionTypes)EditorGUILayout.EnumPopup("Pinky movement", handData.pinkyFingerMovementType);

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(showPreviewProperty);
			EditorGUIUtility.labelWidth = 0;

			if (newThumb != handData.thumbFingerMovementType || newIndex != handData.indexFingerMovementType ||
					newMiddle != handData.middleFingerMovementType || newRing != handData.ringFingerMovementType ||
					newPinky != handData.pinkyFingerMovementType)
			{
				handData.thumbFingerMovementType = newThumb;
				handData.indexFingerMovementType = newIndex;
				handData.middleFingerMovementType = newMiddle;
				handData.ringFingerMovementType = newRing;
				handData.pinkyFingerMovementType = newPinky;

				EditorUtility.SetDirty(poser.skeletonMainPose);
			}
		}

		protected void DrawPoseControlButtons()
		{
			GameObject leftInstance = previewLeftInstanceProperty.objectReferenceValue as GameObject;
			leftSkeleton = null;

			if (leftInstance != null)
			{
				leftSkeleton = leftInstance.GetComponentInChildren<LuxHandPoser>();
			}

			GameObject rightInstance = previewRightInstanceProperty.objectReferenceValue as GameObject;
			rightSkeleton = null;

			if (rightInstance != null)
			{
				rightSkeleton = rightInstance.GetComponentInChildren<LuxHandPoser>();
			}



			//only allow saving if a hand is opened for editing
			EditorGUI.BeginDisabledGroup(showRightPreviewProperty.boolValue == false && showLeftPreviewProperty.boolValue == false);
			GUI.color = new Color(0.9f, 1.0f, 0.9f);
			// save both hands at once, or whichever are being edited
			bool save = GUILayout.Button(string.Format("Save Pose"));
			if (save)
			{
				if (showRightPreviewProperty.boolValue)
					SaveHandData(activePose.rightHand, rightSkeleton);
				if (showLeftPreviewProperty.boolValue)
					SaveHandData(activePose.leftHand, leftSkeleton);
			}
			GUI.color = Color.white;

			if (GUILayout.Button("Save As"))
			{
				string fullPath = EditorUtility.SaveFilePanelInProject("Create New Skeleton Pose", "newPose", "asset", "Save file");

				if (string.IsNullOrEmpty(fullPath) == false)
				{
					LuxHandPose newPose = ScriptableObject.CreateInstance<LuxHandPose>();
					DeepCopyPose(activePose, newPose);
					AssetDatabase.CreateAsset(newPose, fullPath);
					AssetDatabase.SaveAssets();

					activePoseProp.objectReferenceValue = newPose;
					serializedObject.ApplyModifiedProperties();
				}
			}
			
			EditorGUI.EndDisabledGroup();

			//MIRRORING
			//only allow mirroring if both hands are opened for editing
			EditorGUI.BeginDisabledGroup(showRightPreviewProperty.boolValue == false || showLeftPreviewProperty.boolValue == false);

			EditorGUILayout.Space();

			if (GUILayout.Button("Import Pose"))
			{
				CopyPoseSelect();
			}

			EditorGUI.EndDisabledGroup();


			GUILayout.Space(32);


			/*
						GUILayout.Label("Reference Pose:");
						EditorGUILayout.Space();
						forceToReferencePose = (EVRSkeletalReferencePose)EditorGUILayout.EnumPopup(forceToReferencePose);
						GUI.color = new Color(1.0f, 0.73f, 0.7f);
						bool forcePose = GUILayout.Button("RESET TO REFERENCE POSE");
						GUI.color = Color.white;

						if (forcePose)
						{
							bool confirm = EditorUtility.DisplayDialog("SteamVR", string.Format("This will overwrite your current skeleton data. (with data from the {0} reference pose)", forceToReferencePose.ToString()), "Overwrite", "Cancel");
							if (confirm)
							{
								if (forceToReferencePose == EVRSkeletalReferencePose.GripLimit)
								{
									// grip limit is controller-specific, the rest use a baked pose
									if (showLeftPreviewProperty.boolValue)
										leftSkeleton.ForceToReferencePose(forceToReferencePose);
									if (showRightPreviewProperty.boolValue)
										rightSkeleton.ForceToReferencePose(forceToReferencePose);
								}
								else
								{
									LuxHandPose poseResource = null;
									if (forceToReferencePose == EVRSkeletalReferencePose.OpenHand)
										poseResource = (LuxHandPose)Resources.Load("ReferencePose_OpenHand");
									if (forceToReferencePose == EVRSkeletalReferencePose.Fist)
										poseResource = (LuxHandPose)Resources.Load("ReferencePose_Fist");
									if (forceToReferencePose == EVRSkeletalReferencePose.BindPose)
										poseResource = (LuxHandPose)Resources.Load("ReferencePose_BindPose");

									DeepCopyPose(poseResource, activePose);
								}
							}
						}
			*/

		}


		void CopyPoseSelect()
		{
			string selected = EditorUtility.OpenFilePanel("Open Skeleton Pose ScriptableObject", Application.dataPath, "asset");
			selected = selected.Replace(Application.dataPath, "Assets");

			if (selected == null) return;

			LuxHandPose newPose = (LuxHandPose)AssetDatabase.LoadAssetAtPath(selected, typeof(LuxHandPose));
			if (newPose == null)
			{
				EditorUtility.DisplayDialog("WARNING", "Asset could not be loaded. Is it not a LuxHandPose?", "ok");
				return;
			}
			DeepCopyPose(newPose, activePose);
		}


		void DeepCopyPose(LuxHandPose source, LuxHandPose dest)
		{
			int boneNum = LuxHandEnums.numBones;

			if (dest.rightHand.bonePositions == null || dest.rightHand.bonePositions.Length == 0) dest.rightHand.bonePositions = new Vector3[boneNum];
			if (dest.rightHand.boneRotations == null || dest.rightHand.boneRotations.Length == 0) dest.rightHand.boneRotations = new Quaternion[boneNum];

			if (dest.leftHand.bonePositions == null || dest.leftHand.bonePositions.Length == 0) dest.leftHand.bonePositions = new Vector3[boneNum];
			if (dest.leftHand.boneRotations == null || dest.leftHand.boneRotations.Length == 0) dest.leftHand.boneRotations = new Quaternion[boneNum];

			EditorUtility.SetDirty(dest);


			// RIGHT HAND COPY
			Debug.Log("[LuxPickupableEditor] DeepCopyPose boneNum: " + boneNum + " arraySize: " + dest.rightHand.bonePositions.Length);

			dest.rightHand.position = source.rightHand.position;
			dest.rightHand.rotation = source.rightHand.rotation;
			for (int boneIndex = 0; boneIndex < boneNum; boneIndex++)
			{
				dest.rightHand.bonePositions[boneIndex] = source.rightHand.bonePositions[boneIndex];
				dest.rightHand.boneRotations[boneIndex] = source.rightHand.boneRotations[boneIndex];
				EditorUtility.DisplayProgressBar("Copying...", "Copying right hand pose", (float)boneIndex / (float)boneNum / 2f);
			}
			dest.rightHand.thumbFingerMovementType = source.rightHand.thumbFingerMovementType;
			dest.rightHand.indexFingerMovementType = source.rightHand.indexFingerMovementType;
			dest.rightHand.middleFingerMovementType = source.rightHand.middleFingerMovementType;
			dest.rightHand.ringFingerMovementType = source.rightHand.ringFingerMovementType;
			dest.rightHand.pinkyFingerMovementType = source.rightHand.pinkyFingerMovementType;

			// LEFT HAND COPY

			dest.leftHand.position = source.leftHand.position;
			dest.leftHand.rotation = source.leftHand.rotation;
			for (int boneIndex = 0; boneIndex < boneNum; boneIndex++)
			{
				dest.leftHand.bonePositions[boneIndex] = source.leftHand.bonePositions[boneIndex];
				dest.leftHand.boneRotations[boneIndex] = source.leftHand.boneRotations[boneIndex];
				EditorUtility.DisplayProgressBar("Copying...", "Copying left hand pose", (float)boneIndex / (float)boneNum / 2f);
			}
			dest.leftHand.thumbFingerMovementType = source.leftHand.thumbFingerMovementType;
			dest.leftHand.indexFingerMovementType = source.leftHand.indexFingerMovementType;
			dest.leftHand.middleFingerMovementType = source.leftHand.middleFingerMovementType;
			dest.leftHand.ringFingerMovementType = source.leftHand.ringFingerMovementType;
			dest.leftHand.pinkyFingerMovementType = source.leftHand.pinkyFingerMovementType;

			EditorUtility.SetDirty(dest);

			forceUpdateHands = true;
			EditorUtility.ClearProgressBar();
		}

		int activePoseIndex = 0;
		SerializedProperty activePoseProp;
		LuxHandPose activePose;

		bool forceUpdateHands = false;

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			serializedObject.Update();

			DrawPoseEditorMenu();

			DrawBlendingBehaviourMenu();


			serializedObject.ApplyModifiedProperties();
		}

		bool getRightFromOpposite = false;
		bool getLeftFromOpposite = false;

		LuxHandPoser leftSkeleton = null;
		LuxHandPoser rightSkeleton = null;

		void DrawPoseEditorMenu()
		{
			if (Application.isPlaying)
			{
				EditorGUILayout.LabelField("Cannot modify pose while in play mode.");
			}
			else
			{
				bool createNew = false;

				LoadDefaultPreviewHands();


				activePoseIndex = previewPoseSelection.intValue;
				if (activePoseIndex == 0)
					activePoseProp = skeletonMainPoseProperty;
				else
					activePoseProp = skeletonAdditionalPosesProperty.GetArrayElementAtIndex(activePoseIndex - 1);


				//box containing all pose editing controls
				GUILayout.BeginVertical("box");


				poseEditorExpanded.boolValue = IndentedFoldoutHeader(poseEditorExpanded.boolValue, "Pose Editor");


				if (poseEditorExpanded.boolValue)
				{
					//show selectable menu of all poses, highlighting the one that is selected
					EditorGUILayout.Space();


					poser.poseNames = new string[skeletonAdditionalPosesProperty.arraySize + 1];

					for (int i = 0; i < skeletonAdditionalPosesProperty.arraySize + 1; i++)
					{
						if (i == 0)
							// main pose special case
							poser.poseNames[i] = skeletonMainPoseProperty.objectReferenceValue == null ? "[not set]" : skeletonMainPoseProperty.objectReferenceValue.name + " (MAIN)";
						else
							// additional poses from array
							poser.poseNames[i] = skeletonAdditionalPosesProperty.GetArrayElementAtIndex(i - 1).objectReferenceValue == null ? "[not set]" : skeletonAdditionalPosesProperty.GetArrayElementAtIndex(i - 1).objectReferenceValue.name;
					}

					EditorGUILayout.BeginHorizontal();
					int poseSelected = GUILayout.Toolbar(activePoseIndex, poser.poseNames);

					if (poseSelected != activePoseIndex)
					{
						forceUpdateHands = true;
						activePoseIndex = poseSelected;
						PoseChanged = true;
						previewPoseSelection.intValue = activePoseIndex;
						serializedObject.ApplyModifiedProperties();
					}




					EditorGUILayout.BeginVertical(GUILayout.MaxWidth(32));
					if (GUILayout.Button("+", GUILayout.MaxWidth(32)))
					{
						skeletonAdditionalPosesProperty.InsertArrayElementAtIndex(skeletonAdditionalPosesProperty.arraySize);
					}
					//only allow deletion of additional poses
					EditorGUI.BeginDisabledGroup(skeletonAdditionalPosesProperty.arraySize == 0 || activePoseIndex == 0);
					if (GUILayout.Button("-", GUILayout.MaxWidth(32)) && skeletonAdditionalPosesProperty.arraySize > 0)
					{
						skeletonAdditionalPosesProperty.DeleteArrayElementAtIndex(activePoseIndex - 1);
						skeletonAdditionalPosesProperty.DeleteArrayElementAtIndex(activePoseIndex - 1);
						if (activePoseIndex >= skeletonAdditionalPosesProperty.arraySize + 1)
						{
							activePoseIndex = skeletonAdditionalPosesProperty.arraySize;
							previewPoseSelection.intValue = activePoseIndex;
							return;
						}
					}

					EditorGUI.EndDisabledGroup();
					EditorGUILayout.EndVertical();
					GUILayout.FlexibleSpace();

					EditorGUILayout.EndHorizontal();

					GUILayout.BeginVertical("box");

					// sides of pose editor
					GUILayout.BeginHorizontal();

					//pose controls
					GUILayout.BeginVertical(GUILayout.MaxWidth(200));

					GUILayout.Label("Current Pose:");

					if (PoseChanged)
					{
						PoseChanged = false;
						forceUpdateHands = true;

						if (activePoseIndex == 0)
							activePoseProp = skeletonMainPoseProperty;
						else
							activePoseProp = skeletonAdditionalPosesProperty.GetArrayElementAtIndex(activePoseIndex - 1);
						activePose = (LuxHandPose)activePoseProp.objectReferenceValue;

					}


					activePose = (LuxHandPose)activePoseProp.objectReferenceValue;
					if (activePoseProp.objectReferenceValue == null)
					{
						if (previewLeftInstanceProperty.objectReferenceValue != null)
							DestroyImmediate(previewLeftInstanceProperty.objectReferenceValue);
						if (previewRightInstanceProperty.objectReferenceValue != null)
							DestroyImmediate(previewRightInstanceProperty.objectReferenceValue);

						EditorGUILayout.BeginHorizontal();
						activePoseProp.objectReferenceValue = EditorGUILayout.ObjectField(activePoseProp.objectReferenceValue, typeof(LuxHandPose), false);
						if (GUILayout.Button("Create")) createNew = true;
						EditorGUILayout.EndHorizontal();
						if (createNew)
						{
							string fullPath = EditorUtility.SaveFilePanelInProject("Create New Skeleton Pose", "newPose", "asset", "Save file");

							if (string.IsNullOrEmpty(fullPath) == false)
							{
								LuxHandPose newPose = ScriptableObject.CreateInstance<LuxHandPose>();
								AssetDatabase.CreateAsset(newPose, fullPath);
								AssetDatabase.SaveAssets();

								activePoseProp.objectReferenceValue = newPose;
								serializedObject.ApplyModifiedProperties();
							}
						}
					}
					else
					{
						activePoseProp.objectReferenceValue = EditorGUILayout.ObjectField(activePoseProp.objectReferenceValue, typeof(LuxHandPose), false);

						DrawPoseControlButtons();
						var previewHandLeft = FindDefaultPreviewHand("Lux_LeftHand_Poser");
						var previewHandRight = FindDefaultPreviewHand("Lux_RightHand_Poser");
						UpdatePreviewHand(previewLeftInstanceProperty, showLeftPreviewProperty, previewHandLeft, activePose.leftHand, activePose, forceUpdateHands);
						UpdatePreviewHand(previewRightInstanceProperty, showRightPreviewProperty, previewHandRight, activePose.rightHand, activePose, forceUpdateHands);

						forceUpdateHands = false;

						GUILayout.EndVertical();




						GUILayout.Space(10);

						if (handTexL == null)
							handTexL = (Texture)EditorGUIUtility.Load("Assets/Luxsonic/CareGiVR/Rig/Resources/Icons/HandLeftIcon.png");
						if (handTexR == null)
							handTexR = (Texture)EditorGUIUtility.Load("Assets/Luxsonic/CareGiVR/Rig/Resources/Icons/HandRightIcon.png");


						//Left Hand

						GUILayout.Space(32);
						EditorGUILayout.BeginVertical();
						EditorGUILayout.BeginVertical("box");
						EditorGUILayout.BeginHorizontal();
						GUI.color = new Color(1, 1, 1, showLeftPreviewProperty.boolValue ? 1 : 0.25f);
						if (GUILayout.Button(handTexL, GUI.skin.label, GUILayout.Width(64), GUILayout.Height(64)))
						{
							showLeftPreviewProperty.boolValue = !showLeftPreviewProperty.boolValue;
							//forceUpdateHands = true;
						}
						GUI.color = Color.white;

						EditorGUIUtility.labelWidth = 48;
						EditorGUILayout.LabelField("Left Hand", EditorStyles.boldLabel);
						EditorGUIUtility.labelWidth = 0;
						GUILayout.FlexibleSpace();
						EditorGUILayout.EndHorizontal();

						bool showLeft = showLeftPreviewProperty.boolValue;


						DrawHand(showLeft, activePose.leftHand, activePose.rightHand, getLeftFromOpposite, showLeftPreviewProperty);
						EditorGUILayout.EndVertical();
						EditorGUI.BeginDisabledGroup((showLeftPreviewProperty.boolValue && showRightPreviewProperty.boolValue) == false);
						getRightFromOpposite = GUILayout.Button("Copy Left pose to Right hand");
						EditorGUI.EndDisabledGroup();
						EditorGUILayout.EndVertical();



						EditorGUILayout.BeginVertical();
						EditorGUILayout.BeginVertical("box");

						EditorGUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						EditorGUIUtility.labelWidth = 48;
						EditorGUILayout.LabelField("Right Hand", EditorStyles.boldLabel);
						EditorGUIUtility.labelWidth = 0;
						GUI.color = new Color(1, 1, 1, showRightPreviewProperty.boolValue ? 1 : 0.25f);
						if (GUILayout.Button(handTexR, GUI.skin.label, GUILayout.Width(64), GUILayout.Height(64)))
						{
							showRightPreviewProperty.boolValue = !showRightPreviewProperty.boolValue;
							//forceUpdateHands = true;
						}
						GUI.color = Color.white;
						EditorGUILayout.EndHorizontal();

						bool showRight = showLeftPreviewProperty.boolValue;

						DrawHand(showRight, activePose.rightHand, activePose.leftHand, getRightFromOpposite, showRightPreviewProperty);
						EditorGUILayout.EndVertical();
						EditorGUI.BeginDisabledGroup((showLeftPreviewProperty.boolValue && showRightPreviewProperty.boolValue) == false);
						getLeftFromOpposite = GUILayout.Button("Copy Right pose to Left hand");
						EditorGUI.EndDisabledGroup();

					}







					/*




                    if (activePoseProp.objectReferenceValue == null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(activePoseProp);
                        createNew = GUILayout.Button("Create");
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(activePoseProp);

                        DrawDivider();


                        DrawSaveButtons();

                        if (PoseChanged)
                        {
                            PoseChanged = false;
                            forceUpdateHands = true;
                        }

                        UpdatePreviewHand(previewLeftInstanceProperty, showLeftPreviewProperty, previewLeftHandPrefab, activePose.leftHand, forceUpdateHands);
                        UpdatePreviewHand(previewRightInstanceProperty, showRightPreviewProperty, previewRightHandPrefab, activePose.rightHand, forceUpdateHands);

                    }

                                activePoseProp.objectReferenceValue = newPose;
                                serializedObject.ApplyModifiedProperties();
                            }
                        }
                    */
					GUILayout.EndVertical();
					EditorGUILayout.EndVertical();
					GUILayout.EndHorizontal();


					EditorGUI.BeginChangeCheck();
					EditorGUILayout.BeginHorizontal();
					EditorGUIUtility.labelWidth = 120;
					poserScale.floatValue = EditorGUILayout.FloatField("Preview Pose Scale", poserScale.floatValue);
					if (poserScale.floatValue <= 0) poserScale.floatValue = 1;
					EditorGUIUtility.labelWidth = 0;
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();
					if (EditorGUI.EndChangeCheck())
					{
						forceUpdateHands = true;
					}
				}

				GUILayout.EndVertical();
			}
		}


		bool SelectablePoseButton(string name, bool selected)
		{
			if (selected)
			{
				GUI.color = new Color(0.7f, 0.73f, 0.8f);
				GUILayout.Button(name, GUILayout.ExpandWidth(false));
				GUI.color = Color.white;

				return false;
			}
			else
			{
				return GUILayout.Button(name, GUILayout.ExpandWidth(false));
			}
		}

		Texture[] handMaskTextures;

		void DrawBlendingBehaviourMenu()
		{
			if (handMaskTextures == null)
			{
				handMaskTextures = new Texture[] { (Texture)EditorGUIUtility.Load("Assets/Luxsonic/CareGiVR/Rig/Resources/Icons/handmask_Palm.png"),
				(Texture)EditorGUIUtility.Load("Assets/Luxsonic/CareGiVR/Rig/Resources/Icons/handmask_Thumb.png"),
				(Texture)EditorGUIUtility.Load("Assets/Luxsonic/CareGiVR/Rig/Resources/Icons/handmask_Index.png"),
				(Texture)EditorGUIUtility.Load("Assets/Luxsonic/CareGiVR/Rig/Resources/Icons/handmask_Middle.png"),
				(Texture)EditorGUIUtility.Load("Assets/Luxsonic/CareGiVR/Rig/Resources/Icons/handmask_Ring.png"),
				(Texture)EditorGUIUtility.Load("Assets/Luxsonic/CareGiVR/Rig/Resources/Icons/handmask_Pinky.png")
				};
			}

			GUILayout.Space(10);
			/*
            GUILayout.BeginVertical("box");

            blendEditorExpanded.boolValue = IndentedFoldoutHeader(blendEditorExpanded.boolValue, "Blending Editor");

            if (blendEditorExpanded.boolValue)
            {
                //show selectable menu of all poses, highlighting the one that is selected
                EditorGUILayout.Space();
                for (int i = 0; i < blendingBehaviourArray.arraySize; i++)
                {

                    SerializedProperty blender = blendingBehaviourArray.GetArrayElementAtIndex(i);
                    SerializedProperty blenderName = blender.FindPropertyRelative("name");
                    SerializedProperty blenderEnabled = blender.FindPropertyRelative("enabled");
                    SerializedProperty blenderInfluence = blender.FindPropertyRelative("influence");
                    SerializedProperty blenderPose = blender.FindPropertyRelative("pose");
                    SerializedProperty blenderType = blender.FindPropertyRelative("type");
                    SerializedProperty blenderUseMask = blender.FindPropertyRelative("useMask");
                    SerializedProperty blenderValue = blender.FindPropertyRelative("value");
                    SerializedProperty blenderMask = blender.FindPropertyRelative("mask").FindPropertyRelative("values");

                    SerializedProperty blenderPreview = blender.FindPropertyRelative("previewEnabled");

                    GUILayout.Space(10);
                    float bright = blenderEnabled.boolValue ? 0.6f : 0.9f; // grey out box when disabled
                    if (EditorGUIUtility.isProSkin) bright = 1;
                    GUI.color = new Color(bright, bright, bright);
                    GUILayout.BeginVertical("box");
                    GUI.color = Color.white;

                    blenderPreview.boolValue = IndentedFoldoutHeader(blenderPreview.boolValue, blenderName.stringValue, 1);

                    //SerializedProperty blenderValue = blender.FindProperty("value");

                    EditorGUIUtility.labelWidth = 64;

                    EditorGUILayout.BeginHorizontal();
                    DrawBlenderLogo(blenderType);
                    EditorGUILayout.PropertyField(blenderEnabled);
                    GUILayout.FlexibleSpace();

                    EditorGUI.BeginDisabledGroup(i == 0);
                    if (GUILayout.Button("Move Up"))
                    {
                        blendingBehaviourArray.MoveArrayElement(i, i - 1);
                    }
                    EditorGUI.EndDisabledGroup();

                    EditorGUI.BeginDisabledGroup(i == blendingBehaviourArray.arraySize - 1);
                    if (GUILayout.Button("Move Down"))
                    {
                        blendingBehaviourArray.MoveArrayElement(i, i + 1);
                    }
                    EditorGUI.EndDisabledGroup();

                    GUILayout.Space(6);
                    GUI.color = new Color(0.9f, 0.8f, 0.78f);
                    if (GUILayout.Button("Delete"))
                    {
                        if (EditorUtility.DisplayDialog("", "Do you really want to delete this Blend Behaviour?", "Yes", "Cancel"))
                        {
                            blendingBehaviourArray.DeleteArrayElementAtIndex(i);
                            return;
                        }
                    }
                    GUI.color = Color.white;
                    EditorGUILayout.EndHorizontal();

                    if (blenderPreview.boolValue)
                    {

                        EditorGUILayout.PropertyField(blenderName);



                        EditorGUILayout.BeginHorizontal();


                        //EditorGUILayout.BeginVertical();


                        EditorGUILayout.BeginVertical();

                        EditorGUILayout.Slider(blenderInfluence, 0, 1);

                        blenderPose.intValue = EditorGUILayout.Popup("Pose", blenderPose.intValue, poser.poseNames);

                        GUILayout.Space(20);

                        EditorGUILayout.PropertyField(blenderType);

                        if (Application.isPlaying)
                        {
                            GUILayout.Space(10);
                            GUI.color = new Color(0, 0, 0, 0.3f);
                            EditorGUILayout.LabelField("", GUI.skin.box, GUILayout.Height(20), GUILayout.ExpandWidth(true));
                            GUI.color = Color.white;
                            Rect fillRect = GUILayoutUtility.GetLastRect();
                            EditorGUI.DrawRect(fillRectHorizontal(fillRect, blenderValue.floatValue), Color.green);
                        }

                        EditorGUILayout.EndVertical();



                        EditorGUILayout.BeginVertical(GUILayout.MaxWidth(100));
                        EditorGUILayout.PropertyField(blenderUseMask);
                        if (blenderUseMask.boolValue)
                        {
                            DrawMaskEnabled(blenderMask);
                        }
                        else
                        {
                            DrawMaskDisabled(blenderMask);
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.EndHorizontal();



                        DrawDivider();

                        EditorGUIUtility.labelWidth = 0;


                        if (blenderType.intValue == (int)LuxHandPoser.PoseBlendingBehaviour.BlenderTypes.Manual)
                        {
                            EditorGUILayout.Slider(blenderValue, 0, 1);
                        }
                        if (blenderType.intValue == (int)LuxHandPoser.PoseBlendingBehaviour.BlenderTypes.AnalogAction)
                        {
                            SerializedProperty blenderAction = blender.FindPropertyRelative("action_single");
                            SerializedProperty blenderSmooth = blender.FindPropertyRelative("smoothingSpeed");
                            EditorGUILayout.PropertyField(blenderAction);
                            EditorGUILayout.PropertyField(blenderSmooth);
                        }
                        if (blenderType.intValue == (int)LuxHandPoser.PoseBlendingBehaviour.BlenderTypes.BooleanAction)
                        {
                            SerializedProperty blenderAction = blender.FindPropertyRelative("action_bool");
                            SerializedProperty blenderSmooth = blender.FindPropertyRelative("smoothingSpeed");
                            EditorGUILayout.PropertyField(blenderAction);
                            EditorGUILayout.PropertyField(blenderSmooth);
                        }
                    }

                    GUILayout.EndVertical();

                }


                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("+", GUILayout.MaxWidth(32)))
                {
                    int i = blendingBehaviourArray.arraySize;
                    blendingBehaviourArray.InsertArrayElementAtIndex(i);
                    blendingBehaviourArray.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue = "New Behaviour";
                    blendingBehaviourArray.GetArrayElementAtIndex(i).FindPropertyRelative("enabled").boolValue = true;
                    blendingBehaviourArray.GetArrayElementAtIndex(i).FindPropertyRelative("influence").floatValue = 1;
                    blendingBehaviourArray.GetArrayElementAtIndex(i).FindPropertyRelative("previewEnabled").boolValue = true;
                    serializedObject.ApplyModifiedProperties();
                    //poser.blendingBehaviours[i].mask.Reset();
                }
                GUILayout.FlexibleSpace();

                EditorGUILayout.EndHorizontal();

            }
            GUILayout.EndVertical();
			*/
		}

		Rect fillRectHorizontal(Rect r, float f)
		{
			r.xMax--;
			r.yMax--;
			r.xMin++;
			r.yMin++;
			r.width *= f;
			return r;
		}

		/*
        void DrawBlenderLogo(SerializedProperty blenderType)
        {
            Texture icon = null;
            if (blenderType.intValue == (int)LuxHandPoser.PoseBlendingBehaviour.BlenderTypes.Manual)
            {
                icon = (Texture)EditorGUIUtility.Load("Assets/Luxsonic/CareGiVR/Rig/Resources/Icons/BlenderBehaviour_Manual_Icon.png");
            }
            if (blenderType.intValue == (int)LuxHandPoser.PoseBlendingBehaviour.BlenderTypes.AnalogAction)
            {
                icon = (Texture)EditorGUIUtility.Load("Assets/Luxsonic/CareGiVR/Rig/Resources/Icons/BlenderBehaviour_Analog_Icon.png");
            }
            if (blenderType.intValue == (int)LuxHandPoser.PoseBlendingBehaviour.BlenderTypes.BooleanAction)
            {
                icon = (Texture)EditorGUIUtility.Load("Assets/Luxsonic/CareGiVR/Rig/Resources/Icons/BlenderBehaviour_Boolean_Icon.png");
            }

            GUILayout.Label(icon, GUILayout.MaxHeight(32), GUILayout.MaxWidth(32));
        }
		*/

		Color maskColorEnabled = new Color(0.3f, 1.0f, 0.3f, 1.0f);
		Color maskColorDisabled = new Color(0, 0, 0, 0.5f);
		Color maskColorUnused = new Color(0.3f, 0.7f, 0.3f, 0.3f);

		void DrawMaskDisabled(SerializedProperty mask)
		{
			Texture m = (Texture)EditorGUIUtility.Load("Assets/Luxsonic/CareGiVR/Rig/Resources/Icons/handmask.png");
			GUI.color = maskColorUnused;
			GUILayout.Label(m, GUILayout.MaxHeight(100), GUILayout.MaxWidth(100));
			GUI.color = Color.white;
		}

		void DrawMaskEnabled(SerializedProperty mask)
		{
			GUILayout.Label("", GUILayout.Height(100), GUILayout.Width(100));
			Rect maskRect = GUILayoutUtility.GetLastRect();
			for (int i = 0; i < 6; i++)
			{
				GUI.color = mask.GetArrayElementAtIndex(i).boolValue ? maskColorEnabled : maskColorDisabled;
				GUI.Label(maskRect, handMaskTextures[i]);
				GUI.color = new Color(0, 0, 0, 0.0f);
				if (GUI.Button(GetFingerAreaRect(maskRect, i), ""))
				{
					mask.GetArrayElementAtIndex(i).boolValue = !mask.GetArrayElementAtIndex(i).boolValue;
				}
				GUI.color = Color.white;
				//maskVal
			}
		}

		/// <summary>
		/// Defines area of mask icon to be buttons for each finger
		/// </summary>
		Rect GetFingerAreaRect(Rect source, int i)
		{
			Rect outRect = source;
			if (i == 0)
			{
				outRect.xMin = Mathf.Lerp(source.xMin, source.xMax, 0.4f); // left edge
				outRect.xMax = Mathf.Lerp(source.xMin, source.xMax, 0.8f); // right edge

				outRect.yMin = Mathf.Lerp(source.yMin, source.yMax, 0.5f); // top edge
				outRect.yMax = Mathf.Lerp(source.yMin, source.yMax, 1.0f); // bottom edge
			}
			if (i == 1)
			{
				outRect.xMin = Mathf.Lerp(source.xMin, source.xMax, 0.0f); // left edge
				outRect.xMax = Mathf.Lerp(source.xMin, source.xMax, 0.4f); // right edge

				outRect.yMin = Mathf.Lerp(source.yMin, source.yMax, 0.5f); // top edge
				outRect.yMax = Mathf.Lerp(source.yMin, source.yMax, 1.0f); // bottom edge
			}
			if (i == 2)
			{
				outRect.xMin = Mathf.Lerp(source.xMin, source.xMax, 0.3f); // left edge
				outRect.xMax = Mathf.Lerp(source.xMin, source.xMax, 0.425f); // right edge

				outRect.yMin = Mathf.Lerp(source.yMin, source.yMax, 0.0f); // top edge
				outRect.yMax = Mathf.Lerp(source.yMin, source.yMax, 0.5f); // bottom edge
			}
			if (i == 3)
			{
				outRect.xMin = Mathf.Lerp(source.xMin, source.xMax, 0.425f); // left edge
				outRect.xMax = Mathf.Lerp(source.xMin, source.xMax, 0.55f); // right edge

				outRect.yMin = Mathf.Lerp(source.yMin, source.yMax, 0.0f); // top edge
				outRect.yMax = Mathf.Lerp(source.yMin, source.yMax, 0.5f); // bottom edge
			}
			if (i == 4)
			{
				outRect.xMin = Mathf.Lerp(source.xMin, source.xMax, 0.55f); // left edge
				outRect.xMax = Mathf.Lerp(source.xMin, source.xMax, 0.675f); // right edge

				outRect.yMin = Mathf.Lerp(source.yMin, source.yMax, 0.0f); // top edge
				outRect.yMax = Mathf.Lerp(source.yMin, source.yMax, 0.5f); // bottom edge
			}
			if (i == 5)
			{
				outRect.xMin = Mathf.Lerp(source.xMin, source.xMax, 0.675f); // left edge
				outRect.xMax = Mathf.Lerp(source.xMin, source.xMax, 0.8f); // right edge

				outRect.yMin = Mathf.Lerp(source.yMin, source.yMax, 0.0f); // top edge
				outRect.yMax = Mathf.Lerp(source.yMin, source.yMax, 0.5f); // bottom edge
			}
			return outRect;
		}



		void DrawDivider()
		{
			GUI.color = new Color(0, 0, 0, 0.6f);
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			EditorGUILayout.Space();
			GUI.color = Color.white;
		}

		bool IndentedFoldoutHeader(bool fold, string text, int indent = 1)
		{
			GUILayout.BeginHorizontal();
			GUIStyle boldFoldoutStyle = new GUIStyle(EditorStyles.foldout);
			boldFoldoutStyle.fontStyle = FontStyle.Bold;
			GUILayout.Space(14f * indent);
			fold = EditorGUILayout.Foldout(fold, text, boldFoldoutStyle);
			GUILayout.EndHorizontal();
			return fold;
		}


		private static GameObject FindDefaultPreviewHand(string assetName)
		{
#if UNITY_EDITOR
			string[] defaultPaths = UnityEditor.AssetDatabase.FindAssets(string.Format("t:Prefab {0}", assetName));
			if (defaultPaths != null && defaultPaths.Length > 0)
			{
				string defaultGUID = defaultPaths[0];
				string defaultPath = UnityEditor.AssetDatabase.GUIDToAssetPath(defaultGUID);
				GameObject defaultAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(defaultPath);

				if (defaultAsset == null)
					Debug.LogError("[SteamVR] Could not load default hand preview prefab: " + assetName + ". Found path: " + defaultPath);

				return defaultAsset;
			}
			//else //todo: this will generally fail on the first try but will try again before its an issue.
			//Debug.LogError("[SteamVR] Could not load default hand preview prefab: " + assetName);
#endif

			return null;

		}
	}
}
