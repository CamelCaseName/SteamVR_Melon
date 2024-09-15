using UnityEngine;
using System.Collections;
using Valve.VR;
using System;

namespace Valve.VR.InteractionSystem.Sample
{
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class AmbientSound : MonoBehaviour
    {
        public AmbientSound(IntPtr value) : base(value) { }
        AudioSource s;

        public float fadeintime;

        float t;

        public bool fadeblack = false;

        float vol;

        // Use this for initialization
        void Start()
        {
            AudioListener.volume = 1;
            s = GetComponent<AudioSource>();
            s.time =UnityEngine.Random.Range(0, s.clip.length);
            if (fadeintime > 0)
            {
                t = 0;
            }

            vol = s.volume;

            SteamVR_Fade.Start(Color.black, 0);
            SteamVR_Fade.Start(Color.clear, 7);
        }

        // Update is called once per frame
        void Update()
        {
            if (fadeintime > 0 && t < 1)
            {
                t += Time.deltaTime / fadeintime;
                s.volume = t * vol;
            }

        }
    }
}