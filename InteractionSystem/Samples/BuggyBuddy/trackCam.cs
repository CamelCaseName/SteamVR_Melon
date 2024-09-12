using UnityEngine;
using System.Collections;
using System;

namespace Valve.VR.InteractionSystem.Sample
{
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class trackCam : MonoBehaviour
    {
        public trackCam(IntPtr value): base(value) { }
        public float speed;

        public bool negative;

        void Update()
        {
            Vector3 look = Camera.main.transform.position - transform.position;
            if (negative)
            {
                look = -look;
            }
            if (speed == 0)
            {
                transform.rotation = Quaternion.LookRotation(look);
            }
            else
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(look), speed * Time.deltaTime);
            }
        }
    }
}