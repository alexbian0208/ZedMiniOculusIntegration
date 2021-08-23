using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Caregivr
{
	[RequireComponent(typeof(GloveLaserBlocker))]
	[RequireComponent(typeof(Rigidbody))]
	public class LuxPickupable : MonoBehaviour
	{
		// Properties
		public bool isHovered { get { return hoveringHighlightRenderer != null; } }
		public bool isAttached { get { return attachedLuxHand != null; } }
		public bool isAttachedLeft { get { return attachedLuxHand != null && attachedLuxHand.handDirection == LuxHand.LuxHandDirection.Left; } }
		public bool isAttachedRight { get { return attachedLuxHand != null && attachedLuxHand.handDirection == LuxHand.LuxHandDirection.Right; } }
		public LuxHand attachedLuxHand { get; private set; }
		public bool disableKinematicOnDrop { get; set; }
		
		// Public
		public bool isGrabbable = true;
		public bool moveHandToObject = false;
		public UnityEvent OnGrabbed;
		public UnityEvent OnDropped;
		public static float floorLevel = 0.0f;
	
		// Private
		private HighlightRenderer hoveringHighlightRenderer;
		private new Rigidbody rigidbody;
		private ObjectIdentity objectIdentity;
		private Vector3 startPosition;
		private Quaternion startRotation;
		private int originalLayer;

		#region preview stuff
		[HideInInspector]
		public GameObject overridePreviewLeftHandPrefab;
		
		[HideInInspector]
		public GameObject overridePreviewRightHandPrefab;
		
		[HideInInspector]
		public LuxHandPose skeletonMainPose;
		
		[HideInInspector]
		public List<LuxHandPose> skeletonAdditionalPoses = new List<LuxHandPose>();
		
		[HideInInspector]
		[SerializeField]
		protected bool showLeftPreview = false;
		
		[HideInInspector]
		[SerializeField]
		protected bool showRightPreview = true; //show the right hand by default
		
		[HideInInspector]
		[SerializeField]
		protected GameObject previewLeftInstance;

		[HideInInspector]
		[SerializeField]
		protected GameObject previewRightInstance;

		[HideInInspector]
		[SerializeField]
		protected int previewPoseSelection = 0;

		[HideInInspector]
		public float scale;

		#endregion

		#region Editor Storage
		[HideInInspector]
		public bool poseEditorExpanded = true;
		[HideInInspector]
		public bool blendEditorExpanded = true;
		[HideInInspector]
		public string[] poseNames;
		#endregion

		void Awake()
		{
			rigidbody = GetComponent<Rigidbody>();
			objectIdentity = GetComponent<ObjectIdentity>();
			startPosition = transform.localPosition;
			startRotation = transform.localRotation;
			originalLayer = gameObject.layer;
		}
		
		public void OnAttached(LuxHand luxHand)
		{
			attachedLuxHand = luxHand;

			if (objectIdentity != null)
			{
				objectIdentity.EmitEvent("grab");
			}
			
			if (OnGrabbed != null)
			{
				OnGrabbed.Invoke();
			}

			if (rigidbody != null)
			{
				disableKinematicOnDrop = !rigidbody.isKinematic;
				rigidbody.isKinematic = true;
			}
			gameObject.SetLayerRecursive(luxHand.gameObject.layer);
		}

		public void OnDetach(LuxHand luxHand)
		{
			attachedLuxHand = null;
			gameObject.SetLayerRecursive(originalLayer);
			if (objectIdentity != null)
			{
				objectIdentity.EmitEvent("drop");
			}

			if (OnDropped != null)
			{
				OnDropped.Invoke();
			}

			if (disableKinematicOnDrop)
			{
				if (rigidbody != null)
				{
					rigidbody.isKinematic = false;
				}
			}
		}

		public void StartHighlight()
		{
			if (hoveringHighlightRenderer == null)
			{
				hoveringHighlightRenderer = gameObject.AddComponent<HighlightRenderer>();
				hoveringHighlightRenderer.overrideColor = true;
				hoveringHighlightRenderer.highlightColor = Color.green;
				hoveringHighlightRenderer.highlightEnabled = true;
				hoveringHighlightRenderer.highlightWidth = 0.003f;
			}
		}

		public void EndHighlight()
		{
			if (hoveringHighlightRenderer != null)
			{
				Destroy(hoveringHighlightRenderer);
				hoveringHighlightRenderer = null;
			}
		}

		public virtual void Update() {
			if (transform.position.y < floorLevel) {
				rigidbody.transform.localPosition = startPosition;
				rigidbody.transform.localRotation = startRotation;
				rigidbody.velocity = Vector3.zero;
			}

			if (attachedLuxHand != null)
			{
				if (objectIdentity != null)
				{
					objectIdentity.EmitEvent("grab");
				}
			}
		}
	}
}
