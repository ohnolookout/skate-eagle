using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.Rendering.Universal;
using System.Reflection;

public class OldGroundSpline : MonoBehaviour
{
    public SpriteShapeController shapeController;
    public Spline spline;
    private ShadowCaster2D shadow;
    public OldCameraScript cameraScript;
    public int initialCurveCount = 4;
    public float minLength = 10;
    public float maxLength = 30;
    public float minClimb = -5;
    public float maxClimb = 5;
    public float minSlope = 0.2f;
    public float maxSlope = 1.5f;
    public float minVelocity = 3;
    public float maxVelocity = 6;
    public List<CurvePoint> leadingCurvePoints = new List<CurvePoint>();
    public List<CurvePoint> trailingCurvePoints = new List<CurvePoint>(); //List of deleted curve point objects that INCLUDES the current last point in order to preserve that point's tangent information. Points are only removed when they become second to last.
    //CurvePoint contains control point, left tangent, and right tangent.
    private OldCameraFloorPoints cameraFloor = new OldCameraFloorPoints(); //Object that tracks the low points of all the curves in order to keep track of the camera floor's y value
    public OldBezierColliderCreator colliderCreator; //Utility that creates and manages curved edges based on the active sections of the spline.
    private static float lowerBoundY = 200;
    private Vector3[] shadowPointArray;
    public GameObject finishFlag;
    public LogicScript logic; 
    private static BindingFlags accessFlagsPrivate = BindingFlags.NonPublic | BindingFlags.Instance;
    private static FieldInfo meshField = typeof(ShadowCaster2D).GetField("m_Mesh", accessFlagsPrivate);
    private static FieldInfo shapePathField = typeof(ShadowCaster2D).GetField("m_ShapePath", accessFlagsPrivate);
    private static MethodInfo onEnableMethod = typeof(ShadowCaster2D).GetMethod("OnEnable", accessFlagsPrivate);

    void Awake()
    {
        cameraScript = Camera.main.GetComponent<OldCameraScript>();
        spline = shapeController.spline;
        shadow = transform.GetComponent<ShadowCaster2D>();
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicScript>();
        //Hard code first 2 points to get easy start
        spline.isOpenEnded = false;
        //Moves the bottom right corner to the new X
        GenerateTerrain(logic.terrainLimit);
        shapePathField.SetValue(shadow, shadowPointArray);
        meshField.SetValue(shadow, null);
        onEnableMethod.Invoke(shadow, new object[0]);
    }

    private void Update()
    {
        if (logic.Started && !logic.Finished)
        {
            float trailingCameraX = cameraScript.NewTrailingCorner.x;
            float leadingCameraX = cameraScript.NewLeadingCorner.x;
            float trailingSplineX = transform.TransformPoint(spline.GetPosition(1)).x;
            float leadingSplineX = transform.TransformPoint(spline.GetPosition(spline.GetPointCount() - 2)).x;
            if (leadingCurvePoints.Count <= 1) leadingSplineX = transform.TransformPoint(spline.GetPosition(spline.GetPointCount() - 3)).x;
            if( trailingCurvePoints.Count <= 1) trailingSplineX = transform.TransformPoint(spline.GetPosition(2)).x;
            //If the camera's X is a certain amount ahead of the trailing edge of the spline, remove the spline's trailing point
            if (trailingCameraX - 200 > trailingSplineX && leadingCurvePoints.Count > 1) RemoveLeftPoint();
            //If the camera's X is a certain amount behind the trailing edge of the spline, add a deleted point back onto the trailing edge of the spline. Only add left point if there is more than one deleted point stored in leftCurvePoints. Otherwise, player has reached the leftmost end of the level.
            else if (trailingCameraX - 125 < trailingSplineX && trailingCurvePoints.Count > 1) AddLeftPoint(); 
            //If the camera's X is a certain amount behind the leading edge of the spline, remove the spline's leading point
            if (leadingCameraX + 200 < leadingSplineX) RemoveRightPoint();
            //If the camera's X is a certain amount ahead of the leading edge of the spline, add a new leading point.
            //Leading point comes from storage if there are deleted points to use. If not, a new curve is generated.
            else if (leadingCameraX + 125 > leadingSplineX && leadingCurvePoints.Count > 1) AddRightPoint();
        }
    }

