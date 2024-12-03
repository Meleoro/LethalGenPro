using System.Collections.Generic;
using UnityEngine;

public class CorridorCalculator 
{
    
    public List<Vector3> GenerateCorridorPositions(Room[] generatedRooms)
    {
        List<Vector3> corridorPositions = new List<Vector3>();

        for (int i = 0; i < generatedRooms.Length; i++)
        {
            corridorPositions.AddRange(GenerateRoomCorridors(generatedRooms[i], generatedRooms, corridorPositions));
        }
        
        return corridorPositions;
    }

    
    public void ManageCorridorsNeighbors(List<Corridor> corridors)
    {
        for (int i = 0; i < corridors.Count; i++)
        {
            Corridor currentCorridor = corridors[i];

            for (int j = 0; j < corridors.Count; j++)
            {
                if (currentCorridor == corridors[j]) continue;
                if (Vector2.Distance(
                        new Vector2(currentCorridor.transform.position.x, currentCorridor.transform.position.z),
                        new Vector2(corridors[j].transform.position.x, corridors[j].transform.position.z)) > 2) continue;

                if ((int)currentCorridor.transform.position.x == (int)corridors[j].transform.position.x - 2)
                {
                    currentCorridor.rightWall.SetActive(false);
                } else if ((int)currentCorridor.transform.position.x == (int)corridors[j].transform.position.x + 2)
                {
                    currentCorridor.leftWall.SetActive(false);
                } else if ((int)currentCorridor.transform.position.z == (int)corridors[j].transform.position.z - 2)
                {
                    currentCorridor.upWall.SetActive(false);
                } else if ((int)currentCorridor.transform.position.z == (int)corridors[j].transform.position.z + 2)
                {
                    currentCorridor.downWall.SetActive(false);
                } 
            }
        }
    }
    
    
    private List<Vector3> GenerateRoomCorridors(Room room, Room[] otherRooms, List<Vector3> corridorPositions)
    {
        List<Vector3> tilePositions = new List<Vector3>();
        
        for (int i = 0; i < room.corridorSpots.Length; i++)
        {
            if (room.corridorSpots[i].hasCorridor) continue;
            if (room.corridorSpots[i].transform.position.y > 1 + GenProManager.Instance.currentFloorIndex * 4)
            {
                continue;
            }
            
            Vector3 pos1 = room.corridorSpots[i].transform.forward + room.corridorSpots[i].transform.position;
            Vector3 pos2 = Vector3.zero;
            CorridorSpot wantedSpot;
            
            int counter = 0;
            
            while (true)
            {
                counter++;
                if (counter > 20)
                {
                    pos2 = corridorPositions[Random.Range(0, corridorPositions.Count)];
                    
                    List<Vector3> testPath = GenProManager.Instance.pathCalculators[GenProManager.Instance.currentFloorIndex]
                        .GetPath(pos1, pos2);
                    
                    if(testPath.Count != 0 || counter == 50)
                        break;
                }
                
                Room pickedRoom = otherRooms[Random.Range(0, otherRooms.Length)];

                if (pickedRoom == room) continue;
                bool found = false;
                
                for (int j = 0; j < pickedRoom.corridorSpots.Length; j++)
                {
                    if (pickedRoom.corridorSpots[j].hasCorridor) continue;
                    if (pickedRoom.corridorSpots[j].transform.position.y > 1 + GenProManager.Instance.currentFloorIndex * 4) continue;

                    pos2 = pickedRoom.corridorSpots[j].transform.position + pickedRoom.corridorSpots[j].transform.forward;
                    wantedSpot = pickedRoom.corridorSpots[j];
                    found = true;
                    break;
                }

                if (found)
                {
                    List<Vector3> testPath = GenProManager.Instance.pathCalculators[GenProManager.Instance.currentFloorIndex]
                        .GetPath(pos1, pos2);
                    
                    if(testPath.Count != 0)
                        break;
                }
            }

            room.corridorSpots[i].hasCorridor = true;

            List<Vector3> path = GenProManager.Instance.pathCalculators[GenProManager.Instance.currentFloorIndex].GetPath(pos1, pos2);
            if (path.Count == 0)
            {
                room.ReplaceCorridorSpot(i);
                wantedSpot.hasCorridor = false;
            }
            else
            {
                wantedSpot.hasCorridor = true;
                tilePositions.AddRange(path);
            }
        }
        
        return tilePositions;
    }
}
