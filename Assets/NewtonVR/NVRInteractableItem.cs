using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;

namespace NewtonVR
{
    public class NVRInteractableItem : NVRInteractable
    {
        private const float MaxVelocityChange = 10f;
        private const float MaxAngularVelocityChange = 20f;
        private const float VelocityMagic = 6000f;
        private const float AngularVelocityMagic = 50f;

        [Tooltip("If you have a specific point you'd like the object held at, create a transform there and set it to this variable")]
        public Transform InteractionPoint;

        public UnityEvent OnUseButtonDown;
        public UnityEvent OnUseButtonUp;

        public UnityEvent OnBeginInteraction;
        public UnityEvent OnEndInteraction;

        protected Dictionary<NVRHand, Transform> PickupTransforms = new Dictionary<NVRHand, Transform>();

        protected Vector3 ExternalVelocity;
        protected Vector3 ExternalAngularVelocity;

        protected Vector3?[] VelocityHistory;
        protected Vector3?[] AngularVelocityHistory;
        protected int CurrentVelocityHistoryStep = 0;

        protected float StartingDrag = -1;
        protected float StartingAngularDrag = -1;

        protected Dictionary<Collider, PhysicMaterial> MaterialCache = new Dictionary<Collider, PhysicMaterial>();

        protected Transform TwoHandedInteractionPoint;

        protected override void Awake()
        {
            base.Awake();

            this.rigidbody.maxAngularVelocity = 100f;
        }

        protected override void Start()
        {
            base.Start();     

            if (NVRPlayer.Instance.VelocityHistorySteps > 0)
            {
                VelocityHistory = new Vector3?[NVRPlayer.Instance.VelocityHistorySteps];
                AngularVelocityHistory = new Vector3?[NVRPlayer.Instance.VelocityHistorySteps];
            }
        }

        protected virtual void FixedUpdate()
        {
            if (IsAttached == true)
            {
                bool dropped = CheckForDrop();

                if (dropped == false)
                {
                    UpdateVelocities();
                }
            }

            AddExternalVelocities();
        }

        protected virtual void GetTargetValues(out Vector3 targetHandPosition, out Quaternion targetHandRotation, out Vector3 targetItemPosition, out Quaternion targetItemRotation)
        {
            if (AttachedHands.Count == 1) //faster path if only one hand, which is the standard scenario
            {
                NVRHand hand = AttachedHands[0];

                if (InteractionPoint != null)
                {
                    targetItemPosition = InteractionPoint.position;
                    targetItemRotation = InteractionPoint.rotation;

                    targetHandPosition = hand.transform.position;
                    targetHandRotation = hand.transform.rotation;
                }
                else
                {
                    targetItemPosition = PickupTransforms[hand].position;;
                    targetItemRotation = PickupTransforms[hand].rotation;

                    targetHandPosition = hand.transform.position;
                    targetHandRotation = hand.transform.rotation;
                }
            }
            else if (AttachedHands.Count == 2)
            {
                if (InteractionPoint != null)
                {
                    targetItemPosition = InteractionPoint.position;
                    targetItemRotation = InteractionPoint.rotation;

                    targetHandPosition = AttachedHands[0].transform.position;
                    targetHandRotation = Quaternion.LookRotation(AttachedHands[1].transform.position - AttachedHands[0].transform.position, Vector3.up);
                }
                else
                {
                    NVRHand mainHand;
                    NVRHand secondHand;

                    if(Vector3.Distance(AttachedHands[0].transform.position, transform.position) >= Vector3.Distance(AttachedHands[1].transform.position, transform.position))
                    {
                        mainHand = AttachedHands[0];
                        secondHand = AttachedHands[1];
                    }
                    else 
                    {
                        mainHand = AttachedHands[1];
                        secondHand = AttachedHands[0];
                    }

                    targetItemPosition = PickupTransforms[mainHand].position;
                    targetItemRotation = this.transform.rotation;

                    targetHandPosition = mainHand.transform.position; 
                    if(PickupTransforms[mainHand].localPosition.z <= 0) targetHandRotation = Quaternion.LookRotation(secondHand.transform.position - mainHand.transform.position, Vector3.up);
                    else targetHandRotation = Quaternion.LookRotation(mainHand.transform.position - secondHand.transform.position, Vector3.up);
                }
            }
            else
            {
                Vector3 cumulativeItemVector = Vector3.zero;
                Vector4 cumulativeItemRotation = Vector4.zero;
                Quaternion? firstItemRotation = null;
                targetItemRotation = Quaternion.identity;

                Vector3 cumulativeHandVector = Vector3.zero;
                Vector4 cumulativeHandRotation = Vector4.zero;
                Quaternion? firstHandRotation = null;
                targetHandRotation = Quaternion.identity;

                for (int handIndex = 0; handIndex < AttachedHands.Count; handIndex++)
                {
                    NVRHand hand = AttachedHands[handIndex];

                    if (InteractionPoint != null && handIndex == 0)
                    {
                        targetItemRotation = InteractionPoint.rotation;
                        cumulativeItemVector += InteractionPoint.position;

                        targetHandRotation = hand.transform.rotation;
                        cumulativeHandVector += hand.transform.position;
                    }
                    else
                    {
                        targetItemRotation = this.transform.rotation;
                        cumulativeItemVector += this.transform.position;

                        targetHandRotation = PickupTransforms[hand].rotation;
                        cumulativeHandVector += PickupTransforms[hand].position;
                    }

                    if (firstItemRotation == null)
                    {
                        firstItemRotation = targetItemRotation;
                    }
                    if (firstHandRotation == null)
                    {
                        firstHandRotation = targetHandRotation;
                    }
                }
                targetItemPosition = cumulativeItemVector / AttachedHands.Count;
                targetHandPosition = cumulativeHandVector / AttachedHands.Count;
                targetItemRotation = NVRHelpers.AverageQuaternion(ref cumulativeItemRotation, targetItemRotation, firstItemRotation.Value, AttachedHands.Count);
                targetHandRotation = NVRHelpers.AverageQuaternion(ref cumulativeHandRotation, targetHandRotation, firstHandRotation.Value, AttachedHands.Count);
            }
        }

