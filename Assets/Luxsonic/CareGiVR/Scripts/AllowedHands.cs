using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caregivr
{

public enum AllowedHands
{
  	Both, Left, Right, None
}

public static class AllowedHandsUtil {
	public static bool AllowedHandsCompare(this AllowedHands allowedHands, LuxHand.LuxHandDirection handDirection)
	{
		if (allowedHands == AllowedHands.Both)
		{
			return true;
		}
		else if (allowedHands == AllowedHands.Left && handDirection == LuxHand.LuxHandDirection.Left)
		{
			return true;
		}
		else if (allowedHands == AllowedHands.Right && handDirection == LuxHand.LuxHandDirection.Right)
		{
			return true;
		}
		
		return false;
	}
}

}
