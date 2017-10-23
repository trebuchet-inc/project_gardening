using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PostProcesingTAM : MonoBehaviour 
{
	private Material material;

    public Color color;
	[Space]
	public Texture2D layer1;
	public Texture2D layer2;
	public Texture2D layer3;
	public Texture2D layer4;
	[Space]
    public float colorWeight;
    public float tiling;
    public float oscilation;

    void Awake()
    {
        material = new Material(Shader.Find("Custom/SurfaceTAM"));
    }

    void Update()
    {
        ApplyChange();
    }

    public void ApplyChange()
    {
		material.SetColor("_Color", color);
        material.SetTexture("_Layer1", layer1);
		material.SetTexture("_Layer2", layer2);
		material.SetTexture("_Layer3", layer3);
		material.SetTexture("_Layer4", layer4);
        material.SetFloat("_ColorWeight", colorWeight);
        material.SetFloat("_Tiling", tiling);
        material.SetFloat("_Oscilation", oscilation);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, material);
    }
}
