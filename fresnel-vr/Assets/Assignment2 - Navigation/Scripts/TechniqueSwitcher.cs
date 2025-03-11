using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using TMPro;

public class TechniqueSwitcher : MonoBehaviour
{
    public InputActionProperty switchAction;

    public TMP_Text currentTechniqueText;
    public string currentText = "Steering";

    public SteeringNavigation steeringNavigation;
    public TeleportNavigation teleportNavigation;
    public UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals.XRInteractorLineVisual lineVisual;

    // Start is called before the first frame update
    void Start()
    {
        if (steeringNavigation == null)
        {
            steeringNavigation = GetComponent<SteeringNavigation>();
        }
        if (teleportNavigation == null)
        {
            teleportNavigation = GetComponent<TeleportNavigation>();
        }
        steeringNavigation.enabled = true;
        teleportNavigation.enabled = false;
        lineVisual.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (switchAction.action.WasPressedThisFrame())
        {
            if (currentText == "Steering")
            {
                currentText = "Teleport";
                currentTechniqueText.text = currentText;
                steeringNavigation.enabled = false;
                teleportNavigation.enabled = true;
                lineVisual.enabled = false;
            } 
            else if (currentText == "Teleport")
            {
                currentText = "Steering";
                currentTechniqueText.text = currentText;
                steeringNavigation.enabled = true;
                teleportNavigation.enabled = false;
                //lineVisual.enabled = true;
            }
        }
    }
}
