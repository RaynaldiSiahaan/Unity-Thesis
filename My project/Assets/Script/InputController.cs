using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class InputController : MonoBehaviour
{
    public GameObject inputPanelAPC; // Reference to the input panel
    public PlayerMovement playerMovement; // Reference to PlayerMovement script
    public int index;

    public GameObject inputPanelTerrain;
    private string input; // input string from the user
    public float speedKPH; // Speed in km/h
    void Start()
    {
        // Hide the input panel at the start
        if (inputPanelAPC != null && inputPanelTerrain != null)
        {
            inputPanelAPC.SetActive(false);
            inputPanelTerrain.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        index = playerMovement.CurrentPoint();

        // Show the input panel when "H" is pressed
        if (Input.GetKeyDown(KeyCode.H))
        {

            if (index == 1){
                bool isActive = inputPanelAPC.activeSelf; // Check if the panel is currently active
                inputPanelAPC.SetActive(!isActive); // Toggle the active state
                Debug.Log(isActive ? "Input panel for APC deactivated." : "Input panel activated.");
            }
            
            else if (index == 2){
                bool isActive = inputPanelTerrain.activeSelf; // Check if the panel is currently active
                inputPanelTerrain.SetActive(!isActive); // Toggle the active state
                Debug.Log(isActive ? "Input panel for Terrain deactivated." : "Input panel activated.");
            }
            
        }

    }
    public float getInput()
    {
        return speedKPH;
    }
}
