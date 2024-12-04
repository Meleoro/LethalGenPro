using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    private List<Corridor> generatedCorridors;
    private Transform currentFloorParent;
    
    [Header("References")]
    public Room[] possibleRooms;
    public Room[] possibleStairsRooms;
    public Room startRoom;
    [SerializeField] private Corridor corridor;
    [SerializeField] private Transform floorParent;
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

    private async void Start()
    {
        await GenerateMap();
    }

    private async Task GenerateMap()
    {
        roomCalculators = new RoomCalculator[data.floorNumber];
        corridorCalculators = new CorridorCalculator[data.floorNumber];
        pathCalculators = new PathCalculator[data.floorNumber];
        generatedRooms = new List<Room>();
        
        for (int i = 0; i < data.floorNumber; i++)
        {
            currentFloorParent = Instantiate(floorParent);
            currentFloorIndex = i;
            
            pathCalculators[i] = new PathCalculator();
            pathCalculators[i].InitialisePathCalculator();
            
            StartNewFloor();
            
            roomCalculators[i] = new RoomCalculator(data, generatedRooms, i != data.floorNumber - 1);
            corridorCalculators[i] = new CorridorCalculator();
            
            await GenerateFloor();

            await Task.Yield();
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
                generatedRooms[i].AddGroundTilesToPathfinding(true);
            }
        }
    }

    
    private async Task GenerateFloor()
    {
        await GenerateRooms();
        await GenerateCorridors();

    }

    private async Task GenerateRooms()
    {
        Room[] rooms;
        Vector3[] roomPositions;
        
        (rooms, roomPositions) = await roomCalculators[currentFloorIndex].GenerateRoomPositions();
        
        for (int i = 0; i < rooms.Length; i++)
        {
            Room newRoom = Instantiate(rooms[i], roomPositions[i], Quaternion.Euler(0, 0, 0), currentFloorParent);
            generatedRooms.Add(newRoom);
            generatedRooms[generatedRooms.Count - 1].GenerateCorridorSpots();
            generatedRooms[generatedRooms.Count - 1].AddGroundTilesToPathfinding();
            corridorSpots.AddRange(newRoom.GetCorridorsSpots());

            await Task.Yield();
        }
    }
    
    private async Task GenerateCorridors()
    {
        List<Vector3> corridorsPositions = new List<Vector3>();
        generatedCorridors = new List<Corridor>();
        
        for (int i = 0; i < generatedRooms.Count; i++)
        {
            corridorsPositions = corridorCalculators[currentFloorIndex].GenerateRoomCorridors(generatedRooms[i], generatedRooms.ToArray(), 20);
            
            for (int j = 0; j < corridorsPositions.Count; j++)
            {
                Corridor newCorridor = Instantiate(corridor, corridorsPositions[j], Quaternion.Euler(0, 0, 0), currentFloorParent);
                generatedCorridors.Add(newCorridor);
                generatedCorridors[generatedCorridors.Count - 1].ActualiseWalls();
            }
            
            await Task.Yield();
        }
        
        Debug.Log("fdhgrz_hd");
        corridorCalculators[currentFloorIndex].ManageCorridorsNeighbors(generatedCorridors.ToList());
    }
}