using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class FlipTextGenerator : MonoBehaviour
{
    public GameObject popText;
    public float wordSpread = 10;
    private FlipText flipText;
    private Action<LiveRunManager> cancelText;
    private Action<IPlayer, double> newFlipText;
    private List<string> affirmations = new List<string> { "Rad!", "Woah.", "No way!", "Cool flip!", "Really?!", "Settle down...", "Dang!", "So hot!", "Wow, neat.", "Luv it." };

    private void Awake()
    {
        flipText = transform.GetChild(0).gameObject.GetComponent<FlipText>();
    }

    private void OnEnable()
    {
        IPlayer.OnFlip += NewFlipText;
        cancelText += _ => CancelText();
        LiveRunManager.OnGameOver += cancelText;
        
    }
    private void OnDisable()
    {
        IPlayer.OnFlip -= newFlipText;
        LiveRunManager.OnGameOver -= cancelText;
    }
    public void NewFlipText(IPlayer player, double flipCount = 1)
    {
        Vector3 location = GenerateLocation(player, wordSpread);
        Vector3 viewportPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, player.Rigidbody.position + (Vector2) location);
        Vector3 finalPosition = viewportPosition + location;
        flipText.transform.position = finalPosition; 
        float randomZ = UnityEngine.Random.Range(-45, 45);
        flipText.transform.eulerAngles = new Vector3(0, 0, randomZ);
        flipText.SetText(GenerateText());
        flipText.StartLifecycle();

    }


    public void CancelText()
    {
        if(!flipText.Canceled)
        {
            flipText.CancelText();
        }
    }

    private Vector3 GenerateLocation(IPlayer player, float scale)
    {
        float xCoord;
        if(player.Rigidbody.velocity.x >= 0)
        {
            xCoord = UnityEngine.Random.Range(-scale, -scale/2);
        } else
        {
            xCoord = UnityEngine.Random.Range(scale/2, scale);
        }
        float yCoord = UnityEngine.Random.Range(scale * 0.5f, scale);
        return new Vector3(xCoord, yCoord);
    }

    

    private string GenerateText()
    {
        int index = UnityEngine.Random.Range(1, affirmations.Count);
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
