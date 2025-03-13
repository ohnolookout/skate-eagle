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
    public GroundSpawner groundSpawner;
    private List<Ground> _grounds;
    public GameObject groundContainer;
    [SerializeField] private List<Rigidbody2D> _normalBodies, _ragdollBodies;
    public List<Ground> Grounds { get => _grounds; set => _grounds = value; }
    #endregion

    #region Monobehaviors

    void OnEnable()
    {
        GroundSegment.OnSegmentBecomeVisible += OnSegmentBecomeVisible;
        GroundSegment.OnSegmentBecomeInvisible += OnSegmentBecomeInvisible;
    }

    private void OnDisable()
    {
        ClearGround();
        GroundSegment.OnSegmentBecomeVisible -= OnSegmentBecomeVisible;
        GroundSegment.OnSegmentBecomeInvisible -= OnSegmentBecomeInvisible;
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

    public void OnSegmentBecomeVisible(GroundSegment segment)
    {
        ProCamera2D.Instance.AddCameraTarget(segment.HighPoint, 0, 0.15f, 0, new(-0.13f, 0.13f));
        ProCamera2D.Instance.AddCameraTarget(segment.LowPoint, 0, 0.75f, 0, new(-0.13f, 0));
    }
    public void OnSegmentBecomeInvisible(GroundSegment segment)
    {
        ProCamera2D.Instance.RemoveCameraTarget(segment.HighPoint);
        ProCamera2D.Instance.RemoveCameraTarget(segment.LowPoint);
    }
    #endregion
}
