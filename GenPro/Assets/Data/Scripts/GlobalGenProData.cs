using UnityEngine;

[CreateAssetMenu(menuName = "GlobalGenProData")]
public class GlobalGenProData : ScriptableObject
{   
    [Header("Rooms Parameters")]
    public int minRoomNumber;
    public int maxRoomNumber;
    public int roomSeparationMinDistance;
    public int roomSeparationMaxDistance;

    [Header("Floors Parameters")]
    public int floorNumber;
    public int minStairRoomsNumber;
    public int maxStairRoomsNumber;
    
    [Header("Special Rooms Parameters")]
    public int minUniqueRoomsNumber;
    public int maxUniqueRoomsNumber;

    [Header("Props")] 
    [Range(0, 100)] public int propSpawnProba;
}
