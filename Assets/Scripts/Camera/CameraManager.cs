using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    #region Declarations
    //Subclasses
    private CameraMover _cameraMover;
    private CameraTargeter _cameraTargeter;
    private CameraStateMachine _stateMachine;

    //Instance references
    public Camera _camera;
    public Ground currentGround;
    public IPlayer player;
    private Transform _playerTransform;

    //Tracking variables
    public bool doLogPosition = false;
    private float _aspectRatio;
    private float _continuityTimer = 0;
    private const float ContinuityTimerDuration = 1f;

    //Getters/Setters
    public LinkedTargetTracker LookaheadTracker => Targeter.LookaheadTracker;
    public LinkedTargetTracker PlayerTracker => Targeter.PlayerTracker;
    public HighPointTracker HighPointTracker => Targeter.HighPointTracker;
    public CameraMover Mover => _cameraMover;
    public CameraTargeter Targeter => _cameraTargeter;
    public IPlayer Player => player;
    public Transform PlayerTransform => _playerTransform;
    public float AspectRatio => _aspectRatio;
    public Camera Camera => _camera;
    public float ContinuityTimer => _continuityTimer;
    #endregion

    #region Monobehaviours
    void Awake()
    {
        _camera = Camera.main;
        _stateMachine = new CameraStateMachine(this);
        _cameraTargeter = new CameraTargeter(this);
        _cameraMover = new CameraMover(this);

        LevelManager.OnPlayerCreated += AddPlayer;

        _aspectRatio = Camera.aspect;
    }

    void FixedUpdate()
    {
        UpdateContinuityTimer();
        _stateMachine.FixedUpdate();
    }

    private void OnDrawGizmosSelected()
    {
        if(!Application.isPlaying)
        {
            return;
        }

        Gizmos.color = Color.orange;
        if(HighPointTracker.Current != null)
        {
            Gizmos.DrawSphere(HighPointTracker.Current.position, 2f);

            Gizmos.color = Color.darkOrange;
            if (HighPointTracker.Current.Next != null)
            {
                Gizmos.DrawSphere(HighPointTracker.Current.Next.position, 2f);
            }
        }

        if (LookaheadTracker == null || PlayerTracker == null || LookaheadTracker.CompoundTarget == null || PlayerTracker.CompoundTarget == null)
        {
            return;
        }

        Gizmos.color = Color.lightGreen;

        if (LookaheadTracker.Current != null)
        {
            Gizmos.DrawSphere(LookaheadTracker.Current.Position, 2f);
        }

        Gizmos.color = Color.darkOliveGreen;
        if(LookaheadTracker.Next != null)
        {
            Gizmos.DrawSphere(LookaheadTracker.Next.Position, 2f);
        }

        if(player == null || _playerTransform == null)
        {
            return;
        }

        var lowerTargetDelta = new Vector3(0, 2);

        Gizmos.color = Color.lightPink;
        if (PlayerTracker.Current != null)
        {
            Gizmos.DrawSphere(PlayerTracker.Current.Position - lowerTargetDelta, 2f);
        }

        Gizmos.color = Color.rebeccaPurple;
        if (PlayerTracker.Next != null)
        {
            Gizmos.DrawSphere(PlayerTracker.Next.Position - lowerTargetDelta, 2f);
        }

        Gizmos.color = Color.blue;
        var xOffset = Targeter.XOffset;
        var targetX = _playerTransform.position.x + xOffset;
        var bottomY = Camera.transform.position.y - Camera.orthographicSize;
        Gizmos.DrawLine(new Vector3(targetX, bottomY), new Vector3(targetX, bottomY + 5));

        Gizmos.color = Color.darkRed;
        Gizmos.DrawSphere(Targeter.TargetPosition, 2f);

        Gizmos.color = Color.lightPink;
        Gizmos.DrawLine(Camera.transform.position, Targeter.TargetPosition);
    }

    #endregion

    #region Events
    private void AddPlayer(IPlayer player)
    {
        this.player = player;
        _playerTransform = player.Transform;
        Targeter.UpdateTargetsGroundTracking();
    }
    public void ResetCamera(SerializedStartLine startline)
    {
        _continuityTimer = 0;
        Mover.MoveToStart(startline);
        Targeter.ResetTrackers(startline);
    }

    public void StartContinuityTimer()
    {
        _continuityTimer = ContinuityTimerDuration;
    }

    private void UpdateContinuityTimer()
    {
        if (_continuityTimer > 0)
        {
            _continuityTimer -= Time.fixedDeltaTime;
            if (_continuityTimer < 0)
            {
                _continuityTimer = 0;
            }
        }
    }
    public void StopContinuityTimer()
    {
        _continuityTimer = 0;
    }
    #endregion

    #region Camera Utilities
    public bool TrackingPointsAreInCamera() { 
        var leftHighPointCameraPos = Camera.WorldToViewportPoint(Targeter.HighPointTracker.Current.position);
        var leftHighPointInCamera = PointIsInCamera(leftHighPointCameraPos);
        var rightHighPointInCamera = true;
        var nextLowPointInCamera = true;

        if (leftHighPointInCamera)
        {
            rightHighPointInCamera = true;
            if(Targeter.HighPointTracker.Current.Next != null)
            {
                var rightHighPointCameraPos = Camera.WorldToViewportPoint(Targeter.HighPointTracker.Current.Next.position);
                rightHighPointInCamera = PointIsInCamera(leftHighPointCameraPos);
            }

            if (rightHighPointInCamera)
            {
                Vector3 nextLowPoint;
                if (this.player.FacingForward)
                {
                    nextLowPoint = Targeter.PlayerTracker.Next.Position;
                }
                else
                {
                    nextLowPoint = Targeter.PlayerTracker.Current.Position;
                }

                var nextLowPointCameraPos = Camera.WorldToViewportPoint(nextLowPoint);
                nextLowPointInCamera = PointIsInCamera(nextLowPointCameraPos);
            }

        }

        return leftHighPointInCamera && rightHighPointInCamera && nextLowPointInCamera;
    }

    public bool PointIsInCamera(Vector3 viewportPoint)
    {
        return viewportPoint.x >= 0.02f
            && viewportPoint.x <= 0.98f
            && viewportPoint.y >= 0.03f
            && viewportPoint.y <= 0.9f;
    }
    #endregion
}

public class RunningAverager
{
    private float _total = 0;
    private float _valCount = 0;
    private Queue<float> _values;
    private int _maxCount;

    public float RunningAverage => _total / _valCount;

    public RunningAverager(int maxCount)
    {
        _maxCount = maxCount;
        _values = new Queue<float>();
    }

    public float AddSpeed(float speed)
    {
        _total += speed;
        _valCount++;
        _values.Enqueue(speed);
        if (_valCount > _maxCount)
        {
            var removedVal = _values.Dequeue();
            _total -= removedVal;
            _valCount--;
        }

        return _total / _valCount;
    }

}
