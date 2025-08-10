using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameUtilities
{
    public struct SpawnData
    {
        public Vector3 position;
        public Quaternion rotation;

        public SpawnData(Vector3 pos, Quaternion rot)
        {
            position = pos;
            rotation = rot;
        }
    }
}