    private void GenerateTerrain(float length)
    {
        //Creates first curve using fixed parameters
        CreateFirstCurve();
        //Generate random types of curves
        leadingCurvePoints = GroundUtility.GenerateLevelList(length, leadingCurvePoints[^1], out float generatedLength);
        List<Vector3> shadowList = new List<Vector3>();
        for(int i = 0; i < spline.GetPointCount()-1; i++)
        {
            shadowList.Add(spline.GetPosition(i));
        }
        for(int i = leadingCurvePoints.Count - 1; i > 0; i--)
        {
            shadowList.Add(leadingCurvePoints[i].ControlPoint);
            if (i > 0)
            {
                for(int p = 0; p< 10; p++)
                {
                    shadowList.Add(GroundUtility.GetMidpoint(leadingCurvePoints[i], leadingCurvePoints[i - 1], 0.1f + p*0.1f));
                }
            }
        }
        shadowList.Add(new Vector3(leadingCurvePoints[0].ControlPoint.x, leadingCurvePoints[0].ControlPoint.y-200));
        shadowPointArray = shadowList.ToArray();
        while (transform.TransformPoint(spline.GetPosition(spline.GetPointCount() - 2)).x < cameraScript.NewLeadingCorner.x + 125)
        {
            AddRightPoint();
        }
        logic.ActualTerrainLength = generatedLength;
        logic.FinishPoint = transform.TransformPoint(leadingCurvePoints[1].ControlPoint + new Vector3(50, 1f));
    }

    //spline[0] is always lower left corner, spline[spline.GetPointCount() - 1) is always lower right corner.
    //spline[1] and spline [spline.GetPointCount() - 2] form the upper left and upper right corners, respectively.
    //Returns the last point of the curve;
    private void CreateFirstCurve()
    {
        OldCurve firstCurve = OldCurveModes.InitialCurve(shapeController, GameObject.FindGameObjectWithTag("Player").transform);
        //Creates an initial curve with fixed coordinates to create a standard start.
        GroundUtility.InsertCurve(shapeController, firstCurve, 1);
        trailingCurvePoints.Add(firstCurve.GetPoint(0));
        leadingCurvePoints.Add(firstCurve.GetPoint(firstCurve.Count - 1));
        //Once two curve points are established, the bottom corners are created at fixed distances below the two points and with broken tangents.
        UpdateLowerLeftPoint();
        UpdateLowerRightPoint();
    }


    public void RemoveLeftPoint()
    {
        //Create a new CurvePoint object using the second point (index 2 bc index 0 is bottom corner, so index 1 is first "true" point)
        //This point will become the new first point
        CurvePoint newFirstPoint = GroundUtility.SplineToCurvePoint(shapeController, 2);
        //Remove the first "true" point (index 1) from spline
        spline.RemovePointAt(1);
        //Shift the current midpoints being tracked by the camera floor and collider creators
        cameraFloor.ShiftSpline(-1);
        colliderCreator.ShiftIndices(-1);
        //Set the first point's tangent to be broken and point straight down to keep walls straight.
        UpdateLowerLeftPoint();
        //Add the CurvePoint representing the new 1st "true" point to the list of leftCurvePoints.
        trailingCurvePoints.Add(newFirstPoint);
    }

    public void RemoveRightPoint()
    {
        if(leadingCurvePoints.Count == 1)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        //Create a new CurvePoint object using the second to last point (index coint - 3 bc index count - 1 is bottom corner, so count - 2 is last "true" point)
        int index = spline.GetPointCount() - 3;
        CurvePoint newLastPoint = GroundUtility.SplineToCurvePoint(shapeController, index);
        //Remove the current last point from the spline.
        spline.RemovePointAt(index + 1);
        //Update leading bottom corner
        UpdateLowerRightPoint();
        //Add the curve point for the new last point to the rightCurvePoints list.
        //The last true point on the spline and the last point in rightCurvePoints are always the same in order to preserve the right tangent information of the last point.
        //rightCurvePoints.Add(newLastPoint);
        leadingCurvePoints.Add(newLastPoint);
    }

