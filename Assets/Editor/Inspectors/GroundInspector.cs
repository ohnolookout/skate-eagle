using UnityEditor;

[CustomEditor(typeof(Ground))]
public class GroundEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var ground = (Ground)target;
    }
    public void OnSceneGUI()
    {
        var ground = (Ground)target;

        foreach (var point in ground.CurvePointObjects)
        {
            CurvePointObjectInspector.DrawCurvePointHandles(point);
        }
    }
}