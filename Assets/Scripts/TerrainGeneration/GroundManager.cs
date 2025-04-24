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

    public GameObject GetGameObjectByIndices(int[] targetIndices)
    {
        if (targetIndices == null || targetIndices.Length == 0)
        {
            Debug.LogWarning($"GetGameObjectByIndices: No GameObject found due to empty indices");
            return null;
        }

        if (targetIndices[0] < _grounds.Count)
        {
            if(targetIndices.Length == 1)
            {
                return _grounds[targetIndices[0]].gameObject;
            }

            return _grounds[0].SegmentList[targetIndices[1]].gameObject;
        }

        //Add more types to reflect serialization/deserialization order as needed

        Debug.LogWarning($"GetGameObjectByIndices: No GameObject found for indices {string.Join(", ", targetIndices)}");
        return null;
    }
    #endregion
}
