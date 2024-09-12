//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Play one-shot sounds as opposed to continuos/looping ones
//
//=============================================================================

using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class SoundPlayOneshot : MonoBehaviour
    {
        public SoundPlayOneshot(IntPtr value) : base(value) { }
        public AudioClip[] waveFiles;
        private AudioSource thisAudioSource;

        public float volMin;
        public float volMax;

        public float pitchMin;
        public float pitchMax;

        public bool playOnAwake;


        //-------------------------------------------------
        void Awake()
        {
            thisAudioSource = GetComponent<AudioSource>();

            if (playOnAwake)
            {
                Play();
            }
        }


        //-------------------------------------------------
        public void Play()
        {
            if (thisAudioSource != null && thisAudioSource.isActiveAndEnabled && !Util.IsNullOrEmpty(waveFiles))
            {
                //randomly apply a volume between the volume min max
                thisAudioSource.volume = UnityEngine.Random.Range(volMin, volMax);

                //randomly apply a pitch between the pitch min max
                thisAudioSource.pitch = UnityEngine.Random.Range(pitchMin, pitchMax);

                // play the sound
                thisAudioSource.PlayOneShot(waveFiles[UnityEngine.Random.Range(0, waveFiles.Length)]);
            }
        }


        //-------------------------------------------------
        public void Pause()
        {
            if (thisAudioSource != null)
            {
                thisAudioSource.Pause();
            }
        }


        //-------------------------------------------------
        public void UnPause()
        {
            if (thisAudioSource != null)
            {
                thisAudioSource.UnPause();
            }
        }
    }
}
