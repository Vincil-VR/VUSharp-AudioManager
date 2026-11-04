
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Vincil.VUSharp.Audio
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AudioData : UdonSharpBehaviour
    {
        [SerializeField] internal AudioClip[] audioClips;
        [SerializeField, Range(0f, 1f)] internal float volume = 1.0f;
        [SerializeField, Range(.001f, 3f)] internal float pitch = 1f;        
        [SerializeField] internal float far = 40.0f;
        [Space(10)]
        [Header("Advanced Options")]
        [SerializeField, Range(0f, 3f)] internal float randomPitchRange = 0f;
        [SerializeField] internal bool is3d = true;
        [SerializeField] internal float near = 0f;
        [SerializeField] internal float volumetricRadius = 0f;
    }
}
