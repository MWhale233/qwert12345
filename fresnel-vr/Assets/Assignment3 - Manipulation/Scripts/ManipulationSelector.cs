using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
public class ManipulationSelector : NetworkBehaviour
{
    #region Member Variables

    private NetworkVariable<bool> isGrabbed = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    #endregion

    #region Selector Methods
    private void Awake()
    {
        isGrabbed.OnValueChanged += OnIsGrabbedChanged;
    }

    private void OnIsGrabbedChanged(bool previousValue, bool newValue)
    {
        // When isGrabbed is changed，console log
        Debug.Log($"isGrabbed changed from {previousValue} to {newValue}");
    }

    public bool RequestGrab()
    {
        // TODO: your solution for excercise 3.8
        // check if object can be grabbed by a user
        // trigger ownership handling
        // trigger grabbed state update
        Debug.Log("======================================");
        Debug.Log("testing!!  RequestGrab");
        Debug.Log("isGrabbed.Value");
        Debug.Log(isGrabbed.Value);

        // detect if the object is been catched
        if (!isGrabbed.Value)
        {
            // if not catched
            // transfer owner
            Debug.Log("Owner?");
            Debug.Log(IsOwner);
            if (!IsOwner)
            {
                // if the client is not the owner, change owner to itself
                TransferOwnershipServerRpc(NetworkManager.Singleton.LocalClientId);
            }
            Debug.Log("IsServer?");
            Debug.Log(IsServer);

            // renew isGrabbed on server
            if (IsServer)
            {
                isGrabbed.Value = true;
            }
            else
            {
                // if it is not server, update isGrabbed through ServerRPC
                UpdateIsGrabbedServerRpc(true);
                Debug.Log("isGrabbed is trying to be updated through serverRPC");
            }
            
            Debug.Log("======================================");

            return true;
        }
            // false if the object is catched, return false
            return false; // <-- this is just a placeholder, determine the actual return value by your implemented policy
            
    }

    public void Release()
    {
        // TODO: your solution for excercise 3.8
        // use this function trigger a grabbed state update on object release
        // check if the object is catched
        if (isGrabbed.Value)
        {
            Debug.Log("============Release: grabbbed ==true entered================");
            // if catched
            if (IsOwner)
            {
                Debug.Log("Transfer owner back to server");
                // if it is owned by cliend, transfer owner back to server
                TransferOwnershipServerRpc(0); // 0 represent ClientId of server
            }

            // uodate isGrabbed on server
            if (IsServer)
            {
                isGrabbed.Value = false;
            }
            else
            {
                // if it is not server, update isGrabbed through ServerRPC
                UpdateIsGrabbedServerRpc(false);
            }
        }
    }

    #endregion

    #region RPCs

    // TODO: your solution for excercise 3.8
    // implement a rpc to transfer the ownership of an object 
    // implement a rpc to update the isGrabbed value

    // RPC to renew isGrabbed 
    [ServerRpc(RequireOwnership = false)]
    public void UpdateIsGrabbedServerRpc(bool newState)
    {
        isGrabbed.Value = newState;
    }

    // RPC to transfer ownership
    [ServerRpc(RequireOwnership = false)]
    public void TransferOwnershipServerRpc(ulong newOwnerId)
    {
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(newOwnerId))
        {
            // transfer ownership
            GetComponent<NetworkObject>().ChangeOwnership(newOwnerId);
        }
    }

    #endregion
}
