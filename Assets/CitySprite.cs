using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CitySprite : MonoBehaviour, IPosition, IDoublePosition
{
    [SerializeField] private SpriteRenderer _renderer;

    public Vector3 StartPosition => new(transform.position.x - _renderer.bounds.size.x, transform.position.y);
    public Vector3 EndPosition => transform.position;
    public Vector3 Position { get => transform.position; set => transform.position = value; }

}
