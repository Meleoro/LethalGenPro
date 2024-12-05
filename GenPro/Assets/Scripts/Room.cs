using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class Room : MonoBehaviour
{
    [Header("Parameters")] 
    public bool showGizmos;
    
    [Header("Public Infos")] 
    public CorridorSpot[] corridorSpots;
    public bool isConnected;

    [Header("Private Infos")] 
    private GameObject[] corridorBlockWalls;
    
    [Header("References")] 
    [SerializeField] private Transform corridorSpotsParent;
    [SerializeField] private Transform groundTilesParent;
    [SerializeField] private Transform propSpotsParent;
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
        corridorBlockWalls = new GameObject[transforms.Count];
        
        for (int i = 0; i < transforms.Count; i++)
        {
            corridorSpots[i].transform = transforms[i];
            corridorSpots[i].hasCorridor = false;
        }
    }

    public async Task AddGroundTilesToPathfinding(bool noNeighbors = false)
    {
        List<Transform> transforms = new List<Transform>(groundTilesParent.GetComponentsInChildren<Transform>());
        transforms.Remove(groundTilesParent);
        
        for (int i = 0; i < transforms.Count; i++)
        {
            GenProManager.Instance.pathfindingScript.AddBlockedTile(transforms[i].position);
            
            if (noNeighbors) continue;
            if (VerifyNeighborMustBeBlocked(transforms[i].position)){
                GenProManager.Instance.pathfindingScript.AddBlockedTile(transforms[i].position + Vector3.forward * 2);
                GenProManager.Instance.pathfindingScript.AddBlockedTile(transforms[i].position + Vector3.back * 2);
                GenProManager.Instance.pathfindingScript.AddBlockedTile(transforms[i].position + Vector3.left * 2);
                GenProManager.Instance.pathfindingScript.AddBlockedTile(transforms[i].position + Vector3.right * 2);
            }

            if(i % 100 == 0)
                await Task.Yield();
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

        GameObject newWall = Instantiate(wallGameObject, corridorSpotTr.position, corridorSpotTr.rotation * Quaternion.Euler(0, 90, 0), GenProManager.Instance.currentFloorParent);
        corridorBlockWalls[index] = newWall;
    }

    public void ConnectCorridor(int index)
    {
        if (corridorBlockWalls[index] != null)
        {
            Destroy(corridorBlockWalls[index]);
            corridorBlockWalls[index] = null;
        }
    }


    public void GenerateProps()
    {
        if (!propSpotsParent) return;
        
        List<Transform> propsSpots = new List<Transform>(propSpotsParent.GetComponentsInChildren<Transform>());
        propsSpots.Remove(propSpotsParent);

        for (int i = 0; i < propsSpots.Count; i++)
        {
            int pickedProba = Random.Range(0, 100);
            if (pickedProba < GenProManager.Instance.genProData.propSpawnProba)
            {
                Instantiate(
                    GenProManager.Instance.roomsData.possibleProps[Random.Range(0, GenProManager.Instance.roomsData.possibleProps.Length)],
                    propsSpots[i].position, Quaternion.Euler(0, Random.Range(0, 360), 0), GenProManager.Instance.currentFloorParent);
            }
        }
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

    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        if (!showGizmos) return;
        if (!propSpotsParent) return;
        
        List<Transform> propsSpots = new List<Transform>(propSpotsParent.GetComponentsInChildren<Transform>());
        propsSpots.Remove(propSpotsParent);
        
        for (int i = 0; i < propsSpots.Count; i++)
        {
            Gizmos.color = new Color(255, 0, 0, 100);
            Gizmos.DrawSphere(propsSpots[i].position, 0.5f);
        }
    }
}

public struct CorridorSpot
{
    public Transform transform;
    public bool hasCorridor;
}
