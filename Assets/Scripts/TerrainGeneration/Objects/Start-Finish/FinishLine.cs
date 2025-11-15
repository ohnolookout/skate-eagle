using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FinishLine : MonoBehaviour, ISerializable
{
    #region Declarations
    [SerializeField] private GameObject _flag;
    [SerializeField] private SpriteRenderer _flagRenderer;
    [SerializeField] private GameObject _backstop;
    private ResyncRef<CurvePoint> _flagPointRef = new();
    private ResyncRef<CurvePoint> _backstopPointRef = new();
    private ResyncRef<Ground> _groundRef = new();
    private int _flagXOffset = 50;
    private int _backstopXOffset = 0;
    private bool _backstopIsActive;
    private static Vector3 _flagSpriteOffset = new(1.5f, 1f);
    public Action DoFinish;
    private const float _upperYTolerance = 10;
    private const float _lowerYTolerance = 2f;
    private float _upperY = float.PositiveInfinity;
    private float _lowerY = float.NegativeInfinity;
    private Func<float, bool> _isXBetween;
    private IPlayer _player;
    private Rigidbody2D _playerBody;
    public string UID { get; set; }

    public CurvePoint FlagPoint { get => _flagPointRef.Value; set => _flagPointRef.Value = value; }
    public CurvePoint BackstopPoint { get => _backstopPointRef.Value; set => _backstopPointRef.Value = value; }
    public Ground ParentGround { get => _groundRef.Value; set => _groundRef.Value = value; }
    public int FlagXOffset => _flagXOffset;
    public int BackstopXOffset => _backstopXOffset;
    public bool BackstopIsActive => _backstopIsActive;
    public GameObject GameObject => gameObject;
    public ResyncRef<CurvePoint> FlagPointRef { get => _flagPointRef; set => _flagPointRef = value; }
    public ResyncRef<CurvePoint> BackstopPointRef { get => _backstopPointRef; set => _backstopPointRef = value; }
    public ResyncRef<Ground> ParentGroundRef { get => _groundRef; set => _groundRef = value; }
    #endregion

    #region Monobehaviours
    void Awake()
    {
        LevelManager.OnPlayerCreated += OnPlayerCreated;
    }

    void Update()
    {
        if (_playerBody == null || _player == null)
        {
            return;
        }

        // Check if player is between the flag and backstop horizontally
        if (_isXBetween(_playerBody.position.x))
        {
            // Check if player is within the vertical tolerance
            if (_playerBody.position.y > _lowerY && _playerBody.position.y < _upperY)
            {
                // Check if both wheels have collided (player has crossed the finish line)
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

        if (FlagPoint == null || BackstopPoint == null)
        {
            return;
        }

        var flagPosition = FlagPoint.WorldPosition + new Vector3(_flagXOffset, 0);
        var backstopPosition = BackstopPoint.WorldPosition + new Vector3(_backstopXOffset, 0);

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
    /// <summary>
    /// Sets up the finish line using the provided parameters.
    /// </summary>
    /// <param name="parameters">Serialized finish line parameters.</param>
    public void SetFinishLine(SerializedFinishLine parameters)
    {
#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(this, "Refreshing finish line");
        if (parameters == null)
        {
            if (Application.isPlaying)
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

        _flagPointRef = parameters.flagPointRef;
        _backstopPointRef = parameters.backstopPointRef;
        _groundRef = parameters.groundRef;


        _flagXOffset = parameters.flagPointXOffset;
        _backstopXOffset = parameters.backstopPointXOffset;
        _backstopIsActive = parameters.backstopIsActive;
        UID = parameters.uid;

        SerializeLevelUtility.OnDeserializationComplete += OnDeserializationComplete;
    }
    public void OnDeserializationComplete()
    {
        SerializeLevelUtility.OnDeserializationComplete -= OnDeserializationComplete;
        gameObject.SetActive(true);
        SetFlagPoint(FlagPoint);

        if (BackstopIsActive)
        {
            _backstop.SetActive(BackstopIsActive);
            SetBackstopPoint(BackstopPoint);
        }

        UpdateIsForward();
    }


    private void OnPlayerCreated(IPlayer player)
    {
        _player = player;
        _playerBody = player.NormalBody;
    }
    #endregion

    #region Edit Utilities
    public IDeserializable Serialize()
    {
        return new SerializedFinishLine(this);
    }

    public void Refresh(GroundManager _ = null)
    {
#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(this, "Refreshing finish line");
#endif
        UpdateFlagPosition();
        UpdateBackstopPosition();
    }

    public void SetFlagPoint(CurvePoint flagPoint)
    {
        gameObject.SetActive(true);
#if UNITY_EDITOR
        Undo.RecordObject(this, "Set Finish Flag and Backstop");
#endif
        FlagPoint = flagPoint;

        if (flagPoint.CPObject != null)
        {
            ParentGround = flagPoint.CPObject.ParentGround;
        }
        _flag.SetActive(true);
        UpdateFlagPosition();

#if UNITY_EDITOR
        UpdateIsForward();
#endif
    }

    public void SetFlagOffset(int flagXOffset)
    {
#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(this, "Refreshing finish line");
#endif
        _flagXOffset = flagXOffset;
        UpdateFlagPosition();
    }

    public void UpdateFlagPosition()
    {
#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(this, "Refreshing finish line");
#endif
        if(FlagPoint == null)
        {
            return;
        }
        _flag.transform.position = FlagPoint.WorldPosition + new Vector3(_flagXOffset, 0) + _flagSpriteOffset;
    }

    public void SetBackstopPoint(CurvePoint backstopPoint)
    {
        gameObject.SetActive(true);

        if (backstopPoint.CPObject != null) { 
            ParentGround = backstopPoint.CPObject.ParentGround;
        }
#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(this, "Refreshing finish line");
#endif
        BackstopPoint = backstopPoint;
        ActivateBackstop(true);
        UpdateBackstopPosition();

#if UNITY_EDITOR
        UpdateIsForward();
#endif
    }

    public void SetBackstopOffset(int backstopXOffset)
    {
#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(this, "Refreshing finish line");
#endif
        _backstopXOffset = backstopXOffset;
        UpdateBackstopPosition();
    }

    public void UpdateBackstopPosition()
    {
#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(this, "Refreshing finish line");
#endif
        if (BackstopPoint == null)
        {
            return;
        }
        _backstop.transform.position = BackstopPoint.WorldPosition + new Vector3(_backstopXOffset, 0);
    }

    public void UpdateIsForward()
    {
#if UNITY_EDITOR
        if (FlagPoint == null || BackstopPoint == null)
        {
            Debug.Log("Flagpoint or backstoppoint is null. Can't calculate isForward");
            return;
        }
#endif
        var flagX = FlagPoint.WorldPosition.x + _flagXOffset;
        var backstopX = BackstopPoint.WorldPosition.x + _backstopXOffset;
        bool isForward = flagX < backstopX;
        _isXBetween = isForward
            ? (x => x > flagX && x < backstopX)
            : (x => x < flagX && x > backstopX);

        _flagRenderer.flipX = !isForward;
    }

    public void ActivateBackstop(bool doActivate)
    {
        _backstopIsActive = doActivate;
        _backstop.SetActive(doActivate);
    }

    public void Clear()
    {
#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(this, "Refreshing finish line");
#endif
        ClearFlag();
        ClearBackstop();
        gameObject.SetActive(false);
    }

    public void ClearBackstop()
    {
#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(this, "Refreshing finish line");
#endif
        BackstopPoint = null;
        _backstopXOffset = 0;
        _backstop.transform.position = Vector2.zero;
        _backstop.SetActive(false);
    }

    public void ClearFlag()
    {
#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(this, "Refreshing finish line");
#endif
        FlagPoint = null;
        _flagXOffset = 50;
        _flagRenderer.flipX = false;
        _flag.transform.position = Vector2.zero;
    }

#if UNITY_EDITOR
    public bool IsParentGround(GameObject obj)
    {
        if (FlagPoint == null || FlagPoint.CPObject == null)
        {
            return false;
        }

        var cpObj = FlagPoint.CPObject;
        return obj.GetComponent<Ground>() == cpObj.ParentGround;
    }
#endif
    #endregion

    public void RegisterResync()
    {
        LevelManager.ResyncHub.RegisterResync(this);
    }
}
