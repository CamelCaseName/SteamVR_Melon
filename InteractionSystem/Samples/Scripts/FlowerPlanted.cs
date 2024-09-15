//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace Valve.VR.InteractionSystem.Sample
{
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class FlowerPlanted : MonoBehaviour
    {
        public FlowerPlanted(IntPtr value) : base(value) { }
        private void Start()
        {
            Plant();
        }

        public void Plant()
        {
            MelonLoader.MelonCoroutines.Start(DoPlant());
        }

        private IEnumerator DoPlant()
        {
            Vector3 plantPosition;

            RaycastHit hitInfo;
            bool hit = Physics.Raycast(this.transform.position, Vector3.down, out hitInfo);
            if (hit)
            {
                plantPosition = hitInfo.point + (Vector3.up * 0.05f);
            }
            else
            {
                plantPosition = this.transform.position;
                plantPosition.y = Player.instance.transform.position.y;
            }

            GameObject planting = this.gameObject;
            planting.transform.position = plantPosition;
            planting.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.value * 360f, 0);

            Color newColor =UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            newColor.a = 0.75f;
            planting.GetComponentInChildren<MeshRenderer>().material.SetColor("_BaseColor", newColor);
            Rigidbody rigidbody = planting.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.isKinematic = true;
            }

            Vector3 initialScale = Vector3.one * 0.01f;
            Vector3 targetScale = Vector3.one * (1 + (UnityEngine.Random.value * 0.25f));

            float startTime = Time.time;
            float overTime = 0.5f;
            float endTime = startTime + overTime;

            while (Time.time < endTime)
            {
                planting.transform.localScale = Vector3.Slerp(initialScale, targetScale, (Time.time - startTime) / overTime);
                yield return null;
            }


            if (rigidbody != null)
            {
                rigidbody.isKinematic = false;
            }
        }
    }
}