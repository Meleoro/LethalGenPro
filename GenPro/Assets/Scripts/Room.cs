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
    [SerializeField] private GameObject wallGameObject;


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

    public void AddGroundTilesToPathfinding(bool noNeighbors = false)
    {
        List<Transform> transforms = new List<Transform>(groundTilesParent.GetComponentsInChildren<Transform>());
        transforms.Remove(groundTilesParent);
        
        for (int i = 0; i < transforms.Count; i++)
        {
            GenProManager.Instance.pathCalculators[GenProManager.Instance.currentFloorIndex].AddBlockedTile(transforms[i].position);
            
            if (noNeighbors) continue;
            if (VerifyNeighborMustBeBlocked(transforms[i].position)){
                GenProManager.Instance.pathCalculators[GenProManager.Instance.currentFloorIndex].AddBlockedTile(transforms[i].position + Vector3.forward * 2);
                GenProManager.Instance.pathCalculators[GenProManager.Instance.currentFloorIndex].AddBlockedTile(transforms[i].position + Vector3.back * 2);
                GenProManager.Instance.pathCalculators[GenProManager.Instance.currentFloorIndex].AddBlockedTile(transforms[i].position + Vector3.left * 2);
                GenProManager.Instance.pathCalculators[GenProManager.Instance.currentFloorIndex].AddBlockedTile(transforms[i].position + Vector3.right * 2);
            }
        }
    }

    public Transform[] GetGroundTiles()
    {
        List<Transform> transforms = new List<Transform>(groundTilesParent.GetComponentsInChildren<Transform>());
        transforms.Remove(groundTilesParent);
        return transforms.ToArray();
    }

    public void ReplaceCorridorSpot(int index)
    {
        Transform corridorSpotTr = corridorSpots[index].transform;

        Instantiate(wallGameObject, corridorSpotTr.position, corridorSpotTr.rotation * Quaternion.Euler(0, 90, 0));
    }

    private bool VerifyNeighborMustBeBlocked(Vector3 tilePos)
    {
        for (int i = 0; i < corridorSpots.Length; i++)
        {
            if (Vector2.Distance(
                    new Vector2(corridorSpots[i].transform.position.x, corridorSpots[i].transform.position.z),
                    new Vector2(tilePos.x, tilePos.z)) < 2f)
            {
                return false;
            }
        }

        return true;
    }
}

public struct CorridorSpot
{
    public Transform transform;
    public bool hasCorridor;
}
