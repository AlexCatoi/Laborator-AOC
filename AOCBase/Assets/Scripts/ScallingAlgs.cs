using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using static Unity.VisualScripting.Member;

public class ScallingAlgs : MonoBehaviour
{
    public ResidualGraph graph;
    public List<Node> originalNodes;
    GraphManager graphManager;
    GenericAlgorithm algorithm;
    private void Start()
    {
        graph = new ResidualGraph();
        algorithm = FindFirstObjectByType<GenericAlgorithm>();
        graphManager = FindFirstObjectByType<GraphManager>();
        originalNodes = graphManager.nodes;
    }
    
    public void Ahuja_Orlin()
    {
        Node source = originalNodes.First(n => n.Type == NodeType.Source);
        Node sink = originalNodes.First(n => n.Type == NodeType.Stock);

        int maxFlow = algorithm.getInitialFlow(source);

        graph.BuildFrom(originalNodes);

        int Cmax = getMaxCapacity();

        int r_bar = 1;
        while (r_bar * 2 <= Cmax)
            r_bar *= 2;
        while(r_bar>=1)
        {
            while (true)
            {
                List<ResidualConnection> path = FindPathDelta(source, sink, r_bar);
                if (path == null)
                    break;
                int bottleneck = algorithm.GetBottleneck(path);
                algorithm.AugmentPath(path,bottleneck);
                maxFlow += bottleneck;
                graph.BuildFrom(originalNodes);
            }
            r_bar /= 2;
        }
        graphManager.DrawFinalConnections();
    }


    public List<ResidualConnection> GetNeighborsDelta(Node node, int delta)
    {
        return graph.Connections
            .Where(c => c.From == node && c.ResidualCapacity >= delta)
            .ToList();
    }
    public int getMaxCapacity()
    {
        int capacity = 0;
        foreach(var node in originalNodes)
        {
            foreach (var edge in node.OutgoingConnections)
            {
                if(edge.Capacity > capacity)
                    capacity = edge.Capacity;
            }
        }
        return capacity;
    }
    public List<ResidualConnection> FindPathDelta(Node source, Node sink, int delta)
    {
        var visited = new HashSet<Node>();
        var path = new List<ResidualConnection>();

        if (GenericPathSelectionDelta(source, sink, visited, path, delta))
            return path;

        return null;
    }
    private bool GenericPathSelectionDelta(Node source, Node sink, HashSet<Node> visited, List<ResidualConnection> path, int delta)
    {
        List<Node> frontier = new List<Node>();
        Dictionary<Node, ResidualConnection> parent = new Dictionary<Node, ResidualConnection>();

        frontier.Add(source);
        visited.Add(source);

        while (frontier.Count > 0)
        {
            int rand = UnityEngine.Random.Range(0, frontier.Count);
            Node current = frontier[rand];
            frontier.RemoveAt(rand);

            foreach (var edge in GetNeighborsDelta(current, delta))
            {
                Node next = edge.To;

                if (visited.Contains(next))
                    continue;

                visited.Add(next);
                parent[next] = edge;

                if (next == sink)
                {
                    Node cur = sink;
                    while (cur != source)
                    {
                        var e = parent[cur];
                        path.Insert(0, e);
                        cur = e.From;
                    }
                    return true;
                }

                frontier.Add(next);
            }
        }

        return false;
    }

