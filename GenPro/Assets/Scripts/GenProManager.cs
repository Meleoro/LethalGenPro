using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenProManager : MonoBehaviour
{
    public static GenProManager Instance;
    
    [Header("Data")] 
    [SerializeField] private GlobalGenProData data;

    [Header("Public Infos")]
    public List<Transform> corridorSpots;
    public int currentFloorIndex;
    
    [Header("Private Infos")] 
    private Room[] generatedRooms;
    private Corridor[] generatedCorridors;
    
    [Header("References")]
    public Room[] possibleRooms;
    public Room[] possibleStairsRooms;
    public Room startRoom;
    [SerializeField] private Corridor corridor;
    private RoomCalculator[] roomCalculators;
    private CorridorCalculator[] corridorCalculators;
    public PathCalculator[] pathCalculators;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        GenerateMap();
    }

    private void GenerateMap()
    {
        roomCalculators = new RoomCalculator[data.floorNumber];
        corridorCalculators = new CorridorCalculator[data.floorNumber];
        pathCalculators = new PathCalculator[data.floorNumber];

        List<Room> previousFloorStairs = new List<Room>();
        
        for (int i = 0; i < data.floorNumber; i++)
        {
            currentFloorIndex = i;
            
            roomCalculators[i] = new RoomCalculator(data, previousFloorStairs, i != data.floorNumber - 1);
            corridorCalculators[i] = new CorridorCalculator();
            pathCalculators[i] = new PathCalculator();
            
            pathCalculators[i].InitialisePathCalculator();
            
            GenerateFloor();
        }
    }

    private void StartNewFloor()
    {
        for (int i = 0; i < corridorSpots.Count; i++)
        {
            if(corridorSpots[i].)
        }
    }

    
    private void GenerateFloor()
    {
        List<Vector3> corridorsPositions;

        GenerateRooms();
        
        // We generate the corridors
        corridorsPositions =  corridorCalculators[currentFloorIndex].GenerateCorridorPositions(generatedRooms);
        generatedCorridors = new Corridor[corridorsPositions.Count];
        
        for (int i = 0; i < corridorsPositions.Count; i++)
        {
            Corridor newCorridor = Instantiate(corridor, corridorsPositions[i], Quaternion.Euler(0, 0, 0));
            generatedCorridors[i] = newCorridor;
            generatedCorridors[i].ActualiseWalls();
        }
        
        corridorCalculators[currentFloorIndex].ManageCorridorsNeighbors(generatedCorridors.ToList());
    }

    private void GenerateRooms()
    {
        Room[] rooms;
        Vector3[] roomPositions;
        
        (rooms, roomPositions) = roomCalculators[currentFloorIndex].GenerateRoomPositions();
        generatedRooms = new Room[rooms.Length];
        
        for (int i = 0; i < rooms.Length; i++)
        {
            Room newRoom = Instantiate(rooms[i], roomPositions[i], Quaternion.Euler(0, 0, 0));
            generatedRooms[i] = newRoom;
            generatedRooms[i].GenerateCorridorSpots();
            generatedRooms[i].AddGroundTilesToPathfinding();
            corridorSpots.AddRange(newRoom.GetCorridorsSpots());
        }
    }
    
}