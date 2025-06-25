using System;
using OneTon.Utilities;
using OneTon.Utilities.ScriptableObjectHelpers.EnumGeneration;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OneTon.Audio
{
    // /// <summary>
    // /// The SoundEffectPlayRequest class represents an event 
    // /// </summary>
    // public class SoundEffectRequest
    // {
    //     public SoundEffectId SoundEffectId { get; }
    //     public float InstanceVolume { get; }
    //     public Transform Source { get; }
    //     public Action<bool> ResultCallback { get; }


    //     private SoundEffectRequest(SoundEffectId soundEffectId, float instanceVolume, Transform source)
    //     {
    //         SoundEffectId = soundEffectId;
    //         InstanceVolume = instanceVolume;
    //         Source = source;
    //     }

    //     public static AudioSourcePrefab CreateSoundEffect(SoundEffectId soundEffectId, float instanceVolume = 1f, Transform source = null)
    //     {
    //         AudioSourcePrefab audioSourceClone = null;
    //         // AudioPlayer.CreateSoundEffectRequest.Invoke(new SoundEffectRequest(soundEffectId, instanceVolume, source), out audioSourceClone);
    //         return audioSourceClone;
    //     }
    // }


    [CreateAssetMenu(fileName = "_NewSoundEffect.asset", menuName = "1ton/Audio/SoundEffect")]
    [GenerateEnum(outputPath: "Assets/Scripts/Generated/1ton/Enums/SoundEffectId.cs")]
    public class SoundEffect : ScriptableObject
    {
        private static Logging.Logger logger = new();
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

        /*
        // Asset naming enforcement for Enum conversion
        */

#if UNITY_EDITOR
        private void OnValidate()
        {
            string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            string assetName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            string cleanName = GetSanitizedName(assetName);

            if (assetName != cleanName)
            {
                Debug.LogWarning($"SoundEffect '{assetName}' has invalid characters for enum naming. Consider renaming to '{cleanName}'");
            }
        }

        private string GetSanitizedName(string name)
        {
            string sanitized = name.Replace(" ", "_").Replace("-", "_");
            if (char.IsDigit(sanitized[0])) sanitized = "_" + sanitized;
            return sanitized;
        }
#endif
    }
}