    public void Gabow()
    {
        Node source = originalNodes.First(n => n.Type == NodeType.Source);
        Node sink = originalNodes.First(n => n.Type == NodeType.Stock);

        Debug.Log("=== GABOW START ===");

        // Store original capacities ONCE
        List<int> baseCapacities = new List<int>();
        List<(Node from, Node to)> edgeOrder = new List<(Node, Node)>();

        foreach (var node in originalNodes)
        {
            foreach (var edge in node.OutgoingConnections)
            {
                baseCapacities.Add(edge.Capacity);
                edgeOrder.Add((node, edge.To));
                Debug.Log($"Base edge {node.Number}->{edge.To.Number} cap={edge.Capacity}");
            }
        }

        int Cmax = getMaxCapacity();
        int K = 0;
        while ((1 << (K + 1)) <= Cmax)
            K++;

        Debug.Log($"Cmax={Cmax}, K={K}");

        // Zero initial flow
        foreach (var node in originalNodes)
            foreach (var edge in node.OutgoingConnections)
                edge.Flux = 0;

        // Iterate layers
        for (int k = 0; k <= K; k++)
        {
            Debug.Log($"--- Layer k={k} ---");

            // Apply scaled capacities from ORIGINAL values
            int idx = 0;
            foreach (var node in originalNodes)
            {
                foreach (var edge in node.OutgoingConnections)
                {
                    int scaled = baseCapacities[idx] >> (K - k);
                    edge.Capacity = scaled;
                    Debug.Log($"[k={k}] Set cap {node.Number}->{edge.To.Number} = {scaled} (base={baseCapacities[idx]})");
                    idx++;
                }
            }

            // Rebuild residual graph
            graph.BuildFrom(originalNodes);
            Debug.Log($"[k={k}] Residual graph built. Connections: {graph.Connections.Count}");

            // Double previous flow (except k=0)
            if (k > 0)
            {
                Debug.Log($"[k={k}] Doubling previous flow");
                foreach (var node in originalNodes)
                {
                    foreach (var edge in node.OutgoingConnections)
                    {
                        int oldFlux = edge.Flux;
                        edge.Flux *= 2;
                        Debug.Log($"[k={k}] Flux {node.Number}->{edge.To.Number}: {oldFlux} -> {edge.Flux}");
                    }
                }

                graph.BuildFrom(originalNodes);
                Debug.Log($"[k={k}] Residual graph rebuilt after doubling. Connections: {graph.Connections.Count}");
            }

            // Max-flow on this layer
            int augCount = 0;
            while (true)
            {
                List<ResidualConnection> path = FindPathGabow(source, sink);
                if (path == null)
                {
                    Debug.Log($"[k={k}] No more augmenting paths. Total augments: {augCount}");
                    break;
                }

                string pathStr = string.Join(" -> ", path.Select(e => $"{e.From.Number}->{e.To.Number}"));
                int bottleneck = algorithm.GetBottleneck(path);
                Debug.Log($"[k={k}] Augmenting path: {pathStr}, bottleneck={bottleneck}");

                algorithm.AugmentPath(path, bottleneck);
                augCount++;

                graph.BuildFrom(originalNodes);
            }

            // Log final flows after this layer
            Debug.Log($"[k={k}] Flows after layer:");
            foreach (var node in originalNodes)
            {
                foreach (var edge in node.OutgoingConnections)
                {
                    Debug.Log($"[k={k}] Flow {node.Number}->{edge.To.Number}: flux={edge.Flux}, cap={edge.Capacity}");
                }
            }
        }

        Debug.Log("=== GABOW END ===");
        graphManager.DrawFinalConnections();
    }


    public List<ResidualConnection> FindPathGabow(Node source, Node sink)
    {
        var parent = new Dictionary<Node, ResidualConnection>();
        var visited = new HashSet<Node>();
        var queue = new Queue<Node>();

        queue.Enqueue(source);
        visited.Add(source);

        while (queue.Count > 0)
        {
            Node current = queue.Dequeue();

            foreach (var edge in graph.Connections.Where(e => e.From == current && e.ResidualCapacity > 0))
            {
                Node next = edge.To;

                if (visited.Contains(next))
                    continue;

                visited.Add(next);
                parent[next] = edge;

                if (next == sink)
                    return ReconstructGabow(parent, source, sink);

                queue.Enqueue(next);
            }
        }

        return null;
    }

    private List<ResidualConnection> ReconstructGabow(
        Dictionary<Node, ResidualConnection> parent,
        Node source, Node sink)
    {
        List<ResidualConnection> path = new List<ResidualConnection>();
        Node cur = sink;

        while (cur != source)
        {
            var e = parent[cur];
            path.Insert(0, e);
            cur = e.From;
        }

        return path;
    }

}
