using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum TrackingBody { PlayerNormal, PlayerRagdoll, Board}
public class MomentumTracker
{
    private Dictionary<Rigidbody2D, Vector2> _lastVelocityDict;
    private Dictionary<TrackingBody, Rigidbody2D> _bodyDict;
    private float _everyXFrames = 1;

    public MomentumTracker(Rigidbody2D player, Rigidbody2D ragdollPlayer, Rigidbody2D board, int everyXFrames)
    {
        _lastVelocityDict = new();
        _bodyDict = new();
        _lastVelocityDict[player] = player.velocity;
        _lastVelocityDict[ragdollPlayer] = ragdollPlayer.velocity;
        _lastVelocityDict[board] = board.velocity;
        _bodyDict[TrackingBody.PlayerNormal] = player;
        _bodyDict[TrackingBody.PlayerRagdoll] = ragdollPlayer;
        _bodyDict[TrackingBody.Board] = board;
        _everyXFrames = everyXFrames;
    }

    public void Update()
    {
        if(Time.frameCount % _everyXFrames == 0)
        {
            foreach(var key in _lastVelocityDict.Keys.ToList())
            {
                _lastVelocityDict[key] = key.velocity;
            }
        }
    }

    public float MagnitudeDelta(TrackingBody trackingBody)
    {
        Rigidbody2D body = _bodyDict[trackingBody];
        return VectorChange(body).magnitude;
    }
    public Vector2 VectorChange (TrackingBody trackingBody)
    {
        Rigidbody2D body = _bodyDict[trackingBody];
        return VectorChange(body); 
    }
    public Vector2 VectorChange(Rigidbody2D body)
    {
        return new(body.velocity.x - _lastVelocityDict[body].x, body.velocity.y - _lastVelocityDict[body].y);
    }

    public Vector2 Velocity(TrackingBody trackingBody)
    {
        return _bodyDict[trackingBody].velocity;
    }
}
