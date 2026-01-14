
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Vincil.VUSharp.Audio
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AudioData : UdonSharpBehaviour
    {
        [SerializeField] private AudioClip[] audioClips;
        [SerializeField] private float volume = 1.0f;
        [SerializeField] private float pitch = 1.0f;
        [SerializeField] private bool hasRandomPitch = false;

        private bool _hasSingleAudioClip;

        private bool isInitialized = false;
        private void Start()
        {
            _Initialize();
        }
        public void _Initialize()
        {
            if (isInitialized) return;
            isInitialized = true;

            Init();
        }
        private void Init()
        {
            _hasSingleAudioClip = audioClips.Length == 1;
        }

        public AudioClip GetAudioClip()
        {
            if (_hasSingleAudioClip)
            {
                return audioClips[0];
            }
            else
            {
                int rn = Random.Range(0, audioClips.Length);
                return audioClips[rn];
            }
        }

        public float GetVolume()
        {
            return volume;
        }

        public float GetPitch()
        {
            if (hasRandomPitch)
            {
                return Random.Range(pitch * 0.95f, pitch * 1.05f);
            }
            return pitch;
        }
    }
}
