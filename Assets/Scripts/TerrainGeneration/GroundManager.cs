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
    [SerializeField] private FinishLine _finishLine;
    public GroundSpawner groundSpawner;
    private List<Ground> _grounds;
    public GameObject groundContainer;
    [SerializeField] private List<Rigidbody2D> _normalBodies, _ragdollBodies;
    public List<Ground> Grounds { get => _grounds; set => _grounds = value; }
    public FinishLine FinishLine { get => _finishLine;}
    #endregion

    #region Monobehaviors

    private void OnDestroy()
    {
        ClearGround();
    }

    public void ClearGround()
    {
        _finishLine.gameObject.SetActive(false);
        _grounds = new();

        while (groundContainer.transform.childCount > 0)
        {
            DestroyImmediate(groundContainer.transform.GetChild(0).gameObject);
        }
    }

    #endregion
}
