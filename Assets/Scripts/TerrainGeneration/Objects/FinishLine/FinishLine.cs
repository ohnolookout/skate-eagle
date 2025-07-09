using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLine : MonoBehaviour, ISerializable
{
    #region Declarations
    [SerializeField] private GameObject _flag;
    [SerializeField] private GameObject _backstop;
    private Vector2 _flagPosition;
    private Vector2 _backstopPosition;
    public Action DoFinish;
    private const float _upperYTolerance = 10;
    private const float _lowerYTolerance = 2f;
    private float _upperY = float.PositiveInfinity;
    private float _lowerY = float.NegativeInfinity;
    private Func<float, bool> _isXBetween;
    private SerializedFinishLine _parameters;
    private static Vector2 _flagOffset = new(1.5f, 1f);
    private IPlayer _player;
    private Rigidbody2D _playerBody;

    public Vector2 FlagPosition => _flagPosition;
    public Vector2 BackstopPosition => _backstopPosition;
    public GameObject Backstop => _backstop;
    public GameObject Flag => _flag;
    public SerializedFinishLine Parameters { get => _parameters; set => _parameters = value; }
    #endregion

    #region Monobehaviours
    void Awake()
    {
        LevelManager.OnPlayerCreated += OnPlayerCreated;
    }
    void Update()
    {
        if(_playerBody == null || _player == null)
        {
            return;
        }

        if (_isXBetween(_playerBody.position.x))
        {
            if (_playerBody.position.y > _lowerY && _playerBody.position.y < _upperY)
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
    #endregion

    #region Construction
    public void SetFinishLine(SerializedFinishLine parameters)
    {
        if (parameters == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        _parameters = parameters;
        _flagPosition = parameters.flagPosition;
        _backstopPosition = parameters.backstopPosition;
        _flag.transform.position = parameters.flagPosition + _flagOffset;

        if (parameters.backstopIsActive)
        {            
            _backstop.transform.position = parameters.backstopPosition;
            _backstop.SetActive(true);
        } else
        {
            _backstop.SetActive(false);
        }

        _lowerY = _flagPosition.y - _lowerYTolerance;
        _upperY = _flagPosition.y + _upperYTolerance;

        if (parameters.isForward)
        {
            _isXBetween = x => x > _flagPosition.x && x < _backstopPosition.x;
        }
        else
        {
            _isXBetween = x => x < _flagPosition.x && x > _backstopPosition.x;
        }
    }

    public void ClearFinishLine()
    {
        gameObject.SetActive(false);

        _parameters = null;
        _flagPosition = Vector2.zero;
        _backstopPosition = Vector2.zero;

        _flag.transform.position = Vector2.zero;
        _backstop.transform.position = Vector2.zero;

    }

    private void OnPlayerCreated(IPlayer player)
    {
        _player = player;
        _playerBody = player.NormalBody;
    }
    #endregion

    public IDeserializable Serialize()
    {
        if (_parameters == null)
        {
            return null;
        }

        return _parameters;
    }
}
