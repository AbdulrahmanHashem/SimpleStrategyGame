using System;
using UnityEngine;

public class Settings : MonoBehaviour
{
    public static Settings Instance;
    
    public float cameraMovementSpeed;
    public float edgeThreshold;
    public float cameraRotationSpeed;
    
    public float zoomSpeed;
    public float minZoom;
    public float maxZoom;
    
    private void Awake()
    {
        Instance = this;
        
        SetDefaults();
    }

    void SetDefaults()
    {
        cameraMovementSpeed = 15f;
        edgeThreshold = 0.01f;
        cameraRotationSpeed = 5f;
        zoomSpeed = 1f;
        minZoom = 9f;
        maxZoom = 37f;
    }
}
