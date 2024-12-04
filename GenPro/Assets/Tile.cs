using System;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [Header("Parameters")] 
    [SerializeField] private Material[] materialsPerStairs;
    
    [Header("References")] 
    [SerializeField] private MeshRenderer meshRenderer;

    private void Start()
    {
        int currentIndex = (int)(transform.position.y / 4) % materialsPerStairs.Length;

        meshRenderer.material = materialsPerStairs[currentIndex];
    }
}
