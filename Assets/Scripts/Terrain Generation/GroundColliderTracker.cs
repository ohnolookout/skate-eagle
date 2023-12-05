using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GroundColliderTracker
{
    [SerializeField] private Dictionary<Rigidbody2D, int> bodyIndices;
    private List<GroundSegment> segmentList;
    private List<EdgeCollider2D> colliderList;
    private List<int> activeSegments = new();
    private GameObject backstop;

    public GroundColliderTracker(Rigidbody2D body, List<EdgeCollider2D> colliders, List<GroundSegment> segments, GameObject backstop, int startingIndex = 0)
    {
        bodyIndices = new() { { body, startingIndex } };
        segmentList = segments;
        colliderList = colliders;
        this.backstop = backstop;
    }

    public GroundColliderTracker(List<Rigidbody2D> bodies, List<EdgeCollider2D> colliders, List<GroundSegment> segments, GameObject backstop, int startingIndex = 0)
    {
        bodyIndices = new();
        foreach(var body in bodies)
        {
            bodyIndices[body] = startingIndex;
        }
        segmentList = segments;
        colliderList = colliders;
        this.backstop = backstop;
    }

    public void UpdateColliders()
    {
        List<int> toActivate = new();
        List<int> toDeactivate = new();
        foreach (var body in bodyIndices.Keys.ToList())
        {
            UpdateBodyIndex(body);
            //Update body index to make sure it represents the current segment 
            //If current colliders are not valid, update colliders.
            if (!ValidateCurrentColliders(body))
            {
                BuildActivateList(body, toActivate, toDeactivate);
            }
            //If current colliders are valid and there are multiple bodies being tracked,
            //add indices to the activate list so they will not be deactivated
            else if(bodyIndices.Count > 1)
            {
                AddCurrentIndicesToList(body, toActivate);
            }
        }
        if (toActivate.Count > 0 || toDeactivate.Count > 0)
        {
            ActivateColliders(activeSegments, toActivate, toDeactivate);
        }

    }

    //Evaluate whether to current active colliders are correct based on direction
    private bool ValidateCurrentColliders(Rigidbody2D body)
    {
        int bodyIndex = bodyIndices[body];
        //Return true if player is on the first or final segment and heading in the direction of that segment
        if (MovingForward(body) && bodyIndex >= colliderList.Count - 1 || (!MovingForward(body) && bodyIndex == 0))
        {
            return true;
        }
        //Return true if the segment in front of where the body is moving is active.
        if (colliderList[bodyIndex].isActiveAndEnabled &&
            ((MovingForward(body) && colliderList[bodyIndex + 1].isActiveAndEnabled) ||
            (!MovingForward(body) && colliderList[bodyIndex - 1].isActiveAndEnabled)))
        {
            return true;
        }
        //Return false if one of the above conditions are not met.
        return false;
    }
    private bool MovingForward(Rigidbody2D body)
    {
        return body.velocity.x >= 0;
    }

    //If the segment at the current player index doesn't contain the player within its x bounds,
    //Adjust the player's index until it does.
    private void UpdateBodyIndex(Rigidbody2D body)
    {
        while (!segmentList[bodyIndices[body]].ContainsX(body.position.x))
        {
            if (MovingForward(body))
            {
                bodyIndices[body]++;
            }
            else
            {
                bodyIndices[body]--;
            }
        }
    }

    private void BuildActivateList(Rigidbody2D body, List<int> activate, List<int> deactivate)
    {
        //Sets the index change value based on the body's direction
        int indexDirection = 1;
        bool forward = MovingForward(body);
        if (!forward)
        {
            indexDirection = -1;
        }
        if(segmentList[bodyIndices[body] - indexDirection].ContainsX(body.position.x)){
            bodyIndices[body] -= indexDirection;
        }
        //Adds current index and index ahead of current index to activate list.
        AddIfUnique(activate, bodyIndices[body]);
        AddIfUnique(activate, bodyIndices[body] + indexDirection);
        //segmentList[bodyIndices[body] + indexDirection].CollisionActive = true;
        //If birdIndex isn't at 0 or segment length, it deactives the preceding index.
        if ((bodyIndices[body] - indexDirection >= 0 && bodyIndices[body] - indexDirection <= colliderList.Count - 1))
        {
            AddIfUnique(deactivate, bodyIndices[body] - indexDirection);
            //segmentList[bodyIndices[body] - indexDirection].CollisionActive = false;
        }
    }

    //Add current and next index to the given list
    //Validate next index to ensure it is in range.
    private void AddCurrentIndicesToList(Rigidbody2D body, List<int> activate)
    {
        int indexDirection = 1;
        if (!MovingForward(body))
        {
            indexDirection = -1;
        }
        AddIfUnique(activate, bodyIndices[body]);
        if (bodyIndices[body] + indexDirection >= 0 && bodyIndices[body] + indexDirection <= colliderList.Count - 1) {
            AddIfUnique(activate, bodyIndices[body] + indexDirection);
        }
    }
    private void ActivateColliders(List<int> activated, List<int> toActivate, List<int> toDeactivate)
    {       
        //Activate any colliders in toActivate list that aren't already active
        foreach (var index in toActivate)
        {
            if (!activated.Contains(index))
            {
                colliderList[index].gameObject.SetActive(true);
                if(index == colliderList.Count - 1)
                {
                    backstop.SetActive(true);
                }
            }
        }
        //Deactivate any colliders not current in the toActivate list.
        foreach (var index in toDeactivate)
        {
            if (!toActivate.Contains(index))
            {
                colliderList[index].gameObject.SetActive(false);
            }
        }

        activated = toActivate;
    }

    private void AddIfUnique<T>(List<T> list, T value)
    {
        if (!list.Contains(value))
        {
            list.Add(value);
        }
    }

    public void ResetTrackedBodies()
    {
        bodyIndices = new();
    }

    public void RemoveBody(Rigidbody2D body)
    {
        if (bodyIndices.ContainsKey(body))
        {
            bodyIndices.Remove(body);
        }
    }

    public void AddBody(Rigidbody2D body, int startIndex)
    {
        bodyIndices.Add(body, startIndex);
    }

    public void SwapBodies(Rigidbody2D[] currentBodies, Rigidbody2D[] newBodies)
    {
        int currentBodyIndex = 0;
        if (currentBodies.Length > 0 && bodyIndices.ContainsKey(currentBodies[0])){
            currentBodyIndex = bodyIndices[currentBodies[0]];
        }
        foreach(var body in currentBodies)
        {
            RemoveBody(body);
        }
        foreach (var body in newBodies)
        {
            AddBody(body, currentBodyIndex);
        }

    }
}
