using UnityEngine;

[CreateAssetMenu(menuName = "GlobalGenProData")]
public class GlobalGenProData : ScriptableObject
{   
    [Header("Base Parameters")]
    public int minRoomNumber;
    public int maxRoomNumber;
    public int roomSeparationMinDistance;
    public int roomSeparationMaxDistance;

    [Header("FloorsParameters")]
    public int floorNumber;
    public int minStairRoomsNumber;
    public int maxStairRoomsNumber;
}
