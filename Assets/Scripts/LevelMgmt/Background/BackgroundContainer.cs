using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundContainer : MonoBehaviour
{
    private Vector3 _startPosition, _startScale;
    private Camera _mainCam;
    private Transform _camTransform;
    private const float _defaultCamSize = 34;
    private float _initialCamSize;
    private float _initialCamX;
    public float scaleRatio, scaleChange;
    public static List<int> BgPanelSequence;
    [SerializeField] private int _bgPanelPoolCount = 6;
    [SerializeField] private int _bgPanelSequenceCount = 6;

    private void Awake()
    {
        LevelManager.OnRestart += GenerateSequence;
        GenerateSequence();
    }
    void Start()
    {
        _mainCam = Camera.main;
        _camTransform = _mainCam.transform;
        transform.position = _camTransform.position;
        _startPosition = transform.localPosition;
        _startScale = transform.localScale;
        _initialCamX = _camTransform.position.x;
        _initialCamSize = _defaultCamSize;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float camSizeChange = _mainCam.orthographicSize - _initialCamSize;
        float camXChange = _camTransform.position.x - _initialCamX;
        scaleChange = (camSizeChange / _initialCamSize) * scaleRatio;
        transform.localScale = _startScale * (1 + scaleChange);
        transform.localPosition = _camTransform.position - new Vector3(0, camSizeChange/ 2, 0);
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
            var randomSelection = Random.Range(0, indexPool.Count);
            returnSequence.Add(indexPool[randomSelection]);
            indexPool.RemoveAt(randomSelection);
        }

        return returnSequence;
    }

    public void GenerateSequence()
    {
        BgPanelSequence = RandomIndexOrder(_bgPanelPoolCount, _bgPanelSequenceCount);
    }

}
