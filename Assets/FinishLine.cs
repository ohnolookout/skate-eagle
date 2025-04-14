using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLine : MonoBehaviour
{
    [SerializeField] private GameObject _flag;
    [SerializeField] private GameObject _backstop;
    private Vector2 _flagPosition;
    private Vector2 _backstopPosition;
    private bool _checkPlayer = false;
    private IPlayer _player;
    private Rigidbody2D _playerBody;
    public Action DoFinish;
    private const float _upperYTolerance = 10;
    private const float _lowerYTolerance = 2f;

    public Vector2 FlagPosition => _flagPosition;
    public Vector2 BackstopPosition => _backstopPosition;
    public GameObject Backstop => _backstop;
    public GameObject Flag => _flag;

    void Awake()
    {
        LevelManager.OnPlayerCreated += AddPlayer;
    }
    void Update()
    {
        if(_playerBody.position.x > _flagPosition.x && _playerBody.position.x < _backstopPosition.x)
        {
            if(_playerBody.position.y > _flagPosition.y - _lowerYTolerance && _playerBody.position.y < _flagPosition.y + _upperYTolerance)
            {
                if (_player.CollisionManager.BothWheelsCollided)
                {
                    DoFinish?.Invoke();
                }
            }

        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        var lowerLeftPoint = new Vector2(_flagPosition.x, _flagPosition.y - _lowerYTolerance);
        var upperLeftPoint = new Vector2(_flagPosition.x, _flagPosition.y + _upperYTolerance);
        var lowerRightPoint = new Vector2(_backstopPosition.x, _flagPosition.y - _lowerYTolerance);
        var upperRightPoint = new Vector2(_backstopPosition.x, _flagPosition.y + _upperYTolerance);
        
        Gizmos.DrawLine(lowerLeftPoint, upperLeftPoint);
        Gizmos.DrawLine(upperLeftPoint, upperRightPoint);
        Gizmos.DrawLine(upperRightPoint, lowerRightPoint);
        Gizmos.DrawLine(lowerRightPoint, lowerLeftPoint);
    }

    private void AddPlayer(IPlayer player)
    {
        _player = player;
        _playerBody = player.NormalBody;
    }

    public void SetFinishLine(Vector3 flagPosition, Vector3 backstopPosition)
    {
        var flagOffset = new Vector3(1.5f, 1f, 0);
        _flagPosition = flagPosition;
        _backstopPosition = backstopPosition;
        _flag.transform.position = flagPosition + flagOffset;
        _backstop.transform.position = backstopPosition;
    }

}
