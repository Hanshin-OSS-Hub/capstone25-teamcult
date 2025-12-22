using UnityEngine;
using System.Collections.Generic;

public enum Direction { Up=0, Right, Down, Left}
public class RoomConnector : MonoBehaviour
{
    public List<Direction> availableDoors;
}
