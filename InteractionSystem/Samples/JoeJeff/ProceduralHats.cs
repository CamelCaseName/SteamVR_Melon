﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

namespace Valve.VR.InteractionSystem.Sample
{
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class ProceduralHats : MonoBehaviour
    {
        public ProceduralHats(IntPtr value) : base(value) { }
        public GameObject[] hats;

        public float hatSwitchTime;

        private void Start()
        {
            SwitchToHat(0);
        }

        private void OnEnable()
        {
            MelonLoader.MelonCoroutines.Start(HatSwitcher());
        }

        private IEnumerator HatSwitcher()
        {
            while (true)
            {
                yield return new WaitForSeconds(hatSwitchTime);
                //delay before trying to switch

                Transform cam = Camera.main.transform;
                while (Vector3.Angle(cam.forward, transform.position - cam.position) < 90)
                {
                    //wait for player to look away
                    yield return new WaitForSeconds(0.1f);
                }

                ChooseHat();
            }
        }

        private void ChooseHat()
        {
            SwitchToHat(UnityEngine.Random.Range(0, hats.Length));
        }

        private void SwitchToHat(int hat)
        {
            for (int hatIndex = 0; hatIndex < hats.Length; hatIndex++)
            {
                hats[hatIndex].SetActive(hat == hatIndex);
            }
        }
    }
}