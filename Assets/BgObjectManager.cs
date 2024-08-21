using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BgObjectManager : MonoBehaviour
{
    public List<BgObject> BgObjectPool;
    public Transform LightSource;
    public float ParallaxRatio = 1f;
    public int HalfWidth = 205;
    public int PositionBuffer = 10;
    public int CameraBuffer = 30;
    public float LocationVariance = 0.2f;

    private Vector2 _startPosition;
    private Dictionary<BgObjectType, List<Vector2>> _projectionPointDict;
    private SinglePositionalList<BgObject> _bgObjectPositionalList;
    private ICameraOperator _cameraOperator;

    private float _trailingObjectX => _bgObjectPositionalList.AllObjects[0].Position.x;
    private float _leadingObjectX => _bgObjectPositionalList.AllObjects[^1].Position.x;
    private float _trailingBoundX => _cameraOperator.gameObject.transform.position.x - HalfWidth - PositionBuffer;
    private float _leadingBoundX => _cameraOperator.gameObject.transform.position.x + HalfWidth + PositionBuffer;

    void Start()
    {
        _startPosition = transform.position;
        _projectionPointDict = new();
        _cameraOperator = Camera.main.GetComponent<ICameraOperator>();
        var orderedBgObjects = BuildBgObjects(LightSource.position.y / LightSource.position.x);
        _bgObjectPositionalList = PositionalListFactory<BgObject>.CameraOperatorTracker(
            orderedBgObjects, _cameraOperator, CameraBuffer, CameraBuffer);
        _bgObjectPositionalList.OnObjectAdded += OnObjectAdded;
        _bgObjectPositionalList.OnObjectRemoved += OnObjectRemoved;
    }

    void FixedUpdate()
    {
        _bgObjectPositionalList.Update();
        float xDelta = _cameraOperator.gameObject.transform.position.x * ParallaxRatio;
        float camLayerDelta = _cameraOperator.gameObject.transform.position.x * (1 - ParallaxRatio);
        float expectedPercentWidthFromCamera = camLayerDelta / HalfWidth;
        float currentPercentWidthFromCamera = camLayerDelta / HalfWidth;
        float lengthDifference = (expectedPercentWidthFromCamera - currentPercentWidthFromCamera) * HalfWidth;
        transform.position = new Vector3(_startPosition.x + xDelta - lengthDifference, transform.position.y, transform.position.z);

        if (_trailingObjectX <= _trailingBoundX)
        {
            ShiftTrailingToFront();
        }
        else if (_leadingObjectX >= _leadingBoundX)
        {
            ShiftLeadingToBack();
        }
    }

    private List<BgObject> BuildBgObjects(float lightSlope)
    {
        float totalDistance = 0;
        List<BgObject> orderedBgObjects = new();
        float firstXChange = 0;
        while(BgObjectPool.Count > 0)
        {
            //Pick object and build projection
            var index = Random.Range(0, BgObjectPool.Count);
            var bgObject = BgObjectPool[index];
            BuildObjectProjection(bgObject, lightSlope);

            //Set object location
            var baseXChange = ((HalfWidth * 2) - totalDistance) / BgObjectPool.Count;
            var xVariance = baseXChange * LocationVariance;
            var xChange = Random.Range(baseXChange - xVariance, baseXChange + xVariance);
            var objectY = Random.Range(bgObject.YMin, bgObject.YMax);
            var lastX = orderedBgObjects.Count > 0 ? orderedBgObjects[0].Position.x : HalfWidth;
            var objectX = lastX - xChange;
            totalDistance += xChange;

            //Assign object to dict
            bgObject.XDistance = xChange;
            bgObject.YDistance = objectY;
            bgObject.transform.position = new(objectX, objectY);
            orderedBgObjects.Insert(0, bgObject);
            BgObjectPool.RemoveAt(index);
            //Go to next object if this is the first object
            if (orderedBgObjects.Count == 1)
            {
                firstXChange = xChange;
                continue;
            }

            //Build object shadow by referring to previous object
            var lastCastPoint = orderedBgObjects[1].CastPoint.position;
            bgObject.BuildShadow(lastCastPoint, lightSlope);      
        }

        //Create shadow for first/last object
        Vector2 firstCastPoint = new(HalfWidth - firstXChange, orderedBgObjects[0].CastPoint.position.y);
        orderedBgObjects[^1].BuildShadow(firstCastPoint, lightSlope);
        return orderedBgObjects;

    }
    private void BuildObjectProjection(BgObject bgObject, float lightSlope)
    {
        if (_projectionPointDict.ContainsKey(bgObject.Type))
        {
            bgObject.InterceptProjectionPoints = _projectionPointDict[bgObject.Type];
        }
        else
        {
            _projectionPointDict[bgObject.Type] = bgObject.BuildShadowInterceptPoints(lightSlope);
        }
    }

    private void OnObjectAdded(BgObject bgObject, ListSection section)
    {
        Debug.Log($"Turning on {section}");
        bgObject.gameObject.SetActive(true);
    }

    private void OnObjectRemoved(BgObject bgObject, ListSection section)
    {
        Debug.Log($"Turning off {section}");
        bgObject.gameObject.SetActive(false);
    }

    private void ShiftTrailingToFront()
    {
        var trailingObject = _bgObjectPositionalList.AllObjects[0];
        trailingObject.transform.position = new(_leadingObjectX + trailingObject.XDistance, trailingObject.Position.y);
        _bgObjectPositionalList.AllObjects.RemoveAt(0);
        _bgObjectPositionalList.AllObjects.Add(trailingObject);
    }

    private void ShiftLeadingToBack()
    {
        var lastIndex = _bgObjectPositionalList.AllObjects.Count - 1;
        var leadingObject = _bgObjectPositionalList.AllObjects[lastIndex];
        leadingObject.transform.position = new(_trailingObjectX - leadingObject.XDistance, leadingObject.Position.y);
        _bgObjectPositionalList.AllObjects.RemoveAt(lastIndex);
        _bgObjectPositionalList.AllObjects.Insert(0, leadingObject);
    }

}
