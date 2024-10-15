using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundContainer : MonoBehaviour
{
    private Vector3 startPosition, startScale;
    private Camera mainCam;
    private ICameraOperator camScript;
    private float camSize;
    public float scaleRatio, scaleChange;
    public static List<int> BgPanelSequence;
    [SerializeField] private int _bgPanelPoolCount = 6;
    [SerializeField] private int _bgPanelSequenceCount = 6;

    private void Awake()
    {
        startPosition = transform.position;
        startScale = transform.localScale;
        BgPanelSequence = RandomIndexOrder(_bgPanelPoolCount, _bgPanelSequenceCount);
    }
    void Start()
    {
        mainCam = Camera.main;
        camScript = mainCam.GetComponent<ICameraOperator>();
        camSize = mainCam.orthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        float camSizeChange = mainCam.orthographicSize - camSize;
        scaleChange = (camSizeChange / camSize) * scaleRatio;
        transform.localScale = startScale * (1 + scaleChange);
        transform.localPosition = startPosition - new Vector3(0, camScript.Zoom.ZoomYDelta/4, 0);
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
