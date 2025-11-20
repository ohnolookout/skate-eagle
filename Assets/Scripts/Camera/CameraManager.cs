using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private CameraMover _cameraMover;
    private CameraTargeter _cameraTargeter;
    private CameraStateMachine _stateMachine;
    public Camera camera;
    public bool doLogPosition = false;
    public IPlayer player;
    private Transform _playerTransform;
    public Ground currentGround;
    public float dirChangeTimer = 0;
    public const float dirChangeTimerDuration = 1f;
    public float aspectRatio;
    public LinkedTargetTracker LookaheadTracker => Targeter.LookaheadTracker;
    public LinkedTargetTracker PlayerTracker => Targeter.PlayerTracker;
    public HighPointTracker HighPointTracker => Targeter.HighPointTracker;
    public CameraMover Mover => _cameraMover;
    public CameraTargeter Targeter => _cameraTargeter;
    public IPlayer Player => player;
    public Transform PlayerTransform => _playerTransform;

    void Awake()
    {
        camera = Camera.main;
        _stateMachine = new CameraStateMachine(this);
        _cameraTargeter = new CameraTargeter(this);
        _cameraMover = new CameraMover(this);

        LevelManager.OnPlayerCreated += AddPlayer;

        aspectRatio = camera.aspect;
    }

    void FixedUpdate()
    {
        _stateMachine.FixedUpdate();
    }

    private void OnDrawGizmosSelected()
    {
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
        var xOffset = Targeter.xOffset;
        var targetX = _playerTransform.position.x + xOffset;
        var bottomY = camera.transform.position.y - camera.orthographicSize;
        Gizmos.DrawLine(new Vector3(targetX, bottomY), new Vector3(targetX, bottomY + 5));

        Gizmos.color = Color.darkRed;
        Gizmos.DrawSphere(Targeter.TargetPosition, 2f);

        Gizmos.color = Color.lightPink;
        Gizmos.DrawLine(camera.transform.position, Targeter.TargetPosition);
    }

    private void AddPlayer(IPlayer player)
    {
        this.player = player;
        _playerTransform = player.Transform;
        Targeter.UpdateTargets();
    }


    #region Direction Change Handling

    public bool TrackingPointsAreInCamera() { 
        var leftHighPointCameraPos = camera.WorldToViewportPoint(Targeter.HighPointTracker.Current.position);
        var leftHighPointInCamera = PointIsInCamera(leftHighPointCameraPos);
        var rightHighPointInCamera = true;
        var nextLowPointInCamera = true;

        if (leftHighPointInCamera)
        {
            rightHighPointInCamera = true;
            if(Targeter.HighPointTracker.Current.Next != null)
            {
                var rightHighPointCameraPos = camera.WorldToViewportPoint(Targeter.HighPointTracker.Current.Next.position);
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

                var nextLowPointCameraPos = camera.WorldToViewportPoint(nextLowPoint);
                nextLowPointInCamera = PointIsInCamera(nextLowPointCameraPos);
            }

        }

        return leftHighPointInCamera && rightHighPointInCamera && nextLowPointInCamera;
    }

    #endregion

    public bool PointIsInCamera(Vector3 viewportPoint)
    {
        return viewportPoint.x >= 0.02f
            && viewportPoint.x <= 0.98f
            && viewportPoint.y >= 0.03f
            && viewportPoint.y <= 0.9f;
    }
    public void ResetCamera(SerializedStartLine startline)
    {
        ResetVariables();
        Mover.MoveToStart(startline);
        Targeter.ResetTrackers(startline);
    }

    public void ResetVariables()
    {

    }
}
