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

    public float ParallaxRatio = 0.8f;
    private float _width;
    private float _positionCoefficient;
    public int CameraBuffer = 30;


    void Start()
    {
        var panelSequence = RandomIndexOrder(_bgPanelPool.Count, _panelCount);
        _width = _bgPanelPool[0].XWidth;
        _positionCoefficient = (-_panelCount / 2) + 0.5f;

        _cameraOperator = Camera.main.GetComponent<ICameraOperator>();

        foreach (var index in panelSequence)
        {
            AddBgPanel(_bgPanelPool[index]);
        }

        _panelPositionalList = DoublePositionalListFactory<BgPanel>.CameraOperatorTracker(
            _orderedBgPanels,
            _cameraOperator,
            CameraBuffer,
            CameraBuffer
        //OnObjectAdded (need to add to factory)
        //OnObjectRemoved
        );

        _objectPositionalList = PositionalListFactory<PositionObject<GameObject>>.CameraOperatorTracker(
            _orderedBgObjects,
            _cameraOperator,
            CameraBuffer * 2,
            CameraBuffer * 2
            //OnObjectAdded
            //OnObjectRemoved
        );

    }
   
    void Update()
    {
        
    }
    void FixedUpdate()
    {
        _panelPositionalList.Update();
        _objectPositionalList.Update();
    }

    private void AddBgPanel(BgPanel panel)
    {
        panel.transform.localPosition = new(_positionCoefficient + _orderedBgPanels.Count, panel.transform.localPosition.y);
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
}
