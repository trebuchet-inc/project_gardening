using UnityEngine;
using System.Collections;

namespace NewtonVR
{
    /// <summary>
    /// This interactable item script clips through other colliders. If you don't want your item to respect other object's positions 
    /// and have it go through walls/floors/etc then you can use this.
    /// </summary>
    public class NVRInteractableItemClippable : NVRInteractableItem
    {
        protected override void UpdateVelocities()
        {
            Vector3 targetItemPosition;
            Quaternion targetItemRotation;

            Vector3 targetHandPosition;
            Quaternion targetHandRotation;

            GetTargetValues(out targetHandPosition, out targetHandRotation, out targetItemPosition, out targetItemRotation);

            this.rigidbody.MovePosition(targetHandPosition);
            this.rigidbody.MoveRotation(targetHandRotation);
        }

        public override void BeginInteraction(NVRHand hand)
        {
            base.BeginInteraction(hand);

            this.rigidbody.isKinematic = true;
        }

        public override void EndInteraction(NVRHand hand)
        {
            base.EndInteraction(hand);

            this.rigidbody.isKinematic = false;
        }

    }
}