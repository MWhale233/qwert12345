using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class Homer : MonoBehaviour
{
    #region Member Variables

    [Header("H.O.M.E.R. Components")] 
    public Transform head;
    public float originHeadOffset = 0.2f;
    public Transform hand;

    [Header("H.O.M.E.R. Parameters")] 
    public LineRenderer ray;
    public float rayMaxLength = 100f;
    public LayerMask layerMask; // use this mask to raycast only for interactable objects
    
    [Header("Input Actions")] 
    public InputActionProperty grabAction;

    [Header("Grab Configuration")]
    public HandCollider handCollider;

    // grab calculation variables
    private GameObject grabbedObject;
    private Matrix4x4 offsetMatrix;

    //
    private Vector3 rayDirection;
    
    // utility bool to check if you can grab an object  
    private bool canGrab
    {
        get
        {
            if (handCollider.isColliding)
                return handCollider.collidingObject.GetComponent<ManipulationSelector>().RequestGrab();
            return false;
        }
    }
    
    // variables needed for hand offset calculation
    private RaycastHit hit;

    
    // convenience variables for hand offset calculations
    private Vector3 origin
    {
        get
        {
            Vector3 v = head.position;
            v.y -= originHeadOffset;
            return v;
        }
    }
    private Vector3 direction => hand.position - origin;

    #endregion

    #region MonoBehaviour Callbacks
    private Vector3 currentHitWorldPoint;
    private ActionBasedController controller;

    private void Awake()
    {
        ray.enabled = enabled;
        
    }

    private void Start()
    {
        if(GetComponentInParent<NetworkObject>() != null)
            if (!GetComponentInParent<NetworkObject>().IsOwner)
            {
                Destroy(this);
                return;
            }

        ray.positionCount = 2;
    }

    private void Update()
    {
        if (grabbedObject == null)
            UpdateRay();
        else
            ApplyHandOffset();
            
        GrabCalculation();
        
    }

    #endregion

    #region Custom Methods
    // add new variables
    private float scalingFactor; // factor for scaling
    private float grabOffsetDistance;
    private float grabHandDistance;
    private void UpdateRay()
    {
        //TODO: your solution for excercise 3.5
        // use this function to calculate and adjust the ray of the h.o.m.e.r. technique

        // set the start point of the ray
        ray.SetPosition(0, hand.position);
        
        // the direction of the ray
        rayDirection = direction.normalized * rayMaxLength;
        
        // set the end point to maximum
        ray.SetPosition(1, hand.position + rayDirection);

        // detect if the ray hit the object
        if (Physics.Raycast(hand.position, rayDirection, out hit, rayMaxLength, layerMask))
        {

            // renew the end point of the ray
            ray.SetPosition(1, hit.point);

            // change ray color to red
            ray.startColor = Color.red;
            ray.endColor = Color.red;

        }
        else
        {
            // if no intersection, change ray color to white
            ray.startColor = Color.white;
            ray.endColor = Color.white;
        }
    }

    private void ApplyHandOffset()
    {
        //TODO: your solution for excercise 3.5
        // use this function to calculate and adjust the hand as described in the h.o.m.e.r. technique

        Vector3 handToOriginVector = hand.position - origin;

        // Scale the vector by the scalingFactor.
        Vector3 scaledHandMovement = handToOriginVector * scalingFactor;

        // The new position is the origin plus the scaled vector.
        transform.position = origin + scaledHandMovement;

    }
    private bool RequestResult;

    private void GrabCalculation()
    {
        // TODO: your solution for excercise 3.5
        // use this function to calculate the grabbing of an object
        if (grabAction.action.IsPressed())
        {

            if (grabbedObject == null && Physics.Raycast(hand.position, rayDirection, out hit, rayMaxLength, layerMask)){
                
                grabHandDistance = Vector3.Distance(hand.position, origin);
                // when the grab button is pushed, move hand to hit point
                transform.position = hit.point; 
                

            }

            if (grabbedObject == null && handCollider.isColliding && canGrab)
            {
                grabbedObject = handCollider.collidingObject;


                HighlightObject(grabbedObject);

                // calculate initial object to body distance
                grabOffsetDistance = Vector3.Distance(grabbedObject.transform.position, origin);

                // calculate scalingFactor
                scalingFactor = grabOffsetDistance / grabHandDistance;

                // Calculate the world space transformation of the grabbed object
                Matrix4x4 objectWorldMatrix = GetTransformationMatrix(grabbedObject.transform);

                // Calculate the inverse of the world space transformation of the hand
                Matrix4x4 handInverseMatrix = GetTransformationMatrix(transform).inverse;

                // Calculate the offset in world space
                offsetMatrix = handInverseMatrix * objectWorldMatrix;


                
            }
            if (grabbedObject != null)
            {
                Matrix4x4 currentMatrix = GetTransformationMatrix(transform) * offsetMatrix;
                grabbedObject.transform.position = currentMatrix.GetColumn(3);
                grabbedObject.transform.rotation = Quaternion.LookRotation(currentMatrix.GetColumn(2), currentMatrix.GetColumn(1));
            }
        }
        else if (grabAction.action.WasReleasedThisFrame())
        {
            transform.position = hand.position; // move the hand model to the position of right controller
            if (grabbedObject != null)
            {
                grabbedObject.GetComponent<ManipulationSelector>().Release();
                grabbedObject = null;
                ClearHighlight();
            }
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
