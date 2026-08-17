using System.Collections.Generic;
using UnityEditor.EventSystems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GraphManager : MonoBehaviour
{
    public GameObject nodePrefab;

    private int nodeCounter = 0;
    public List<Node> nodes = new List<Node>();
    public ConnectionInputUI inputUI;
    public List<GameObject> nodeObjects = new List<GameObject>();
    public List<GameObject> connectionObjects = new List<GameObject>();
    private PlayerInputActions inputActions;
    private NodeView draggingNode = null;
    private Camera cam;
    public Sprite arrowSprite;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        cam = Camera.main;
    }

    private void OnEnable()
    {
        inputActions.Enable();

       
        inputActions.Gameplay.Click.performed += OnLeftClick;

       
        inputActions.Gameplay.RightClick.started += OnRightClickStarted;
        inputActions.Gameplay.RightClick.canceled += OnRightClickEnded;
    }

    private void OnDisable()
    {
        inputActions.Gameplay.Click.performed -= OnLeftClick;
        inputActions.Gameplay.RightClick.started -= OnRightClickStarted;
        inputActions.Gameplay.RightClick.canceled -= OnRightClickEnded;
        inputActions.Disable();
    }

    private void OnLeftClick(InputAction.CallbackContext context)
    {
        if (inputUI != null && inputUI.panel.activeSelf)
            return;
        if (EventSystem.current.IsPointerOverGameObject(PointerInputModule.kMouseLeftId))
            return;
        Vector2 mousePos;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            mousePos = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else
        {
            mousePos = Mouse.current.position.ReadValue();
        }
        CreateNode(mousePos);
    }

    private void OnRightClickStarted(InputAction.CallbackContext context)
    {
        Vector2 mousePos;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            mousePos = Touchscreen.current.primaryTouch.position.ReadValue();
        else
            mousePos = Mouse.current.position.ReadValue();
        RaycastHit2D hit = Physics2D.Raycast(cam.ScreenToWorldPoint(mousePos), Vector2.zero);

        if (hit.collider != null)
        {
            NodeView node = hit.collider.GetComponent<NodeView>();
            if (node != null)
            {
                draggingNode = node;
                node.StartDrag(mousePos);
            }
        }
    }

    private void OnRightClickEnded(InputAction.CallbackContext context)
    {
        if (draggingNode != null)
        {
            draggingNode.EndDrag();
            draggingNode = null;
        }
    }

    private void CreateNode(Vector2 screenPosition)
    {
        Vector3 worldPos = cam.ScreenToWorldPoint(screenPosition);
        worldPos.z = 0f;

       
        float checkRadius = 0.5f; 
        Collider2D hit = Physics2D.OverlapCircle(worldPos, checkRadius);
        if (hit != null)
        {
            Debug.Log("Cannot create node here, space occupied!");
            return;
        }

        nodeCounter++;

        NodeType type = NodeType.Normal;
        if (nodes.Count == 0)
            type = NodeType.Source;
        if(nodes.Count==1)
            type=NodeType.Stock;

        Node newNode = new Node(nodeCounter, type);
        nodes.Add(newNode);
        if (nodes.Count > 2)
        {
            nodes[nodes.Count - 2].Type = NodeType.Normal;

            // Set the newest node as Stock
            newNode.Type = NodeType.Stock;
        }
        GameObject nodeObj = Instantiate(nodePrefab, worldPos, Quaternion.identity);
        nodeObjects.Add(nodeObj);
        nodeObj.GetComponent<NodeView>().Initialize(newNode);

        foreach(Node n in nodes)
        {
            Debug.Log(n.Number.ToString() + " " +n.Type.ToString());
        }
    }

    public void Clear()
    {
        nodes.Clear();
        foreach(GameObject obj in nodeObjects)
        {
            Destroy(obj);
        }
        foreach (GameObject obj in connectionObjects)
        {
            Destroy(obj);
        }
        nodeCounter = 0;
    }
    public void Reset()
    {
        foreach (Node node in nodes)
        {
            foreach (var edge in node.OutgoingConnections)
                edge.Flux = 0;
        }
        DrawFinalConnections();
    }
        public GameObject CreateResidualConnectionVisual(
    Transform from,
    Transform to,
    Sprite arrowSprite,
    int resCapacity)
    {
        GameObject connectionObj = new GameObject("Connection");

        ConnectionView view = connectionObj.AddComponent<ConnectionView>();
        view.from = from;
        view.to = to;

        GameObject arrowGO = new GameObject("ArrowSprite");
        arrowGO.transform.parent = connectionObj.transform;

        SpriteRenderer sr = arrowGO.AddComponent<SpriteRenderer>();
        sr.sprite = arrowSprite;
        sr.sortingOrder = 5;

        view.arrowSprite = sr;

        view.SetResidualValue(resCapacity);

        return connectionObj;
    }

    public void ClearConnections()
    {
        foreach (var obj in connectionObjects)
            Destroy(obj);

        connectionObjects.Clear();
    }
    public void DrawFinalConnections()
    {
        ClearConnections(); 

        foreach (var node in nodes)
        {
            foreach (var edge in node.OutgoingConnections)
            {
                // Creeazã vizualul muchiei finale
                GameObject obj = CreateFinalConnectionVisual(
                    nodeObjects[nodes.IndexOf(node)].transform,
                    nodeObjects[nodes.IndexOf(edge.To)].transform,
                    edge.Flux,
                    edge.Capacity
                );

                connectionObjects.Add(obj);
            }
        }
    }

    public GameObject CreateFinalConnectionVisual(
    Transform from,
    Transform to,
    int flux,
    int capacity)
    {
        GameObject connectionObj = new GameObject("FinalConnection");

        ConnectionView view = connectionObj.AddComponent<ConnectionView>();
        view.from = from;
        view.to = to;

        GameObject arrowGO = new GameObject("ArrowSprite");
        arrowGO.transform.parent = connectionObj.transform;

        SpriteRenderer sr = arrowGO.AddComponent<SpriteRenderer>();
        sr.sprite = arrowSprite;
        sr.sortingOrder = 5;

        view.arrowSprite = sr;

        view.SetValues(flux, capacity);

        return connectionObj;
    }


}