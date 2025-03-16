using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundContainer : MonoBehaviour
{
    private Vector3 startPosition, startScale;
    private Camera mainCam;
    private Transform _camTransform;
    private float initialCamSize;
    private float initialCamX;
    public float scaleRatio, scaleChange;
    public static List<int> BgPanelSequence;
    [SerializeField] private int _bgPanelPoolCount = 6;
    [SerializeField] private int _bgPanelSequenceCount = 6;

    private void Awake()
    {
        BgPanelSequence = RandomIndexOrder(_bgPanelPoolCount, _bgPanelSequenceCount);
    }
    void Start()
    {
        mainCam = Camera.main;
        _camTransform = mainCam.transform;
        transform.position = _camTransform.position;
        startPosition = transform.localPosition;
        startScale = transform.localScale;
        initialCamX = _camTransform.position.x;
        initialCamSize = mainCam.orthographicSize;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float camSizeChange = mainCam.orthographicSize - initialCamSize;
        float camXChange = _camTransform.position.x - initialCamX;
        scaleChange = (camSizeChange / initialCamSize) * scaleRatio;
        transform.localScale = startScale * (1 + scaleChange);
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

}
