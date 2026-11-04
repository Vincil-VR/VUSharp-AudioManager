using System.Collections;
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using UnityEngine.Animations;
using VRC.Dynamics;
using VRC.SDK3.Components;
using VRC.SDK3.Dynamics.Constraint.Components;

namespace Vincil.VUSharp.Audio
{
    [RequireComponent(typeof(AudioSource), typeof(VRCSpatialAudioSource), typeof(PositionConstraint)), UdonBehaviourSyncMode(BehaviourSyncMode.None)]
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

        //internal string priorityTag;
        //internal string PriorityTag { set { priorityTag = value; } get { return priorityTag; } }
        //internal string typeTag;
        //internal int stoppableUID;

        private PositionConstraint positionConstraint;

        private VUSharpAudioManager audioManager;

        private int id;

        //private bool isPlaying = false;

        private VRCTweenHandle delayCallCheckIfAudioFinishedLoopHandle;

        private bool isInitialized = false;
        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (isInitialized) return;
            isInitialized = true;

            positionConstraint = GetComponent<PositionConstraint>();

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        internal void Setup(VUSharpAudioManager audioManager, int id)
        {
            Initialize();
            this.audioManager = audioManager;
            this.id = id;
            //this.typeTag = typeTag;
            //stoppableUID = -1;
        }

        internal void Play(AudioData data, Vector3 pos, bool isLooping)
        {
            transform.position = pos;
            HandlePlayRequest(data, isLooping, 1f, 1f);
        }
        internal void PlayWithVolumeAndPitch(AudioData data, Vector3 pos, bool isLooping, float volumeScale, float pitchScale)
        {
            transform.position = pos;
            HandlePlayRequest(data, isLooping, volumeScale, pitchScale);
        }
        internal void PlayWithOverrides(AudioData data, Vector3 pos, bool isLooping, bool is3d, float near, float far, float volumetricRadius)
        {
            transform.position = pos;
            HandlePlayRequestWithOverrides(data, isLooping, is3d, near, far, volumetricRadius);
        }
        internal void Play(AudioData data, Transform target, bool isLooping)
        {
            ConstraintSource source = new ConstraintSource();
            source.sourceTransform = target;
            source.weight = 1.0f;
            positionConstraint.AddSource(source);
            positionConstraint.constraintActive = true;
            HandlePlayRequest(data, isLooping, 1f, 1f);
        }
        internal void PlayWithOverrides(AudioData data, Transform target, bool isLooping, bool is3d, float near, float far, float volumetricRadius)
        {
            ConstraintSource source = new ConstraintSource();
            source.sourceTransform = target;
            source.weight = 1.0f;
            positionConstraint.AddSource(source);
            positionConstraint.constraintActive = true;
            HandlePlayRequestWithOverrides(data, isLooping, is3d, near, far, volumetricRadius);
        }
        private void HandlePlayRequest(AudioData data, bool isLooping, float volumeScale, float pitchScale)
        {
            //if (data == null)
            //{
            //    Debug.LogError("audio data is null");
            //}

            //if (data.audioClips == null)
            //{
            //    Debug.LogError("audio clips is null");
            //}
            AudioClip clip = data.audioClips[Random.Range(0, data.audioClips.Length)];
            float volume = Mathf.Clamp01(data.volume * volumeScale);
            float pitch = data.pitch * pitchScale;
            pitch = data.randomPitchRange > 0 ? Random.Range(pitch * (1 / (1 + data.randomPitchRange)), pitch * (1 + data.randomPitchRange)) : pitch;

            if (data.is3d)
            {
                Play3DWithParameters(clip, volume, pitch, isLooping, data.near, data.far, data.volumetricRadius);
            }
            else
            {
                Play2DWithParameters(clip, volume, pitch, isLooping);
            }
        }
        private void HandlePlayRequestWithOverrides(AudioData data, bool isLooping, bool is3d, float near, float far, float volumetricRadius)
        {
            AudioClip clip = data.audioClips[Random.Range(0, data.audioClips.Length)];
            float volume = data.volume;
            float pitch = data.pitch;
            pitch = data.randomPitchRange > 0 ? Random.Range(pitch * (1 / (1 + data.randomPitchRange)), pitch * (1 + data.randomPitchRange)) : pitch;

            if (is3d)
            {
                Play3DWithParameters(clip, volume, pitch, isLooping, near, far, volumetricRadius);
            }
            else
            {
                Play2DWithParameters(clip, volume, pitch, isLooping);
            }
        }
        private void Play3DWithParameters(AudioClip clip, float volume, float pitch, bool isLooping, float near, float far, float volumetricRadius)
        {
            audioSource.spatialize = true;
            audioSource.spatialBlend = 1f;
            audioSource.SetInverseSquaredRolloff(near, far, volumetricRadius);

            PlayWithParameters(clip, volume, pitch, isLooping);
        }

        private void Play2DWithParameters(AudioClip clip, float volume, float pitch, bool isLooping)
        {
            audioSource.spatialize = false;
            audioSource.spatialBlend = 0f;

            PlayWithParameters(clip, volume, pitch, isLooping);
        }

        private void PlayWithParameters(AudioClip clip, float volume, float pitch, bool isLooping)
        {
            //Debug.LogWarning("Playing audio clip");
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.loop = isLooping;
            audioSource.Play();

            if (!isLooping)
            {
                delayCallCheckIfAudioFinishedLoopHandle = VRCTween.DelayedCall(this, nameof(_CheckIfAudioFinishedLoop), clip.length);
            }
        }
        internal void ForceStop()
        {
            //Debug.LogWarning($"Force stop {audioSource.clip}");
            audioSource.Stop();
            delayCallCheckIfAudioFinishedLoopHandle.Kill();
            CleanUpAudioPlayerAndReturnToPool();
        }

        internal void CleanUpAudioPlayerAndReturnToPool()
        {
            if (positionConstraint.constraintActive == true)
            {
                if(positionConstraint.sourceCount == 1)
                {
                    positionConstraint.RemoveSource(0);
                }
                else
                {
                    Debug.LogError($"audio constraint has an unexptected number of sources: {positionConstraint.sourceCount}");
                }
                
                positionConstraint.constraintActive = false;
            }

            audioManager.ReportAudioPlayerStoppedPlaying(id);
        }

        public void _CheckIfAudioFinishedLoop()
        {

            if (!audioSource.isPlaying)
            {
                CleanUpAudioPlayerAndReturnToPool();
            }
            else
            {
                delayCallCheckIfAudioFinishedLoopHandle = VRCTween.DelayedCall(this, nameof(_CheckIfAudioFinishedLoop), audioSource.clip.length - audioSource.time);
            }
        }
    }
}
