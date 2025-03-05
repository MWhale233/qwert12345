using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class GoGo : MonoBehaviour
{
    #region Member Variables

    [Header("Go-Go Components")] 
    public Transform head;
    public float originHeadOffset = 0.2f;
    public Transform hand;

    [Header("Go-Go Parameters")] 
    public float distanceThreshold;
    [Range(0, 30)] public float k;
    
    [Header("Input Actions")] 
    public InputActionProperty grabAction;
    
    [Header("Grab Configuration")]
    public HandCollider handCollider;
    
    // calculation variables
    private GameObject grabbedObject;
    private Matrix4x4 offsetMatrix;
    
    private bool canGrab
    {
        get
        {
            if (handCollider.isColliding)
                return handCollider.collidingObject.GetComponent<ManipulationSelector>().RequestGrab();
            return false;
        }
    }

    #endregion

    #region MonoBehaviour Callbacks

    private void Start()
    {
        if(GetComponentInParent<NetworkObject>() != null)
            if (!GetComponentInParent<NetworkObject>().IsOwner)
            {
                Destroy(this);
                return;
            }
    }

    private void Update()
    {
        ApplyHandOffset();
        GrabCalculation();
    }

    #endregion

    #region Custom Methods

    private void ApplyHandOffset()
    {
        // TODO: your solution for excercise 3.6
        // use this function to calculate and apply the hand displacement according to the go-go technique
        // Calculate the real-world distance between the user and the hand
        float realWorldDistance = Vector3.Distance(hand.position, head.position);

        // Apply isomorphic mapping if within the threshold
        if (realWorldDistance > distanceThreshold)
        {
            // Apply non-isomorphic mapping if beyond the threshold
            float virtualDistance = distanceThreshold + k * Mathf.Pow((realWorldDistance - distanceThreshold), 2);
            transform.position = head.position + (hand.position - head.position).normalized * virtualDistance;
        }
    }

    private void GrabCalculation()
    {
        // TODO: your solution for excercise 3.6
        // use this function to calculate the grabbing of an object
        if (grabAction.action.IsPressed())
        {
            // Attempt to grab an object if the hand is colliding and can grab
            if (canGrab && grabbedObject == null)
            {
                grabbedObject = handCollider.collidingObject;
                // Highlight the object's material
                HighlightObject(grabbedObject);

                // Calculate the world space transformation of the grabbed object
                Matrix4x4 objectWorldMatrix = GetTransformationMatrix(grabbedObject.transform);

                // Calculate the inverse of the world space transformation of the hand
                Matrix4x4 handInverseMatrix = GetTransformationMatrix(transform).inverse;

                // Calculate the offset in world space
                offsetMatrix = handInverseMatrix * objectWorldMatrix;
            }

            // If an object is grabbed, apply the offset to move and rotate it with the hand
            if (grabbedObject != null)
            {
                // Transform the grabbed object by the offset matrix to follow the hand's movement
                Matrix4x4 currentMatrix = GetTransformationMatrix(transform) * offsetMatrix;
                grabbedObject.transform.position = currentMatrix.GetColumn(3);
                grabbedObject.transform.rotation = Quaternion.LookRotation(currentMatrix.GetColumn(2), currentMatrix.GetColumn(1));
            }
        }
        else if (grabAction.action.WasReleasedThisFrame() && grabbedObject != null)
        {
            // Release the object and unhighlight its material
            grabbedObject.GetComponent<ManipulationSelector>().Release();
            ClearHighlight();
            grabbedObject = null;
        }
    }

    #endregion

    #region Utility Functions

    public Matrix4x4 GetTransformationMatrix(Transform t, bool inWorldSpace = true)
    {
        if (inWorldSpace)
        {
            return Matrix4x4.TRS(t.position, t.rotation, t.lossyScale);
        }
        else
        {
            return Matrix4x4.TRS(t.localPosition, t.localRotation, t.localScale);
        }
    }
    public Material outlineMaterial; // set new material
    private Material originalMaterial; // save original material
    private Renderer targetRenderer; // get renderer

    void HighlightObject(GameObject obj)
    {
        targetRenderer = obj.GetComponent<Renderer>();
        originalMaterial = targetRenderer.material;

        // apply new material(red)
        targetRenderer.material = outlineMaterial;
    }

    void ClearHighlight()
    {
        if(targetRenderer != null)
            targetRenderer.material = originalMaterial;
    }
    #endregion
}
