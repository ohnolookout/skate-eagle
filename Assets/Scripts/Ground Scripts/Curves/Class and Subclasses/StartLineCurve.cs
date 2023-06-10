using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartLineCurve : Curve
{
    public StartLineCurve(CurvePoint startPoint)
    {
        curvePoints = GenerateStartPoints(startPoint);
        curveType = CurveType.FinishLine;
        GenerateCurveStats();
    }

    private List<CurvePoint> GenerateStartPoints(CurvePoint startPoint)
    {
        Transform eagle;
        List<CurvePoint> curvePoints = new();
        GameObject[] playerObj = GameObject.FindGameObjectsWithTag("Player");
        if (playerObj.Length < 1) eagle = Camera.main.transform;
        else eagle = playerObj[0].transform;
        Vector3 startLocation = new Vector3(eagle.position.x - 400, eagle.position.y + 150);
        CurvePoint firstPoint = new CurvePoint(startLocation, new Vector2(0, -1), new Vector2(40, -130));
        curvePoints.Add(firstPoint);
        Vector3 secondLocation = new Vector3(0, 0); ;
        if (playerObj.Length >= 1) secondLocation = new Vector3(eagle.position.x, eagle.position.y - eagle.gameObject.GetComponent<SpriteRenderer>().bounds.size.y / 2 - 1.1f);
        CurvePoint secondPoint = new CurvePoint(secondLocation, new Vector2(-45, 0.5f), new Vector2(10, -0.5f));
        curvePoints.Add(secondPoint);
        CurvePoint thirdPoint = new CurvePoint();
        float xVelocity = 10;
        float slope = -1.1f;
        thirdPoint.ControlPoint = secondPoint.ControlPoint + new Vector3(30, -12);
        thirdPoint.LeftTangent = new Vector3(-xVelocity, -xVelocity * slope);
        thirdPoint.RightTangent = new Vector3(xVelocity, xVelocity * slope);
        curvePoints.Add(thirdPoint);

        return curvePoints;
    }
}
