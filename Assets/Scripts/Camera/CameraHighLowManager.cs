using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHighLowManager
{
    private Camera _camera;
    private float _leadingX, _trailingX, _buffer;
    public CameraHighLowManager(Camera camera, List<PositionObject<Vector3>> highPoints, List<PositionObject<Vector3>> lowPoints)
    {
        _camera = camera;
        UpdateTargetXValues();

    }

    public void Update()
    {
        UpdateTargetXValues();
    }

    private void UpdateTargetXValues()
    {
        float camCenterX = _camera.ViewportToWorldPoint(new Vector3(0.5f, 0, 0)).x;
        _leadingX = camCenterX + _buffer;
        _trailingX = camCenterX - _buffer;
    }


}
