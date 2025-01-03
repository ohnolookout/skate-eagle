using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BgPanel : MonoBehaviour, IDoublePosition, IPosition
{
    public Transform LeftAnchor;
    public Transform RightAnchor;
    public List<CitySprite> SpriteObjects;
    public float XWidth => RightAnchor.position.x - LeftAnchor.position.x;
    public Vector3 Position { get => transform.position; set => transform.position = value; }
    public Vector3 StartPosition => LeftAnchor.position;
    public Vector3 EndPosition => RightAnchor.position; 

    /*
    void OnDrawGizmos()
    {

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 15);
    }
    */
}
