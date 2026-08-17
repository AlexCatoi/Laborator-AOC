using System.IO.IsolatedStorage;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class NodeView : MonoBehaviour
{
    public Node Data;
    public Sprite arrow;
    private bool isDragging = false;
    private LineRenderer templine;
    private GraphManager graphManager;

    private Camera cam;

    public void Awake()
    {
        cam = Camera.main;
    }

    public void Initialize(Node node)
    {
        this.Data = node;
        GetComponentInChildren<TMPro.TextMeshPro>().text = node.Number.ToString();
    }

    private void Start()
    {
        graphManager=FindFirstObjectByType<GraphManager>();
    }

    public void StartDrag(Vector2 screenPos)
    {
        if (templine != null) Destroy(templine.gameObject);

        isDragging = true;
        templine = new GameObject("Templine").AddComponent<LineRenderer>();
        templine.positionCount = 2;
        templine.startWidth = 0.05f;
        templine.endWidth = 0.05f;
        templine.material = new Material(Shader.Find("Sprites/Default"));
    }

    // End dragging (called from GraphManager right-click release)
    public void EndDrag()
    {
        isDragging = false;

        Vector2 mouseWorldPos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null)
        {
            NodeView targetNode = hit.collider.GetComponent<NodeView>();
            if (targetNode != null && targetNode != this)
            {
                CreateConnection(targetNode);
            }
        }

        if (templine != null) Destroy(templine.gameObject);
    }

    private void Update()
    {
        if (isDragging && templine != null)
        {
            Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mouseWorldPos.z = 0f;

            float startRadius = GetComponent<CircleCollider2D>().radius * transform.localScale.x;
            Vector3 startPos = GetPointOnCircleEdge(transform.position, mouseWorldPos, startRadius);

            templine.SetPosition(0, startPos);
            templine.SetPosition(1, mouseWorldPos);
        }
    }

    private void CreateConnection(NodeView target)
    {
        GameObject connectionObj = new GameObject("Connection");
        ConnectionView view = connectionObj.AddComponent<ConnectionView>();
        view.from = this.transform;
        view.to = target.transform;

        GameObject arrowGO = new GameObject("ArrowSprite");
        arrowGO.transform.parent = connectionObj.transform;
        SpriteRenderer sr = arrowGO.AddComponent<SpriteRenderer>();
        sr.sprite = arrow;
        sr.sortingOrder = 5;
        view.arrowSprite = sr;

        // Show the input UI
        ConnectionInputUI inputUI = FindFirstObjectByType<ConnectionInputUI>();
        inputUI.Show((flux, capacity) =>
        {
            Connection connection = new Connection(Data, target.Data, capacity);
            connection.Flux = flux;

            Data.OutgoingConnections.Add(connection);
            target.Data.IncomingConnections.Add(connection);

            view.SetValues(flux,capacity);
        });
        graphManager.connectionObjects.Add(connectionObj);
    }

    private Vector3 GetPointOnCircleEdge(Vector3 from, Vector3 to, float radius)
    {
        Vector3 direction = (to - from).normalized;
        return from + direction * radius;
    }
}
