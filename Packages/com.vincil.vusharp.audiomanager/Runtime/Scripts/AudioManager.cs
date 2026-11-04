
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;


namespace Vincil.VUSharp.Audio
{
    public enum AudioPriority
    {
        Low,
        Medium,
        High,
        Max
    }

    public abstract class AudioManager : UdonSharpBehaviour
    {
        public static string GetHierarchyPath()
        {
            return "/AudioManager";
        }
        public static AudioManager Instance()
        {
            AudioManager audioManager = GameObject.Find("/AudioManager").GetComponent<AudioManager>();
            if (audioManager == null)
            {
                Debug.LogError("Failed to find Audio Manager!");
            }
            return audioManager;
        }

        public abstract void _Initialize();

        public abstract int PlayFeedbackAudio(GameObject dataPrefabGameObject, bool isLooping = false);
        public abstract int PlayAudio(GameObject dataPrefabGameObject, Transform transformToConstrainTo, AudioPriority priority, bool isLooping = false);
        public abstract int PlayAudio(GameObject dataPrefabGameObject, Vector3 position, AudioPriority priority, bool isLooping = false);
        /// <summary>
        /// Plays like PlayAudio, with the data's volume and pitch multiplied by volumeScale and pitchScale.
        /// </summary>
        public abstract int PlayAudioWithVolumeAndPitch(GameObject dataPrefabGameObject, Vector3 position, AudioPriority priority, float volumeScale, float pitchScale, bool isLooping = false);
        public abstract int PlayAudioWithOverrides(GameObject dataPrefabGameObject, Transform transformToConstrainTo, AudioPriority priority, bool isLooping, bool is3d, float near = 0, float far = 0, float volumetricRadius = 0);
        public abstract int PlayAudioWithOverrides(GameObject dataPrefabGameObject, Vector3 position, AudioPriority priority, bool isLooping, bool is3d, float near = 0, float far = 0, float volumetricRadius = 0);
        public abstract bool StopAudio(int playId);
        public abstract void StopAllAudio();
    }
}

