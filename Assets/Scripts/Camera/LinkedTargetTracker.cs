using Com.LuisPedroFonseca.ProCamera2D.Platformer;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class LinkedTargetTracker
{
    private CompoundTarget _currentTarget = new();
    private Ground _currentGround;
    private bool _isOverExtended = false;
    public CompoundTarget CompoundTarget => _currentTarget;
    public LinkedCameraTarget Current => _currentTarget.current;
    public LinkedCameraTarget Next => _currentTarget.next;
    public LinkedCameraTarget Previous => _currentTarget.prev;
    public bool IsOverExtended => _isOverExtended;


    public LinkedTargetTracker(Ground ground, CompoundTarget startingTarget)
    {
        _currentTarget = new(startingTarget);
        _currentGround = ground;
    }

    public LinkedTargetTracker()
    {

    }

    public void Update(float xPos)
    {
        _isOverExtended = CheckOverExtension(xPos);

#if UNITY_EDITOR
        int searchCount = 0;
#endif

        while (Previous != null && xPos < Current.Position.x)
        {
#if UNITY_EDITOR
            searchCount++;
            if (searchCount > 1)
            {
                Debug.LogWarning("Cam target search iterations > 1: " + searchCount);
            }
#endif
            AssignPrevAndNextTargets(Previous, false);
        }

        while (Next != null && xPos > Next.Position.x)
        {
#if UNITY_EDITOR
            searchCount++;
            if (searchCount > 1)
            {
                Debug.LogWarning("Cam target search iterations > 1: " + searchCount);
            }
#endif
            AssignPrevAndNextTargets(Next, true);
        }

    }

    private void AssignPrevAndNextTargets(LinkedCameraTarget newTarget, bool moveRight)
    {

        if (moveRight)
        {

            _currentTarget.prev = Current;
            if (newTarget.NextTarget != null)
            {
                
                _currentTarget.next = newTarget.NextTarget;
            }
            else
            {
                var currentIndex = _currentGround.LowTargets.IndexOf(newTarget);
                if (currentIndex >= 0 && currentIndex < _currentGround.LowTargets.Count - 1)
                {
                    _currentTarget.next = _currentGround.LowTargets[currentIndex + 1];
                }
                else
                {
                    _currentTarget.next = null;
                }
            }
        }
        else
        {
            _currentTarget.next = Current;            
            if (newTarget.PrevTarget != null)
            {
                _currentTarget.prev = newTarget.PrevTarget;
            }
            else
            {
                var currentIndex = _currentGround.LowTargets.IndexOf(newTarget);
                if (currentIndex > 0)
                {
                    _currentTarget.prev = _currentGround.LowTargets[currentIndex - 1];
                }
                else
                {
                    _currentTarget.prev = null;
                }
            }
        }

        _currentTarget.current = newTarget;
    }

    public void EnterGround(GroundSegment segment, bool doContinuity, bool isForward)
    {
        _currentGround = segment.parentGround;

        if(_currentGround.LowTargets == null || _currentGround.LowTargets.Count == 0)
        {
            _currentTarget = new();
            _isOverExtended = true;
            return;
        }

        _isOverExtended = false;

        if (doContinuity)
        {
            AssignPrevAndNextTargets(segment.FirstLeftTarget, isForward);
        }
        else
        {
            _currentTarget = new(segment.FirstLeftTarget);
        }
    }

    public void SetStartingTarget(SerializedStartLine startline)
    {
        _isOverExtended = false;

        if (startline.FirstCameraTarget != null)
        {
            _currentTarget = new(startline.FirstCameraTarget);
        } else
        {
            _isOverExtended = true;
            _currentTarget = new();
        }

        if (startline.CurvePoint != null)
        {
            _currentGround = startline.CurvePoint.ParentGround;
        } else
        {
            _currentGround = null;
        }
    }

    private bool CheckOverExtension(float posX)
    {
        if (_currentTarget == null || Current == null || _currentGround == null)
        {
            return true;
        }

        if (Next == null && (posX > _currentGround.HighTargets[^1].position.x || posX > _currentGround.CurvePoints[^1].Position.x))
        {
            return true;
        }

        if(Previous == null && (posX < _currentGround.HighTargets[0].position.x || posX < _currentGround.CurvePoints[0].Position.x))
        {
            return true;
        }

        return false;
    }
}
public class CompoundTarget
{
    public LinkedCameraTarget current;
    public LinkedCameraTarget prev;
    public LinkedCameraTarget next;

    public CompoundTarget(LinkedCameraTarget current, LinkedCameraTarget prev, LinkedCameraTarget next)
    {
        this.current = current;
        this.prev = prev;
        this.next = next;
    }

    public CompoundTarget(LinkedCameraTarget current)
    {
        this.current = current;
        this.prev = current.PrevTarget;
        this.next = current.NextTarget;
    }

    public CompoundTarget(CompoundTarget other)
    {
        this.current = other.current;
        this.prev = other.prev;
        this.next = other.next;
    }

    public CompoundTarget()
    {
        current = null;
        prev = null;
        next = null;
    }
}
