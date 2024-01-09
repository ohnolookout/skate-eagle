using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GroundColliderManager
{
    private Dictionary<Rigidbody2D, int> _bodyIndices;
    private List<EdgeCollider2D> _colliderList;
    private List<int> activeSegments = new();
    private GameObject backstop;
    private int containmentBuffer = 20;
    private List<Rigidbody2D> _normalBodies, _ragdollBodies;

    public List<EdgeCollider2D> ColliderList { get => _colliderList; }

    public GroundColliderManager(Rigidbody2D body, List<EdgeCollider2D> colliders,  GameObject backstop, int startingIndex = 0)
    {
        _bodyIndices = new() { { body, startingIndex } };
        _colliderList = colliders;
        this.backstop = backstop;
    }

    public GroundColliderManager(List<Rigidbody2D> normalBodies, List<Rigidbody2D> ragdollBodies, Terrain terrain, int startingIndex = 0)
    {
        LevelManager.OnGameOver += _ => SwitchToRagdoll();
        _normalBodies = normalBodies;
        _bodyIndices = BodyIndexDict(normalBodies, startingIndex);
        _ragdollBodies = ragdollBodies;
        _colliderList = terrain.ColliderList;
        backstop = terrain.Backstop;
    }

    public Dictionary<Rigidbody2D, int> BodyIndexDict(List<Rigidbody2D> bodies, int index)
    {
        Dictionary<Rigidbody2D, int> bodyDict = new();
        foreach (var body in bodies)
        {
            bodyDict[body] = index;
        }
        return bodyDict;
    }
    public void UpdateColliders()
    {
        List<int> toActivate = new();
        List<int> toDeactivate = new();
        foreach (var body in _bodyIndices.Keys.ToList())
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
            else if(_bodyIndices.Count > 1)
            {
                AddCurrentIndicesToList(body, toActivate);
            }
        }
        if (toActivate.Count > 0 || toDeactivate.Count > 0)
        {
            ActivateColliders(activeSegments, toActivate, toDeactivate);
        }

    }

    private void SwitchToRagdoll()
    {
        ReplaceBodies(_ragdollBodies);
    }

    //Evaluate whether to current active colliders are correct based on direction
    private bool ValidateCurrentColliders(Rigidbody2D body)
    {
        int bodyIndex = _bodyIndices[body];
        //Return true if player is on the first or final segment and heading in the direction of that segment
        if (MovingForward(body) && bodyIndex >= _colliderList.Count - 1 || (!MovingForward(body) && bodyIndex == 0))
        {
            return true;
        }
        //Return true if the segment in front of where the body is moving is active.
        if (_colliderList[bodyIndex].isActiveAndEnabled &&
            ((MovingForward(body) && _colliderList[bodyIndex + 1].isActiveAndEnabled) ||
            (!MovingForward(body) && _colliderList[bodyIndex - 1].isActiveAndEnabled)))
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
        while (!ColliderContainsBodyX(_colliderList[_bodyIndices[body]], body))
        {
            if (MovingForward(body))
            {
                _bodyIndices[body]++;
            }
            else
            {
                _bodyIndices[body]--;
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
        EdgeCollider2D nextCollider = _colliderList[_bodyIndices[body] - indexDirection];
        if (ColliderContainsBodyX(nextCollider, body)){
            _bodyIndices[body] -= indexDirection;
        }
        //Adds current index and index ahead of current index to activate list.
        AddIfUnique(activate, _bodyIndices[body]);
        AddIfUnique(activate, _bodyIndices[body] + indexDirection);
        //segmentList[bodyIndices[body] + indexDirection].CollisionActive = true;
        //If birdIndex isn't at 0 or segment length, it deactives the preceding index.
        if ((_bodyIndices[body] - indexDirection >= 0 && _bodyIndices[body] - indexDirection <= _colliderList.Count - 1))
        {
            AddIfUnique(deactivate, _bodyIndices[body] - indexDirection);
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
        AddIfUnique(activate, _bodyIndices[body]);
        if (_bodyIndices[body] + indexDirection >= 0 && _bodyIndices[body] + indexDirection <= _colliderList.Count - 1) {
            AddIfUnique(activate, _bodyIndices[body] + indexDirection);
        }
    }
    private void ActivateColliders(List<int> activated, List<int> toActivate, List<int> toDeactivate)
    {       
        //Activate any colliders in toActivate list that aren't already active
        foreach (var index in toActivate)
        {
            if (!activated.Contains(index))
            {
                _colliderList[index].gameObject.SetActive(true);
                if(index == _colliderList.Count - 1)
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
                _colliderList[index].gameObject.SetActive(false);
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
        _bodyIndices = new();
    }

    public void RemoveBody(Rigidbody2D body)
    {
        if (_bodyIndices.ContainsKey(body))
        {
            _bodyIndices.Remove(body);
        }
    }

    public void AddBody(Rigidbody2D body, int startIndex)
    {
        _bodyIndices.Add(body, startIndex);
    }

    public void SwapBodies(Rigidbody2D[] currentBodies, Rigidbody2D[] newBodies)
    {
        int currentBodyIndex = 0;
        if (currentBodies.Length > 0 && _bodyIndices.ContainsKey(currentBodies[0])){
            currentBodyIndex = _bodyIndices[currentBodies[0]];
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
    public void ReplaceBodies(List<Rigidbody2D> newBodies)
    {
        int currentBodyIndex;
        if (_bodyIndices.Count > 0) {
            currentBodyIndex = _bodyIndices.Values.ToList()[0];
            ResetTrackedBodies();
        }
        else
        {
            currentBodyIndex = 0;
        }
        foreach (var body in newBodies)
        {
            AddBody(body, currentBodyIndex);
        }
    }

    public bool ColliderContainsBodyX(EdgeCollider2D collider, Rigidbody2D body)
    {
        float targetX = body.position.x;
        return (targetX > collider.points[0].x - containmentBuffer 
            && targetX < collider.points[^1].x + containmentBuffer);
    }
}
