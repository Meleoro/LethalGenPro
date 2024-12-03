using System.Collections.Generic;
using UnityEngine;

public class RoomCalculator
{
    [Header("References / Parameters")]
    private GlobalGenProData data;
    private Room[] possibleRooms;
    private Room[] possibleStairsRooms;
    private Room[] previousFloorStairRooms;
    private Room startRoom;
    private bool mustGenerateStairs;

    [Header("Private Infos")] 
    private List<Vector3> takenPositions = new List<Vector3>();
    private List<int> stairRoomIndexes = new List<int>();

    
    public RoomCalculator(GlobalGenProData data, List<Room> previousFloorStairRooms, bool haveFloorUpside = false)
    {
        possibleRooms = GenProManager.Instance.possibleRooms;
        possibleStairsRooms = GenProManager.Instance.possibleStairsRooms;
        startRoom = GenProManager.Instance.startRoom;
        this.data = data;

        this.previousFloorStairRooms = previousFloorStairRooms.ToArray();
        mustGenerateStairs = haveFloorUpside;
    }
    
    
    // Returns the positions and prefab of the map rooms
    public (Room[], Vector3[]) GenerateRoomPositions()
    {
        int roomAmount = Random.Range(data.minRoomNumber, data.maxRoomNumber + 1);
        Room[] rooms = new Room[roomAmount];
        Vector3[] roomPositions = new Vector3[roomAmount];
        
        // We select at which indexes we will spawn stairs
        if (mustGenerateStairs)
        {
            int stairRoomNumber = Random.Range(data.minStairRoomsNumber, data.maxStairRoomsNumber + 1);
            while (stairRoomIndexes.Count != stairRoomNumber)
            {
                int pickedIndex = Random.Range(1, roomAmount);
                if (stairRoomIndexes.Contains(pickedIndex)) continue;
                
                stairRoomIndexes.Add(pickedIndex);
            }
        }
        
        
        
        // We generate the spawn
        if (previousFloorStairRooms.Length == 0)
        {
            rooms[0] = startRoom;
            roomPositions[0] = new Vector3(2000, GenProManager.Instance.currentFloorIndex * 2, 2000);
            takenPositions.Add(roomPositions[0]);
        }
        else
        {
            for (int i = 0; i < previousFloorStairRooms.Length; i++)
            {
                takenPositions.Add(previousFloorStairRooms[i].transform.position + new Vector3Int(0, 2, 0));
            }
        }

        // We generate all the other rooms
        for (int i = 1; i < roomAmount; i++)
        {
            Room currentRoom = GetCurrentRoom(i);
            Vector3 currentPos = GetCurrentRoomPos();

            rooms[i] = currentRoom;
            roomPositions[i] = currentPos;
            takenPositions.Add(roomPositions[i]);
        }

        return (rooms, roomPositions);
    }

    
    private Room GetCurrentRoom(int currentRoomIndex)
    {
        if (stairRoomIndexes.Contains(currentRoomIndex))
        {
            return possibleStairsRooms[Random.Range(0, possibleStairsRooms.Length)];
        }
        
        return possibleRooms[Random.Range(0, possibleRooms.Length)];
    }

    
    private Vector3 GetCurrentRoomPos()
    {
        int counter = 0;
        
        while (true)
        {
            counter++;
            if (counter > 50)
            {
                Debug.LogError("Aidez-moi");
                return new Vector3();
            }

            float amplitude = Random.Range(data.roomSeparationMinDistance, data.roomSeparationMaxDistance + 1);
            Vector3 direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            direction.Normalize();
            
            Vector3 refRoomPos = takenPositions[Random.Range(0, takenPositions.Count)];
            Vector3 finalPos = (refRoomPos + direction * amplitude);
            finalPos = new Vector3((int)finalPos.x, (int)finalPos.y, (int)finalPos.z);
            finalPos += new Vector3(finalPos.x % 2, finalPos.y % 2, finalPos.z % 2);
                
            if (VerifyRoomPos(finalPos))
            {
                return finalPos;
            }
        }
    }
    

    private bool VerifyRoomPos(Vector3 wantedRoomPos)
    {
        for (int i = 0; i < takenPositions.Count; i++)
        {
            float dist = Vector2.Distance(new Vector2(wantedRoomPos.x, wantedRoomPos.z), new Vector2(takenPositions[i].x, takenPositions[i].z));

            if (dist < data.roomSeparationMinDistance)
            {
                return false;
            }
        }
        
        return true;
    }
}
