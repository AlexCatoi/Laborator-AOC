using UnityEngine;
using TMPro;

public class ConnectionView : MonoBehaviour
{
    public Transform from;
    public Transform to;
    public SpriteRenderer arrowSprite;

    public TMP_Text labelText; // reference to TMP text

    private float fromRadius = 0.5f;
    private float toRadius = 0.5f;

    public int flux;
    public int capacity;
    public int residualCapacity;

    void Start()
    {
        if (from.TryGetComponent<CircleCollider2D>(out var fc))
            fromRadius = fc.radius * from.localScale.x;
        if (to.TryGetComponent<CircleCollider2D>(out var tc))
            toRadius = tc.radius * to.localScale.x;
        if (labelText != null)
        {
            Destroy(labelText);
            labelText = null;
        }
        // Create 3D world-space TMP label at runtime
        if (labelText == null)
        {
            GameObject labelGO = new GameObject("ConnectionLabel");
            labelGO.transform.SetParent(transform);

            // Add TextMeshPro component (world-space)
            labelText = labelGO.AddComponent<TextMeshPro>();
            labelText.text = "";
            labelText.fontSize = 3;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = Color.black;

            // Add a MeshRenderer so it appears in the scene
            if (labelGO.GetComponent<MeshRenderer>() == null)
                labelGO.AddComponent<MeshRenderer>();
        }
        if (capacity > 0)
            UpdateLabel();
        else if(residualCapacity>0)
            UpdateResidualLabel();
    }

    void Update()
    {
        if (from == null || to == null || arrowSprite == null) return;

        Vector3 startPos = GetPointOnCircleEdge(from.position, to.position, fromRadius);
        Vector3 endPos = GetPointOnCircleEdge(to.position, from.position, toRadius);

        
        arrowSprite.transform.position = startPos;
        Vector3 direction = endPos - startPos;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        arrowSprite.transform.rotation = Quaternion.Euler(0, 0, angle);

        float spriteWidth = arrowSprite.sprite.bounds.size.x;
        Vector3 scale = arrowSprite.transform.localScale;
        scale.x = direction.magnitude / spriteWidth;
        scale.y = 0.2f;
        arrowSprite.transform.localScale = scale;

        
        if (labelText != null)
        {
            Vector3 midPoint = (startPos + endPos) / 2f;

            // Perpendicular offset
            Vector3 direction1 = (endPos - startPos).normalized;
            Vector3 perpendicular = new Vector3(-direction1.y, direction1.x, 0); 
            float offsetAmount = 0.3f;
            midPoint += perpendicular * offsetAmount;

            labelText.transform.position = midPoint;

            // Keep label readable (optional: flip if upside down)
            float readableAngle = Mathf.Atan2(direction1.y, direction1.x) * Mathf.Rad2Deg;
            if (readableAngle > 90f || readableAngle < -90f) readableAngle += 180f;
            labelText.transform.rotation = Quaternion.Euler(0, 0, readableAngle);
        }
    }

    public void SetValues(int flux, int capacity)
    {
        this.flux = flux;
        this.capacity = capacity;
        UpdateLabel();
    }

    public void SetResidualValue(int residualCapacity)
    {
        this.residualCapacity = residualCapacity;
        UpdateResidualLabel();
    }
    private void UpdateLabel()
    {
        if (labelText != null)
            labelText.text = $"{flux}, {capacity}";
    }
    private void UpdateResidualLabel()
    {
        if (labelText != null)
        {
            labelText.text = $"{residualCapacity}";
        }
    }
    private Vector3 GetPointOnCircleEdge(Vector3 from, Vector3 to, float radius)
    {
        return from + (to - from).normalized * radius;
    }
}