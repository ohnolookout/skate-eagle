using UnityEngine;

public class HighPointTracker
{
    private LinkedHighPoint _currentHighPoint;

    public LinkedHighPoint Current => _currentHighPoint;

    public bool Update(float xPos)
    {
        bool hasChanged = false;
#if UNITY_EDITOR
        int searchCount = 0;
#endif
        if (_currentHighPoint == null)
        {
            Debug.LogWarning("CameraManager: Current high point is null. Assign a high point to ground.");
            return hasChanged;
        }

        while (_currentHighPoint.Previous != null && xPos < _currentHighPoint.position.x)
        {
#if UNITY_EDITOR
            searchCount++;
            if (searchCount > 1)
            {
                Debug.LogWarning("Cam highpoint search iterations > 1: " + searchCount);
            }
#endif
            hasChanged = true;
            _currentHighPoint = _currentHighPoint.Previous;
        }

        while (_currentHighPoint.Next != null && xPos > _currentHighPoint.Next.position.x)
        {
#if UNITY_EDITOR
            searchCount++;
            if (searchCount > 1)
            {
                Debug.LogWarning("Cam highpoint search iterations > 1: " + searchCount);
            }
#endif
            hasChanged = true;
            _currentHighPoint = _currentHighPoint.Next;
        }

        return hasChanged;
    }

    public void SetHighPoint(LinkedHighPoint startingHighPoint)
    {
        _currentHighPoint = startingHighPoint;
    }
}
