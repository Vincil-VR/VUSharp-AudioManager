
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

//public enum AudioClipPriority
//{
//    Max,
//    High,
//    Medium,
//    Low,

//    Count
//}

namespace Vincil.VUSharp.Audio
{
    public enum AudioPriority
    {
        Low,
        Medium,
        High,
        Max
    }

    //public static class AudioPriorityTag
    //{
    //    public static string Low()
    //    {
    //        return "Low";
    //    }
    //    public static string Medium()
    //    {
    //        return "Medium";
    //    }
    //    public static string High()
    //    {
    //        return "High";
    //    }
    //    public static string Max()
    //    {
    //        return "Max";
    //    }
    //}

    //public static class AudioPlayerTypeTags
    //{
    //    public static string Standard3D()
    //    {
    //        return "Standard3D";
    //    }
    //    public static string LongRange3D()
    //    {
    //        return "LongRange3D";
    //    }
    //    public static string Audio2D()
    //    {
    //        return "Audio2D";
    //    }
    //}

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

        //public abstract int PlayStoppableAudio(AudioData data, Transform transformToConstrainTo, string audioPriorityTag = "Medium", string audioPlayerTypeTag = "Standard3D", bool shouldLoop = false);

        //public abstract int PlayStoppableAudio(GameObject dataPrefab, Transform transformToConstrainTo, string audioPriorityTag = "Medium", string audioPlayerTypeTag = "Standard3D", bool shouldLoop = false);

        //public abstract int PlayStoppableAudio(AudioData data, Vector3 position, string audioPriorityTag = "Medium", string audioPlayerTypeTag = "Standard3D", bool shouldLoop = false);

        //public abstract int PlayStoppableAudio(GameObject dataPrefab, Vector3 position, string audioPriorityTag = "Medium", string audioPlayerTypeTag = "Standard3D", bool shouldLoop = false);

        //public abstract int PlayStoppableAudioIn2d(AudioData data, string audioPriorityTag = "Medium", bool shouldLoop = false);

        //public abstract int PlayStoppableAudioIn2d(GameObject dataPrefab, string audioPriorityTag = "Medium", bool shouldLoop = false);

        //public abstract bool PlayOneShotAudio(AudioData data, Transform transformToConstrainTo, string audioPriorityTag = "Medium", string audioPlayerTypeTag = "Standard3D");

        //public abstract bool PlayOneShotAudio(GameObject dataPrefab, Transform transformToConstrainTo, string audioPriorityTag = "Medium", string audioPlayerTypeTag = "Standard3D");

        //public abstract bool PlayOneShotAudio(AudioData data, Vector3 position,  string audioPriorityTag = "Medium", string audioPlayerTypeTag = "Standard3D");

        //public abstract bool PlayOneShotAudio(GameObject dataPrefab, Vector3 position, string audioPriorityTag = "Medium", string audioPlayerTypeTag = "Standard3D");

        //public abstract bool PlayOneShotAudioIn2d(AudioData data, string audioPriorityTag = "Medium");

        //public abstract bool PlayOneShotAudioIn2d(GameObject dataPrefab, string audioPriorityTag = "Medium");

        public abstract bool StopAudio(int playId);

        public abstract void StopAllAudio();
    }
}

