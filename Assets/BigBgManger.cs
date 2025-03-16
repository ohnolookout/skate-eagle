using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigBgManger : MonoBehaviour
{
    #region Declarations
    [SerializeField] private List<BgPanel> _bgPanelPool;
    [SerializeField] private int _panelCount;
    [SerializeField] private int _panelOrder;
    [SerializeField] private Transform _leftAnchor;
    [SerializeField] private Transform _rightAnchor;
    private Camera _cam;
    private Transform _camTransform;
    public bool HasSharedObjectPool = false;

    private List<BgPanel> _orderedBgPanels;
    //private List<CitySprite> _orderedSpriteObjects = new();
    //private DoublePositionalList<CitySprite> _spriteObjectPositionalList;
    private DoublePositionalList<BgPanel> _panelPositionalList;
    //private ICameraOperator _cameraOperator;
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
    private float _trailingBoundX => Camera.main.transform.position.x - _currentHalfWidth - CameraBuffer;
    private float _leadingBoundX => _camTransform.position.x + _currentHalfWidth + CameraBuffer;
    #endregion

    #region Monobehaviours
    private void Awake()
    {
        _panelWidth = _bgPanelPool[0].XWidth;
        _totalWidth = _panelWidth * _panelCount;
        _positionCoefficient = (-_panelCount / 2) + 0.5f;
        _cam = Camera.main;
        _camTransform = _cam.transform;
        LevelManager.OnRestart += Restart;
    }
    void Start()
    {
        _startPosition = transform.position;
        _leftAnchor.transform.position = new(-_totalWidth / 2, 0);
        _rightAnchor.transform.position = new(_totalWidth / 2, 0);
        _currentHalfWidth = (_rightAnchor.position.x - _leftAnchor.position.x) / 2;

        _orderedBgPanels = new();
        ArrangePanels();
        BuildPositionalLists();

    }
    void Update()
    {
        _panelPositionalList.Update();

        _currentHalfWidth = (_rightAnchor.position.x - _leftAnchor.position.x) / 2;
        float xDelta = _camTransform.position.x * ParallaxRatio;
        float camLayerDelta = _camTransform.position.x * (1 - ParallaxRatio);
        float expectedPercentWidthFromCamera = camLayerDelta / _panelWidth/2;
        float currentPercentWidthFromCamera = camLayerDelta / _currentHalfWidth;
        float lengthDifference = (expectedPercentWidthFromCamera - currentPercentWidthFromCamera) * _currentHalfWidth;
        transform.position = new Vector3(_startPosition.x + xDelta - lengthDifference, transform.position.y, transform.position.z);

    }

    private void Restart()
    {
        Start();
    }
    /*
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
    */
    #endregion

    #region Initialization
    private void ArrangePanels()
    {
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

    }

    private void AddPanelToBg(BgPanel panel)
    {
        panel.gameObject.SetActive(true);
        panel.transform.localPosition = new((_positionCoefficient + _orderedBgPanels.Count) * _panelWidth, panel.transform.localPosition.y);
        _orderedBgPanels.Add(panel);

    }
    
    private void BuildPositionalLists()
    {

        _panelPositionalList = DoublePositionalListFactory<BgPanel>.CameraTracker(
            _orderedBgPanels,
            _cam,
            CameraBuffer * 1.5f,
            CameraBuffer * 1.5f,
            OnPanelAdded,
            OnPanelRemoved
        );

    }



    #endregion

    #region Add/Remove
    private void OnPanelAdded(BgPanel addedPanel, ListSection section)
    {
        return;
    }

    private void OnPanelRemoved(BgPanel removedPanel, ListSection section)
    {
        if (section == ListSection.Trailing)
        {
            MoveTrailingPanelToLeading(removedPanel);
        }
        else
        {
            MoveLeadingPanelToTrailing(removedPanel);
        }
    }

    #endregion

    #region MovePanels
    private void MoveTrailingPanelToLeading(BgPanel removedPanel)
    {
        _panelPositionalList.MoveTrailingToLeading(new(_leadingObjectX + _currentPanelWidth, removedPanel.Position.y));
    }

    private void MoveLeadingPanelToTrailing(BgPanel removedPanel)
    {
        _panelPositionalList.MoveLeadingToTrailing(new(_trailingObjectX - _currentPanelWidth, removedPanel.Position.y));
    }

    #endregion

}
