using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PostProcesingDrawing : MonoBehaviour {
	private Material material;

    public Color _Color;
    [Range(0f, 0.01f)]
    public float Offset;
    [Range(0f, 1f)]
    public float Silhouette;
    [Range(0f, 0.2f)]
    public float lignSensitivity;

    public Texture2D filter;

    void Awake()
    {
        material = new Material(Shader.Find("Custom/Drawing"));
    }

    void Update()
    {
        _Color = Camera.main.backgroundColor;
        ApplyChange();
    }

    public void ApplyChange()
    {
        material.SetFloat("_Offset", Offset);
        material.SetFloat("_Silhouette", Silhouette);
        material.SetFloat("_LignSensitivity", lignSensitivity);
        material.SetColor("_Color", _Color);
        material.SetTexture("_DrawingTexture", filter);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, material);
    }
}
