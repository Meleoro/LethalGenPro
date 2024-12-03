using UnityEngine;

[CreateAssetMenu(menuName = "GlobalGenProData")]
public class GlobalGenProData : ScriptableObject
{
    public int minRoomNumber;
    public int maxRoomNumber;

    public int roomSeparationMinDistance;
    public int roomSeparationMaxDistance;

    public int stairsNumber;
}
