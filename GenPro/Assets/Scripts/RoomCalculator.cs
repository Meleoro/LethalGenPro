using System.Collections.Generic;
using UnityEngine;

public class RoomCalculator
{
    [Header("References / Parameters")]
    private GlobalGenProData data;
    private Room[] possibleRooms;
    private Room startRoom;

    [Header("Private Infos")] 
    private List<Vector3> takenPositions = new List<Vector3>();

    
    public RoomCalculator(Room[] possibleRooms, Room startRoom, GlobalGenProData data)
    {
        this.possibleRooms = possibleRooms;
        this.startRoom = startRoom;
        this.data = data;
    }
    
    
    // Returns the positions and prefab of the map rooms
    public (Room[], Vector3[]) GenerateRoomPositions()
    {
        int roomAmount = Random.Range(data.minRoomNumber, data.maxRoomNumber + 1);
        Room[] rooms = new Room[roomAmount];
        Vector3[] roomPositions = new Vector3[roomAmount];
        
        // We generate the spawn
        rooms[0] = startRoom;
        roomPositions[0] = new Vector3(2000, 0, 2000);
        takenPositions.Add(roomPositions[0]);
        
        // We generate all the other rooms
        for (int i = 1; i < roomAmount; i++)
        {
            Room currentRoom = GetCurrentRoom();
            Vector3 currentPos = GetCurrentRoomPos();

            rooms[i] = currentRoom;
            roomPositions[i] = currentPos;
            takenPositions.Add(roomPositions[i]);
        }

        return (rooms, roomPositions);
    }

    
    private Room GetCurrentRoom()
    {
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
