using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
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

    }
    public void OnSegmentBecomeInvisible(GroundSegment segment)
    {

    }
    #endregion
}
