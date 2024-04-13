using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class GroundColliderManager
{
    #region Declarations
    private List<PositionalEdgeCollider> _positionalColliders;
    private Dictionary<PositionalEdgeCollider, int> _colliderStatusDict;
    private int _containmentBuffer = 10;
    private List<Rigidbody2D> _normalBodies, _ragdollBodies;
    private List<DoublePositionalList<PositionalEdgeCollider>> _normalBodyColliders, _ragdollBodyColliders, _activeColliderList;
    private List<PositionalEdgeCollider> _toActivate, _toDeactivate;
    private LevelTerrain _terrain;
    public Action OnActivateLastSegment;
    #endregion

    #region Constructor
    public GroundColliderManager(List<Rigidbody2D> normalBodies, List<Rigidbody2D> ragdollBodies, LevelTerrain terrain, int startingIndex = 0)
    {
        LevelManager.OnGameOver += _ => SwitchToRagdoll();
        _normalBodies = normalBodies;
        _ragdollBodies = ragdollBodies;
        _terrain = terrain;
        _toActivate = new();
        _toDeactivate = new();
        InstantiatePositionalLists();
        ActivateCurrentColliders();
    }
    #endregion

    #region Update Behaviors
    private void ActivateCurrentColliders()
    {
        foreach(var colliderList in _activeColliderList)
        {
            foreach(var collider in colliderList.CurrentObjects)
            {
                ColliderAdded(collider, ListSection.Leading);
            }
        }
    }

    public void Update()
    {
        foreach (var colliderList in _activeColliderList)
        {
            colliderList.Update();
        }

        foreach (var collider in _toDeactivate)
        {
            collider.Collider.gameObject.SetActive(false);
        }
        _toDeactivate = new();

        foreach (var collider in _toActivate)
        {
            collider.Collider.gameObject.SetActive(true);
        }
        _toActivate = new();

    }

    private void SwitchToRagdoll()
    {
        _activeColliderList = _ragdollBodyColliders;
        _colliderStatusDict = _positionalColliders.ToDictionary(collider => collider, _ => 0);
        foreach (var positionalList in _activeColliderList)
        {
            positionalList.FindInitialValues();
        }
    }
    #endregion

    #region Activation Management
    private void ColliderAdded(PositionalEdgeCollider collider, ListSection listSection)
    {
        _colliderStatusDict[collider] += 1;
        if(_colliderStatusDict[collider] == 1)
        {
            _toActivate.Add(collider);
        }
    }

    private void ColliderRemoved(PositionalEdgeCollider collider, ListSection listSection)
    {
        _colliderStatusDict[collider] = Mathf.Max(_colliderStatusDict[collider] - 1, 0);
        if(_colliderStatusDict[collider] < 1)
        {
            _toDeactivate.Add(collider);
        }
    }

    #endregion

    #region Build Positional Lists
    private void InstantiatePositionalLists()
    {
        _positionalColliders = _terrain.PositionalColliderList;
        _colliderStatusDict = _positionalColliders.ToDictionary(collider => collider, _ => 0);
        _normalBodyColliders = PositionalListsFromBodies(_normalBodies, _positionalColliders);
        SubscribeToAllListsEvents(_normalBodyColliders);
        _ragdollBodyColliders = PositionalListsFromBodies(_ragdollBodies, _positionalColliders);
        SubscribeToAllListsEvents(_ragdollBodyColliders);
        _activeColliderList = _normalBodyColliders;
    }

    private List<DoublePositionalList<PositionalEdgeCollider>> PositionalListsFromBodies(
        List<Rigidbody2D> bodies, List<PositionalEdgeCollider> colliders)
    {
        List<DoublePositionalList<PositionalEdgeCollider>> allLists = new();
        foreach(var body in bodies)
        {
            var newList = DoublePositionalListFactory<PositionalEdgeCollider>.BodyTracker(
                colliders, body, _containmentBuffer, _containmentBuffer);
            allLists.Add(newList);
        }

        return allLists;

    }

    private void SubscribeToAllListsEvents(List<DoublePositionalList<PositionalEdgeCollider>> allLists)
    {
        foreach(var list in allLists)
        {
            list.OnObjectAdded += ColliderAdded;
            list.OnObjectRemoved += ColliderRemoved;
        }
    }

    #endregion
}
