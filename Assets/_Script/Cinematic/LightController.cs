﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour {
	public float speed;
	void Update () {
		if(Input.GetKey(KeyCode.Space)) transform.Rotate(speed * Time.deltaTime, 0, 0);
	}
}
