using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Parameters")] 
    public bool isStairRoom;
    
    [Header("Public Infos")] 
    public CorridorSpot[] corridorSpots;
    
    [Header("References")] 
    [SerializeField] private Transform corridorSpotsParent;
    [SerializeField] private Transform groundTilesParent;


    public List<Transform> GetCorridorsSpots()
    {
        List<Transform> transforms = new List<Transform>(corridorSpotsParent.GetComponentsInChildren<Transform>());
        transforms.Remove(corridorSpotsParent);
        return transforms;
    }

    public void GenerateCorridorSpots()
    {
        List<Transform> transforms = new List<Transform>(corridorSpotsParent.GetComponentsInChildren<Transform>());
        transforms.Remove(corridorSpotsParent);

        corridorSpots = new CorridorSpot[transforms.Count];
        
        for (int i = 0; i < transforms.Count; i++)
        {
            corridorSpots[i].transform = transforms[i];
            corridorSpots[i].hasCorridor = false;
        }
    }

    public void AddGroundTilesToPathfinding()
    {
        List<Transform> transforms = new List<Transform>(groundTilesParent.GetComponentsInChildren<Transform>());
        transforms.Remove(groundTilesParent);
        
        for (int i = 0; i < transforms.Count; i++)
        {
            GenProManager.Instance.pathCalculators[GenProManager.Instance.currentFloorIndex].AddBlockedTile(transforms[i].position);
        }
    }
}

public struct CorridorSpot
{
    public Transform transform;
    public bool hasCorridor;
}
