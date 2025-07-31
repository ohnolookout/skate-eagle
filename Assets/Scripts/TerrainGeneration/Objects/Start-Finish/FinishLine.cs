using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLine : MonoBehaviour, ISerializable
{
    #region Declarations
    [SerializeField] private GameObject _flag;
    [SerializeField] private SpriteRenderer _flagRenderer;
    [SerializeField] private GameObject _backstop;
    private CurvePoint _flagPoint;
    private CurvePoint _backstopPoint;
    private int _flagXOffset = 50;
    private int _backstopXOffset = 0;
    private static Vector3 _flagSpriteOffset = new(1.5f, 1f);
    public Action DoFinish;
    private const float _upperYTolerance = 10;
    private const float _lowerYTolerance = 2f;
    private float _upperY = float.PositiveInfinity;
    private float _lowerY = float.NegativeInfinity;
    private Func<float, bool> _isXBetween;
    private IPlayer _player;
    private Rigidbody2D _playerBody;

    public CurvePoint FlagPoint => _flagPoint;
    public CurvePoint BackstopPoint => _backstopPoint;
    public int FlagXOffset => _flagXOffset;
    public int BackstopXOffset => _backstopXOffset;
    public bool BackstopIsActive => _backstop.activeSelf;
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

        if(_flagPoint == null || _backstopPoint == null)
        {
            return;
        }

        var flagPosition = _flagPoint.WorldPosition + new Vector3(_flagXOffset, 0);
        var backstopPosition = _backstopPoint.WorldPosition + new Vector3(_backstopXOffset, 0);

        var lowerLeftPoint = new Vector2(flagPosition.x, flagPosition.y - _lowerYTolerance);
        var upperLeftPoint = new Vector2(flagPosition.x, flagPosition.y + _upperYTolerance);
        var lowerRightPoint = new Vector2(backstopPosition.x, flagPosition.y - _lowerYTolerance);
        var upperRightPoint = new Vector2(backstopPosition.x, flagPosition.y + _upperYTolerance);
        
        Gizmos.DrawLine(lowerLeftPoint, upperLeftPoint);
        Gizmos.DrawLine(upperLeftPoint, upperRightPoint);
        Gizmos.DrawLine(upperRightPoint, lowerRightPoint);
        Gizmos.DrawLine(lowerRightPoint, lowerLeftPoint);
    }
    #endregion

    #region Construction
    public void SetFinishLine(SerializedFinishLine parameters)
    {
#if UNITY_EDITOR
        if (parameters == null)
        {
            if(Application.isPlaying)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
            return;
        }

        if (Application.isPlaying && (parameters.flagPoint == null || parameters.backstopPoint == null))
        {
            Destroy(gameObject);
            return;
        }
#endif

        gameObject.SetActive(true);

        _flagXOffset = parameters.flagPointXOffset;
        _backstopXOffset = parameters.backstopPointXOffset;

        SetFlagPoint(parameters.flagPoint);
        SetBackstopPoint(parameters.backstopPoint);

        _backstop.SetActive(parameters.backstopIsActive);

        UpdateIsForward();
    }


    private void OnPlayerCreated(IPlayer player)
    {
        _player = player;
        _playerBody = player.NormalBody;
    }
#endregion

    public IDeserializable Serialize()
    {
        return new SerializedFinishLine(this);
    }

    public void SetFlagPoint(CurvePoint flagPoint)
    {
        flagPoint.LinkedCameraTarget.doTargetLow = true;
        _flagPoint = flagPoint;

        _flag.SetActive(true);
        UpdateFlagPosition();

#if UNITY_EDITOR
        UpdateIsForward();
#endif
    }

    public void UpdateFinish()
    {
        UpdateFlagPosition();
        UpdateBackstopPosition();
    }

    public void SetFlagOffset(int flagXOffset)
    {
        _flagXOffset = flagXOffset;
        UpdateFlagPosition();
    }


    public void UpdateFlagPosition()
    {
        _flag.transform.position = _flagPoint.WorldPosition + new Vector3(_flagXOffset, 0) + _flagSpriteOffset;
    }

    public void SetBackstopPoint(CurvePoint backstopPoint)
    {
        _backstopPoint = backstopPoint;
        _backstop.SetActive(true);
        UpdateBackstopPosition();

#if UNITY_EDITOR
        UpdateIsForward();
#endif
    }

    public void SetBackstopOffset(int backstopXOffset)
    {
        _backstopXOffset = backstopXOffset;
        UpdateBackstopPosition();
    }

    public void UpdateBackstopPosition()
    {
        _backstop.transform.position = _backstopPoint.WorldPosition + new Vector3(_backstopXOffset, 0);
    }

    public void UpdateIsForward()
    {
#if UNITY_EDITOR
        if(_flagPoint == null || _backstopPoint == null)
        {
            return;
        }
#endif
        var flagX = _flagPoint.WorldPosition.x + _flagXOffset;
        var backstopX = _backstopPoint.WorldPosition.x + _backstopXOffset;
        bool isForward = flagX < backstopX;
        _isXBetween = isForward 
            ? (x => x > flagX && x < backstopX) 
            : (x => x < flagX && x > backstopX);

        _flagRenderer.flipX = !isForward;
    }

    public void ActivateBackstop(bool doActivate)
    {
        if (_backstopPoint == null)
        {
            doActivate = false;
            Debug.LogWarning("FinishLine: Attempted to activate backstop without a backstop point set.");
        }
        _backstop.SetActive(doActivate);
    }
#if UNITY_EDITOR

    public void Clear()
    {
        ClearFlag();
        ClearBackstop();
    }
    public void ClearBackstop()
    {
        _backstopPoint = null;
        _backstopXOffset = 0;
        _backstop.transform.position = Vector2.zero;
        _backstop.SetActive(false);
    }

    public void ClearFlag()
    {
        _flagPoint = null;
        _flagXOffset = 50;
        _flagRenderer.flipX = false;
        _flag.transform.position = Vector2.zero;
    }
#endif
}
