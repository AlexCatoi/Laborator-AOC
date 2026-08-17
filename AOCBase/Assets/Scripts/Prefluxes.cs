using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Prefluxes : MonoBehaviour
{
    public ResidualGraph graph;
    public List<Node> originalNodes;
    GraphManager graphManager;
    GenericAlgorithm algorithm;
    private void Awake()
    {
        graph = new ResidualGraph();
        algorithm = FindFirstObjectByType<GenericAlgorithm>();
        graphManager = FindFirstObjectByType<GraphManager>();
        originalNodes = graphManager.nodes;
    }

    public void Generic_Preflux()
    {
        List<Node> actives = new List<Node>();
        List<int> d = new List<int>();
        List<int> excess = new List<int>();

        Node source = originalNodes.First(n => n.Type == NodeType.Source);
        Node stock = originalNodes.First(n => n.Type == NodeType.Stock);

        int n = originalNodes.Count;

        foreach (Node node in originalNodes)
        {
            foreach (var edge in node.OutgoingConnections)
                edge.Flux = 0;

            d.Add(0);
            excess.Add(0);
        }

        graph.BuildFrom(originalNodes);

        CalculateTags(d, stock);
        d[source.Number - 1] = n;

        foreach (var c in source.OutgoingConnections)
        {
            c.Flux = c.Capacity;

            int toIndex = c.To.Number - 1;
            int sourceIndex = source.Number - 1;

            excess[toIndex] += c.Capacity;
            excess[sourceIndex] -= c.Capacity;

            if (c.To != stock && !actives.Contains(c.To))
                actives.Add(c.To);
        }

        while (actives.Count > 0)
        {
            int random = UnityEngine.Random.Range(0, actives.Count);
            Node x = actives[random];
            int xIndex = x.Number - 1;

            ResidualConnection edge = GetAdmissibleEdge(x, d);

            if (edge != null)
            {
                int yIndex = edge.To.Number - 1;

                int augment = Mathf.Min(excess[xIndex], edge.ResidualCapacity);

                if (edge.IsForward)
                    edge.Original.Flux += augment;
                else
                    edge.Original.Flux -= augment;

                excess[xIndex] -= augment;
                excess[yIndex] += augment;

                if (edge.To != source && edge.To != stock && excess[yIndex] == augment)
                {
                    actives.Add(edge.To);
                }

                if (excess[xIndex] == 0)
                {
                    actives.Remove(x);
                }
                graph.BuildFrom(originalNodes);
            }
            else
            {
                int min = int.MaxValue;

                foreach (var e in graph.Connections.Where(e => e.From == x))
                {
                    if (e.ResidualCapacity > 0)
                    {
                        int yIndex = e.To.Number - 1;
                        min = Mathf.Min(min, d[yIndex]);
                    }
                }

                if (min < int.MaxValue)
                {
                    d[xIndex] = min + 1;
                }
                else
                {
                    actives.Remove(x);
                }
            }
        }
        graphManager.DrawFinalConnections();
        Debug.Log("Preflow finished!");
    }

    private void CalculateTags(List<int> d, Node stock)
    {
        int n = originalNodes.Count;
        for (int i = 0; i < n; i++)
            d[i] = int.MaxValue;

        Queue<Node> queue = new Queue<Node>();

        d[stock.Number - 1] = 0;
        queue.Enqueue(stock);

        while (queue.Count > 0)
        {
            Node current = queue.Dequeue();

            foreach (var edge in current.IncomingConnections)
            {
                Node prev = edge.From;

                int residual = edge.Capacity - edge.Flux;
                int prevIndex = prev.Number - 1;
                int currIndex = current.Number - 1;
                if (residual > 0 && d[prevIndex] == int.MaxValue)
                {
                    d[prevIndex] = d[currIndex] + 1;
                    queue.Enqueue(prev);
                }
            }
        }
    }
    ResidualConnection GetAdmissibleEdge(Node x, List<int> d)
    {
        int xIndex = x.Number - 1;

        foreach (var edge in graph.Connections.Where(e => e.From == x))
        {
            int yIndex = edge.To.Number - 1;

            if (edge.ResidualCapacity > 0 && d[xIndex] == d[yIndex] + 1)
                return edge;
        }

        return null;
    }


    public void Preflux_FIFO() 
    {
        Queue<Node> L = new Queue<Node>();
        List<int> d = new List<int>();
        List<int> excess = new List<int>();

        Node source = originalNodes.First(n => n.Type == NodeType.Source);
        Node stock = originalNodes.First(n => n.Type == NodeType.Stock);

        int n = originalNodes.Count;

        foreach (Node node in originalNodes)
        {
            foreach (var edge in node.OutgoingConnections)
                edge.Flux = 0;

            d.Add(0);
            excess.Add(0);
        }

        graph.BuildFrom(originalNodes);

        CalculateTags(d, stock);
        d[source.Number - 1] = n;

        foreach (var c in source.OutgoingConnections)
        {
            c.Flux = c.Capacity;

            int toIndex = c.To.Number - 1;
            int sourceIndex = source.Number - 1;

            excess[toIndex] += c.Capacity;
            excess[sourceIndex] -= c.Capacity;

            if (c.To != stock && !L.Contains(c.To))
                L.Enqueue(c.To);
        }

        while (L.Count>0)
        {
            Node x = L.Dequeue();
            ResidualConnection edge = GetAdmissibleEdge(x, d);
            while (excess[x.Number-1]>0 && edge!=null)
            {
                int yIndex = edge.To.Number - 1;

                int augment = Mathf.Min(excess[x.Number-1], edge.ResidualCapacity);

                if (edge.IsForward)
                    edge.Original.Flux += augment;
                else
                    edge.Original.Flux -= augment;

                excess[x.Number-1] -= augment;
                excess[yIndex] += augment;

                if (edge.To != source && edge.To != stock && !L.Contains(edge.To))
                {
                    L.Enqueue(edge.To);
                }
                graph.BuildFrom(originalNodes);
                edge = GetAdmissibleEdge(x, d);
            }
            if (excess[x.Number - 1] > 0)
            {
                int min = int.MaxValue;

                foreach (var e in graph.Connections.Where(e => e.From == x))
                {
                    if (e.ResidualCapacity > 0)
                    {
                        int yIndex = e.To.Number - 1;
                        min = Mathf.Min(min, d[yIndex]);
                    }
                }

                if (min < int.MaxValue)
                {
                    d[x.Number - 1] = min + 1;
                }
                L.Enqueue(x);
            }
        }
        graphManager.DrawFinalConnections();

    }


    public void Preflux_maxTag()
    {
        PriorityQueue<Node> L = new PriorityQueue<Node>();
        List<int> d = new List<int>();
        List<int> excess = new List<int>();

        Node source = originalNodes.First(n => n.Type == NodeType.Source);
        Node stock = originalNodes.First(n => n.Type == NodeType.Stock);

        int n = originalNodes.Count;

        foreach (Node node in originalNodes)
        {
            foreach (var edge in node.OutgoingConnections)
                edge.Flux = 0;

            d.Add(0);
            excess.Add(0);
        }

        graph.BuildFrom(originalNodes);

        CalculateTags(d, stock);
        d[source.Number - 1] = n;

        foreach (var c in source.OutgoingConnections)
        {
            c.Flux = c.Capacity;

            int toIndex = c.To.Number - 1;
            int sourceIndex = source.Number - 1;

            excess[toIndex] += c.Capacity;
            excess[sourceIndex] -= c.Capacity;

            if (c.To != stock && !L.Contains(c.To))
                L.Enqueue(c.To, d[c.To.Number-1]);
        }

        while (L.Count > 0)
        {
            Node x = L.Dequeue();
            ResidualConnection edge = GetAdmissibleEdge(x, d);
            while (excess[x.Number - 1] > 0 && edge != null)
            {
                int yIndex = edge.To.Number - 1;

                int augment = Mathf.Min(excess[x.Number - 1], edge.ResidualCapacity);

                if (edge.IsForward)
                    edge.Original.Flux += augment;
                else
                    edge.Original.Flux -= augment;

                excess[x.Number - 1] -= augment;
                excess[yIndex] += augment;

                if (edge.To != source && edge.To != stock && !L.Contains(edge.To))
                {
                    L.Enqueue(edge.To, d[edge.To.Number - 1]);
                }
                graph.BuildFrom(originalNodes);
                edge = GetAdmissibleEdge(x, d);
            }
            if (excess[x.Number - 1] > 0)
            {
                int min = int.MaxValue;

                foreach (var e in graph.Connections.Where(e => e.From == x))
                {
                    if (e.ResidualCapacity > 0)
                    {
                        int yIndex = e.To.Number - 1;
                        min = Mathf.Min(min, d[yIndex]);
                    }
                }

                if (min < int.MaxValue)
                {
                    d[x.Number - 1] = min + 1;
                }
                L.Enqueue(x, d[x.Number - 1]);
            }
        }
        graphManager.DrawFinalConnections();

    }
}
