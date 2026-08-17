using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public int Number;
    public NodeType Type;

    public List<Connection> OutgoingConnections = new List<Connection>();
    public List<Connection> IncomingConnections = new List<Connection>();

    public Node(int number, NodeType type)
    {
        Number = number;
        Type = type;
    }
}

public enum NodeType
{
    Normal=0,
    Source=1,
    Stock=2
}
