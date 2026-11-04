
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
        [SerializeField] internal float volume = 1.0f;
        [SerializeField] internal float pitch = 1.0f;
        [SerializeField] internal bool hasRandomPitch = false;

        //private bool _hasSingleAudioClip;

        //private bool isInitialized = false;
        //private void Start()
        //{
        //    _Initialize();
        //}
        //public void _Initialize()
        //{
        //    if (isInitialized) return;
        //    isInitialized = true;

        //    Init();
        //}
        //private void Init()
        //{
        //    if (audioClips == null || audioClips.Length == 0 || audioClips[0] == null)
        //    {
        //        Debug.LogError($"AudioData {gameObject.name} has no audio clips assigned!");
        //    }
        //    else
        //    {
        //        _hasSingleAudioClip = audioClips.Length == 1;
        //    }                
        //}

        //public AudioClip GetAudioClip()
        //{
        //    if (_hasSingleAudioClip)
        //    {
        //        return audioClips[0];
        //    }
        //    else
        //    {
        //        int rn = Random.Range(0, audioClips.Length);
        //        return audioClips[rn];
        //    }
        //}

        //public float GetVolume()
        //{
        //    return volume;
        //}

        //public float GetPitch()
        //{
        //    if (hasRandomPitch)
        //    {
        //        return Random.Range(pitch * 0.95f, pitch * 1.05f);
        //    }
        //    return pitch;
        //}
    }
}
