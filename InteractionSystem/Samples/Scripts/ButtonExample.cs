using UnityEngine;
using System.Collections;
using System;

namespace Valve.VR.InteractionSystem.Sample
{
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class ButtonExample : MonoBehaviour
    {
        public ButtonExample(IntPtr value) : base(value) { }
        public HoverButton hoverButton;

        public GameObject prefab;

        private void Start()
        {
            hoverButton.onButtonDown.Listen(OnButtonDown);
        }

        private void OnButtonDown(Hand hand)
        {
            MelonLoader.MelonCoroutines.Start(DoPlant());
        }

        private IEnumerator DoPlant()
        {
            GameObject planting = GameObject.Instantiate<GameObject>(prefab);
            planting.transform.position = this.transform.position;
            planting.transform.rotation = Quaternion.Euler(0,UnityEngine.Random.value * 360f, 0);

            planting.GetComponentInChildren<MeshRenderer>().material.SetColor("_TintColor",UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f));

            Rigidbody rigidbody = planting.GetComponent<Rigidbody>();
            if (rigidbody != null)
                rigidbody.isKinematic = true;


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
                rigidbody.isKinematic = false;
        }
    }
}