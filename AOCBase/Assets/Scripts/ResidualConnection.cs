using UnityEngine;

public class ResidualConnection
{
    public Node From;
    public Node To;
    public int Capacity;
    public int ResidualCapacity;

    public Connection Original; 
    public bool IsForward;
}
