using System;
using System.Collections.Generic;
using OneTon.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using OneTon.Logging;

namespace OneTon.Audio
{
    [CreateAssetMenu(fileName = "_AudioPlayer.asset", menuName = "1ton/Audio/AudioPlayer")]
    public class AudioPlayer : ScriptableObject
    {
        private static LogService logger = LogService.Get<AudioPlayer>();
        private static Dictionary<int, SoundEffect> sfxDictionary; // auto-populated dictionary
        [SerializeField][ReadOnly] private GameObject audioParent; // a parent transform for the audio source clones. will auto-generate if not provided via SetNewAudioParent
        [SerializeField] private List<SoundEffect> soundEffects; // sound effects for use in the project
        [SerializeField] GameObject audioSourcePrefab;
        ObjectPool<AudioSourcePrefab> audioSourcePool;

        // Called whenever this SO is loaded (both edit- and play-mode)
        private void OnEnable()
        {
            BuildDictionary();
        }

        /// <summary>
        /// Creates the lookup table from the provided soundEffect instances
        /// </summary>
        /// <param name="soundEffect"></param>
        [Button("Build Dictionary")]
        private void BuildDictionary()
        {
            // Build a lookup from asset name → instance
            sfxDictionary = new Dictionary<int, SoundEffect>();
            foreach (SoundEffect soundEffect in soundEffects)
            {
                if (soundEffect == null) continue;

                int soundEffectId = Utilities.Utils.GetDeterministicHashCode(Utilities.Utils.SanitizeName(soundEffect.name));
                if (!sfxDictionary.ContainsKey(soundEffectId))
                {
                    sfxDictionary[soundEffectId] = soundEffect;
                }
                else
                {
                    logger.Warn($"Duplicate SoundEffect asset name detected: '{soundEffect.name}', {soundEffectId}.");
                }
            }
        }

        private AudioSourcePrefab GetAudioSource()
        {
            if (audioSourcePool == null)
            {
                audioSourcePool = new ObjectPool<AudioSourcePrefab>(CreateAudioSourcePrefabClone);
            }

            AudioSourcePrefab clone = null;

            while (clone?.gameObject == null)
            {
                // NOTE
                // The above conditional leverages Unity's decision to override the == operator to
                // return true if the object has been destroyed, even if the C# reference is non-null
                ///

                clone = audioSourcePool.GetObject();
            }

            return clone;
        }

        private AudioSourcePrefab CreateAudioSourcePrefabClone()
        {
            if (audioParent == null)
            {
                audioParent = new GameObject("[Generated]_AudioParent");
            }

            AudioSourcePrefab clone = GameObject.Instantiate(audioSourcePrefab, audioParent.transform).GetComponentInChildren<AudioSourcePrefab>();
            clone.name = $"{clone.name} {audioSourcePool?.CreatedCount}";
            return clone;
        }

        /// <summary> If auto-generation of the audio parent is not desired, you can explicitly provide a parent</summary>
        /// <param name="newParent">A gameobject that will be used to parent AudioSource clones for grouping/organization</param>
        /// <returns>Returns the original parent, if any</returns>
        public GameObject SetNewAudioParent(GameObject newParent)
        {
            GameObject originalParent = audioParent;
            if (originalParent != null)
            {
                logger.Debug($"Replacing audioParent {originalParent.name} with {newParent.name}");
            }

            audioParent = newParent;
            return originalParent;
        }

        /// <param name="soundEffectId">the identifier in the generated SoundEffectId enum</param>
        /// <param name="instanceVolume">The volume multiplier for this instance</param>
        /// <param name="location">A world-location at which to play the effect (for spatial audio). If null, will play "everywhere" via non-spatial audio</param>
        /// <returns>Returns the AudioSourcePrefab component attached to the cloned gameobject</returns>
        public AudioSourcePrefab PlaySoundEffect(int soundEffectId, float instanceVolume = 1f, Vector3? location = null)
        {
            SoundEffect soundEffect = sfxDictionary[soundEffectId];
            return PlaySoundEffect(soundEffect, instanceVolume, location);
        }

        /// <param name="soundEffect">a SoundEffect scriptable object</param>
        /// <param name="instanceVolume">The volume multiplier for this instance</param>
        /// <param name="location">A world-location at which to play the effect (for spatial audio). If null, will play "everywhere" via non-spatial audio</param>
        /// <returns>Returns the AudioSourcePrefab component attached to the cloned gameobject</returns>
        public AudioSourcePrefab PlaySoundEffect(SoundEffect soundEffect, float instanceVolume = 1f, Vector3? location = null)
        {
            AudioSourcePrefab audioSource = GetAudioSource();
            bool isSpatial = location != null;
            if (isSpatial)
            {
                audioSource.transform.position = (Vector3)location;
            }

            audioSource.Play(soundEffect, instanceVolume, isSpatial, AudioCompleteHandler);
            return audioSource;
        }

        private void AudioCompleteHandler(AudioSourcePrefab audioSource)
        {
            audioSourcePool.AddObjectToPool(audioSource);
            audioSource.transform.SetParent(audioParent.transform);
        }

        /// <summary>
        /// Works with the SoundEffect's "Test" button to enable easier tuning of SoundEfect instances
        /// </summary>
        /// <param name="soundEffect"></param>
        internal void TestSoundEffect(SoundEffect soundEffect)
        {
            CreateAudioSourcePrefabClone().Play(soundEffect, 1f, false, (AudioSourcePrefab audioSource) => {
                if (audioSource.transform.parent == audioParent && audioParent.transform.childCount == 1)
            {
                DestroyImmediate(audioParent);
            }
                else{
                    DestroyImmediate(audioSource.gameObject);
                }
                });
        }
    }

}