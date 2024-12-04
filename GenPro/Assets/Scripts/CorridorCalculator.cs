using System.Collections.Generic;
using UnityEngine;

public class CorridorCalculator
{
    private Corridor currentCorridor;
    public void ManageCorridorsNeighbors(List<Corridor> corridors)
    {
        for (int i = 0; i < corridors.Count; i++)
        {
            currentCorridor = corridors[i];

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
    
    public List<Vector3> GenerateRoomCorridors(Room room, Room[] otherRooms, int maxCorridorLength)
    {
        List<Vector3> tilePositions = new List<Vector3>();
        Room[] validRooms = GetValidRooms(room, otherRooms, (int)(maxCorridorLength * 0.5f));

        if (validRooms.Length == 0)
        {
            if (!room.isConnected)
                return GenerateRoomCorridors(room, otherRooms, maxCorridorLength + 15);
            
            for (int i = 0; i < room.corridorSpots.Length; i++)
            {
                if(!room.corridorSpots[i].hasCorridor)
                    room.ReplaceCorridorSpot(i);
                room.corridorSpots[i].hasCorridor = true;
            }
            
            return new List<Vector3>();
        }
        
        for (int i = 0; i < room.corridorSpots.Length; i++)
        {
            if (room.corridorSpots[i].hasCorridor) continue;
            if (room.corridorSpots[i].transform.position.y > 1 + GenProManager.Instance.currentFloorIndex * 4) continue;
            
            Vector3 pos1 = room.corridorSpots[i].transform.forward + room.corridorSpots[i].transform.position;
            Vector3 pos2 = Vector3.zero;
            CorridorSpot wantedSpot;
            
            int counter = 0;
            
            bool found = false;
            Room pickedRoom = null;
            
            while (true)
            {
                counter++;
                if (counter > 20) break;
                
                pickedRoom = validRooms[Random.Range(0, validRooms.Length)];
                
                for (int j = 0; j < pickedRoom.corridorSpots.Length; j++)
                {
                    //if (pickedRoom.corridorSpots[j].hasCorridor) continue;
                    if (pickedRoom.corridorSpots[j].transform.position.y > 1 + GenProManager.Instance.currentFloorIndex * 4) continue;
                    if (Mathf.Abs(pickedRoom.corridorSpots[j].transform.position.y - room.corridorSpots[i].transform.position.y) > 1) continue;

                    pos2 = pickedRoom.corridorSpots[j].transform.position + pickedRoom.corridorSpots[j].transform.forward;
                    wantedSpot = pickedRoom.corridorSpots[j];
                    found = true;
                    
                    List<Vector3> testPath = GenProManager.Instance.pathCalculators[GenProManager.Instance.currentFloorIndex]
                        .GetPath(pos1, pos2);
                    if (testPath.Count < maxCorridorLength)
                        continue;
                    
                    break;
                }

                if (found) break;
            }

            List<Vector3> path = GenProManager.Instance.pathCalculators[GenProManager.Instance.currentFloorIndex].GetPath(pos1, pos2);
            if (path.Count == 0 && room.isConnected)
            {
                room.ReplaceCorridorSpot(i);
                wantedSpot.hasCorridor = false;
                room.corridorSpots[i].hasCorridor = true;
            }
            else if (path.Count != 0)
            {
                room.isConnected = true;
                pickedRoom.isConnected = true;
                wantedSpot.hasCorridor = true;
                room.corridorSpots[i].hasCorridor = true;
                tilePositions.AddRange(path);
            }
        }

        if (!room.isConnected)
        {
            return GenerateRoomCorridors(room, otherRooms, maxCorridorLength + 15);
        }
        
        return tilePositions;
    }


    private Room[] GetValidRooms(Room room, Room[] otherRooms, int maxDist)
    {
        List<Room> returnedList = new List<Room>();
        
        for (int i = 0; i < otherRooms.Length; i++)
        {
            if (room == otherRooms[i]) continue;
            if (VerifyRoom(room, otherRooms[i], maxDist))
            {
                returnedList.Add(otherRooms[i]);
            }
        }

        return returnedList.ToArray();
    }

    private bool VerifyRoom(Room room1, Room room2, int maxDist)
    {
        foreach (var corridorSpot1 in room1.corridorSpots)
        {
            if (corridorSpot1.hasCorridor) continue;
            
            foreach (var corridorSpot2 in room2.corridorSpots)
            {
                if (Vector2.Distance(
                        new Vector2(corridorSpot1.transform.position.x, corridorSpot1.transform.position.z),
                        new Vector2(corridorSpot2.transform.position.x, corridorSpot2.transform.position.z)) < maxDist)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
}
