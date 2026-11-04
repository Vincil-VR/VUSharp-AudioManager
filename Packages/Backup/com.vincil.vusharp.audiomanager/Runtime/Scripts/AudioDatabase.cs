
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace Vincil.VUSharp.Audio
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    internal class AudioDatabase : UdonSharpBehaviour
    {
        DataDictionary cachedAudioData = new DataDictionary();

        public AudioData GetAudioData(GameObject audioDataGOPrefab)
        {
            if (cachedAudioData.TryGetValue(audioDataGOPrefab.name, TokenType.Reference, out DataToken token))
            {
                return (AudioData)token.Reference;
            }
            else
            {
                GameObject audioDataGO = Instantiate(audioDataGOPrefab, transform);
                AudioData audioData = audioDataGO.GetComponent<AudioData>();
                cachedAudioData.SetValue(audioDataGOPrefab.name, new DataToken(audioData));
                return audioData;
            }
        }
    }
}
