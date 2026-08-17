using System.Collections.Generic;
using UnityEngine;

public class GraphViewController : MonoBehaviour
{
    private GraphManager graphManager;
    private ResidualGraph residualGraph;

    private List<GameObject> residualObjects = new List<GameObject>();

    private void Awake()
    {
        graphManager = FindFirstObjectByType<GraphManager>();
    }

    // This is called from the Toggle
    public void OnToggleResidual(bool showResidual)
    {
        if (showResidual)
            ShowResidualGraph();
        else
            ShowNormalGraph();
    }

    private void ShowResidualGraph()
    {
        HideNormalGraph();

        residualGraph = new ResidualGraph();
        residualGraph.BuildFrom(graphManager.nodes);

        BuildResidualVisual();
    }

    private void ShowNormalGraph()
    {
        HideResidualGraph();

        foreach (GameObject obj in graphManager.connectionObjects)
            obj.SetActive(true);
    }

    private void HideNormalGraph()
    {
        foreach (GameObject obj in graphManager.connectionObjects)
            obj.SetActive(false);
    }

    private void HideResidualGraph()
    {
        foreach (GameObject obj in residualObjects)
        {
            Destroy(obj);
        }

        residualObjects.Clear();
    }

    private void BuildResidualVisual()
    {
        foreach (ResidualConnection edge in residualGraph.ConnectionsForDisplay)
        {
            if (edge.ResidualCapacity <= 0)
                continue;

            Transform from = GetNodeTransform(edge.From);
            Transform to = GetNodeTransform(edge.To);

            if (from == null || to == null)
                continue;

            // Get arrow sprite from any node
            Sprite arrowSprite = graphManager.nodeObjects[0]
                .GetComponent<NodeView>().arrow;

            GameObject obj = graphManager.CreateResidualConnectionVisual(
                from,
                to,
                arrowSprite,
                edge.ResidualCapacity  
            );

            residualObjects.Add(obj);
        }
    }

    private Transform GetNodeTransform(Node node)
    {
        foreach (GameObject obj in graphManager.nodeObjects)
        {
            NodeView view = obj.GetComponent<NodeView>();
            if (view != null && view.Data == node)
                return view.transform;
        }

        return null;
    }
}