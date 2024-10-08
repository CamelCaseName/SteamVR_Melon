﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace Valve.VR.InteractionSystem.Sample
{
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class Planting : MonoBehaviour
    {
        public Planting(IntPtr value) : base(value) { }
        public SteamVR_Action_Boolean plantAction;

        public Hand hand;

        public GameObject prefabToPlant;


        private void OnEnable()
        {
            if (hand == null)
            {
                hand = this.GetComponent<Hand>();
            }

            if (plantAction == null)
            {
                MelonLoader.MelonLogger.Error("[HPVR Interaction] No plant action assigned", this);
                return;
            }

            plantAction.AddOnChangeListener(OnPlantActionChange, hand.handType);
        }

        private void OnDisable()
        {
            plantAction?.RemoveOnChangeListener(OnPlantActionChange, hand.handType);
        }

        private void OnPlantActionChange(SteamVR_Action_Boolean actionIn, SteamVR_Input_Sources inputSource, bool newValue)
        {
            if (newValue)
            {
                Plant();
            }
        }

        public void Plant()
        {
            MelonLoader.MelonCoroutines.Start(DoPlant());
        }

        private IEnumerator DoPlant()
        {
            Vector3 plantPosition;

            RaycastHit hitInfo;
            bool hit = Physics.Raycast(hand.transform.position, Vector3.down, out hitInfo);
            if (hit)
            {
                plantPosition = hitInfo.point + (Vector3.up * 0.05f);
            }
            else
            {
                plantPosition = hand.transform.position;
                plantPosition.y = Player.instance.transform.position.y;
            }

            GameObject planting = GameObject.Instantiate<GameObject>(prefabToPlant);
            planting.transform.position = plantPosition;
            planting.transform.rotation = Quaternion.Euler(0,UnityEngine.Random.value * 360f, 0);

            planting.GetComponentInChildren<MeshRenderer>().material.SetColor("_TintColor",UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f));

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