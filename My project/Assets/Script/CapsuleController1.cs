using UnityEngine;

public class CapsuleController1 : MonoBehaviour
{
    public float distance;
    public float horizontal;

    private string input1;
    private string input2;
    public GameObject inputPanel;
    public GameObject screen;

    // Start is called once before the first execution of Update after the MonoBehaviour is created


    public void CloseButton(){
            inputPanel.SetActive(false); // Hide the input panel after reading the input
            screen.SetActive(false); // Hide the input panel after reading the input
            Debug.Log("Input panel deactivated.");
    }

    public void ReadDistanceInput(string d)
    {
        Vector3 pos = transform.position;
        input1 = d;
        distance = float.Parse(input1) +11; // Convert string to float
        transform.position = new Vector3(pos.x, pos.y, distance); 
    }

    public void ReadHorizontalInput(string h)
    {
        Vector3 pos = transform.position;
        input2 = h;
        horizontal = float.Parse(input2) +37; // Convert string to float
        transform.position = new Vector3(horizontal, pos.y, pos.z);
    }


}
