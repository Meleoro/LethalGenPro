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
    public List<Transform> corridorSpot;
    
    [Header("Private Infos")] 
    private Room[] generatedRooms;
    private Corridor[] generatedCorridors;
    
    [Header("References")]
    [SerializeField] private Room[] possibleRooms;
    [SerializeField] private Room startRoom;
    [SerializeField] private Corridor corridor;
    private RoomCalculator _roomCalculator;
    private CorridorCalculator _corridorCalculator;
    public PathCalculator _pathCalculator;


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
        _roomCalculator = new RoomCalculator(possibleRooms, startRoom, data);
        _corridorCalculator = new CorridorCalculator();
        _pathCalculator = new PathCalculator();
        
        _pathCalculator.InitialisePathCalculator();
        GenerateMap();
    }

    private void GenerateMap()
    {
        Room[] rooms;
        Vector3[] roomPositions;
        List<Vector3> corridorsPositions;
        
        // We generate the rooms
        (rooms, roomPositions) = _roomCalculator.GenerateRoomPositions();
        generatedRooms = new Room[rooms.Length];
        
        for (int i = 0; i < rooms.Length; i++)
        {
            Room newRoom = Instantiate(rooms[i], roomPositions[i], Quaternion.Euler(0, 0, 0));
            generatedRooms[i] = newRoom;
            generatedRooms[i].GenerateCorridorSpots();
            generatedRooms[i].AddGroundTilesToPathfinding();
            corridorSpot.AddRange(newRoom.GetCorridorsSpots());
        }
        
        // We generate the corridors
        corridorsPositions = _corridorCalculator.GenerateCorridorPositions(generatedRooms);
        generatedCorridors = new Corridor[corridorsPositions.Count];
        
        for (int i = 0; i < corridorsPositions.Count; i++)
        {
            Corridor newCorridor = Instantiate(corridor, corridorsPositions[i], Quaternion.Euler(0, 0, 0));
            generatedCorridors[i] = newCorridor;
            generatedCorridors[i].ActualiseWalls();
        }
        
        _corridorCalculator.ManageCorridorsNeighbors(generatedCorridors.ToList());
    }
    
    
    
}