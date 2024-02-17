using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum TrackingType { PlayerNormal, PlayerRagdoll, Board}
public class MomentumTracker
{
    private Dictionary<Rigidbody2D, Vector2> _lastVelocityDict;
    private Dictionary<TrackingType, Rigidbody2D> _bodyDict;
    private float _everyXFrames = 1;
    int _frameCounter = 0;

    public MomentumTracker(Rigidbody2D player, Rigidbody2D ragdollPlayer, Rigidbody2D board, int everyXFrames)
    {
        _lastVelocityDict = new();
        _bodyDict = new();
        _lastVelocityDict[player] = player.velocity;
        _lastVelocityDict[ragdollPlayer] = ragdollPlayer.velocity;
        _lastVelocityDict[board] = board.velocity;
        _bodyDict[TrackingType.PlayerNormal] = player;
        _bodyDict[TrackingType.PlayerRagdoll] = ragdollPlayer;
        _bodyDict[TrackingType.Board] = board;
        _everyXFrames = everyXFrames;
    }

    public void Update()
    {
        _frameCounter++;
        if(_frameCounter % _everyXFrames == 0)
        {
            foreach(var key in _lastVelocityDict.Keys.ToList())
            {
                _lastVelocityDict[key] = key.velocity;
            }
            _frameCounter = 0;
        }
    }

    public float ReboundMagnitudeFromBody(Rigidbody2D inputBody, TrackingType trackingType)
    {
        float mag = VectorChangeFromBody(inputBody, trackingType).magnitude;
        return mag;
    }
    public Vector2 VectorChangeFromBody(Rigidbody2D inputBody, TrackingType trackingType)
    {
        Rigidbody2D trackingBody = _bodyDict[trackingType];
        return inputBody.velocity - _lastVelocityDict[trackingBody];
    }

    public float ReboundMagnitude(TrackingType trackingBody)
    {
        Rigidbody2D body = _bodyDict[trackingBody];
        return VectorChange(body).magnitude;
    }
    public float CalculateRebound(Vector2 inputVector, Vector2 deltaVector)
    {
        float xRebound, yRebound;
        if ((deltaVector.x > 0 && inputVector.x > 0) || (deltaVector.x < 0 && inputVector.x < 0))
        {
            xRebound = 0;
        }
        else
        {
            xRebound = deltaVector.x;
        }
        if ((deltaVector.y > 0 && inputVector.y > 0) || (deltaVector.y < 0 && inputVector.y < 0))
        {
            yRebound = 0;
        }
        else
        {
            yRebound = deltaVector.y;
        }
        return Mathf.Abs(xRebound + yRebound);
    }
    public Vector2 VectorChange (TrackingType trackingBody)
    {
        Rigidbody2D body = _bodyDict[trackingBody];
        return VectorChange(body); 
    }
    public Vector2 VectorChange(Rigidbody2D body)
    {
        return body.velocity - _lastVelocityDict[body];
    }

    public Vector2 Velocity(TrackingType trackingBody)
    {
        return _bodyDict[trackingBody].velocity;
    }
}
