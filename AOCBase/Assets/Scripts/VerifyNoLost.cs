using Unity.VisualScripting;
using UnityEngine;

public class VerifyNoLost : MonoBehaviour
{
    GraphManager graphManager;

    private void Start()
    {
        graphManager = FindFirstObjectByType<GraphManager>();
    }
    private bool IsFlux()
    {
        Node source = graphManager.nodes[0];
        Node stock = graphManager.nodes[graphManager.nodes.Count-1];
        int output = 0;
        int input = 0;
        foreach(Connection c in source.OutgoingConnections)
        {
            output += c.Flux;
        }
        foreach (Connection c in stock.IncomingConnections)
        {
            input += c.Flux;
        }
        Debug.Log("din sursa "+output + " in stock " + input);
        if (output != input)
            return false;
        if (graphManager.nodes.Count > 2)
            for (int i = 1; i < graphManager.nodes.Count - 2; i++)
            {
                int s1 = 0;
                int s2 = 0;
                foreach (Connection c in graphManager.nodes[i].OutgoingConnections)
                {
                    s1 += c.Flux;
                }
                foreach (Connection c in graphManager.nodes[i].IncomingConnections)
                {
                    s2 += c.Flux;
                }
                int node = i + 1;
                Debug.Log("In\t " + node  + "intra\t " + s2 + "iese\t" + s1);
                if (s1 > s2)
                    return false;
            }
        return true;
    }

    public void Test()
    {
        if (IsFlux()) { Debug.Log("Este flux!"); }
        else
            Debug.Log("Nu e flux");
    }
}
