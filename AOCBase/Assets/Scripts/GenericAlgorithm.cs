using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;
using System;

public class GenericAlgorithm:MonoBehaviour
{
    public ResidualGraph graph;
    public List<Node> originalNodes;
    GraphManager graphManager;
    private void Awake()
    {
        graph = new ResidualGraph();
        graphManager = FindFirstObjectByType<GraphManager>();
        originalNodes = graphManager.nodes;
    }
    public List<ResidualConnection> GetNeighbors(Node node)
    {
        return graph.Connections
            .Where(c => c.From == node && c.ResidualCapacity > 0)
            .ToList();
    }

    public List<ResidualConnection> FindPath(Node source, Node sink)
    {
        var visited = new HashSet<Node>();
        var path = new List<ResidualConnection>();

        if (GenericPathSelection(source, sink, visited, path))
            return path;

        return null;
    }
    /*
    private bool DFS(Node current, Node sink, HashSet<Node> visited, List<ResidualConnection> path)
    {
        if (current == sink)
            return true;

        visited.Add(current);

        foreach (var edge in GetNeighbors(current))
        {
            Node next = edge.To;

            if (visited.Contains(next))
                continue;

            path.Add(edge);

            if (DFS(next, sink, visited, path))
                return true;

            path.RemoveAt(path.Count - 1); 
        }

        return false;
    }
    */
    private bool GenericPathSelection(Node source, Node sink, HashSet<Node> visited, List<ResidualConnection> path)
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

            foreach (var edge in GetNeighbors(current))
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

    public void GenericAlg()
    {
        int maxFlow = getInitialFlow(originalNodes[0]);

        Node source = originalNodes.First(n => n.Type == NodeType.Source);
        Node stock = originalNodes.First(n => n.Type == NodeType.Stock);

        graph.BuildFrom(originalNodes);

        while (true)
        {
            List<ResidualConnection> path = FindPath(source, stock);

            if (path == null)
                break;

            int bottleneck = GetBottleneck(path);

            AugmentPath(path, bottleneck);

            maxFlow += bottleneck;

            graph.BuildFrom(originalNodes);
        }
        graphManager.DrawFinalConnections();
    }

    public int GetBottleneck(List<ResidualConnection> path)
    {
        int min = int.MaxValue;

        foreach (var edge in path)
        {
            if (edge.ResidualCapacity < min)
                min = edge.ResidualCapacity;
        }
        return min;
    }

    public void AugmentPath(List<ResidualConnection> path, int flow)
    {
        foreach (var edge in path)
        {
            if (edge.IsForward)
                edge.Original.Flux += flow;
            else
                edge.Original.Flux -= flow;
        }
    }


    public int getInitialFlow(Node source)
    {
        int flow = 0;
        foreach(var edge in source.OutgoingConnections)
        {
            flow += edge.Flux;
        }
        return flow;    
    }

    public void FordFulkerson()
    {
        int maxFlow = getInitialFlow(originalNodes[0]);
        Node source = originalNodes.First(n => n.Type == NodeType.Source);
        Node stock = originalNodes.First(n => n.Type == NodeType.Stock);

        graph.BuildFrom(originalNodes);

        List<int> p = Enumerable.Repeat(0, originalNodes.Count).ToList();
        List<Node> V = new List<Node>();

        do
        {
            for (int i = 0; i < originalNodes.Count; i++)
                p[i] = 0;

            V.Clear();

            p[source.Number - 1] = source.Number;
            V.Add(source);

            while (V.Count > 0 && p[stock.Number - 1] == 0)
            {
                int random= UnityEngine.Random.Range(0, V.Count);
                Node aux = V[random];
                V.Remove(aux);

                foreach (var edge in GetNeighbors(aux))
                {
                    Node next = edge.To;

                    if (p[next.Number - 1] == 0)
                    {
                        p[next.Number - 1] = aux.Number;
                        V.Add(next);
                    }
                }
            }

            if (p[stock.Number - 1] != 0)
            {
                List<ResidualConnection> path = ReconstructPath(p, source, stock);

                int bottleneck = GetBottleneck(path);

                AugmentPath(path, bottleneck);

                maxFlow += bottleneck;

                graph.BuildFrom(originalNodes);
            }

        } while (p[stock.Number - 1] != 0);
        graphManager.DrawFinalConnections();
    }

    List<ResidualConnection> ReconstructPath(List<int> p, Node source, Node sink)
    {
        List<ResidualConnection> path = new List<ResidualConnection>();

        int current = sink.Number;

        while (current != source.Number)
        {
            int parent = p[current - 1];

            Node from = originalNodes[parent - 1];
            Node to = originalNodes[current - 1];

            ResidualConnection edge = graph.Connections
                .First(e => e.From == from && e.To == to);

            path.Add(edge);

            current = parent;
        }

        path.Reverse();

        return path;
    }

    public void EdmondKarp()
    {
        int maxFlow = getInitialFlow(originalNodes[0]);
        Node source = originalNodes.First(n => n.Type == NodeType.Source);
        Node stock = originalNodes.First(n => n.Type == NodeType.Stock);

        graph.BuildFrom(originalNodes);

        List<int> p = Enumerable.Repeat(0, originalNodes.Count).ToList();
        Queue<Node> V = new Queue<Node>();

        do
        {
            for (int i = 0; i < originalNodes.Count; i++)
                p[i] = 0;

            V.Clear();

            p[source.Number - 1] = source.Number;
            V.Enqueue(source);

            while (V.Count > 0 && p[stock.Number - 1] == 0)
            {
                Node aux = V.Dequeue();

                foreach (var edge in GetNeighbors(aux))
                {
                    Node next = edge.To;

                    if (p[next.Number - 1] == 0)
                    {
                        p[next.Number - 1] = aux.Number;
                        V.Enqueue(next);
                    }
                }
            }

            if (p[stock.Number - 1] != 0)
            {
                List<ResidualConnection> path = ReconstructPath(p, source, stock);

                int bottleneck = GetBottleneck(path);

                AugmentPath(path, bottleneck);

                maxFlow += bottleneck;

                graph.BuildFrom(originalNodes);
            }

        } while (p[stock.Number - 1] != 0);
        graphManager.DrawFinalConnections();
    }

    
}
