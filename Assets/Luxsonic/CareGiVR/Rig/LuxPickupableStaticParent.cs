using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caregivr
{
    public class LuxPickupableStaticParent : MonoBehaviour
    {
        [HideInInspector]
        public Transform psuedoHand = null;

        public void Update() {
            if (psuedoHand) {
                this.transform.position = psuedoHand.transform.position;
            }
        }
    }
}
