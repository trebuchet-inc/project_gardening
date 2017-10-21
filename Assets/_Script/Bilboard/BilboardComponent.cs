using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BilboardComponent : MonoBehaviour {
	Camera _targetCamera;

	void Start () {
		_targetCamera = Camera.main;
	}
		
	void Update () {
		transform.LookAt(_targetCamera.transform);
	}
}
