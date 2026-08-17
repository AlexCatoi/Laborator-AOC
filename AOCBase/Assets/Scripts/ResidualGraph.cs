using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResidualGraph 
{
    public List<Node> Nodes = new();
    public List<ResidualConnection> Connections = new();
    public List<ResidualConnection> ConnectionsForDisplay = new();

    public void BuildFrom(List<Node> originalNodes)
    {
        Nodes = originalNodes.ToList();
        Connections.Clear();

        foreach (Node u in originalNodes)
        {
            foreach (Connection uv in u.OutgoingConnections)
            {
                Node v = uv.To;

                int Cuv = uv.Capacity;
                int Fuv = uv.Flux;

                // Forward residual edge
                int forwardResidual = Cuv - Fuv;
                if (forwardResidual > 0)
                {
                    Connections.Add(new ResidualConnection
                    {
                        From = u,
                        To = v,
                        ResidualCapacity = forwardResidual,
                        Original = uv,
                        IsForward = true
                    });

                }

                // Backward residual edge
                if (Fuv > 0)
                {
                    Connections.Add(new ResidualConnection
                    {
                        From = v,
                        To = u,
                        ResidualCapacity = Fuv,
                        Original = uv,
                        IsForward = false
                    });
                }
            }
        }
        ConnectionsForDisplay = Connections
    .GroupBy(c => (c.From, c.To))
    .Select(g => new ResidualConnection
    {
        From = g.Key.From,
        To = g.Key.To,
        ResidualCapacity = g.Sum(x => x.ResidualCapacity)
    })
    .ToList();
    }
}