        protected virtual void UpdateVelocities()
        {
            Vector3 targetItemPosition;
            Quaternion targetItemRotation;

            Vector3 targetHandPosition;
            Quaternion targetHandRotation;

            GetTargetValues(out targetHandPosition, out targetHandRotation, out targetItemPosition, out targetItemRotation);


            float velocityMagic = VelocityMagic / (Time.deltaTime / NVRPlayer.NewtonVRExpectedDeltaTime);
            float angularVelocityMagic = AngularVelocityMagic / (Time.deltaTime / NVRPlayer.NewtonVRExpectedDeltaTime);

            Vector3 positionDelta;
            Quaternion rotationDelta;

            float angle;
            Vector3 axis;

            positionDelta = (targetHandPosition - targetItemPosition);

            rotationDelta = targetHandRotation * Quaternion.Inverse(targetItemRotation);

            rotationDelta.ToAngleAxis(out angle, out axis);

            if (angle > 180)
            {
                angle -= 360;
            }
                
            if (angle != 0)
            {
                Vector3 angularTarget = angle * axis;
                if (float.IsNaN(angularTarget.x) == false)
                {
                    angularTarget = (angularTarget * angularVelocityMagic) * Time.deltaTime;
                    this.rigidbody.angularVelocity = Vector3.MoveTowards(this.rigidbody.angularVelocity, angularTarget, MaxAngularVelocityChange);
                }
            }

            Vector3 velocityTarget = (positionDelta * velocityMagic) * Time.deltaTime;
            if (float.IsNaN(velocityTarget.x) == false)
            {
                this.rigidbody.velocity = Vector3.MoveTowards(this.rigidbody.velocity, velocityTarget, MaxVelocityChange);
            }
            
            if (VelocityHistory != null)
            {
                CurrentVelocityHistoryStep++;
                if (CurrentVelocityHistoryStep >= VelocityHistory.Length)
                {
                    CurrentVelocityHistoryStep = 0;
                }

                VelocityHistory[CurrentVelocityHistoryStep] = this.rigidbody.velocity;
                AngularVelocityHistory[CurrentVelocityHistoryStep] = this.rigidbody.angularVelocity;
            }
        }

        protected virtual void AddExternalVelocities()
        {
            if (ExternalVelocity != Vector3.zero)
            {
                this.rigidbody.velocity = Vector3.Lerp(this.rigidbody.velocity, ExternalVelocity, 0.5f);
                ExternalVelocity = Vector3.zero;
            }

            if (ExternalAngularVelocity != Vector3.zero)
            {
                this.rigidbody.angularVelocity = Vector3.Lerp(this.rigidbody.angularVelocity, ExternalAngularVelocity, 0.5f);
                ExternalAngularVelocity = Vector3.zero;
            }
        }

        public override void AddExternalVelocity(Vector3 velocity)
        {
            if (ExternalVelocity == Vector3.zero)
            {
                ExternalVelocity = velocity;
            }
            else
            {
                ExternalVelocity = Vector3.Lerp(ExternalVelocity, velocity, 0.5f);
            }
        }

        public override void AddExternalAngularVelocity(Vector3 angularVelocity)
        {
            if (ExternalAngularVelocity == Vector3.zero)
            {
                ExternalAngularVelocity = angularVelocity;
            }
            else
            {
                ExternalAngularVelocity = Vector3.Lerp(ExternalAngularVelocity, angularVelocity, 0.5f);
            }
        }

