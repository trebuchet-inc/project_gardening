using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SproutableAgent : MonoBehaviour 
{
	public float growTime;
	public float maxScale;
	[Range(0f, 1f)] public float scaleVariation;
	
	protected float _growRatio;
	protected float _growRate;
	
	public virtual void Start() 
	{
		_growRatio = maxScale/growTime;
		maxScale *= Random.Range(1.0f - scaleVariation, 1.0f + scaleVariation);
	}
	
	public virtual void Update() 
	{
		if(_growRate <= maxScale)
		{
			_growRate += Time.deltaTime * _growRatio;
		}
	}

	public virtual void LateUpdate()
	{
		this.transform.localScale = Vector3.one * _growRate;
	}
}
