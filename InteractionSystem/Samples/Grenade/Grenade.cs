using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem.Sample
{
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class Grenade : MonoBehaviour
    {
        public Grenade(IntPtr value) : base(value) { }
        public GameObject explodePartPrefab;
        public int explodeCount = 10;

        public float minMagnitudeToExplode = 1f;

        private Interactable interactable;

        private void Start()
        {
            interactable = this.GetComponent<Interactable>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (interactable != null && interactable.attachedToHand != null) //don't explode in hand
            {
                return;
            }

            if (collision.impulse.magnitude > minMagnitudeToExplode)
            {
                for (int explodeIndex = 0; explodeIndex < explodeCount; explodeIndex++)
                {
                    GameObject explodePart = (GameObject)GameObject.Instantiate(explodePartPrefab, this.transform.position, this.transform.rotation);
                    explodePart.GetComponentInChildren<MeshRenderer>().material.SetColor("_TintColor", UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f));
                }

                Destroy(this.gameObject);
            }
        }
    }
}