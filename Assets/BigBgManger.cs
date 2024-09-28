using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigBgManger : MonoBehaviour
{
    [SerializeField] private List<BgPanel> _bgPanelPool;
    [SerializeField] private int _panelCount;
    [SerializeField] private Transform _leftAnchor;
    [SerializeField] private Transform _rightAnchor;

    private List<BgPanel> _orderedBgPanels = new();
    private List<PositionObject<GameObject>> _orderedBgObjects = new();
    private PositionalList<PositionObject<GameObject>> _objectPositionalList;
    private DoublePositionalList<BgPanel> _panelPositionalList;
    private ICameraOperator _cameraOperator;
    private Vector2 _startPosition;
    public float ParallaxRatio = 0.8f;
    private float _panelWidth;
    private float _totalWidth;
    private float _positionCoefficient;
    public int CameraBuffer = 30;

    private float _trailingObjectX => _panelPositionalList.AllObjects[0].StartPosition.x;
    private float _leadingObjectX => _panelPositionalList.AllObjects[^1].EndPosition.x;
    private float _trailingBoundX => _cameraOperator.gameObject.transform.position.x - (_totalWidth /2) - CameraBuffer;
    private float _leadingBoundX => _cameraOperator.gameObject.transform.position.x + (_totalWidth / 2) + CameraBuffer;


    void Start()
    {
        _startPosition = transform.position;
        _cameraOperator = Camera.main.GetComponent<ICameraOperator>();
        _panelWidth = _bgPanelPool[0].XWidth;
        _totalWidth = _panelWidth * _panelCount;
        _positionCoefficient = (-_panelCount / 2) + 0.5f;

        _cameraOperator = Camera.main.GetComponent<ICameraOperator>();

        var panelSequence = RandomIndexOrder(_bgPanelPool.Count, _panelCount);
        foreach (var index in panelSequence)
        {
            AddPanelToBg(_bgPanelPool[index]);
        }

        _panelPositionalList = DoublePositionalListFactory<BgPanel>.CameraOperatorTracker(
            _orderedBgPanels,
            _cameraOperator,
            CameraBuffer,
            CameraBuffer,
            OnPanelAdded,
            OnPanelRemoved
        );
        Debug.Log($"{_orderedBgObjects.Count} ordered bg objects");
        foreach(var obj in _orderedBgObjects)
        {
            Debug.Log($"{obj.Position}");
        }
        _objectPositionalList = PositionalListFactory<PositionObject<GameObject>>.CameraOperatorTracker(
            _orderedBgObjects,
            _cameraOperator,
            CameraBuffer * 2,
            CameraBuffer * 2,
            OnObjectAdded,
            OnObjectRemoved
        );
        _objectPositionalList.DoLog = true;

    }

    void Update()
    {

    }
    void FixedUpdate()
    {
        _panelPositionalList.Update();
        _objectPositionalList.Update();


        float currentHalfLayerWidth = (_rightAnchor.position.x - _leftAnchor.position.x) / 2;
        float xDelta = _cameraOperator.gameObject.transform.position.x * ParallaxRatio;
        float camLayerDelta = _cameraOperator.gameObject.transform.position.x * (1 - ParallaxRatio);
        float expectedPercentWidthFromCamera = camLayerDelta / _panelWidth/2;
        float currentPercentWidthFromCamera = camLayerDelta / currentHalfLayerWidth;
        float lengthDifference = (expectedPercentWidthFromCamera - currentPercentWidthFromCamera) * currentHalfLayerWidth;
        transform.position = new Vector3(_startPosition.x + xDelta - lengthDifference, transform.position.y, transform.position.z);

        //Shift objects from back to front if they exceed bounds
        //Use object's distance to determine new x coord

        if (_trailingObjectX <= _trailingBoundX)
        {
            var trailingObject = _panelPositionalList.AllObjects[0];
            _panelPositionalList.MoveTrailingToLeading(new(_leadingObjectX + _panelWidth, trailingObject.Position.y));
        }
        else if (_leadingObjectX >= _leadingBoundX)
        {
            var leadingObject = _panelPositionalList.AllObjects[^1];
            _panelPositionalList.MoveLeadingToTrailing(new(_trailingObjectX - _panelWidth, leadingObject.Position.y));
        }
    }

    private void AddPanelToBg(BgPanel panel)
    {
        Debug.Log("Adding panel to orderdBgPanels at X position: " + (_positionCoefficient + _orderedBgPanels.Count) * _panelWidth);
        panel.transform.localPosition = new((_positionCoefficient + _orderedBgPanels.Count) * _panelWidth, panel.transform.localPosition.y);
        _orderedBgPanels.Add(panel);
        _orderedBgObjects.AddRange(panel.PositionBgObjects);
    }

    public static List<int> RandomIndexOrder(int poolSize, int listSize)
    {
        var indexPool = new List<int>();
        var returnSequence = new List<int>();
        for (int i = 0; i < poolSize; i++)
        {
            indexPool.Add(i);
        }
        while (returnSequence.Count < listSize)
        {
            var randomSelection = UnityEngine.Random.Range(0, indexPool.Count);
            returnSequence.Add(indexPool[randomSelection]);
            indexPool.RemoveAt(randomSelection);
        }

        return returnSequence;
    }

    #region Add/Remove
    private void OnPanelAdded(BgPanel addedPanel, ListSection section)
    {

    }

    private void OnPanelRemoved(BgPanel removedPanel, ListSection section)
    {

    }

    private void OnObjectAdded(PositionObject<GameObject> addedObj, ListSection section)
    {
        addedObj.Value.SetActive(true);
    }

    private void OnObjectRemoved(PositionObject<GameObject> removedObj, ListSection section)
    {
        removedObj.Value.SetActive(false);
    }

    #endregion

    #region MovePanels
    private void MoveTrailingPanelToLeading()
    {

    }

    private void MoveLeadingPanelToTrailing()
    {

    }

    #endregion

}
