using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vincil.VUSharp.Audio
{
    public static class AudioExtensions
    {
        public static void SetInverseSquaredRolloff(this AudioSource audioSource, float near, float far, float volumetricRadius)
        {
            if (audioSource == null || near < 0f || volumetricRadius < 0f || far < near)
                return;
            near += volumetricRadius;
            far += volumetricRadius;
            audioSource.rolloffMode = AudioRolloffMode.Custom;
            audioSource.minDistance = near;
            audioSource.maxDistance = far;
            float distance = far - near;
            AnimationCurve curve = audioSource.GetCustomCurve(AudioSourceCurveType.CustomRolloff);
            curve.ClearKeys();
            curve.AddKey(near, 1f);
            curve.AddInverseSquaredRolloffKey(near, far, 0.0078125f);
            curve.AddInverseSquaredRolloffKey(near, far, 0.015625f);
            curve.AddInverseSquaredRolloffKey(near, far, 0.03125f);
            curve.AddInverseSquaredRolloffKey(near, far, 0.0625f);
            curve.AddInverseSquaredRolloffKey(near, far, 0.125f);
            curve.AddInverseSquaredRolloffKey(near, far, 0.25f);
            curve.AddInverseSquaredRolloffKey(near, far, 0.5f);
            curve.AddKey(far, 0f);
            for (int i = 0; i < curve.length; i++)
            {
                curve.SmoothTangents(i, 0f);
            }
            audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, curve);
        }
        private static void AddInverseSquaredRolloffKey(this AnimationCurve curve, float near, float far, float time)
        {
            float range = far - near; // falloff range
            float distance = range * time; // distance starting from near
            float normalizedDistance = distance / range; // normalize the distance as 0-1 range
            float referenceDistance = normalizedDistance * 62f; // scale the distance with a reference scale to match VRChat
            float gain = 1f / Mathf.Pow(1f + referenceDistance, 2f); // inverse square amplitude
            gain += 0.003f; // to better match VRChat
            gain = Mathf.Clamp01(gain);
            curve.AddKey(near + distance, gain);
        }
    }
}
