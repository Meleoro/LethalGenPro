using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class GenProManager : MonoBehaviour
{
    public static GenProManager Instance;
    
    [Header("Data")] 
    public GlobalGenProData genProData;
    public PossibleRoomsData roomsData;

    [Header("Public Infos")]
    [HideInInspector] public List<Transform> corridorSpots;
    [HideInInspector] public List<int> neededRoomsFloorIndexes;
    [HideInInspector] public List<int> uniqueRoomsFloorIndexes;
    [HideInInspector] public int currentFloorIndex;
    [HideInInspector] public List<Room> remainingUniqueRooms;
    [HideInInspector] public List<Room> remainingNeededRooms;
    [HideInInspector] public Transform currentFloorParent;
    
    [Header("Private Infos")] 
    private List<Room> generatedRooms;
    private List<Corridor> generatedCorridors;
    
    [Header("References")]
    [SerializeField] private Corridor corridor;
    [SerializeField] private Transform floorParent;
    private RoomCalculator[] roomCalculators;
    private CorridorCalculator[] corridorCalculators;
    public PathCalculator pathfindingScript;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        GenerateMap();
    }

    private async Task GenerateMap()
    {
        InitialiseVariables();
        
        // Initialises the pathfinding script
        await pathfindingScript.InitialisePathCalculator();
        
        for (int i = 0; i < genProData.floorNumber; i++)
        {
            currentFloorParent = Instantiate(floorParent);
            currentFloorIndex = i;
            
            // Resets the values of our pathfinding algorithme for the new floor
            await pathfindingScript.ResetPathCalculator();
            
            await StartNewFloor();
            
            // We generates new scripts for this floor
            roomCalculators[i] = new RoomCalculator(genProData, generatedRooms, i != genProData.floorNumber - 1);
            corridorCalculators[i] = new CorridorCalculator();
            
            await GenerateFloor();
            
        }
    }
    
    private void InitialiseVariables()
    {
        roomCalculators = new RoomCalculator[genProData.floorNumber];
        corridorCalculators = new CorridorCalculator[genProData.floorNumber];
        pathfindingScript = new PathCalculator();
        generatedRooms = new List<Room>();
        neededRoomsFloorIndexes = new List<int>();
        uniqueRoomsFloorIndexes = new List<int>();
        remainingNeededRooms = roomsData.neededRooms.ToList();
        remainingUniqueRooms = roomsData.possiblesUniqueRooms.ToList();
        
        // Select on which floors there will be unique rooms
        int uniqueRoomAmount = Random.Range(genProData.minUniqueRoomsNumber, genProData.maxUniqueRoomsNumber);
        if (uniqueRoomAmount > roomsData.possiblesUniqueRooms.Length) uniqueRoomAmount = roomsData.possiblesUniqueRooms.Length;
        for(int i = 0; i < uniqueRoomAmount; i++)
        {
            int randomFloor = Random.Range(0, genProData.floorNumber - 1);
            
            uniqueRoomsFloorIndexes.Add(randomFloor);
        }
        
        // Select on which floors there will be needed rooms
        for(int i = 0; i < roomsData.neededRooms.Length; i++)
        {
            int randomFloor = Random.Range(0, genProData.floorNumber - 1); 
            neededRoomsFloorIndexes.Add(randomFloor);
        }
    } 
    
    private async Task StartNewFloor()
    {
        corridorSpots.Clear();

        // We remove every room except the ones with stairs on the previous floor
        for (int i = generatedRooms.Count - 1; i >= 0; i--)
        {
            CorridorSpot[] roomCorridorSpots = generatedRooms[i].corridorSpots;
            bool canBeRemoved = true;

            for (int j = 0; j < roomCorridorSpots.Length; j++)
            {
                if (!roomCorridorSpots[j].hasCorridor)
                {
                    canBeRemoved = false;
                    corridorSpots.Add(roomCorridorSpots[j].transform);
                }
            }

            if (canBeRemoved)
                generatedRooms.RemoveAt(i);
            
            else
                await generatedRooms[i].AddGroundTilesToPathfinding(true);
            
        }
    }


    #region Generate Floor Functions

    private async Task GenerateFloor()
    {
        await GenerateRooms();
        
        await GenerateCorridors();

    }

    private async Task GenerateRooms()
    {
        Room room = null;
        Vector3 roomPosition = Vector3.zero;
        
        for (int i = 0; i < roomCalculators[currentFloorIndex].roomAmount; i++)
        {
            (room, roomPosition) = roomCalculators[currentFloorIndex].GenerateRoomPosition(i);
            
            Room newRoom = Instantiate(room, roomPosition, Quaternion.Euler(0, 0, 0), currentFloorParent);
            generatedRooms.Add(newRoom);
            
            // A connected room is a room on which corridors can be attached, this bool becomes true when another room connects with a connected room, so we need at least one room "connected" at the start
            if (i == 0) newRoom.isConnected = true;   
            
            newRoom.GenerateCorridorSpots();
            await newRoom.AddGroundTilesToPathfinding();
            
            newRoom.GenerateProps();
            corridorSpots.AddRange(newRoom.GetCorridorsSpots());     // We stock the possible spots where this new room can be connected to other rooms
        }
    }
    
    private async Task GenerateCorridors()
    {
        List<Vector3> corridorsPositions = new List<Vector3>();
        List<Vector3> bannedPositions = new List<Vector3>();
        generatedCorridors = new List<Corridor>();
        
        for (int i = 0; i < generatedRooms.Count; i++)
        {
            corridorsPositions = corridorCalculators[currentFloorIndex].GenerateRoomCorridors(generatedRooms[i], generatedRooms.ToArray(), 20);
            
            // We generate all the corridors connected to this room
            for (int j = 0; j < corridorsPositions.Count; j++)
            {
                if (bannedPositions.Contains(corridorsPositions[j])) continue;
                bannedPositions.Add(corridorsPositions[j]);
                
                Corridor newCorridor = Instantiate(corridor, corridorsPositions[j], Quaternion.Euler(0, 0, 0), currentFloorParent);
                generatedCorridors.Add(newCorridor);
                generatedCorridors[generatedCorridors.Count - 1].ActualiseWalls();     // Remove walls connected to roomes
            }
            
            await Task.Yield();
        }

        await corridorCalculators[currentFloorIndex].ManageCorridorsNeighbors(generatedCorridors.ToList());   // Remove walls connected to other corridors
    }

    #endregion

    
    #region Public Functions
    
    // Needed to avoid to spawn twice an unique or a needed room
    public void SpawnUniqueRoom(int index)
    {
        remainingUniqueRooms.RemoveAt(index);
    }

    public void SpawnNeededRoom(int index)
    {
        remainingNeededRooms.RemoveAt(index);
    }

    #endregion
}