        public override void BeginInteraction(NVRHand hand)
        {
            base.BeginInteraction(hand);

            StartingDrag = rigidbody.drag;
            StartingAngularDrag = rigidbody.angularDrag;
            rigidbody.drag = 0;
            rigidbody.angularDrag = 0.05f;

            DisablePhysicalMaterials();

            Transform pickupTransform = new GameObject(string.Format("[{0}] NVRPickupTransform", this.gameObject.name)).transform;
            pickupTransform.parent = this.transform;
            pickupTransform.position = hand.transform.position;
            pickupTransform.rotation = hand.transform.rotation;
            PickupTransforms.Add(hand, pickupTransform);

            ResetVelocityHistory();

            if (OnBeginInteraction != null)
            {
                OnBeginInteraction.Invoke();
            }
        }

        public override void EndInteraction(NVRHand hand)
        {
            base.EndInteraction(hand);

            if (hand == null)
            {
                var pickupTransformsEnumerator = PickupTransforms.GetEnumerator();
                while (pickupTransformsEnumerator.MoveNext())
                {
                    var pickupTransform = pickupTransformsEnumerator.Current;
                    if (pickupTransform.Value != null)
                    {
                        Destroy(pickupTransform.Value.gameObject);
                    }
                }

                PickupTransforms.Clear();
            }
            else if (PickupTransforms.ContainsKey(hand))
            {
                Destroy(PickupTransforms[hand].gameObject);
                PickupTransforms.Remove(hand);
            }

            if (PickupTransforms.Count == 0)
            {
                rigidbody.drag = StartingDrag;
                rigidbody.angularDrag = StartingAngularDrag;

                EnablePhysicalMaterials();

                ApplyVelocityHistory();
                ResetVelocityHistory();

                if (OnEndInteraction != null)
                {
                    OnEndInteraction.Invoke();
                }
            }
        }

        public override void HoveringUpdate(NVRHand hand, float forTime)
        {
            base.HoveringUpdate(hand, forTime);
        }

        public override void ResetInteractable()
        {
            EndInteraction(null);
            base.ResetInteractable();
        }

        public override void UseButtonDown()
        {
            base.UseButtonDown();

            if (OnUseButtonDown != null)
            {
                OnUseButtonDown.Invoke();
            }
        }

        public override void UseButtonUp()
        {
            base.UseButtonUp();

            if (OnUseButtonUp != null)
            {
                OnUseButtonUp.Invoke();
            }
        }

        protected virtual void ApplyVelocityHistory()
        {
            if (VelocityHistory != null)
            {
                Vector3? meanVelocity = GetMeanVector(VelocityHistory);
                if (meanVelocity != null)
                {
                    this.rigidbody.velocity = meanVelocity.Value;
                }

                Vector3? meanAngularVelocity = GetMeanVector(AngularVelocityHistory);
                if (meanAngularVelocity != null)
                {
                    this.rigidbody.angularVelocity = meanAngularVelocity.Value;
                }
            }
        }

        protected virtual void ResetVelocityHistory()
        {
            CurrentVelocityHistoryStep = 0;

            if (VelocityHistory != null && VelocityHistory.Length > 0)
            {
                VelocityHistory = new Vector3?[VelocityHistory.Length];
                AngularVelocityHistory = new Vector3?[VelocityHistory.Length];
            }
        }

        protected Vector3? GetMeanVector(Vector3?[] positions)
        {
            float x = 0f;
            float y = 0f;
            float z = 0f;

            int count = 0;
            for (int index = 0; index < positions.Length; index++)
            {
                if (positions[index] != null)
                {
                    x += positions[index].Value.x;
                    y += positions[index].Value.y;
                    z += positions[index].Value.z;

                    count++;
                }
            }

            if (count > 0)
            {
                return new Vector3(x / count, y / count, z / count);
            }

            return null;
        }

        protected void DisablePhysicalMaterials()
        {
            for (int colliderIndex = 0; colliderIndex < Colliders.Length; colliderIndex++)
            {
                if (Colliders[colliderIndex] == null)
                {
                    continue;
                }

                MaterialCache[Colliders[colliderIndex]] = Colliders[colliderIndex].sharedMaterial;
                Colliders[colliderIndex].sharedMaterial = null;
            }
        }

        protected void EnablePhysicalMaterials()
        {
            for (int colliderIndex = 0; colliderIndex < Colliders.Length; colliderIndex++)
            {
                if (Colliders[colliderIndex] == null)
                {
                    continue;
                }

                if (MaterialCache.ContainsKey(Colliders[colliderIndex]) == true)
                {
                    Colliders[colliderIndex].sharedMaterial = MaterialCache[Colliders[colliderIndex]];
                }
            }
        }

        public override void UpdateColliders()
        {
            base.UpdateColliders();

            for (int colliderIndex = 0; colliderIndex < Colliders.Length; colliderIndex++)
            {
                if (MaterialCache.ContainsKey(Colliders[colliderIndex]) == false)
                {
                    MaterialCache.Add(Colliders[colliderIndex], Colliders[colliderIndex].sharedMaterial);

                    if (IsAttached == true)
                    {
                        Colliders[colliderIndex].sharedMaterial = null;
                    }
                }
            }
        }
    }
}