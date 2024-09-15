//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Spawns balloons
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class BalloonSpawner : MonoBehaviour
    {
        public BalloonSpawner(IntPtr value) : base(value) { }
        public float minSpawnTime = 5f;
        public float maxSpawnTime = 15f;
        private float nextSpawnTime;
        public GameObject balloonPrefab;

        public bool autoSpawn = true;
        public bool spawnAtStartup = true;

        public bool playSounds = true;
        public SoundPlayOneshot inflateSound;
        public SoundPlayOneshot stretchSound;

        public bool sendSpawnMessageToParent = false;

        public float scale = 1f;

        public Transform spawnDirectionTransform;
        public float spawnForce;

        public bool attachBalloon = false;

        public Balloon.BalloonColor color = Balloon.BalloonColor.Random;


        //-------------------------------------------------
        void Start()
        {
            if ( balloonPrefab == null )
            {
                return;
            }

            if ( autoSpawn && spawnAtStartup )
            {
                SpawnBalloon( color );
                nextSpawnTime =UnityEngine.Random.Range( minSpawnTime, maxSpawnTime ) + Time.time;
            }
        }


        //-------------------------------------------------
        void Update()
        {
            if ( balloonPrefab == null )
            {
                return;
            }

            if ( ( Time.time > nextSpawnTime ) && autoSpawn )
            {
                SpawnBalloon( color );
                nextSpawnTime =UnityEngine.Random.Range( minSpawnTime, maxSpawnTime ) + Time.time;
            }
        }


        //-------------------------------------------------
        public GameObject SpawnBalloon( Balloon.BalloonColor color = Balloon.BalloonColor.Red )
        {
            if ( balloonPrefab == null )
            {
                return null;
            }
            GameObject balloon = Instantiate( balloonPrefab, transform.position, transform.rotation ) as GameObject;
            balloon.transform.localScale = new Vector3( scale, scale, scale );
            if ( attachBalloon )
            {
                balloon.transform.parent = transform;
            }

            if ( sendSpawnMessageToParent )
            {
                transform.parent?.SendMessage( "OnBalloonSpawned", balloon, SendMessageOptions.DontRequireReceiver );
            }

            if ( playSounds )
            {
                inflateSound?.Play();
                stretchSound?.Play();
            }
            balloon.GetComponentInChildren<Balloon>().SetColor( color );
            if ( spawnDirectionTransform != null )
            {
                balloon.GetComponentInChildren<Rigidbody>().AddForce( spawnDirectionTransform.forward * spawnForce );
            }

            return balloon;
        }


        //-------------------------------------------------
        public void SpawnBalloonFromEvent( int color )
        {
            // Copy of SpawnBalloon using int because we can't pass in enums through the event system
            SpawnBalloon( (Balloon.BalloonColor)color );
        }
    }
}
