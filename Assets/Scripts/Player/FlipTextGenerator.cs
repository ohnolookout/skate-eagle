using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlipTextGenerator : MonoBehaviour
{
    public GameObject popText;
    public float wordSpread = 10;
    [SerializeField] private EagleScript playerScript;
    [SerializeField] private FlipText flipText;
    private List<string> affirmations = new List<string> { "Rad!", "Woah.", "No way!", "Cool flip!", "Really?!", "Settle down...", "Dang!", "So hot!", "Wow, neat.", "Luv it." };

    void Awake()
    {
        playerScript.EndFlip += (_, flipCount) => NewFlipText(flipCount);
    }

    public void NewFlipText(int flipCount = 1)
    {
        Vector3 location = GenerateLocation(wordSpread);
        Vector3 viewportPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, playerScript.gameObject.transform.position + location);
        Vector3 finalPosition = viewportPosition + location;
        flipText.transform.position = finalPosition; 
        float randomZ = Random.Range(-45, 45);
        flipText.transform.eulerAngles = new Vector3(0, 0, randomZ);
        flipText.SetText(GenerateText());
        flipText.StartLifecycle();

    }


    private Vector3 GenerateLocation(float scale)
    {
        float xCoord;
        if(playerScript.Rigidbody.velocity.x >= 0)
        {
            xCoord = Random.Range(-scale, -scale/2);
        } else
        {
            xCoord = Random.Range(scale/2, scale);
        }
        float yCoord = Random.Range(scale * 0.5f, scale);
        return new Vector3(xCoord, yCoord);
    }

    

    private string GenerateText()
    {
        int index = Random.Range(1, affirmations.Count);
        string returnString = affirmations[index];
        MoveStringToFront(index);
        return returnString;
    }

    private void MoveStringToFront(int index) 
    {
        string lastString = affirmations[index];
        affirmations.RemoveAt(index);
        affirmations.Insert(0, lastString);
    }
}
