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
    public Action DoFinish;
    private const float _upperYTolerance = 10;
    private const float _lowerYTolerance = 2f;
    private float _upperY = float.PositiveInfinity;
    private float _lowerY = float.NegativeInfinity;
    private Func<float, bool> _isXBetween;
    private FinishLineParameters _parameters;
    private static Vector2 _flagOffset = new(1.5f, 1f);
    private LevelManager _levelManager;

    public Vector2 FlagPosition => _flagPosition;
    public Vector2 BackstopPosition => _backstopPosition;
    public GameObject Backstop => _backstop;
    public GameObject Flag => _flag;
    public FinishLineParameters Parameters { get => _parameters; set => _parameters = value; }


    void Update()
    {
        if(_levelManager == null || _levelManager.PlayerBody == null)
        {
            return;
        }

        if (_isXBetween(_levelManager.PlayerBody.position.x))
        {
            if (_levelManager.PlayerBody.position.y > _lowerY && _levelManager.PlayerBody.position.y < _upperY)
            {
                if (_levelManager.Player.CollisionManager.BothWheelsCollided)
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

    public void SetFinishLine(FinishLineParameters parameters, LevelManager levelManager)
    {
        if (parameters == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        _levelManager = levelManager;
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

}
