using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RoomCalculator
{
    [Header("References / Parameters")]
    private GlobalGenProData data;
    private Room[] possibleRooms;
    private Room[] possibleStairsRooms;
    private Room[] previousFloorStairRooms;
    private bool mustGenerateStairs;

    [Header("Private Infos")] 
    private List<Vector3> takenPositions = new List<Vector3>();
    private List<Vector3Int> takenTilesPositions = new List<Vector3Int>();
    private List<int> stairRoomIndexes = new List<int>();
    private List<int> uniqueRoomIndexes = new List<int>();
    private List<int> neededRoomIndexes = new List<int>();

    [Header("Public Infos")] 
    public int roomAmount;
    
    
    public RoomCalculator(GlobalGenProData data, List<Room> previousFloorStairRooms, bool haveFloorUpside = false)
    {
        possibleRooms = GenProManager.Instance.roomsData.possiblesRooms;
        possibleStairsRooms = GenProManager.Instance.roomsData.possiblesStairsRooms;
        this.data = data;
        this.previousFloorStairRooms = previousFloorStairRooms.ToArray();
        mustGenerateStairs = haveFloorUpside;
        
        
        roomAmount = Random.Range(data.minRoomNumber, data.maxRoomNumber + 1);
        
        // We select at which indexes we will spawn stairs
        if (mustGenerateStairs)
        {
            int stairRoomNumber = Random.Range(data.minStairRoomsNumber, data.maxStairRoomsNumber + 1);
            while (stairRoomIndexes.Count != stairRoomNumber)
            {
                int pickedIndex = Random.Range(0, roomAmount);
                if (stairRoomIndexes.Contains(pickedIndex)) continue;
                
                stairRoomIndexes.Add(pickedIndex);
            }
        }

        // We select at which indexes we will spawn unique rooms
        if (GenProManager.Instance.uniqueRoomsFloorIndexes.Contains(GenProManager.Instance.currentFloorIndex))
        {
            for (int i = 0; i < GenProManager.Instance.uniqueRoomsFloorIndexes.Count; i++)
            {
                if (GenProManager.Instance.uniqueRoomsFloorIndexes[i] == GenProManager.Instance.currentFloorIndex)
                {
                    int counter = 0;
                    
                    while (true)
                    {
                        counter++;
                        // If there are not enough rooms available we add one
                        if (counter > 200)
                        {
                            roomAmount++;
                            uniqueRoomIndexes.Add(roomAmount-1);
                        }
                        
                        // We verify if this room isn't already special
                        int pickedIndex = Random.Range(0, roomAmount);
                        if (stairRoomIndexes.Contains(pickedIndex)) continue;
                        if (uniqueRoomIndexes.Contains(pickedIndex)) continue;
                
                        uniqueRoomIndexes.Add(pickedIndex);
                        break;
                    }
                }
            }
        }
        
        
        // We select at which indexes we will spawn needed rooms
        if (GenProManager.Instance.neededRoomsFloorIndexes.Contains(GenProManager.Instance.currentFloorIndex))
        {
            for (int i = 0; i < GenProManager.Instance.neededRoomsFloorIndexes.Count; i++)
            {
                if (GenProManager.Instance.neededRoomsFloorIndexes[i] == GenProManager.Instance.currentFloorIndex)
                {
                    int antiCrashCounter = 0;
                    
                    while (true)
                    {
                        antiCrashCounter++;
                        // If there are not enough rooms available we add one
                        if (antiCrashCounter > 200)
                        {
                            roomAmount++;
                            neededRoomIndexes.Add(roomAmount-1);
                        }
                        
                        // We verify if this room isn't already special
                        int pickedIndex = Random.Range(0, roomAmount);
                        if (stairRoomIndexes.Contains(pickedIndex)) continue;
                        if (uniqueRoomIndexes.Contains(pickedIndex)) continue;
                        if (neededRoomIndexes.Contains(pickedIndex)) continue;
                
                        neededRoomIndexes.Add(pickedIndex);
                        break;
                    }
                }
            }
        }
        
        // If this floor isn't the first, we add the stairs room of the previous stairs to the scripts
        if (GenProManager.Instance.currentFloorIndex != 0)
        {
            for (int i = 0; i < this.previousFloorStairRooms.Length; i++)
            {
                takenPositions.Add(this.previousFloorStairRooms[i].transform.position + new Vector3Int(0, 4, 0));
                AddGroundTiles(this.previousFloorStairRooms[i].transform.position, this.previousFloorStairRooms[i]);
            }
        }
    }

    public (Room, Vector3) GenerateRoomPosition(int index)
    {
        Room currentRoom = GetCurrentRoom(index);
        Vector3 currentPos = GetCurrentRoomPos(currentRoom);

        AddGroundTiles(currentPos, currentRoom);
        takenPositions.Add(currentPos);

        return (currentRoom, currentPos);
    }

    
    private Room GetCurrentRoom(int currentRoomIndex)
    {
        if (stairRoomIndexes.Contains(currentRoomIndex))
        {
            return possibleStairsRooms[Random.Range(0, possibleStairsRooms.Length)];
        }

        if (uniqueRoomIndexes.Contains(currentRoomIndex))
        {
            int pickedIndex = Random.Range(0, GenProManager.Instance.remainingUniqueRooms.Count);
            Room pickedRoom = GenProManager.Instance.remainingUniqueRooms[pickedIndex];
            GenProManager.Instance.SpawnUniqueRoom(pickedIndex);
            
            return pickedRoom;
        }
        
        if (neededRoomIndexes.Contains(currentRoomIndex))
        {
            int pickedIndex = Random.Range(0, GenProManager.Instance.remainingNeededRooms.Count);
            Room pickedRoom = GenProManager.Instance.remainingNeededRooms[pickedIndex];
            GenProManager.Instance.SpawnNeededRoom(pickedIndex);
            
            return pickedRoom;
        }
        
        return possibleRooms[Random.Range(0, possibleRooms.Length)];
    }

    
    private Vector3 GetCurrentRoomPos(Room wantedRoom)
    {
        int counter = 0;

        if (takenPositions.Count == 0)
        {
            takenPositions.Add(new Vector3(2000, 0, 2000));
        }
        
        while (true)
        {
            counter++;
            if (counter > 50)
            {
                Debug.LogError("No possible position for a room found");
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

    
    private Transform[] roomTiles;
    private List<Vector3Int> roomTilePositions;
    private bool VerifyRoomPos(Vector3 wantedRoomPos, Room wantedRoom)
    {
        roomTiles = wantedRoom.GetGroundTiles();
        roomTilePositions = new List<Vector3Int>();
        
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

    
    // Add ground tiles to avoid having two rooms into each others
    private void AddGroundTiles(Vector3 pos, Room room)
    {
        roomTiles = room.GetGroundTiles();

        for (int i = 0; i < roomTiles.Length; i++)
        {
            Vector3 tilePos = pos + roomTiles[i].localPosition;
            takenTilesPositions.Add(new Vector3Int((int)tilePos.x, 0, (int)tilePos.z));
        }
    }
}
