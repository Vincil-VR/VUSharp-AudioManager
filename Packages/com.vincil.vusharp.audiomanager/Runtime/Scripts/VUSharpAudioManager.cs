
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace Vincil.VUSharp.Audio
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VUSharpAudioManager : AudioManager
    {
        [SerializeField] private int maxAudioSources = 24; // Total number of audio sources to use
        [SerializeField] GameObject audioPlayerObjectToUse;

        private AudioPlayer[] audioPlayers;
        private int[] audioPlayersUsagePriority;
        private int[] audioPlayersStopId;

        private int afterLastUsedAudioIndex;

        DataDictionary cachedAudioData = new DataDictionary();

        

        private int stoppableUIDCounter = 0;

        private bool isInitialized = false;
        private void Start()
        {
            _Initialize();
        }
        public  override void _Initialize()
        {
            if (isInitialized) return;
            isInitialized = true;

            Init();
        }
        private void Init()
        {
            if (audioPlayerObjectToUse == null)
            {
                Debug.LogError($"{nameof(audioPlayerObjectToUse)} not set", this);
                return;
            }
            audioPlayerObjectToUse.transform.parent = null;

            audioPlayers = new AudioPlayer[maxAudioSources];

            for (int i = 0; i < audioPlayers.Length; i++)
            {
                GameObject go = i == 0 ? audioPlayerObjectToUse : Instantiate(audioPlayerObjectToUse);
                AudioPlayer player = go.GetComponent<AudioPlayer>();
                if (player != null)
                {
                    player.Setup(this, i);
                    audioPlayers[i] = player;
                }
                else
                {
                    Debug.LogError($"AudioPlayer missing from {go.name}", this);
                }
            }

            audioPlayersUsagePriority = new int[maxAudioSources];
            audioPlayersStopId = new int[maxAudioSources];
            for (int i = 0; i < maxAudioSources; i++)
            {
                audioPlayersUsagePriority[i] = -1;
            }
            for (int i = 0; i < maxAudioSources; i++)
            {
                audioPlayersStopId[i] = -1;
            }
        }

        public override int PlayFeedbackAudio(GameObject dataPrefabGameObject, bool isLooping = false)
        {
            return PlayAudio(dataPrefabGameObject, Vector3.zero, AudioPriority.Max, isLooping);
        }

        public override int PlayAudio(GameObject dataPrefabGameObject, Transform transformToConstrainTo, AudioPriority priority, bool isLooping = false)
        {
            int stopId;
            AudioPlayer audioPlayer = GetNextFreeAudioPlayer(priority, out stopId);
            AudioData data = GetOrCreateAudioDataInstance(dataPrefabGameObject);
            audioPlayer.Play(data, transformToConstrainTo, isLooping);
            return stopId;
        }

        public override int PlayAudio(GameObject dataPrefabGameObject, Vector3 position, AudioPriority priority, bool isLooping = false)
        {
            int stopId;
            AudioPlayer audioPlayer = GetNextFreeAudioPlayer(priority, out stopId);
            AudioData data = GetOrCreateAudioDataInstance(dataPrefabGameObject);
            audioPlayer.Play(data, position, isLooping);
            return stopId;
        }

        public override int PlayAudioWithVolumeAndPitch(GameObject dataPrefabGameObject, Vector3 position, AudioPriority priority, float volumeScale, float pitchScale, bool isLooping = false)
        {
            int stopId;
            AudioPlayer audioPlayer = GetNextFreeAudioPlayer(priority, out stopId);
            AudioData data = GetOrCreateAudioDataInstance(dataPrefabGameObject);
            audioPlayer.PlayWithVolumeAndPitch(data, position, isLooping, volumeScale, pitchScale);
            return stopId;
        }

        public override int PlayAudioWithOverrides(GameObject dataPrefabGameObject, Transform transformToConstrainTo, AudioPriority priority, bool isLooping, bool is3d, float near = 0, float far = 0, float volumetricRadius = 0)
        {
            int stopId;
            AudioPlayer audioPlayer = GetNextFreeAudioPlayer(priority, out stopId);
            AudioData data = GetOrCreateAudioDataInstance(dataPrefabGameObject);
            audioPlayer.PlayWithOverrides(data, transformToConstrainTo, isLooping, is3d, near, far, volumetricRadius);
            return stopId;
        }

        public override int PlayAudioWithOverrides(GameObject dataPrefabGameObject, Vector3 position, AudioPriority priority, bool isLooping, bool is3d, float near = 0, float far = 0, float volumetricRadius = 0)
        {
            int stopId;
            AudioPlayer audioPlayer = GetNextFreeAudioPlayer(priority, out stopId);
            AudioData data = GetOrCreateAudioDataInstance(dataPrefabGameObject);
            audioPlayer.PlayWithOverrides(data, position, isLooping, is3d, near, far, volumetricRadius);
            return stopId;
        }

        public override bool StopAudio(int stopId)
        {
            int index = Array.IndexOf(audioPlayersStopId, stopId);
            if (index == -1)
            {
                audioPlayers[index].ForceStop();
                return true;
            }
            return false;
        }

        public override void StopAllAudio()
        {
            for (int i = 0; i < audioPlayers.Length; i++)
            {
                if (audioPlayersUsagePriority[i] == -1)
                {
                    audioPlayers[i].ForceStop();
                }
            }
        }

        internal void ReportAudioPlayerStoppedPlaying(int audioPlayerId)
        {
            audioPlayersStopId[audioPlayerId] = -1;
            audioPlayersUsagePriority[audioPlayerId] = -1;
        }

        private AudioPlayer GetNextFreeAudioPlayer(AudioPriority priority, out int stopId)
        {
            int index = -1;
            int priorityToOverride = -2;
            while (index == -1 && priorityToOverride <= (int)priority)
            {
                priorityToOverride++;
                index = Array.IndexOf(audioPlayersUsagePriority, priorityToOverride, afterLastUsedAudioIndex);
                if (index == -1) Array.IndexOf(audioPlayersUsagePriority, priorityToOverride);                
            }

            if(index == -1)
            {
                Debug.LogError("Couldn't find any valid audio players to use!  Using fallback!", this);
                index = (afterLastUsedAudioIndex + 1) % audioPlayers.Length; 
            }

            if (priorityToOverride > -1)
            {
                Debug.Log("All audio players in use.  Overwriting playing audio", this);
                audioPlayers[index].ForceStop();
            }
            
            if (priorityToOverride == (int)AudioPriority.Max) Debug.LogWarning("Overwriting max priority audio source!", this);

            afterLastUsedAudioIndex = (index + 1) % audioPlayers.Length;

            stopId = stoppableUIDCounter++;
            audioPlayersStopId[index] = stopId;
            audioPlayersUsagePriority[index] =  (int)priority;

            return audioPlayers[index];
        }

        private AudioData GetOrCreateAudioDataInstance(GameObject audioDataPrefab)
        {
            if (cachedAudioData.TryGetValue(audioDataPrefab.name, TokenType.Reference, out DataToken token))
            {
                return (AudioData)token.Reference;
            }
            else
            {
                GameObject audioDataGO = Instantiate(audioDataPrefab, transform);
                AudioData audioData = audioDataGO.GetComponent<AudioData>();
                cachedAudioData.SetValue(audioDataPrefab.name, new DataToken(audioData));
                return audioData;
            }
        }
    }
}
