using System.Collections.Generic;
using UnityEngine;

public class Corridor : MonoBehaviour
{
    [Header("Public Infos")] 
    public List<Corridor> neighbors;

    [Header("References")] 
    public GameObject upWall;
    public GameObject downWall;
    public GameObject leftWall;
    public GameObject rightWall;

    // Removes walls in contact with a room's corridor spot
    public void ActualiseWalls()
    {
        List<Transform> corridorSpots = GenProManager.Instance.corridorSpots;
        for (int i = 0; i < corridorSpots.Count; i++)
        {
            Vector2Int pos1 = new Vector2Int((int)corridorSpots[i].transform.position.x, (int)corridorSpots[i].transform.position.z);
            Vector2Int pos2 = new Vector2Int((int)upWall.transform.position.x, (int)upWall.transform.position.z);
            if (pos1 == pos2)
            {
                upWall.SetActive(false);
            } 
            pos2 = new Vector2Int((int)downWall.transform.position.x, (int)downWall.transform.position.z);
            if (pos1 == pos2)
            {
                downWall.SetActive(false);
            } 
            pos2 = new Vector2Int((int)leftWall.transform.position.x, (int)leftWall.transform.position.z);
            if (pos1 == pos2)
            {
                leftWall.SetActive(false);
            } 
            pos2 = new Vector2Int((int)rightWall.transform.position.x, (int)rightWall.transform.position.z);
            if (pos1 == pos2)
            {
                rightWall.SetActive(false);
            }
        }
    }
}
