using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BgObjectManager : MonoBehaviour
{
    public List<BgObject> BgObjectPool;
    public Transform LightSource;
    public float ParallaxRatio = 0.8f;
    public int HalfWidth = 205;
    public int PositionBuffer = 10;
    public int CameraBuffer = 30;
    public float LocationVariance = 0.2f;
    [SerializeField] private BgShadowDataLibrary _projectionLibrary;
    [SerializeField] private Transform _leftAnchor;
    [SerializeField] private Transform _rightAnchor;
    public bool DoOverwriteProjectionLibrary = false;

    private Vector2 _startPosition;
    private Dictionary<BgObjectType, BgShadowData> _projectionDict;
    private SinglePositionalList<BgObject> _bgObjectPositionalList;
    private ICameraOperator _cameraOperator;

    private float _trailingObjectX => _bgObjectPositionalList.AllObjects[0].Position.x;
    private float _leadingObjectX => _bgObjectPositionalList.AllObjects[^1].Position.x;
    private float _trailingBoundX => _cameraOperator.gameObject.transform.position.x - HalfWidth - PositionBuffer;
    private float _leadingBoundX => _cameraOperator.gameObject.transform.position.x + HalfWidth + PositionBuffer;

    void Start()
    {
        _startPosition = transform.position;
        _projectionDict = new();
        _cameraOperator = Camera.main.GetComponent<ICameraOperator>();
        var orderedBgObjects = BuildBgObjects(LightSource.position.y / LightSource.position.x);
        _bgObjectPositionalList = PositionalListFactory<BgObject>.CameraOperatorTracker(
            orderedBgObjects, _cameraOperator, CameraBuffer, CameraBuffer, OnObjectAdded, OnObjectRemoved);
        _leftAnchor.position = new(-HalfWidth, 0);
        _rightAnchor.position = new(HalfWidth, 0);
    }

    void FixedUpdate()
    {

        _bgObjectPositionalList.Update();

        float currentHalfLayerWidth = (_rightAnchor.position.x - _leftAnchor.position.x)/2;
        float xDelta = _cameraOperator.gameObject.transform.position.x * ParallaxRatio;
        float camLayerDelta = _cameraOperator.gameObject.transform.position.x * (1 - ParallaxRatio);
        float expectedPercentWidthFromCamera = camLayerDelta / HalfWidth;
        float currentPercentWidthFromCamera = camLayerDelta / currentHalfLayerWidth;
        float lengthDifference = (expectedPercentWidthFromCamera - currentPercentWidthFromCamera) * currentHalfLayerWidth;
        transform.position = new Vector3(_startPosition.x + xDelta - lengthDifference, transform.position.y, transform.position.z);

        //Shift objects from back to front if they exceed bounds
        //Use object's distance to determine new x coord
        if (_trailingObjectX <= _trailingBoundX)
        {
            var trailingObject = _bgObjectPositionalList.AllObjects[0];
            _bgObjectPositionalList.MoveTrailingToLeading(new(_leadingObjectX + trailingObject.XDistance, trailingObject.Position.y));
        }
        else if (_leadingObjectX >= _leadingBoundX)
        {
            var leadingObject = _bgObjectPositionalList.AllObjects[^1];
            _bgObjectPositionalList.MoveLeadingToTrailing(new(_trailingObjectX - leadingObject.XDistance, leadingObject.Position.y));
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
            var lastX = orderedBgObjects.Count > 0 ? orderedBgObjects[^1].Position.x : HalfWidth;
            var objectX = lastX - xChange;
            totalDistance += xChange;

            //Assign object to dict
            bgObject.XDistance = xChange;
            bgObject.YDistance = objectY;
            bgObject.transform.position = new(objectX, objectY, 50);
            orderedBgObjects.Add(bgObject);
            BgObjectPool.RemoveAt(index);
            //Go to next object if this is the first object
            if (orderedBgObjects.Count == 1)
            {
                firstXChange = xChange;
                continue;
            }

            //Build object shadow by referring to previous object           
            var lastCastPoint = orderedBgObjects[^2].OutboundCastPoint;
            bgObject.BuildShadow(lastCastPoint, lightSlope);      
        }

        //Create shadow for first/last object
        Vector2 firstCastPoint = new(HalfWidth - firstXChange, orderedBgObjects[^1].OutboundCastPoint.y);
        orderedBgObjects[0].BuildShadow(firstCastPoint, lightSlope);
        orderedBgObjects.Reverse();

        return orderedBgObjects;

    }
    private void BuildObjectProjection(BgObject bgObject, float lightSlope)
    {
        if (DoOverwriteProjectionLibrary || !_projectionLibrary.ProjectionDict.ContainsKey(bgObject.Type))
        {
            DoOverwriteProjectionLibrary = false;
            _projectionLibrary.ProjectionDict[bgObject.Type] = bgObject.BuildInterceptProjection(lightSlope);

        }
        else
        {
            bgObject.InterceptProjection = _projectionLibrary.ProjectionDict[bgObject.Type];
        }
    }

    private void OnObjectAdded(BgObject bgObject, ListSection section)
    {
        bgObject.gameObject.SetActive(true);
    }

    private void OnObjectRemoved(BgObject bgObject, ListSection section)
    {
        bgObject.gameObject.SetActive(false);
    }

}
