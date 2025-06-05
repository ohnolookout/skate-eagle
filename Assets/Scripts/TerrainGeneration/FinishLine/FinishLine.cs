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
    private IPlayer _player;
    private Rigidbody2D _playerBody;
    public Action DoFinish;
    private const float _upperYTolerance = 10;
    private const float _lowerYTolerance = 2f;
    private float _upperY = float.PositiveInfinity;
    private float _lowerY = float.NegativeInfinity;
    private Func<float, bool> _isXBetween;
    private FinishLineParameters _params;

    public Vector2 FlagPosition => _flagPosition;
    public Vector2 BackstopPosition => _backstopPosition;
    public GameObject Backstop => _backstop;
    public GameObject Flag => _flag;
    public FinishLineParameters Parameters { get => _params; set => _params = value; }

    void Awake()
    {
        LevelManager.OnPlayerCreated += AddPlayer;
    }

    void Update()
    {
        if(_isXBetween(_playerBody.position.x))
        {
            Debug.Log("Player is between finish line X bounds");
            if (_playerBody.position.y > _lowerY && _playerBody.position.y < _upperY)
            {
                Debug.Log("Player is between finish line Y bounds");
                if (_player.CollisionManager.BothWheelsCollided)
                {
                    Debug.Log("Player has both wheels on the finish line");
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
        _lowerY = _flagPosition.y - _lowerYTolerance;
        _upperY = _flagPosition.y + _upperYTolerance;

        if (_flagPosition.x < _backstopPosition.x)
        {
            _isXBetween = x => x > _flagPosition.x && x < _backstopPosition.x;
        } else
        {
            _isXBetween = x => x < _flagPosition.x && x > _backstopPosition.x;
        }
    }

}
