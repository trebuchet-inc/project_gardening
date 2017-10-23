using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	public float speed;
	public float rotationSpeed;
	public AnimationCurve acceleration;
	
	Vector3 _origPos;
	Quaternion _origRot;
	float _accelerationTime;
	float _sign;
	
	void Start () 
	{
		_origPos = transform.position;
		_origRot = transform.rotation;
	}
	
	void Update () 
	{
		float verticalAxis = Input.GetAxis("Vertical");
		float horizontalAxis = Input.GetAxis("Horizontal");
		float thirdAxis =  Input.GetAxis("ThirdAxis");

		if(verticalAxis != 0 )
		{
			if(_accelerationTime < 1) _accelerationTime += Time.deltaTime;
		}
		else
		{
			if(_accelerationTime > 0) _accelerationTime -= Time.deltaTime;
		}

		_accelerationTime = Mathf.Clamp(_accelerationTime, 0f, 0.99f);

		transform.position += 	transform.forward 
								* verticalAxis
								* acceleration.Evaluate(_accelerationTime)
								* speed
								* Time.deltaTime;

		transform.Rotate(rotationSpeed * thirdAxis * Time.deltaTime, rotationSpeed * horizontalAxis * Time.deltaTime, 0);

		if(Input.GetKeyDown(KeyCode.Alpha1)) speed *= 0.8f;
		if(Input.GetKeyDown(KeyCode.Alpha2)) speed *= 1.2f;
		if(Input.GetKeyDown(KeyCode.Alpha3)) rotationSpeed *= 0.8f;
		if(Input.GetKeyDown(KeyCode.Alpha4)) rotationSpeed *= 1.2f;
		if(Input.GetKeyDown(KeyCode.R))
		{
			transform.position = _origPos;
			transform.rotation = _origRot;
		}
		
	}
}
