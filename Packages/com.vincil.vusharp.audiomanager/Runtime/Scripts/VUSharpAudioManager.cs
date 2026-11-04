
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using Vincil.VUSharp.Extensions;

namespace Vincil.VUSharp.Audio
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VUSharpAudioManager : AudioManager
    {
        [SerializeField] private int maxAudioSources = 24; // Total number of audio sources to use
        [SerializeField] GameObject audioPlayerObjectToUse;
        //[SerializeField] GameObject audioSourceNormal3dGOPrefab;
        //[SerializeField] GameObject audioSourceLongRange3dGOPrefab;
        //[SerializeField] GameObject audioSource2dGOPrefab;

        private AudioPlayer[] audioPlayers;
        private int[] audioPlayersUsagePriority;
        private int[] audioPlayersStopId;

        private int afterLastUsedAudioIndex;

        //private DataDictionary audioPlayersInUse = new DataDictionary();
        //private const string TOTAL_AUDIO_PLAYERS_IN_USE_KEY = "InUse";

        //private DataDictionary audioPlayersUnused = new DataDictionary();

        //private DataDictionary stoppableAudioPlayers = new DataDictionary();

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
            //audioPlayersInUse.Add(TOTAL_AUDIO_PLAYERS_IN_USE_KEY, 0);
            //audioPlayersInUse.Add(AudioPriorityTag.Low(), new DataList());
            //audioPlayersInUse.Add(AudioPriorityTag.Medium(), new DataList());
            //audioPlayersInUse.Add(AudioPriorityTag.High(), new DataList());
            //audioPlayersInUse.Add(AudioPriorityTag.Max(), new DataList());

            //SetupAudioPlayers(audioSourceNormal3dGOPrefab, AudioPlayerTypeTags.Standard3D());
            //SetupAudioPlayers(audioSourceLongRange3dGOPrefab, AudioPlayerTypeTags.LongRange3D());
            //SetupAudioPlayers(audioSource2dGOPrefab, AudioPlayerTypeTags.Audio2D());
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
            //string s = $"audio player {audioPlayerId} returned to pool; [{audioPlayersUsagePriority[0]}";
            //for (int i = 1; i < audioPlayersUsagePriority.Length; i++) s += $", {audioPlayersUsagePriority[i]}";
            //Debug.LogWarning(s + "]");
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
            //Debug.LogError($"audioDataPrefab is null {audioDataPrefab == null}");
            //Debug.LogError($"audioDataPrefab name {audioDataPrefab.name}");
            if (cachedAudioData.TryGetValue(audioDataPrefab.name, TokenType.Reference, out DataToken token))
            {
                //Debug.LogError($"audioDataPrefab found");
                //Debug.LogError($"(AudioData)token.Reference is null {(AudioData)token.Reference == null}");
                return (AudioData)token.Reference;
            }
            else
            {
                //Debug.LogError($"audioDataPrefab ceated");
                GameObject audioDataGO = Instantiate(audioDataPrefab, transform);
                //Debug.LogError($"audioDataGO is null {audioDataGO == null}");
                AudioData audioData = audioDataGO.GetComponent<AudioData>();
                //Debug.LogError($"audioData is null {audioData == null}");
                cachedAudioData.SetValue(audioDataPrefab.name, new DataToken(audioData));
                return audioData;
            }
        }

        #region old code

        //private void SetupAudioPlayers(GameObject audioPlayerPrefabGO, string typeTag)
        //{
        //    DataList audioPlayersList = new DataList();
        //    for (int i = 0; i < maxAudioSources; i++)
        //    {
        //        GameObject audioPlayerGO = Instantiate(audioPlayerPrefabGO, transform);
        //        audioPlayerGO.name = $"{audioPlayerPrefabGO.name}_{i}";
        //        AudioPlayer audioPlayer = audioPlayerGO.GetComponent<AudioPlayer>();
        //        audioPlayer.Setup(this, typeTag);
        //        audioPlayersList.Add(audioPlayer);
        //    }

        //    audioPlayersUnused.Add(typeTag, audioPlayersList.ShallowClone());
        //}

        //public override int PlayStoppableAudio(GameObject dataPrefab, Transform transformToConstrainTo, string audioPriorityTag = "Medium", string audioPlayerTypeTag = "Standard3D", bool shouldLoop = false)
        //{
        //    AudioData data = GetOrCreateAudioDataInstance(dataPrefab);
        //    return PlayStoppableAudio(data, transformToConstrainTo, audioPriorityTag, audioPlayerTypeTag, shouldLoop);
        //}

        //public override int PlayStoppableAudio(AudioData data, Transform transformToConstrainTo, string audioPriorityTag = "Medium", string audioPlayerTypeTag = "Standard3D", bool shouldLoop = false)
        //{            
        //    AudioPlayer audioPlayer = GetAvailableAudioPlayer(audioPlayerTypeTag, audioPriorityTag);
        //    if (audioPlayer == null) return -1;

        //    int uid = RegisterStoppableAudioPlayer(audioPlayer);

        //    audioPlayer.Play(data, transformToConstrainTo, shouldLoop);
        //    return uid;
        //}

        //public override int PlayStoppableAudio(GameObject dataPrefab, Vector3 position, string audioPriorityTag = "Medium", string audioPlayerTypeTag = "Standard3D", bool shouldLoop = false)
        //{
        //    AudioData data = GetOrCreateAudioDataInstance(dataPrefab);
        //    return PlayStoppableAudio(data, position, audioPriorityTag, audioPlayerTypeTag, shouldLoop);
        //}

        //public override int PlayStoppableAudio(AudioData data, Vector3 position, string audioPriorityTag = "Medium", string audioPlayerTypeTag = "Standard3D", bool shouldLoop = false)
        //{
        //    AudioPlayer audioPlayer = GetAvailableAudioPlayer(audioPlayerTypeTag, audioPriorityTag);
        //    if (audioPlayer == null)
        //    {
        //        Debug.LogWarning("No available audio players to play stoppable audio");
        //        return -1;
        //    }
        //    int uid = RegisterStoppableAudioPlayer(audioPlayer);

        //    audioPlayer.Play(data, position, shouldLoop);
        //    return uid;
        //}

        //public override int PlayStoppableAudioIn2d(GameObject dataPrefab, string audioPriorityTag = "Medium", bool shouldLoop = false)
        //{
        //    AudioData data = GetOrCreateAudioDataInstance(dataPrefab);
        //    return PlayStoppableAudioIn2d(data, audioPriorityTag, shouldLoop);
        //}

        //public override int PlayStoppableAudioIn2d(AudioData data, string audioPriorityTag = "Medium", bool shouldLoop = false)
        //{
        //    AudioPlayer audioPlayer = GetAvailableAudioPlayer(AudioPlayerTypeTags.Audio2D(), audioPriorityTag);
        //    if (audioPlayer == null)
        //    {
        //        Debug.LogWarning("No available audio players to play stoppable audio");
        //        return -1;
        //    }
        //    int uid = RegisterStoppableAudioPlayer(audioPlayer);

        //    audioPlayer.Play(data, Vector3.zero, shouldLoop);
        //    return uid;
        //}

        //private int RegisterStoppableAudioPlayer(AudioPlayer audioPlayer)
        //{
        //    int uid = stoppableUIDCounter++;
        //    audioPlayer.stoppableUID = uid;
        //    stoppableAudioPlayers.Add(uid, new DataToken(audioPlayer));
        //    return uid;
        //}

        //public override bool PlayOneShotAudio(GameObject dataPrefab, Transform transformToConstrainTo, string audioPriorityTag = "Medium", string audioPlayerTypeTag = "Standard3D")
        //{
        //    AudioData data = GetOrCreateAudioDataInstance(dataPrefab);
        //    return PlayOneShotAudio(data, transformToConstrainTo, audioPriorityTag, audioPlayerTypeTag);
        //}

        //public override bool PlayOneShotAudio(AudioData data, Transform transformToConstrainTo, string audioPriorityTag = "Medium", string audioPlayerTypeTag = "Standard3D")
        //{
        //    AudioPlayer audioPlayer = GetAvailableAudioPlayer(audioPlayerTypeTag, audioPriorityTag);
        //    if (audioPlayer == null) return false;

        //    audioPlayer.Play(data, transformToConstrainTo);
        //    return true;
        //}

        //public override bool PlayOneShotAudio(GameObject dataPrefab, Vector3 position, string audioPriorityTag = "Medium", string audioPlayerTypeTag = "Standard3D")
        //{
        //    AudioData data = GetOrCreateAudioDataInstance(dataPrefab);
        //    return PlayOneShotAudio(data, position, audioPriorityTag, audioPlayerTypeTag);
        //}

        //public override bool PlayOneShotAudio(AudioData data, Vector3 position, string audioPriorityTag = "Medium", string audioPlayerTypeTag = "Standard3D")
        //{
        //    AudioPlayer audioPlayer = GetAvailableAudioPlayer(audioPlayerTypeTag, audioPriorityTag);
        //    if (audioPlayer == null) return false;

        //    audioPlayer.Play(data, position);
        //    return true;
        //}

        //public override bool PlayOneShotAudioIn2d(GameObject dataPrefab, string audioPriorityTag = "Medium")
        //{
        //    AudioData data = GetOrCreateAudioDataInstance(dataPrefab);
        //    return PlayOneShotAudioIn2d(data, audioPriorityTag);
        //}

        //public override bool PlayOneShotAudioIn2d(AudioData data, string audioPriorityTag = "Medium")
        //{
        //    AudioPlayer audioPlayer = GetAvailableAudioPlayer(AudioPlayerTypeTags.Audio2D(), audioPriorityTag);
        //    if (audioPlayer == null) return false;

        //    audioPlayer.Play(data, Vector3.zero);
        //    return true;
        //}



        //public override void StopAllAudio()
        //{
        //    DataList audioPlayersToStop;
        //    DataList keys = audioPlayersInUse.GetKeys();
        //    for (int i = 0; i < keys.Count; i++)
        //    {
        //        string key = keys[i].String;
        //        if (key == TOTAL_AUDIO_PLAYERS_IN_USE_KEY)
        //        {
        //            continue;
        //        }
        //        audioPlayersToStop = audioPlayersInUse[key].DataList;
        //        while (audioPlayersToStop.Count > 0)
        //        {
        //            AudioPlayer audioPlayer = (AudioPlayer)audioPlayersToStop[0].Reference;
        //            audioPlayer.ForceStop();
        //        }
        //    }
        //}



        //internal void ReportAudioPlayerStoppedPlaying(AudioPlayer audioPlayer)
        //{
        //    if(audioPlayer.stoppableUID != -1)
        //    {
        //        stoppableAudioPlayers.Remove(audioPlayer.stoppableUID);
        //        audioPlayer.stoppableUID = -1;
        //    }
        //    //Debug.LogWarning($"[{audioPlayer.gameObject.name} {audioPlayer.gameObject.GetInstanceID()}] audioPlayer.PriorityTag: {audioPlayer.priorityTag}");
        //    if (audioPlayersInUse[audioPlayer.priorityTag].DataList.Remove(new DataToken(audioPlayer)))
        //    {
        //        audioPlayersInUse[TOTAL_AUDIO_PLAYERS_IN_USE_KEY] = audioPlayersInUse[TOTAL_AUDIO_PLAYERS_IN_USE_KEY].Int - 1;
        //        audioPlayersUnused[audioPlayer.typeTag].DataList.Add(new DataToken(audioPlayer));
        //    }
        //    else
        //    {
        //        Debug.LogWarning($"[AudioManager] Audio player reported stopped playing but was not found in in-use list: {audioPlayer.priorityTag}");
        //    }           
        //}

        //private AudioPlayer GetAvailableAudioPlayer(string audioPlayerTypeTag, string audioPriorityTag)
        //{
        //    if (!audioPlayersUnused.ContainsKey(audioPlayerTypeTag))
        //    {
        //        Debug.LogError($"Audio player type tag {audioPlayerTypeTag} is invalid!");
        //        return null;
        //    }
        //    if(!audioPlayersInUse.ContainsKey(audioPriorityTag))
        //    {
        //        Debug.LogError($"Audio player type tag {audioPriorityTag} is invalid!");
        //        return null;
        //    }

        //    AudioPlayer audioPlayer = null;
        //    if (audioPlayersInUse[TOTAL_AUDIO_PLAYERS_IN_USE_KEY].Int < maxAudioSources)
        //    {
        //        audioPlayer = (AudioPlayer)audioPlayersUnused[audioPlayerTypeTag].DataList.PullOutLastElement().Reference;
        //    }
        //    else if (audioPriorityTag != AudioPriorityTag.Low())
        //    {
        //        if(audioPlayersInUse[AudioPriorityTag.Low()].DataList.Count > 0)
        //        {
        //            AudioPlayer audioPlayerToStop = (AudioPlayer)audioPlayersInUse[AudioPriorityTag.Low()].DataList[0].Reference;
        //            audioPlayerToStop.ForceStop();
        //            audioPlayer = (AudioPlayer)audioPlayersUnused[audioPlayerTypeTag].DataList.PullOutLastElement().Reference;
        //        }
        //        else if(audioPriorityTag != AudioPriorityTag.Medium())
        //        {
        //            if (audioPlayersInUse[AudioPriorityTag.Medium()].DataList.Count > 0)
        //            {
        //                AudioPlayer audioPlayerToStop = (AudioPlayer)audioPlayersInUse[AudioPriorityTag.Medium()].DataList[0].Reference;
        //                audioPlayerToStop.ForceStop();
        //                audioPlayer = (AudioPlayer)audioPlayersUnused[audioPlayerTypeTag].DataList.PullOutLastElement().Reference;
        //            }
        //            else if (audioPriorityTag != AudioPriorityTag.High())
        //            {
        //                if (audioPlayersInUse[AudioPriorityTag.High()].DataList.Count > 0)
        //                {
        //                    AudioPlayer audioPlayerToStop = (AudioPlayer)audioPlayersInUse[AudioPriorityTag.High()].DataList[0].Reference;
        //                    audioPlayerToStop.ForceStop();
        //                    audioPlayer = (AudioPlayer)audioPlayersUnused[audioPlayerTypeTag].DataList.PullOutLastElement().Reference;
        //                }
        //                else
        //                {
        //                    AudioPlayer audioPlayerToStop = (AudioPlayer)audioPlayersInUse[AudioPriorityTag.Max()].DataList[0].Reference;
        //                    audioPlayerToStop.ForceStop();
        //                    audioPlayer = (AudioPlayer)audioPlayersUnused[audioPlayerTypeTag].DataList.PullOutLastElement().Reference;
        //                }                        
        //            }
        //        }
        //    }

        //    if (audioPlayer != null)
        //    {
        //        audioPlayersInUse[TOTAL_AUDIO_PLAYERS_IN_USE_KEY] = audioPlayersInUse[TOTAL_AUDIO_PLAYERS_IN_USE_KEY].Int + 1;
        //        audioPlayersInUse[audioPriorityTag].DataList.Add(new DataToken(audioPlayer));
        //        //Debug.LogWarning($"[{audioPlayer.gameObject.name} {audioPlayer.gameObject.GetInstanceID()}] Assigning audio player priority tag: {audioPriorityTag}");
        //        audioPlayer.priorityTag = audioPriorityTag;
        //        //Debug.LogWarning($"[{audioPlayer.gameObject.name} {audioPlayer.gameObject.GetInstanceID()}] audioPlayer.PriorityTag: {audioPlayer.priorityTag}");
        //    }

        //    return audioPlayer;
        //}




        //actual looping is not advised until I can make sure the loop ends in edge cases, like during a player dc
        //public int PlayStoppableAudio(AudioData data, Vector3 postion, AudioClipPriority priority)
        //{
        //    if (data == null) return -1;
        //    int index = -1;
        //    if (priority == AudioClipPriority.Max)
        //    {
        //        index = GetAvailableAudioSourceIndex(true);
        //    }
        //    else if (_audioSourcesPlayingPerPriority[(int)priority] < _maxAudioSourcesPerPriority[(int)priority])
        //    {
        //        index = GetAvailableAudioSourceIndex();
        //    }
        //    if (index != -1)
        //    {
        //        audioPlayers[index].transform.position = postion;
        //        PlayAudioClipOnTargetAudioSource(index, false, priority, data.GetAudioClip(), data.GetVolume(), data.GetPitch(), false);
        //    }
        //    return index;
        //}

        //public void StopStoppableAudio(int index, bool isLongRange = false)
        //{
        //    if (!isLongRange)
        //    {
        //        audioPlayers[index].Stop();
        //    }
        //    else
        //    {
        //        audioPlayersLongRange[index].Stop();
        //    }
        //}

        //public void PlayAudioClip(AudioData data, Transform transformToAttachTo, AudioClipPriority priority)
        //{
        //    if (data == null) return;
        //    int index = -1;
        //    if (priority == AudioClipPriority.Max)
        //    {
        //        index = GetAvailableAudioSourceIndex(true);
        //    }
        //    else if (_audioSourcesPlayingPerPriority[(int)priority] < _maxAudioSourcesPerPriority[(int)priority])
        //    {
        //        index = GetAvailableAudioSourceIndex();
        //    }
        //    if (index != -1)
        //    {
        //        audioPlayers[index].transform.parent = transformToAttachTo;
        //        audioPlayers[index].transform.localPosition = Vector3.zero; // Reset position relative to parent
        //        PlayAudioClipOnTargetAudioSource(index, false, priority, data.GetAudioClip(), data.GetVolume(), data.GetPitch());
        //    }
        //}

        //public void PlayAudioClip(AudioData data, Vector3 position)
        //{
        //    PlayAudioClip(data, position, AudioClipPriority.Medium);
        //}

        //public void PlayAudioClip(AudioData data, Vector3 position, AudioClipPriority priority)
        //{
        //    if (data == null)
        //    {
        //        Debug.LogWarning("Tried to play null audio data");
        //        return;
        //    }
        //    int index = -1;
        //    if (priority == AudioClipPriority.Max)
        //    {
        //        index = GetAvailableAudioSourceIndex(true);
        //    }
        //    else if (_audioSourcesPlayingPerPriority[(int)priority] < _maxAudioSourcesPerPriority[(int)priority])
        //    {
        //        index = GetAvailableAudioSourceIndex();
        //    }
        //    if (index != -1)
        //    {
        //        audioPlayers[index].transform.position = position;
        //        PlayAudioClipOnTargetAudioSource(index, false, priority, data.GetAudioClip(), data.GetVolume(), data.GetPitch());
        //    }
        //}


        //public void PlayAudioClip(AudioClip clip, Vector3 position, AudioClipPriority priority, float volume = 1.0f, float pitch = 1.0f)
        //{
        //    if (clip == null) return;
        //    int index = -1;
        //    if (priority == AudioClipPriority.Max)
        //    {
        //        index = GetAvailableAudioSourceIndex(true);
        //    }
        //    else if (_audioSourcesPlayingPerPriority[(int)priority] < _maxAudioSourcesPerPriority[(int)priority])
        //    {
        //        index = GetAvailableAudioSourceIndex();
        //    }
        //    if (index != -1)
        //    {
        //        audioPlayers[index].transform.position = position;
        //        PlayAudioClipOnTargetAudioSource(index, false, priority, clip, volume, pitch);
        //    }
        //}

        //public void PlayAudioClip(AudioClip clip, Transform transformToAttachTo, AudioClipPriority priority, float volume = 1.0f, float pitch = 1.0f)
        //{
        //    if (clip == null) return;
        //    int index = -1;
        //    if (priority == AudioClipPriority.Max)
        //    {
        //        index = GetAvailableAudioSourceIndex(true);
        //    }
        //    else if (_audioSourcesPlayingPerPriority[(int)priority] < _maxAudioSourcesPerPriority[(int)priority])
        //    {
        //        index = GetAvailableAudioSourceIndex();
        //    }
        //    if (index != -1)
        //    {
        //        audioPlayers[index].transform.parent = transformToAttachTo;
        //        audioPlayers[index].transform.localPosition = Vector3.zero; // Reset position relative to parent
        //        Debug.LogError($"Position of target transform: {transformToAttachTo.position}; audio position: {audioPlayers[index].transform.position}");
        //        PlayAudioClipOnTargetAudioSource(index, false, priority, clip, volume, pitch);
        //    }
        //}

        //public void PlayAudioClipLongRange(AudioData data, Vector3 position, AudioClipPriority priority)
        //{
        //    if (data == null)
        //    {
        //        Debug.LogWarning("Tried to play null audio data");
        //        return;
        //    }
        //    int index = -1;
        //    if (priority == AudioClipPriority.Max)
        //    {
        //        index = GetAvailableAudioSourceIndex(true, true);
        //    }
        //    else if (_audioSourcesPlayingPerPriority[(int)priority] < _maxAudioSourcesPerPriority[(int)priority])
        //    {
        //        index = GetAvailableAudioSourceIndex(false, true);
        //    }
        //    if (index != -1)
        //    {
        //        audioPlayersLongRange[index].transform.position = position;
        //        PlayAudioClipOnTargetAudioSource(index, true, priority, data.GetAudioClip(), data.GetVolume(), data.GetPitch());
        //    }
        //}

        //private void PlayAudioClipOnTargetAudioSource(int audioSourceIndex, bool isLongRange, AudioClipPriority priority, AudioClip clip, float volume, float pitch, bool isLooping = false)
        //{
        //    if (!isLongRange)
        //    {
        //        audioPlayers[audioSourceIndex].clip = clip;
        //        audioPlayers[audioSourceIndex].volume = volume;
        //        audioPlayers[audioSourceIndex].pitch = pitch;
        //        audioPlayers[audioSourceIndex].loop = isLooping;
        //        audioPlayers[audioSourceIndex].Play();
        //        _isActiveArray[audioSourceIndex] = true;
        //    }
        //    else
        //    {
        //        audioPlayersLongRange[audioSourceIndex].clip = clip;
        //        audioPlayersLongRange[audioSourceIndex].volume = volume;
        //        audioPlayersLongRange[audioSourceIndex].pitch = pitch;
        //        audioPlayersLongRange[audioSourceIndex].loop = isLooping;
        //        audioPlayersLongRange[audioSourceIndex].Play();
        //        _isActiveLongRangeArray[audioSourceIndex] = true;
        //    }

        //    _activelyPlayingAudioSourceIndexes.Add(new DataToken(audioSourceIndex));
        //    _activelyPlayingAudioSourceIsLongRange.Add(new DataToken(isLongRange));
        //    _activelyPlayingAudioSourcePriorities.Add(new DataToken((int)priority));

        //    _audioSourcesPlayingPerPriority[(int)priority]++;

        //    if (!_isWatchingForEndingClips)
        //    {
        //        _isWatchingForEndingClips = true;
        //        SendCustomEventDelayedFrames(nameof(_WatchAndReturnAudioSourcesThatFinishPlayingLoop), 5);
        //    }
        //}

        ////public void _PlayAudioSourcesNeedingToBePlayed()
        ////{
        ////    if (_audioSourcesNeedingToBePlayed.Count == 0) return;
        ////    for (int i = 0; i < _audioSourcesNeedingToBePlayed.Count; i++)
        ////    {
        ////        ((AudioSource)_audioSourcesNeedingToBePlayed[i].Reference).Play();
        ////    }
        ////    _audioSourcesNeedingToBePlayed.Clear();
        ////    _hasQueuedPlayingAudioSources = false;
        ////}

        //private int GetAvailableAudioSourceIndex(bool shouldForce = false, bool islongRangeSound = false)
        //{
        //    int index = -1;
        //    if (_activelyPlayingAudioSourceIndexes.Count < maxAudioSources)
        //    {
        //        index = !islongRangeSound ? Array.IndexOf(_isActiveArray, false) : Array.IndexOf(_isActiveLongRangeArray, false);
        //        if (index == -1)
        //        {
        //            Debug.LogError("No available audio sources found when one should be findable!");
        //        }
        //    }
        //    else if (shouldForce)
        //    {
        //        index = ForceStopOldestSound();
        //    }
        //    return index;
        //}


        ///// <summary>
        ///// Forces the oldest audio source to stop playing so it can be reused immedietly
        ///// </summary>
        ///// <returns>index of audio source stopped and now available for use</returns>
        //private int ForceStopOldestSound()
        //{
        //    int index = _activelyPlayingAudioSourceIndexes[0].Int;
        //    _activelyPlayingAudioSourceIndexes.RemoveAt(0);
        //    _activelyPlayingAudioSourceIsLongRange.RemoveAt(0);
        //    _activelyPlayingAudioSourcePriorities.RemoveAt(0);
        //    Debug.LogWarning($"Stopping audio clip {audioPlayers[index].clip.name}");
        //    audioPlayers[index].Stop();
        //    _isActiveArray[index] = false;
        //    return index;
        //}

        //public void _WatchAndReturnAudioSourcesThatFinishPlayingLoop()
        //{
        //    DataList newActivelyPlayingAudioSourceIndexes = _activelyPlayingAudioSourceIndexes;
        //    DataList newActivelyPlayingAudioSourceIsLongRange = _activelyPlayingAudioSourceIsLongRange;
        //    DataList newActivelyPlayingAudioSourcePriorities = _activelyPlayingAudioSourcePriorities;
        //    for (int i = 0; i < _activelyPlayingAudioSourceIndexes.Count; i++)
        //    {
        //        int index = _activelyPlayingAudioSourceIndexes[i].Int;
        //        AudioSource audioSource = !_activelyPlayingAudioSourceIsLongRange[i].Boolean ? audioPlayers[index] : audioPlayersLongRange[index];
        //        if (audioSource.isPlaying == false)
        //        {
        //            audioSource.transform.parent = null;
        //            if (!_activelyPlayingAudioSourceIsLongRange[i].Boolean)
        //            {
        //                _isActiveArray[index] = false;
        //            }
        //            else
        //            {
        //                _isActiveLongRangeArray[index] = false;
        //            }

        //            _audioSourcesPlayingPerPriority[_activelyPlayingAudioSourcePriorities[i].Int]--;
        //            newActivelyPlayingAudioSourceIndexes.RemoveAt(i);
        //            newActivelyPlayingAudioSourceIsLongRange.RemoveAt(i);
        //            newActivelyPlayingAudioSourcePriorities.RemoveAt(i);
        //        }
        //    }
        //    _activelyPlayingAudioSourceIndexes = newActivelyPlayingAudioSourceIndexes;
        //    _activelyPlayingAudioSourceIsLongRange = newActivelyPlayingAudioSourceIsLongRange;
        //    _activelyPlayingAudioSourcePriorities = newActivelyPlayingAudioSourcePriorities;

        //    //if(_activelyPlayingAudioSourceLongRangeIndexes.Count > 0)
        //    //{
        //    //    DataList newActivelyPlayingAudioSourceLongRangeIndexes = _activelyPlayingAudioSourceLongRangeIndexes;
        //    //    for (int i = 0; i < _activelyPlayingAudioSourceLongRangeIndexes.Count; i++)
        //    //    {
        //    //        int index = _activelyPlayingAudioSourceLongRangeIndexes[i].Int;
        //    //        if (_audioSources[index].isPlaying != true)
        //    //        {
        //    //            _audioSources[index].transform.parent = null; // Unparent the audio source
        //    //            _isActiveArray[index] = false;
        //    //            _audioSourcesPlayingPerPriority[(int)_currentAudioSourcePriorities[index]]--;
        //    //            newActivelyPlayingAudioSourceLongRangeIndexes.Remove(index);
        //    //        }
        //    //    }
        //    //    _activelyPlayingAudioSourceLongRangeIndexes = newActivelyPlayingAudioSourceLongRangeIndexes;
        //    //}        

        //    if (_activelyPlayingAudioSourceIndexes.Count == 0)
        //    {
        //        _isWatchingForEndingClips = false;
        //    }
        //    else
        //    {
        //        SendCustomEventDelayedFrames(nameof(_WatchAndReturnAudioSourcesThatFinishPlayingLoop), 5);
        //    }
        //}

        //public int PlayLoopingAudio2d(AudioData audioData)
        //{
        //    return PlayAudioClip2D(audioData.GetAudioClip(), audioData.GetVolume(), audioData.GetPitch(), true);
        //}

        //public void StopLoopingAudio2d(int index)
        //{
        //    if (_audioSources2dLooping[index])
        //    {
        //        audioPlayers2d[index].Stop();
        //        _audioSources2dLooping[index] = false;
        //    }
        //    else
        //    {
        //        Debug.LogWarning("Tried to stop a looping clip when it was not a looping clip");
        //    }

        //}

        //public void PlayAudioClip2D(AudioData audioData)
        //{
        //    PlayAudioClip2D(audioData.GetAudioClip(), audioData.GetVolume(), audioData.GetPitch());
        //}

        ////actual looping is disabled until I can make sure the loop ends in edge cases, like during a player dc
        //public int PlayAudioClip2D(AudioClip clip, float volume = 1.0f, float pitch = 1.0f, bool isLooping = false)
        //{
        //    if (clip == null)
        //    {
        //        Debug.LogError("Tried to play 2d audio with null clip!");
        //        return -1;
        //    }
        //    int index = GetAvailableAudioSourceIndex2d();
        //    audioPlayers2d[index].Stop();
        //    audioPlayers2d[index].clip = clip;
        //    audioPlayers2d[index].volume = volume;
        //    audioPlayers2d[index].pitch = pitch;
        //    audioPlayers2d[index].loop = isLooping;
        //    _audioSources2dLooping[index] = isLooping;
        //    audioPlayers2d[index].Play();
        //    return index;
        //}

        //private int GetAvailableAudioSourceIndex2d()
        //{
        //    bool hasValid2dAudioIndex = true;
        //    if (_audioSources2dLooping[_audioSources2dIndex])
        //    {
        //        int startingIndex = _audioSources2dIndex;
        //        hasValid2dAudioIndex = false;

        //        do
        //        {
        //            _audioSources2dIndex++;
        //            _audioSources2dIndex %= audioPlayers2d.Length;
        //            hasValid2dAudioIndex = !_audioSources2dLooping[_audioSources2dIndex];
        //        }
        //        while (!hasValid2dAudioIndex && _audioSources2dIndex != startingIndex);
        //    }

        //    if (!hasValid2dAudioIndex)
        //    {
        //        Debug.LogWarning("All 2d audio sources are looping!  Force stopping!");
        //        audioPlayers2d[_audioSources2dIndex].Stop();
        //        _audioSources2dLooping[_audioSources2dIndex] = false;
        //    }
        //    else if (audioPlayers2d[_audioSources2dIndex].isPlaying)
        //    {
        //        Debug.LogWarning("Next 2d audio source is still playing.  Force stopping!");
        //        audioPlayers2d[_audioSources2dIndex].Stop();
        //    }

        //    int result = _audioSources2dIndex;
        //    _audioSources2dIndex++;
        //    _audioSources2dIndex %= audioPlayers2d.Length;
        //    return result;
        //}

        ////public void PlayMusic(AudioData musicAudioData, bool isLooping = true)
        ////{
        ////    _audioSourceMusic.clip = musicAudioData.GetAudioClip();
        ////    _audioSourceMusic.volume = musicAudioData.GetVolume();
        ////    _audioSourceMusic.pitch = musicAudioData.GetPitch();
        ////    _audioSourceMusic.loop = isLooping;
        ////    _audioSourceMusic.Play();
        ////}

        ////public void _StopMusic()
        ////{
        ////    _audioSourceMusic.Stop();
        ////}
        #endregion
    }
}
