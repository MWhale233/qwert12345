using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HandCollider : MonoBehaviour
{
    #region Member Variables

    public bool isColliding { get; private set; } = false;
    public GameObject collidingObject { get; private set; }

    #endregion

    #region MonoBehaviour Callbacks

    private void OnTriggerEnter(Collider other)
    {
        if (!isColliding)
        {
            isColliding = true;
            collidingObject = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isColliding && other.gameObject == collidingObject)
        {
            collidingObject = null;
            isColliding = false;
        }
    }

    #endregion
}
