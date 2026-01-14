
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

public enum AudioClipPriority
{
    Max,
    High,
    Medium,
    Low,

    Count
}

namespace Vincil.VUSharp.Audio
{

    public static class AudioPriorityTag
    {
        public static string Low()
        {
            return "Low";
        }
        public static string Medium()
        {
            return "Medium";
        }
        public static string High()
        {
            return "High";
        }
        public static string Max()
        {
            return "Max";
        }
    }

    public static class AudioPlayerTypeTags
    {
        public static string Standard3D()
        {
            return "Standard3D";
        }
        public static string LongRange3D()
        {
            return "LongRange3D";
        }
        public static string Audio2D()
        {
            return "Audio2D";
        }
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class AudioManager : UdonSharpBehaviour
    {
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

        public abstract int PlayStoppableAudio(AudioData data, Transform transformToConstrainTo, string audioPriorityTag = "Medium", string audioPlayerTypeTag = "Standard3D", bool shouldLoop = false);

        public abstract int PlayStoppableAudio(AudioData data, Vector3 position, string audioPriorityTag = "Medium", string audioPlayerTypeTag = "Standard3D", bool shouldLoop = false);

        public abstract int PlayStoppableAudioIn2d(AudioData data, string audioPriorityTag = "Medium", bool shouldLoop = false);

        public abstract bool PlayOneShotAudio(AudioData data, Transform transformToConstrainTo, string audioPriorityTag = "Medium", string audioPlayerTypeTag = "Standard3D");

        public abstract bool PlayOneShotAudio(AudioData data, Vector3 position,  string audioPriorityTag = "Medium", string audioPlayerTypeTag = "Standard3D");

        public abstract bool PlayOneShotAudioIn2d(AudioData data, string audioPriorityTag = "Medium");

        public abstract bool StopAudio(int playId);

        public abstract void StopAllAudio();

        ////actual looping is not advised until I can make sure the loop ends in edge cases, like during a player dc
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

        //public void _PlayAudioSourcesNeedingToBePlayed()
        //{
        //    if (_audioSourcesNeedingToBePlayed.Count == 0) return;
        //    for (int i = 0; i < _audioSourcesNeedingToBePlayed.Count; i++)
        //    {
        //        ((AudioSource)_audioSourcesNeedingToBePlayed[i].Reference).Play();
        //    }
        //    _audioSourcesNeedingToBePlayed.Clear();
        //    _hasQueuedPlayingAudioSources = false;
        //}

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

        //public void PlayMusic(AudioData musicAudioData, bool isLooping = true)
        //{
        //    _audioSourceMusic.clip = musicAudioData.GetAudioClip();
        //    _audioSourceMusic.volume = musicAudioData.GetVolume();
        //    _audioSourceMusic.pitch = musicAudioData.GetPitch();
        //    _audioSourceMusic.loop = isLooping;
        //    _audioSourceMusic.Play();
        //}

        //public void _StopMusic()
        //{
        //    _audioSourceMusic.Stop();
        //}
    }
}

