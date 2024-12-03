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

    public void AddNeighbor(Corridor corridor)
    {
        neighbors.Add(corridor);
    }

    private void ActualiseWalls()
    {
        
    }
}
