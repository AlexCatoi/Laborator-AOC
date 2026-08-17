using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class AlgsWithTags : MonoBehaviour
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

    public void Ahuja_Orlin_Tags()
    {
        int flux = 0;
        List<int> p = new List<int>();
        List<int> d = new List<int>();
        Node source = originalNodes.First(n => n.Type == NodeType.Source);
        Node stock = originalNodes.First(n => n.Type == NodeType.Stock);
        foreach (Node node in originalNodes)
        {
            foreach(var edge in node.OutgoingConnections)
                edge.Flux = flux;
            p.Add(0);
            d.Add(0);
        }

        graph.BuildFrom(originalNodes);
        CalculateTags(d,stock);
        Node x = source;
        while (d[x.Number-1]<originalNodes.Count)
        {
            Connection edge = GetAdmissibleEdge(x, d);
            if(edge!=null)
            {
                Node y = edge.To;
                p[y.Number - 1] = x.Number - 1;
                x= y;
                if(x==stock)
                {
                    AugmentPath(p,source,stock);
                    graph.BuildFrom(originalNodes);
                    x = source;
                }
            }
            else
            {
                int min = int.MaxValue;
                foreach (var edge2 in x.OutgoingConnections)
                {
                    int residual = edge2.Capacity - edge2.Flux;

                    if (residual > 0)
                    {
                        int yIndex = edge2.To.Number - 1;
                        min = Mathf.Min(min, d[yIndex]);
                    }
                }

                if (min < int.MaxValue)
                    d[x.Number - 1] = min + 1;
                else
                    d[x.Number - 1] = int.MaxValue;

                if (x != source)
                {
                    x = originalNodes[p[x.Number - 1]];
                }
            }
        }
        graphManager.DrawFinalConnections();
    }

    private void CalculateTags(List<int> d,Node stock)
    {
        int n = originalNodes.Count;
        for (int i = 0; i < n; i++)
            d[i] = int.MaxValue;

        Queue<Node> queue = new Queue<Node>();

        d[stock.Number-1] = 0;
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
    Connection GetAdmissibleEdge(Node x, List<int> d)
    {
        int xIndex = x.Number - 1;

        foreach (var edge in x.OutgoingConnections)
        {
            int residual = edge.Capacity - edge.Flux;
            int yIndex = edge.To.Number - 1;

            if (residual > 0 && d[xIndex] == d[yIndex] + 1)
                return edge;
        }

        return null;
    }
    void AugmentPath(List<int> p, Node source, Node sink)
    {
        int flow = int.MaxValue;

        Node current = sink;

        while (current != source)
        {
            Node prev = originalNodes[p[current.Number - 1]];

            Connection edge = prev.OutgoingConnections
                .First(e => e.To == current);

            flow = Mathf.Min(flow, edge.Capacity - edge.Flux);

            current = prev;
        }

        current = sink;

        while (current != source)
        {
            Node prev = originalNodes[p[current.Number - 1]];

            Connection edge = prev.OutgoingConnections
                .First(e => e.To == current);

            edge.Flux += flow;

            current = prev;
        }
    }



    public void Ahuja_Orlin_Stratified()
    {
        int flux = 0;
        List<int> p = new List<int>();
        List<int> d = new List<int>();
        List<int> B = new List<int>();
        Node source = originalNodes.First(n => n.Type == NodeType.Source);
        Node stock = originalNodes.First(n => n.Type == NodeType.Stock);
        foreach (Node node in originalNodes)
        {
            foreach (var edge in node.OutgoingConnections)
                edge.Flux = flux;
            p.Add(0);
            d.Add(0);
            B.Add(0);
        }

        graph.BuildFrom(originalNodes);
        CalculateTags(d, stock);

        Node x = source;
        while (d[x.Number-1]<originalNodes.Count)
        {
            if (B[x.Number-1]==0)
            {
                Connection edge = GetAdmissibleEdge(x, d);
                if(edge!=null && B[edge.To.Number-1]==0)
                {
                    Node y = edge.To;
                    p[y.Number - 1] = x.Number - 1;
                    x = y;
                    if (x == stock)
                    {
                        AugmentPath(p, source, stock);
                        graph.BuildFrom(originalNodes);
                        x = source;
                    }
                }
                else
                {
                    B[x.Number - 1] = 1;
                    if (x != source)
                        x = originalNodes[p[x.Number - 1]];
                }
            }
            else
            {
                CalculateTags(d, stock);
                foreach (Node node in originalNodes)
                    B[node.Number - 1] = 0;
            }
        }
        graphManager.DrawFinalConnections();
    }
}
