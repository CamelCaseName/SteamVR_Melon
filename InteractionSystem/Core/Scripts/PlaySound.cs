//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Plays one of many audio files with possibleUnityEngine.Randomized parameters
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class PlaySound : MonoBehaviour
    {
        public PlaySound(IntPtr value) : base(value) { }
        ///<summary>List of audio clips to play.</summary>
        public AudioClip[] waveFile;
        ///<summary>Stops the currently playing clip in the audioSource. Otherwise clips will overlap/mix.</summary>
        public bool stopOnPlay;
        ///<summary>After the audio clip finishes playing, disable the game object the sound is on.</summary>
        public bool disableOnEnd;
        ///<summary>Loop the sound after the wave file variation has been chosen.</summary>
        public bool looping;
        ///<summary>If the sound is looping and updating it's position every frame, stop the sound at the end of the wav/clip length. </summary>
        public bool stopOnEnd;
        ///<summary>Start a wave file playing on awake, but after a delay.</summary>
        public bool playOnAwakeWithDelay;


        public bool useRandomVolume = true;
        ///<summary>Minimum volume that will be used whenUnityEngine.Randomly set.</summary>

        public float volMin = 1.0f;
        ///<summary>Maximum volume that will be used whenUnityEngine.Randomly set.</summary>

        public float volMax = 1.0f;

        ///<summary>Use min and maxUnityEngine.Random pitch levels when playing sounds.</summary>
        public bool useRandomPitch = true;
        ///<summary>Minimum pitch that will be used whenUnityEngine.Randomly set.</summary>

        public float pitchMin = 1.0f;
        ///<summary>Maximum pitch that will be used whenUnityEngine.Randomly set.</summary>

        public float pitchMax = 1.0f;


        ///<summary>Use Retrigger Time to repeat the sound within a time range</summary>
        public bool useRetriggerTime = false;
        ///<summary>Inital time before the first repetion starts</summary>
        public float timeInitial = 0.0f;
        ///<summary>Minimum time that will pass before the sound is retriggered</summary>
        public float timeMin = 0.0f;
        ///<summary>Maximum pitch that will be used whenUnityEngine.Randomly set.</summary>
        public float timeMax = 0.0f;

        ///<summary>Use Retrigger Time to repeat the sound within a time range</summary>
        public bool useRandomSilence = false;
        ///<summary>Percent chance that the wave file will not play</summary>
        public float percentToNotPlay = 0.0f;

        ///<summary>Time to offset playback of sound</summary>
        public float delayOffsetTime = 0.0f;


        private AudioSource audioSource;
        private AudioClip clip;

        //-------------------------------------------------
        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            clip = audioSource.clip;

            // audio source play on awake is true, just play the PlaySound immediately
            if (audioSource.playOnAwake)
            {
                if (useRetriggerTime)
                {
                    InvokeRepeating("Play", timeInitial, UnityEngine.Random.Range(timeMin, timeMax));
                }
                else
                {
                    Play();
                }
            }

            // if playOnAwake is false, but the playOnAwakeWithDelay on the PlaySound is true, play the sound on away but with a delay
            else if (!audioSource.playOnAwake && playOnAwakeWithDelay)
            {
                PlayWithDelay(delayOffsetTime);

                if (useRetriggerTime)
                {
                    InvokeRepeating("Play", timeInitial, UnityEngine.Random.Range(timeMin, timeMax));
                }
            }

            // in the case where both playOnAwake and playOnAwakeWithDelay are both set to true, just to the same as above, play the sound but with a delay
            else if (audioSource.playOnAwake && playOnAwakeWithDelay)
            {
                PlayWithDelay(delayOffsetTime);

                if (useRetriggerTime)
                {
                    InvokeRepeating("Play", timeInitial, UnityEngine.Random.Range(timeMin, timeMax));
                }
            }
        }


        //-------------------------------------------------
        // Play aUnityEngine.Random clip from those available
        //-------------------------------------------------
        public void Play()
        {
            if (looping)
            {
                PlayLooping();

            }

            else
            {
                PlayOneShotSound();
            }
        }


        //-------------------------------------------------
        public void PlayWithDelay(float delayTime)
        {
            if (looping)
            {
                Invoke("PlayLooping", delayTime);
            }
            else
            {
                Invoke("PlayOneShotSound", delayTime);
            }
        }


        //-------------------------------------------------
        // PlayUnityEngine.Random wave clip on audiosource as a one shot
        //-------------------------------------------------
        public AudioClip PlayOneShotSound()
        {
            if (!this.audioSource.isActiveAndEnabled)
            {
                return null;
            }

            SetAudioSource();
            if (this.stopOnPlay)
            {
                audioSource.Stop();
            }

            if (this.disableOnEnd)
            {
                Invoke("Disable", clip.length);
            }

            this.audioSource.PlayOneShot(this.clip);
            return this.clip;
        }


        //-------------------------------------------------
        public AudioClip PlayLooping()
        {
            // get audio source properties, and do any specialUnityEngine.Randomizations
            SetAudioSource();

            // if the audio source has forgotten to be set to looping, set it to looping
            if (!audioSource.loop)
            {
                audioSource.loop = true;
            }

            // play the clip in the audio source, all the meanwhile updating it's location
            this.audioSource.Play();

            // if disable on end is checked, stop playing the wave file after the first loop has finished.
            if (stopOnEnd)
            {
                Invoke("Stop", audioSource.clip.length);
            }

            return this.clip;
        }


        //-------------------------------------------------
        public void Disable()
        {
            gameObject.SetActive(false);
        }


        //-------------------------------------------------
        public void Stop()
        {
            audioSource.Stop();
        }


        //-------------------------------------------------
        private void SetAudioSource()
        {
            if (this.useRandomVolume)
            {
                //randomly apply a volume between the volume min max
                this.audioSource.volume = UnityEngine.Random.Range(this.volMin, this.volMax);

                if (useRandomSilence && (UnityEngine.Random.Range(0, 1) < percentToNotPlay))
                {
                    this.audioSource.volume = 0;
                }
            }

            if (this.useRandomPitch)
            {
                //randomly apply a pitch between the pitch min max
                this.audioSource.pitch = UnityEngine.Random.Range(this.pitchMin, this.pitchMax);
            }

            if (this.waveFile.Length > 0)
            {
                //UnityEngine.Randomly assign a wave file from the array into the audioSource clip property
                audioSource.clip = this.waveFile[UnityEngine.Random.Range(0, waveFile.Length)];
                clip = audioSource.clip;
            }
        }
    }
}
