using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeWindController : MonoBehaviour {
	ParticleSystem particle;
	
	void Start () {
		particle = GetComponent<ParticleSystem>();
	}
	
	void Update () {
		if(Input.GetKeyDown(KeyCode.O)) particle.Play();
		if(Input.GetKeyUp(KeyCode.O)) particle.Stop();
	}
}
