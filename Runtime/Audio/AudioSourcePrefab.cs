using System;
using System.Collections;
using UnityEngine;

namespace OneTon.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioSourcePrefab : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;

        internal void Play(SoundEffect soundEffect, float instanceVolume, bool isSpatial, Action<AudioSourcePrefab> onAudioComplete)
        {
            audioSource.spatialBlend = isSpatial ? 1f : 0f;
            StartCoroutine(PlayRoutine(soundEffect, instanceVolume, isSpatial, onAudioComplete));
        }

        IEnumerator PlayRoutine(SoundEffect soundEffect, float instanceVolume, bool isSpatial, Action<AudioSourcePrefab> onAudioComplete)
        {
            soundEffect.PlayEffectViaAudioSource(audioSource, instanceVolume);
            while (audioSource.isPlaying)
            {
                yield return null;
            }

            onAudioComplete(this);
        }

        public void Stop()
        {
            audioSource.Stop();
        }
    }
}