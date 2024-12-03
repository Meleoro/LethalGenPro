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
    private List<Vector3Int> takenTilesPositions = new List<Vector3Int>();
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

        int startIndex = 0;

        // We generate the spawn
        if (GenProManager.Instance.currentFloorIndex == 0)
        {
            startIndex++;
            rooms[0] = startRoom;
            roomPositions[0] = new Vector3(2000, GenProManager.Instance.currentFloorIndex * 4, 2000);
            takenPositions.Add(roomPositions[0]);
            
            AddGroundTiles(roomPositions[0], rooms[0]);
        }
        else
        {
            for (int i = 0; i < previousFloorStairRooms.Length; i++)
            {
                takenPositions.Add(previousFloorStairRooms[i].transform.position + new Vector3Int(0, 4, 0));
                AddGroundTiles(previousFloorStairRooms[i].transform.position, previousFloorStairRooms[i]);
            }
        }

        // We generate all the other rooms
        for (int i = startIndex; i < roomAmount; i++)
        {
            Room currentRoom = GetCurrentRoom(i);
            Vector3 currentPos = GetCurrentRoomPos(currentRoom);

            AddGroundTiles(currentPos, currentRoom);
            
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

    
    private Vector3 GetCurrentRoomPos(Room wantedRoom)
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

            float amplitude = Random.Range(data.roomSeparationMinDistance + counter, data.roomSeparationMaxDistance + counter);
            Vector3 direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            direction.Normalize();
            
            Vector3 refRoomPos = takenPositions[Random.Range(0, takenPositions.Count)];
            Vector3 finalPos = (refRoomPos + direction * amplitude);
            finalPos = new Vector3((int)finalPos.x, (int)finalPos.y, (int)finalPos.z);
            finalPos += new Vector3(finalPos.x % 2, finalPos.y % 2, finalPos.z % 2);
                
            if (VerifyRoomPos(finalPos, wantedRoom))
            {
                return finalPos;
            }
        }
    }
    

    private bool VerifyRoomPos(Vector3 wantedRoomPos, Room wantedRoom)
    {
        Transform[] roomTiles = wantedRoom.GetGroundTiles();

        List<Vector3Int> roomTilePositions = new List<Vector3Int>();
        for (int i = 0; i < roomTiles.Length; i++)
        {
            Vector3 tilePos = wantedRoomPos + roomTiles[i].localPosition;
            roomTilePositions.Add(new Vector3Int((int)tilePos.x, 0, (int)tilePos.z));
        }

        for (int i = 0; i < roomTilePositions.Count; i++)
        {
            for (int j = 0; j < takenTilesPositions.Count; j++)
            {
                if (roomTilePositions[i] == takenTilesPositions[j])
                {
                    return false;
                }

                if (Vector2.Distance(new Vector2(roomTilePositions[i].x, roomTilePositions[i].z),
                        new Vector2(takenTilesPositions[j].x, takenTilesPositions[j].z)) < 7)
                {
                    return false;
                }
            }
        }
        
        return true;
    }

    private void AddGroundTiles(Vector3 pos, Room room)
    {
        Transform[] roomTiles = room.GetGroundTiles();

        for (int i = 0; i < roomTiles.Length; i++)
        {
            Vector3 tilePos = pos + roomTiles[i].localPosition;
            takenTilesPositions.Add(new Vector3Int((int)tilePos.x, 0, (int)tilePos.z));
        }
    }
}
