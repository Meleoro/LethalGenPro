using UnityEngine;

[CreateAssetMenu(menuName = "PossibleRoomsData")]
public class PossibleRoomsData : ScriptableObject
{
    public Room[] possiblesRooms;
    public Room[] possiblesStairsRooms;
    public Room[] possiblesUniqueRooms;
    public Room[] neededRooms;
    public GameObject[] possibleProps;
}
