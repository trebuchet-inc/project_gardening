using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NewtonVR;
using UnityEngine.Events;

public class NVRConstrainedItem : NVRInteractable {
	public bool rotationLock;
	public bool movementLock;
	public bool blockMirorMovement = false;
	public Vector3 motor;

	Transform _origin;
	Vector3 _originPosition;
	Quaternion _originRotation;
	Vector3 _originLocalPosition;
	Vector3 _attachementPoint;
	Joint _joint;

	protected override void Start () 
	{
		base.Start();
		_origin = transform.Find("Origin");
		if(_origin == null)
		{
			_origin = new GameObject(string.Format("Origin", this.gameObject.name)).transform; 
			_origin.parent = this.transform;
		} 
		_joint = GetComponent<Joint>();
	}

	protected override void Update()
	{
		base.Update();

		if(!rotationLock && !IsAttached && motor.magnitude != 0)
		{
			rigidbody.AddRelativeTorque(motor * Time.deltaTime, ForceMode.VelocityChange);
		}
	}

	public void setJoint(Vector3 pos, Quaternion rot, Vector3 localPos)
	{
		_originPosition = pos;
		_originRotation = rot;
		_originLocalPosition = localPos;

		if(_joint == null) _joint = GetComponent<Joint>();
		_joint.connectedAnchor = pos;

		rigidbody.centerOfMass = _originLocalPosition;
	}
	
	public override void InteractingUpdate(NVRHand hand)
	{
		base.InteractingUpdate(hand);
		SetVelocity(hand);
	}

	public override void BeginInteraction(NVRHand hand)
	{
		base.BeginInteraction(hand);
		if(!movementLock)
		{
			_origin.position = transform.position;
			_origin.rotation = _originRotation;
			_attachementPoint = _origin.InverseTransformPoint(hand.transform.position);
		}
	}

	public override void EndInteraction(NVRHand hand)
	{
		base.EndInteraction(hand);
	}

	public override void HoveringUpdate(NVRHand hand, float forTime)
	{ 
		base.HoveringUpdate(hand, forTime);
		OnHovering.Invoke();
	}

	void SetVelocity(NVRHand hand)
	{
		_origin.position = _originPosition;
		_origin.rotation = _originRotation;
		
		if(!movementLock)
		{
			Vector3 velocity = _origin.InverseTransformPoint(hand.transform.position) - _attachementPoint;
			velocity = (velocity - transform.localPosition) * 10;
			rigidbody.velocity = transform.TransformVector(velocity);
		} 

		if(!rotationLock)
		{
			Vector3 forward = hand.transform.position - transform.position;
			Vector3 localHand = _origin.InverseTransformPoint(hand.transform.position);
			Vector3 torque = Quaternion.LookRotation(forward, transform.up).eulerAngles - transform.eulerAngles;
			if(blockMirorMovement && localHand.z < 0)
			{
				torque.x = torque.x >= 0 ? -torque.x : torque.x; 
			} 

			torque.x = Math.Abs(torque.x) > 180 ? torque.x - 360 * Math.Sign(torque.x) : torque.x;
			torque.y = Math.Abs(torque.y) > 180 ? torque.y - 360 * Math.Sign(torque.y) : torque.y;
			torque.z = Math.Abs(torque.z) > 180 ? torque.z - 360 * Math.Sign(torque.z) : torque.z;
			torque = new Vector3(_joint.axis.x != 0 ? torque.x : 0,
			 					_joint.axis.y != 0 ? torque.y : 0,
			 					_joint.axis.z != 0 ? torque.z : 0);
			torque *= Mathf.Deg2Rad * 100;

			rigidbody.angularVelocity = transform.TransformVector(torque);
		}
	}
}