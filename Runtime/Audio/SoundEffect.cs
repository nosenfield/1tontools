using System;
using OneTon.Utilities;
using OneTon.ScriptableObjectHelpers;
using OneTon.EnumGeneration;
using OneTon.Logging;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OneTon.Audio
{
    [CreateAssetMenu(fileName = "_NewSoundEffect.asset", menuName = "1ton/Audio/SoundEffect")]
    [GenerateEnum(outputPath: "Assets/Scripts/Generated/1ton/Enums/SoundEffectId.cs")]
    public class SoundEffect : ScriptableObject
    {
        private static LogService logger = LogService.Get<SoundEffect>();
        [SerializeField] private AudioClip[] clips;
        [SerializeField] private bool randomize;
        [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;

        private int fileIndex = 0;

        public void PlayEffectViaAudioSource(AudioSource audioSource, float instanceVolume)
        {
            audioSource.clip = GetClip();
            audioSource.volume = masterVolume * instanceVolume;
            audioSource.Play();
        }

        private AudioClip GetClip()
        {
            if (clips.Length == 0)
            {
                logger.Error($"SoundEffect {name} has no audio clip!");
            }

            if (clips.Length == 1)
            {
                fileIndex = 0;
            }
            else if (!randomize)
            {
                fileIndex = (fileIndex + 1) % clips.Length;
            }
            else
            {
                int nextIndex = fileIndex;
                while (nextIndex == fileIndex)
                {
                    nextIndex = UnityEngine.Random.Range(0, clips.Length);
                }
                fileIndex = nextIndex;
            }

            return clips[fileIndex];
        }

        /// <summary>
        /// Provides in-editor testing of SFX
        /// </summary>
        [Button("Test")]
        private void Test()
        {
            ScriptableObjectSingleton<AudioPlayer>.Instance.TestSoundEffect(this);
        }
    }
}