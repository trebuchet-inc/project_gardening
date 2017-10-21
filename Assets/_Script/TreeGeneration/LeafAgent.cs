using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafAgent : SproutableAgent 
{
	[Space]
	public Color[] palette;

	public  override void Start () 
	{
		base.Start();
		GetComponent<SpriteRenderer>().material.color = palette[Random.Range(0,palette.Length)];
	}
}
