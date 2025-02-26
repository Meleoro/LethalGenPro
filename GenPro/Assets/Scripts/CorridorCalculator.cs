using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CorridorCalculator
{
    private Corridor currentCorridor;
    
    // Deactivates walls according to the corridors neighbors
    public async Task ManageCorridorsNeighbors(List<Corridor> corridors)
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

            if (i % 40 == 0) await Task.Yield();
        }
    }
    
    public List<Vector3> GenerateRoomCorridors(Room room, Room[] otherRooms, int maxCorridorLength)
    {
        List<Vector3> tilePositions = new List<Vector3>();
        Room[] validRooms = GetValidRooms(room, otherRooms, (int)(maxCorridorLength * 0.5f));

        if (validRooms.Length == 0)
        {
            return GenerateRoomCorridors(room, otherRooms, maxCorridorLength + 15);
        }
        
        // For every possible corridor spot of this room
        for (int i = 0; i < room.corridorSpots.Length; i++)
        {
            if (room.corridorSpots[i].hasCorridor) continue;
            if (room.corridorSpots[i].transform.position.y > 1 + GenProManager.Instance.currentFloorIndex * 4) continue;
            
            Vector3 pos1 = room.corridorSpots[i].transform.forward + room.corridorSpots[i].transform.position;
            Vector3 pos2 = Vector3.zero;
            CorridorSpot wantedSpot = new CorridorSpot();
            int counter = 0;
            bool found = false;
            Room pickedRoom = null;
            int pickedIndex = 0;
            
            while (true)
            {
                counter++;
                if (counter > 20) break;
                
                pickedRoom = validRooms[Random.Range(0, validRooms.Length)];
                
                for (int j = 0; j < pickedRoom.corridorSpots.Length; j++)
                {
                    if (pickedRoom.corridorSpots[j].transform.position.y > 1 + GenProManager.Instance.currentFloorIndex * 4) continue;
                    if (Mathf.Abs(pickedRoom.corridorSpots[j].transform.position.y - room.corridorSpots[i].transform.position.y) > 1) continue;
                    if (!pickedRoom.isConnected) continue;

                    pos2 = pickedRoom.corridorSpots[j].transform.position + pickedRoom.corridorSpots[j].transform.forward;
                    wantedSpot = pickedRoom.corridorSpots[j];
                    found = true;
                    pickedIndex = j;
                    
                    List<Vector3> testPath = GenProManager.Instance.pathfindingScript.GetPath(pos1, pos2);     // Use the pathfinding algo to find the path between the two corridors spots
                    if (testPath.Count < maxCorridorLength || testPath.Count == 0)
                        continue;
                    
                    break;
                }

                if (found) break;
            }

            List<Vector3> path = GenProManager.Instance.pathfindingScript.GetPath(pos1, pos2);     // Use the pathfinding algo to find the path between the two corridors spots
            // If the path isn't valid but this room has already connections, we close this corridor spot
            if (path.Count == 0 && room.isConnected)
            {
                room.ReplaceCorridorSpot(i);
                room.corridorSpots[i].hasCorridor = true;
            }
            else if (path.Count != 0)     // If the path is valid
            {
                room.isConnected = true;
                wantedSpot.hasCorridor = true;
                room.corridorSpots[i].hasCorridor = true;
                tilePositions.AddRange(path);

                pickedRoom.ConnectCorridor(pickedIndex);
            }
        }

        if (!room.isConnected)
        {
            return GenerateRoomCorridors(room, otherRooms, maxCorridorLength + 15);
        }
        
        return tilePositions;
    }


    // Gets rooms in a given range 
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
