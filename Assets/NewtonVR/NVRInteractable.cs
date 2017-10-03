using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace NewtonVR
{
    public abstract class NVRInteractable : MonoBehaviour
    {
        public Rigidbody rigidbody;

        public bool CanAttach = true;

        public bool AllowTwoHanded = false;
        
        public bool DisableKinematicOnAttach = true;
        public bool EnableKinematicOnDetach = false;
        public float DropDistance = 1;

        public bool EnableGravityOnDetach = true;

        public List<NVRHand> AttachedHands = new List<NVRHand>();
        public NVRHand AttachedHand
        {
            get
            {
                if (AttachedHands.Count == 0)
                    return null;

                return AttachedHands[0];
            }
        }

        protected Collider[] Colliders = new Collider[0];
        protected Vector3 ClosestHeldPoint;

        public UnityEvent OnHovering;

        public virtual bool IsAttached
        {
            get
            {
                return AttachedHand != null;
            }
        }

        protected virtual void Awake()
        {   
            if (rigidbody == null)
                rigidbody = this.GetComponent<Rigidbody>();

            if (rigidbody == null)
            {
                Debug.LogError("There is no rigidbody attached to this interactable.");
            }

        }

        protected virtual void Start()
        {
            UpdateColliders();
        }

        public virtual void ResetInteractable()
        {
            Awake();
            Start();
            AttachedHands.Clear();
        }

        public virtual void UpdateColliders()
        {
            Colliders = this.GetComponentsInChildren<Collider>();
            NVRInteractables.Register(this, Colliders);
        }

        protected virtual bool CheckForDrop()
        {

            for (int handIndex = 0; handIndex < AttachedHands.Count; handIndex++)
            {
                float shortestDistance = float.MaxValue;
                NVRHand hand = AttachedHands[handIndex];
                for (int index = 0; index < Colliders.Length; index++)
                {
                    //todo: this does not do what I think it does.
                    Vector3 closest = Colliders[index].ClosestPointOnBounds(hand.transform.position);
                    float distance = Vector3.Distance(hand.transform.position, closest);

                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        ClosestHeldPoint = closest;
                    }
                }

                if (DropDistance != -1 && hand.CurrentInteractionStyle != InterationStyle.ByScript && shortestDistance > DropDistance)
                {
                    DroppedBecauseOfDistance(hand);

                    if (IsAttached == false)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        protected virtual void Update()
        {
        }

        public virtual void BeginInteraction(NVRHand hand)
        {
            AttachedHands.Add(hand);

            if (DisableKinematicOnAttach == true)
            {
                rigidbody.isKinematic = false;
            }
        }

        public virtual void InteractingUpdate(NVRHand hand)
        {
            if (hand.UseButtonUp == true)
            {
                UseButtonUp();
            }

            if (hand.UseButtonDown == true)
            {
                UseButtonDown();
            }
        }

        public virtual void HoveringUpdate(NVRHand hand, float forTime)
        {
            if (OnHovering != null)
            {
                OnHovering.Invoke();
            }
        }

        public void ForceDetach(NVRHand hand = null)
        {
            if (hand != null)
            {
                hand.EndInteraction(this);
                this.EndInteraction(hand); //hand should call this in most cases, but rarely hand / item gets disconnected on force detach
            }
            else
            {
                for (int handIndex = 0; handIndex < AttachedHands.Count; handIndex++)
                {
                    AttachedHands[handIndex].EndInteraction(this);
                    this.EndInteraction(AttachedHands[handIndex]);
                }
            }
        }

        public virtual void EndInteraction(NVRHand hand)
        {
            AttachedHands.Remove(hand);
            ClosestHeldPoint = Vector3.zero;

            if (EnableKinematicOnDetach == true)
            {
                rigidbody.isKinematic = true;
            }

            if (EnableGravityOnDetach == true)
            {
                rigidbody.useGravity = true;
            }
        }

        protected virtual void DroppedBecauseOfDistance(NVRHand hand)
        {
            hand.EndInteraction(this);
        }

        public virtual void UseButtonUp()
        {

        }

        public virtual void UseButtonDown()
        {

        }


        public virtual void AddExternalVelocity(Vector3 velocity)
        {
            rigidbody.AddForce(velocity, ForceMode.VelocityChange);
        }

        public virtual void AddExternalAngularVelocity(Vector3 angularVelocity)
        {
            rigidbody.AddTorque(angularVelocity, ForceMode.VelocityChange);
        }

        protected virtual void OnDestroy()
        {
            ForceDetach();
            NVRInteractables.Deregister(this);
        }
    }
}
