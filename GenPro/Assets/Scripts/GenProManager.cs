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
    private List<Room> generatedRooms;
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
        generatedRooms = new List<Room>();
        
        for (int i = 0; i < data.floorNumber; i++)
        {
            currentFloorIndex = i;
            
            pathCalculators[i] = new PathCalculator();
            pathCalculators[i].InitialisePathCalculator();
            
            StartNewFloor();
            
            roomCalculators[i] = new RoomCalculator(data, generatedRooms, i != data.floorNumber - 1);
            corridorCalculators[i] = new CorridorCalculator();
            
            GenerateFloor();
        }
    }

    private void StartNewFloor()
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
            {
                generatedRooms.RemoveAt(i);
            }
            else
            {
                generatedRooms[i].AddGroundTilesToPathfinding();
            }
        }
    }

    
    private void GenerateFloor()
    {
        List<Vector3> corridorsPositions;

        GenerateRooms();
        
        // We generate the corridors
        corridorsPositions =  corridorCalculators[currentFloorIndex].GenerateCorridorPositions(generatedRooms.ToArray());
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
        
        for (int i = 0; i < rooms.Length; i++)
        {
            Room newRoom = Instantiate(rooms[i], roomPositions[i], Quaternion.Euler(0, 0, 0));
            generatedRooms.Add(newRoom);
            generatedRooms[generatedRooms.Count - 1].GenerateCorridorSpots();
            generatedRooms[generatedRooms.Count - 1].AddGroundTilesToPathfinding();
            corridorSpots.AddRange(newRoom.GetCorridorsSpots());
        }
    }
    
}