using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caregivr
{
	public class LuxInput : MonoBehaviour
	{
		public LuxInputController leftController {get; private set;}
		public LuxInputController rightController {get; private set;}

		public LuxInputController[] controllers;

		public enum InputMode {controllers,gloves}
		public InputMode inputMode;

		void Start()
		{
			leftController = LuxInputController.AddPlatformComponent(gameObject, LuxHand.LuxHandDirection.Left);
			rightController = LuxInputController.AddPlatformComponent(gameObject, LuxHand.LuxHandDirection.Right);
			controllers = new LuxInputController[] { leftController, rightController };
		}

		public LuxInputController GetLuxInputController(LuxHand.LuxHandDirection luxHandDirection)
		{
			if (luxHandDirection == LuxHand.LuxHandDirection.Left)
			{
				return leftController;
			}
			return rightController;
		}
	}
}
