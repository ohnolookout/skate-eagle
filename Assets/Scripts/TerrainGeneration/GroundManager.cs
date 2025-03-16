using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Com.LuisPedroFonseca.ProCamera2D;
public class GroundManager : MonoBehaviour
{
    #region Declarations
    [SerializeField] private GameObject _terrainPrefab;
    [SerializeField] private GameObject _finishFlagPrefab;
    [SerializeField] private GameObject _backstopPrefab;
    [SerializeField] private GameObject _finishFlag;
    [SerializeField] private GameObject _backstop;
    private ProCamera2D _camera;
    public GroundSpawner groundSpawner;
    private List<Ground> _grounds;
    public GameObject groundContainer;
    [SerializeField] private List<Rigidbody2D> _normalBodies, _ragdollBodies;
    public List<Ground> Grounds { get => _grounds; set => _grounds = value; }
    #endregion

    #region Monobehaviors
    private void Awake()
    {
        SubscribeToSegmentEvents();
        _camera = ProCamera2D.Instance;
    }

    private void Start()
    {
    }


    private void OnDestroy()
    {
        ClearGround();
        UnsubscribeToSegmentEvents();
    }

    public void ClearGround()
    {
        groundSpawner.ClearStartFinishObjects();
        _grounds = new();

        while (groundContainer.transform.childCount > 0)
        {
            DestroyImmediate(groundContainer.transform.GetChild(0).gameObject);
        }
    }

    private void SubscribeToSegmentEvents()
    {
        GroundSegment.OnSegmentBecomeVisible += OnSegmentBecomeVisible;
        GroundSegment.OnSegmentBecomeInvisible += OnSegmentBecomeInvisible;
    }

    private void UnsubscribeToSegmentEvents()
    {
        GroundSegment.OnSegmentBecomeVisible -= OnSegmentBecomeVisible;
        GroundSegment.OnSegmentBecomeInvisible -= OnSegmentBecomeInvisible;
    }

    private void OnSegmentBecomeVisible(GroundSegment segment)
    {
        _camera.AddCameraTarget(segment.HighPoint, 0, 0.15f, 0.25f, new(0, -12));
        _camera.AddCameraTarget(segment.LowPoint, 0, 1f, 0.25f, new(0, 12));
    }
    private void OnSegmentBecomeInvisible(GroundSegment segment)
    {
        _camera.RemoveCameraTarget(segment.HighPoint);
        _camera.RemoveCameraTarget(segment.LowPoint);
    }
    #endregion
}
