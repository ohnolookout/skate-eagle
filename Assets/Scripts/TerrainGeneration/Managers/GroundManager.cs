using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Com.LuisPedroFonseca.ProCamera2D;
using System.Linq;
public class GroundManager : MonoBehaviour
{
    #region Declarations
    [SerializeField] private GameObject _terrainPrefab;
    [SerializeField] private FinishLine _finishLine;
    [SerializeField] private StartLine _startLine;
    public GroundSpawner groundSpawner;
    public GameObject groundContainer;
    [SerializeField] private List<Rigidbody2D> _normalBodies, _ragdollBodies;
    public FinishLine FinishLine { get => _finishLine; }
    public StartLine StartLine { get => _startLine; }
    public ICameraTargetable[] CameraTargetables => GetComponentsInChildren<ICameraTargetable>();
    #endregion

    #region Monobehaviors

    private void OnDestroy()
    {
        ClearGround();
    }

    public void ClearGround()
    {
        _finishLine.gameObject.SetActive(false);

        while (groundContainer.transform.childCount > 0)
        {
            DestroyImmediate(groundContainer.transform.GetChild(0).gameObject);
        }
    }

    public GameObject GetGameObjectByIndices(int[] targetIndices)
    {
        var grounds = GetGrounds();
        if (targetIndices == null || targetIndices.Length == 0)
        {
            Debug.LogWarning($"GetGameObjectByIndices: No GameObject found due to empty indices");
            return null;
        }

        if (targetIndices[0] < grounds.Length)
        {
            if(targetIndices.Length == 1)
            {
                return grounds[targetIndices[0]].gameObject;
            }

            return grounds[targetIndices[0]].CurvePointObjects[targetIndices[1]].gameObject;
        }

        //Add more types to reflect serialization/deserialization order as needed

        Debug.LogWarning($"GetGameObjectByIndices: No GameObject found for indices {string.Join(", ", targetIndices)}");
        return null;
    }

    public Ground[] GetGrounds()
    {
        return groundContainer.GetComponentsInChildren<Ground>();
    }
    #endregion
}
