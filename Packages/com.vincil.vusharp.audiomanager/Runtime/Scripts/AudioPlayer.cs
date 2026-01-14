using System.Collections;
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Components;
using VRC.SDK3.Dynamics.Constraint.Components;

namespace Vincil.VUSharp.Audio
{
    [RequireComponent(typeof(AudioSource), typeof(VRCSpatialAudioSource), typeof(VRCPositionConstraint)), UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    internal class AudioPlayer : UdonSharpBehaviour
    {
        AudioSource audioSource;
        internal AudioSource AudioSource
        {
            get
            {
                if (audioSource == null)
                {
                    audioSource = GetComponent<AudioSource>();
                }
                return audioSource;
            }
        }

        internal string PriorityTag { set; get; }
        internal string TypeTag { set; get; }
        internal int StoppableUID { set; get; }

        private VRCPositionConstraint positionConstraint;

        private VUSharpAudioManager audioManager;

        private bool isPlaying = false;

        private bool isInitialized = false;
        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (isInitialized) return;
            isInitialized = true;

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        internal void Setup(VUSharpAudioManager audioManager)
        {
            Initialize();
            this.audioManager = audioManager;
            StoppableUID = -1;
        }

        internal void Play(AudioData data, Vector3 pos, bool isLooping = false)
        {
            transform.position = pos;
            Play(data, isLooping);
        }
        internal void Play(AudioData data, Transform target, bool isLooping = false)
        {
            positionConstraint.Sources.Add(new VRCConstraintSource(target, 1.0f));
            positionConstraint.enabled = true;
            positionConstraint.ApplyConfigurationChanges();
            Play(data, isLooping);
        }
        private void Play(AudioData data, bool isLooping)
        {
            isPlaying = true;
            audioSource.clip = data.GetAudioClip();
            audioSource.volume = data.GetVolume();
            audioSource.pitch = data.GetVolume();
            audioSource.loop = isLooping;
            audioSource.Play();
            if(!isLooping)
            {
                SendCustomEventDelayedSeconds("_CheckIfAudioFinishedLoop", audioSource.clip.length);
            }
        }
        internal void Stop()
        {
            isPlaying = false;
            audioSource.Stop();
            if (positionConstraint.enabled == true)
            {
                positionConstraint.Sources.RemoveAt(0);
                positionConstraint.enabled = false;
                positionConstraint.ApplyConfigurationChanges();
            }

            audioManager.ReportAudioPlayerStoppedPlaying(this);
        }

        public void _CheckIfAudioFinishedLoop()
        {
            if (!isPlaying || audioSource.loop) return;

            if (!audioSource.isPlaying)
            {
                Stop();
            }
            else
            {
                SendCustomEventDelayedSeconds("_CheckIfAudioFinishedLoop", audioSource.clip.length - audioSource.time);
            }
        }
    }
}
