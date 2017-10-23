using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BranchAgent : SproutableAgent 
{
	[Space]
	public float branchFrequency;
	[Range(0f, 1f)] public float frequencyVariation;
	public float minHeight;
	public float maxHeight;
	public GameObject[] alphabet;
	
	Mesh _mesh;
	Material _material;
	int _branchCount = 0;
	float _nextBranchSpawn;
	
	public override void Start () 
	{
		base.Start();
		_nextBranchSpawn = (branchFrequency * Random.Range(1.0f - frequencyVariation, 1.0f + frequencyVariation)) * _growRatio;
		_mesh = GetComponentInChildren<MeshFilter>().mesh;
		_material = GetComponentInChildren<MeshRenderer>().material;
		transform.rotation = Quaternion.Euler(0,Random.Range(0,360),0);
	}
	
	public override void Update () 
	{
		if(_growRate <= maxScale)
		{
			_growRate += Time.deltaTime * _growRatio;
			if(_growRate >= _nextBranchSpawn)
			{
				addBranch();
			}
		}
	}

	void addBranch()
	{
		if(alphabet.Length <= 0) return;

		int i = Random.Range(0, _mesh.vertexCount);
		
		Vector3 pos = getSpawningPoint(Random.Range(minHeight, maxHeight));
		GameObject o = Instantiate(alphabet[_branchCount % alphabet.Length], this.transform);
		
		o.transform.localPosition = pos;
		_nextBranchSpawn += (branchFrequency * Random.Range(1.0f - frequencyVariation, 1.0f + frequencyVariation)) * _growRatio;
		_branchCount++;
	}
	
	Vector3 getSpawningPoint(float Height)
	{
		Vector3 pos = Vector3.zero;
		int count = 0;

		foreach(Vector3 v in _mesh.vertices)
		{
			if(Mathf.Abs(v.y - Height) < 0.05f)
			{
				pos += v;
				count++;
			}
		}
		if(count > 0) pos /= count;

		return pos;
	}

	public override void LateUpdate()
	{
		base.LateUpdate();
		_material.SetFloat("_Tiling", transform.lossyScale.x);
	}
}
