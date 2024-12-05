using UnityEngine;

[CreateAssetMenu(menuName = "GlobalGenProData")]
public class GlobalGenProData : ScriptableObject
{   
    [Header("Rooms Parameters")]
    [Range(5, 50)] public int minRoomNumber;
    [Range(5, 50)] public int maxRoomNumber;
    public int roomSeparationMinDistance;
    public int roomSeparationMaxDistance;

    [Header("Floors Parameters")]
    [Range(2, 8)] public int floorNumber;
    [Range(1, 10)] public int minStairRoomsNumber;
    [Range(1, 10)] public int maxStairRoomsNumber;
    
    [Header("Special Rooms Parameters")]
    public int minUniqueRoomsNumber;
    public int maxUniqueRoomsNumber;

    [Header("Props")] 
    [Range(0, 100)] public int propSpawnProba;
}
