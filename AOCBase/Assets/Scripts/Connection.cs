using UnityEngine;

public class Connection
{
    public Node From;
    public Node To;

    public int Capacity;
    public int Flux;

    public Connection(Node from, Node to, int capacity)
    {
        From = from;
        To = to;
        Capacity = capacity;
        Flux = 0;
    }
}
