using System;
using UnityEngine;

namespace Com.LuisPedroFonseca.ProCamera2D
{
    [Serializable]
    public class CameraTarget
    {
        public Transform TargetTransform;
        public float CurrentInfluence
        {
            set 
            { 
                CurrentInfluenceH = value;
                CurrentInfluenceV = value;
            }
        }

        public float TargetInfluence
        {
            set
            {
                TargetInfluenceH = value;
                TargetInfluenceV = value;
            }
        }

        [RangeAttribute(0f, 1f)]
        public float CurrentInfluenceH = 1f;

        [RangeAttribute(0f, 1f)]
        public float CurrentInfluenceV = 1f;

        [RangeAttribute(0f, 1f)]
        public float TargetInfluenceH = 1f;

        [RangeAttribute(0f, 1f)]
        public float TargetInfluenceV = 1f;

        public Vector2 TargetOffset;

        public Vector3 TargetPosition
        {
            get
            {
                if (TargetTransform != null)
                    return SerializedPosition = TargetTransform.position;
                else
                    return SerializedPosition;
            }
        }
        public Vector3 SerializedPosition;

        public bool RemovalPending = false;
    }
}
