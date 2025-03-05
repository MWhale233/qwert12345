using Unity.Netcode;
using UnityEngine;

public class PreviewAvatarSerializer : NetworkBehaviour
{
    public GameObject previewAvatar;
    private NetworkVariable<bool> previewAvatarEnabled = new NetworkVariable<bool>(default, writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<Vector3> previewAvatarPosition = new NetworkVariable<Vector3>(default, writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<Quaternion> previewAvatarDirection = new NetworkVariable<Quaternion>(default, writePerm: NetworkVariableWritePermission.Owner);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    #region NetworkBehaviour Callbacks

    public override void OnNetworkSpawn()
    {
        previewAvatar.SetActive(false);

        if (IsOwner)
            return;
       
        ApplyPreviewUpdates();
    }

    #endregion

    #region MonoBehaviour Callbacks

    private bool SerializePreviewUpdates(out bool previewEnabled, out Vector3 previewPos, out Quaternion previewDir)
    {
        previewEnabled = false;
        previewPos = previewAvatar.transform.position;
        previewDir = previewAvatar.transform.rotation;
        if (previewAvatar.activeSelf)
        {
            previewEnabled = true;
            return true;
        }
        else if (!previewAvatar.activeSelf && previewAvatarEnabled.Value)
        {
            previewEnabled = false;
            return true;
        }
        
        return false;
    }

    private void ApplyPreviewUpdates()
    {
        previewAvatar.SetActive(previewAvatarEnabled.Value);
        if (previewAvatar.activeSelf)
        {
            previewAvatar.transform.position = previewAvatarPosition.Value;
            previewAvatar.transform.rotation = previewAvatarDirection.Value;
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            if (SerializePreviewUpdates(out bool enabled, out Vector3 pos, out Quaternion dir))
            {
                previewAvatarEnabled.Value = enabled;
                previewAvatarPosition.Value = pos;
                previewAvatarDirection.Value = dir;
            }
        }
        else
        {
            ApplyPreviewUpdates();
        }
    }

    #endregion
}
