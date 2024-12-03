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

    public void ActualiseWalls()
    {
        List<Transform> corridorSpots = GenProManager.Instance.corridorSpots;
        for (int i = 0; i < corridorSpots.Count; i++)
        {
            if (corridorSpots[i].position == upWall.transform.position)
            {
                upWall.SetActive(false);
            } else if (corridorSpots[i].position == downWall.transform.position)
            {
                downWall.SetActive(false);
            } else if (corridorSpots[i].position == leftWall.transform.position)
            {
                leftWall.SetActive(false);
            } else if (corridorSpots[i].position == rightWall.transform.position)
            {
                rightWall.SetActive(false);
            }
        }
    }
}
