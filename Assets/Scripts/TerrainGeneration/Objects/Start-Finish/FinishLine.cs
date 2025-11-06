using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FinishLine : MonoBehaviour, ISerializable, IObjectResync
{
    #region Declarations
    [SerializeField] private GameObject _flag;
    [SerializeField] private SpriteRenderer _flagRenderer;
    [SerializeField] private GameObject _backstop;
    private ResyncRef<CurvePoint> _flagPointRef;
    private ResyncRef<CurvePoint> _backstopPointRef;
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
    public string UID { get; set; }

    public CurvePoint FlagPoint
    {
        get => _flagPoint;
        set
        {
            _flagPoint = value;
            _flagPointRef.Value = value;
        }
    }
    public CurvePoint BackstopPoint
    {
        get => _backstopPoint;
        set
        {
            _backstopPoint = value;
        }
    }
    public int FlagXOffset => _flagXOffset;
    public int BackstopXOffset => _backstopXOffset;
    public bool BackstopIsActive => _backstop.activeSelf;
    public GameObject GameObject => gameObject;
    public ResyncRef<CurvePoint> FlagPointRef { get => _flagPointRef; set => _flagPointRef = value; }
    public ResyncRef<CurvePoint> BackstopPointRef { get => _flagPointRef; set => _flagPointRef = value; }
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

        gameObject.SetActive(true);

        _flagXOffset = parameters.flagPointXOffset;
        _backstopXOffset = parameters.backstopPointXOffset;

        SetFlagPoint(parameters.flagPoint);
        SetBackstopPoint(parameters.backstopPoint);

        _backstop.SetActive(parameters.backstopIsActive);

        UpdateIsForward();
    }

    /// <summary>
    /// Callback for when the player is created. Stores references to player and its Rigidbody2D.
    /// </summary>
    /// <param name="player">The player instance.</param>
    private void OnPlayerCreated(IPlayer player)
    {
        _player = player;
        _playerBody = player.NormalBody;
    }
    #endregion

    #region Edit Utilities
    /// <summary>
    /// Serializes the finish line to a serializable object.
    /// </summary>
    /// <returns>Serialized finish line.</returns>
    public IDeserializable Serialize()
    {
        return new SerializedFinishLine(this);
    }

    /// <summary>
    /// Gets the list of object resyncs for the finish line.
    /// </summary>
    /// <returns>List of ObjectResync.</returns>
    public List<ObjectResync> GetObjectResyncs()
    {
        List<ObjectResync> resyncs = new();
        if (FlagPoint != null)
        {
            var resync = new ObjectResync(FlagPoint.LinkedCameraTarget.serializedObjectLocation);
            resync.resyncFunc = (obj) => { FlagPoint.Object = obj; };
            resyncs.Add(resync);
        }

        if (_backstop != null)
        {
            var resync = new ObjectResync(BackstopPoint.LinkedCameraTarget.serializedObjectLocation);
            resync.resyncFunc = (obj) => { BackstopPoint.Object = obj; };
            resyncs.Add(resync);
        }

        return resyncs;
    }

    /// <summary>
    /// Refreshes the finish line's flag and backstop positions.
    /// </summary>
    /// <param name="_">Optional ground manager (unused).</param>
    public void Refresh(GroundManager _ = null)
    {
#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(this, "Refreshing finish line");
#endif
        UpdateFlagPosition();
        UpdateBackstopPosition();
    }

    /// <summary>
    /// Sets the flag point and updates its position.
    /// </summary>
    /// <param name="flagPoint">The new flag point.</param>
    public void SetFlagPoint(CurvePoint flagPoint)
    {
        gameObject.SetActive(true);
#if UNITY_EDITOR
        Undo.RecordObject(this, "Set Finish Flag and Backstop");
#endif
        flagPoint.LinkedCameraTarget.doLowTarget = true;
        FlagPoint = flagPoint;
        _flag.SetActive(true);
        UpdateFlagPosition();

#if UNITY_EDITOR
        UpdateIsForward();
#endif
    }

    /// <summary>
    /// Sets the flag's X offset and updates its position.
    /// </summary>
    /// <param name="flagXOffset">The new X offset.</param>
    public void SetFlagOffset(int flagXOffset)
    {
#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(this, "Refreshing finish line");
#endif
        _flagXOffset = flagXOffset;
        UpdateFlagPosition();
    }

    /// <summary>
    /// Updates the flag's world position.
    /// </summary>
    public void UpdateFlagPosition()
    {
#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(this, "Refreshing finish line");
#endif
        _flag.transform.position = FlagPoint.WorldPosition + new Vector3(_flagXOffset, 0) + _flagSpriteOffset;
    }

    /// <summary>
    /// Sets the backstop point and updates its position.
    /// </summary>
    /// <param name="backstopPoint">The new backstop point.</param>
    public void SetBackstopPoint(CurvePoint backstopPoint)
    {
        gameObject.SetActive(true);
#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(this, "Refreshing finish line");
#endif
        BackstopPoint = backstopPoint;
        _backstop.SetActive(true);
        UpdateBackstopPosition();

#if UNITY_EDITOR
        UpdateIsForward();
#endif
    }

    /// <summary>
    /// Sets the backstop's X offset and updates its position.
    /// </summary>
    /// <param name="backstopXOffset">The new X offset.</param>
    public void SetBackstopOffset(int backstopXOffset)
    {
#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(this, "Refreshing finish line");
#endif
        _backstopXOffset = backstopXOffset;
        UpdateBackstopPosition();
    }

    /// <summary>
    /// Updates the backstop's world position.
    /// </summary>
    public void UpdateBackstopPosition()
    {
#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(this, "Refreshing finish line");
#endif
        _backstop.transform.position = BackstopPoint.WorldPosition + new Vector3(_backstopXOffset, 0);
    }

    /// <summary>
    /// Updates the direction of the finish line and sets the flag's flip state.
    /// </summary>
    public void UpdateIsForward()
    {
#if UNITY_EDITOR
        if (FlagPoint == null || BackstopPoint == null)
        {
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

    /// <summary>
    /// Activates or deactivates the backstop.
    /// </summary>
    /// <param name="doActivate">Whether to activate the backstop.</param>
    public void ActivateBackstop(bool doActivate)
    {
        if (BackstopPoint == null)
        {
            doActivate = false;
            Debug.LogWarning("FinishLine: Attempted to activate backstop without a backstop point set.");
        }
        _backstop.SetActive(doActivate);
    }

    /// <summary>
    /// Clears the finish line, flag, and backstop.
    /// </summary>
    public void Clear()
    {
#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(this, "Refreshing finish line");
#endif
        ClearFlag();
        ClearBackstop();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Clears the backstop and resets its state.
    /// </summary>
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

    /// <summary>
    /// Clears the flag and resets its state.
    /// </summary>
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
    /// <summary>
    /// Checks if the given object is the parent ground of the flag point.
    /// </summary>
    /// <param name="obj">The object to check.</param>
    /// <returns>True if the object is the parent ground, false otherwise.</returns>
    public bool IsParentGround(GameObject obj)
    {
        if (FlagPoint == null || FlagPoint.Object == null)
        {
            return false;
        }

        var cpObj = FlagPoint.Object.GetComponent<CurvePointEditObject>();
        return obj.GetComponent<Ground>() == cpObj.ParentGround;
    }
#endif
    #endregion

    public void RegisterResync()
    {
        LevelManager.ResyncHub.RegisterResync(this);
    }
}
