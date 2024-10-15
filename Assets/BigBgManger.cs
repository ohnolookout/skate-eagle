using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigBgManger : MonoBehaviour
{
    [SerializeField] private List<BgPanel> _bgPanelPool;
    [SerializeField] private int _panelCount;
    [SerializeField] private int _panelOrder;
    [SerializeField] private Transform _leftAnchor;
    [SerializeField] private Transform _rightAnchor;
    public bool HasSharedObjectPool = false;

    private List<BgPanel> _orderedBgPanels = new();
    private List<CitySprite> _orderedSpriteObjects = new();
    private DoublePositionalList<CitySprite> _spriteObjectPositionalList;
    private DoublePositionalList<BgPanel> _panelPositionalList;
    private ICameraOperator _cameraOperator;
    private Vector2 _startPosition;
    public float ParallaxRatio = 0.8f;
    private float _panelWidth;
    private float _totalWidth;
    private float _positionCoefficient;
    private bool _hasIndividualSprites;
    public int CameraBuffer = 30;

    private float _currentHalfWidth;
    private float _currentPanelWidth => _panelPositionalList.AllObjects[0].XWidth;

    private float _trailingObjectX => _panelPositionalList.AllObjects[0].Position.x;
    private float _leadingObjectX => _panelPositionalList.AllObjects[^1].Position.x;
    private float _trailingBoundX => _cameraOperator.gameObject.transform.position.x - _currentHalfWidth - CameraBuffer;
    private float _leadingBoundX => _cameraOperator.gameObject.transform.position.x + _currentHalfWidth + CameraBuffer;


    void Start()
    {
        _startPosition = transform.position;
        _cameraOperator = Camera.main.GetComponent<ICameraOperator>();
        _panelWidth = _bgPanelPool[0].XWidth;
        _totalWidth = _panelWidth * _panelCount;
        _positionCoefficient = (-_panelCount / 2) + 0.5f;

        _cameraOperator = Camera.main.GetComponent<ICameraOperator>();

        _leftAnchor.transform.position = new(-_totalWidth / 2, 0);
        _rightAnchor.transform.position = new(_totalWidth / 2, 0);
        _currentHalfWidth = (_rightAnchor.position.x - _leftAnchor.position.x) / 2;

        List<int> indexSequence;
        if (HasSharedObjectPool)
        {
            indexSequence = BackgroundContainer.BgPanelSequence.GetRange(_panelOrder * _panelCount, _panelCount);
        }
        else
        {
            indexSequence = BackgroundContainer.RandomIndexOrder(_panelCount, _panelCount);
        }
        foreach (var index in indexSequence)
        {
            AddPanelToBg(_bgPanelPool[index]);
        }

        _hasIndividualSprites = _orderedSpriteObjects.Count > 0;

        _panelPositionalList = DoublePositionalListFactory<BgPanel>.CameraOperatorTracker(
            _orderedBgPanels,
            _cameraOperator,
            CameraBuffer,
            CameraBuffer,
            OnPanelAdded,
            OnPanelRemoved
        );

        if (_hasIndividualSprites)
        {
            _spriteObjectPositionalList = DoublePositionalListFactory<CitySprite>.CameraOperatorTracker(
                _orderedSpriteObjects,
                _cameraOperator,
                CameraBuffer,
                CameraBuffer,
                OnSpriteAdded,
                OnSpriteRemoved
            );
        }

    }
    void Update()
    {
        _panelPositionalList.Update();

        if (_hasIndividualSprites)
        {
            _spriteObjectPositionalList.Update();
        }


        _currentHalfWidth = (_rightAnchor.position.x - _leftAnchor.position.x) / 2;
        float xDelta = _cameraOperator.gameObject.transform.position.x * ParallaxRatio;
        float camLayerDelta = _cameraOperator.gameObject.transform.position.x * (1 - ParallaxRatio);
        float expectedPercentWidthFromCamera = camLayerDelta / _panelWidth/2;
        float currentPercentWidthFromCamera = camLayerDelta / _currentHalfWidth;
        float lengthDifference = (expectedPercentWidthFromCamera - currentPercentWidthFromCamera) * _currentHalfWidth;
        transform.position = new Vector3(_startPosition.x + xDelta - lengthDifference, transform.position.y, transform.position.z);

        //Shift objects from back to front if they exceed bounds
        //Use object's distance to determine new x coord

        if (_trailingObjectX <= _trailingBoundX)
        {
            var trailingObject = _panelPositionalList.AllObjects[0];
            _panelPositionalList.MoveTrailingToLeading(new(_leadingObjectX + _currentPanelWidth, trailingObject.Position.y));
            if (_hasIndividualSprites)
            {
                _spriteObjectPositionalList.OrderTrailingToLeading(trailingObject.SpriteObjects.Count);
            }
        }
        else if (_leadingObjectX >= _leadingBoundX)
        {
            var leadingObject = _panelPositionalList.AllObjects[^1];
            _panelPositionalList.MoveLeadingToTrailing(new(_trailingObjectX - _currentPanelWidth, leadingObject.Position.y));
            if (_hasIndividualSprites)
            {
                _spriteObjectPositionalList.OrderLeadingToTrailing(leadingObject.SpriteObjects.Count);
            }
        }
    }
    
    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new(_trailingBoundX, -300), new(_trailingBoundX, 300));
        Gizmos.DrawLine(new(_leadingBoundX, -300), new(_leadingBoundX, 300));

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(new(_leadingObjectX, 0), 25);
        Gizmos.DrawSphere(new(_trailingObjectX, 0), 25);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(new(_leadingObjectX + _currentPanelWidth, 0), 25);
        Gizmos.DrawSphere(new(_trailingObjectX - _currentPanelWidth, 0), 25);
    }
    
    private void AddPanelToBg(BgPanel panel)
    {
        //Debug.Log("Adding panel to orderdBgPanels at X position: " + (_positionCoefficient + _orderedBgPanels.Count) * _panelWidth);
        panel.gameObject.SetActive(true);
        panel.transform.localPosition = new((_positionCoefficient + _orderedBgPanels.Count) * _panelWidth, panel.transform.localPosition.y);
        _orderedBgPanels.Add(panel);
        if (panel.SpriteObjects.Count > 0)
        {
            _orderedSpriteObjects.AddRange(panel.SpriteObjects);
        }
    }

    

    #region Add/Remove
    private void OnPanelAdded(BgPanel addedPanel, ListSection section)
    {

    }

    private void OnPanelRemoved(BgPanel removedPanel, ListSection section)
    {

    }

    private void OnSpriteAdded(CitySprite addedObj, ListSection section)
    {
        Debug.Log("Sprite added...");
        addedObj.gameObject.SetActive(true);
    }

    private void OnSpriteRemoved(CitySprite removedObj, ListSection section)
    {
        Debug.Log("Sprite removed...");
        removedObj.gameObject.SetActive(false);
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
