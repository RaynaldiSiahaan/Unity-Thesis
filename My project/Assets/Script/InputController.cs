using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class InputController : MonoBehaviour
{
    public GameObject inputPanelAPC; // Reference to the input panel
    public PlayerMovement playerMovement; // Reference to PlayerMovement script
    public int index;
    public Camera cam;
    public GameObject screen;
    public GameObject inputPanelJarak; // Reference to the input panel
    public GameObject inputPanelTerrain;
    private string input; // input string from the user
    public float speedKPH; // Speed in km/h
    void Start()
    {

            inputPanelAPC.SetActive(false);
            inputPanelTerrain.SetActive(false);
            inputPanelJarak.SetActive(false);
            screen.SetActive(false);
            cam.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        index = playerMovement.CurrentPoint();

        // Show the input panel when "H" is pressed
        if (Input.GetKeyDown(KeyCode.H))
        {

            if (index == 0){
                cam.enabled = true;
                bool isActive1 = screen.activeSelf; // Check if the panel is currently active
                bool isActive2 = inputPanelJarak.activeSelf; // Check if the panel is currently active
                screen.SetActive(!isActive1); // Toggle the active state
                inputPanelJarak.SetActive(!isActive2); // Toggle the active state
            }
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