    public void AddLeftPoint()
    {
        //Left points are always restored from leftCurvePoints because new points are not created going left.
        //Pull the second to last curve point in leftcurvepoints. The last curvepoint is the current first point.
        //Restore the left tangent on the current first point from its data in leftCurvePoints
        if (trailingCurvePoints.Count >= 2) {
            CurvePoint newPoint = RestoreLeftCorner();
            GroundUtility.InsertCurvePoint(shapeController, newPoint, 1);
            //Shift the current indices for the camera floor and collider to account for the new point
            cameraFloor.ShiftSpline(1);
            colliderCreator.ShiftIndices(1);
            //Update the trailing bottom corner
            UpdateLowerLeftPoint();
        }
    }

    public void AddRightPoint()
    {
        int index = spline.GetPointCount() - 1;
        CurvePoint newPoint = RestoreRightCorner();
        GroundUtility.InsertCurvePoint(shapeController, newPoint, spline.GetPointCount() - 1);
        if (leadingCurvePoints.Count == 1)
        {
            //Otherwise, create a new curve and store the new last point as the only point in rightCurvePoints.
            CreateFinishLine();
        }
        //Update the lower bottom corner.
        UpdateLowerRightPoint();

    }

    public void UpdateLowerRightPoint()
    {
        //Reassigns the lower right corner (last index on the spline) to the same x as the preceding point and the y of the preceding point - the lowerBoundY buffer.
        int lastIndex = spline.GetPointCount() - 1;
        spline.SetPosition(lastIndex, new Vector3(spline.GetPosition(lastIndex - 1).x, spline.GetPosition(lastIndex - 1).y - lowerBoundY));
        //Resets the corner point's tangent mode in case it was changed.
        spline.SetTangentMode(lastIndex - 1, ShapeTangentMode.Broken);
        spline.SetRightTangent(lastIndex - 1, new Vector2(0, -1));
    }
    public void UpdateLowerLeftPoint()
    {
        spline.SetPosition(0, new Vector3(spline.GetPosition(1).x, spline.GetPosition(1).y - lowerBoundY));
        spline.SetTangentMode(1, ShapeTangentMode.Broken);
        spline.SetLeftTangent(1, new Vector2(0, -1));
    }


    public void CreateFinishLine()
    {
        logic.TerrainCompleted = true;
        Instantiate(finishFlag, logic.FinishPoint, transform.rotation, transform);
    }  

    //Restores the tangent on current leading corner from information in rightCurvePoints, deletes that information from rightCurvePoints, and returns the new last CurvePoint in rightCurvePoints
    private CurvePoint RestoreRightCorner()
    {
        //Restore the current last points right tangent using the last CurvePoint in rightCurvePoints
        spline.SetTangentMode(spline.GetPointCount() - 2, ShapeTangentMode.Continuous);
        spline.SetRightTangent(spline.GetPointCount() - 2, leadingCurvePoints[^1].RightTangent);
        //Remove the last point from rightCurvePoints
        leadingCurvePoints.RemoveAt(leadingCurvePoints.Count - 1);
        return leadingCurvePoints[^1];
    }

    private CurvePoint RestoreLeftCorner()
    {
        spline.SetTangentMode(1, ShapeTangentMode.Continuous);
        spline.SetLeftTangent(1, trailingCurvePoints[^1].LeftTangent);
        trailingCurvePoints.RemoveAt(trailingCurvePoints.Count - 1);
        return trailingCurvePoints[^1];
    }

    public OldCameraFloorPoints OldCameraFloor
    {
        get
        {
            return cameraFloor;
        }
    }

    public Vector3[] shadowPoints()
    {
        int index = cameraFloor.firstSplineIndex;
        if (index < 4) index = 5;
        Vector3[] pointArray = new Vector3[13];
        for (int i = 1; i < 12; i+=2)
        {
            pointArray[i] = spline.GetPosition(index - 3 + (i/2));
            pointArray[i + 1] = cameraFloor.GetMidpoint(shapeController, index - 3 + (i / 2), index - 2 + (i / 2));
        }
        pointArray[0] = new Vector3(pointArray[1].x, pointArray[1].y - 50);
        pointArray[12] = new Vector3(pointArray[11].x, pointArray[11].y - 50);
        return pointArray;
    }
}
