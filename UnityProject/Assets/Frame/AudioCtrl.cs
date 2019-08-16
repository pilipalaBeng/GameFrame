using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oasis
{
    [DisallowMultipleComponent]
    public class AudioCtrl : OasisBase
    {
        private AudioCtrl()
        {
            //防止它人滥用实例化，导致报错
        }
        private List<AudioSource> audioSources;

        public List<AudioSource> AudioSources
        {
            get
            {
                if (audioSources == null)
                {
                    audioSources = new List<AudioSource>();
                }
                return audioSources;
            }

            private set
            {
                audioSources = value;
            }
        }

        public void PlayAudio(AudioSource audioSource, AudioClip audioClip, bool isLoop)
        {
            AddAudioSource(audioSource);
            audioSource.clip = audioClip;
            audioSource.loop = isLoop;
            audioSource.Play();
        }
        public void PlayAudio(AudioSource audioSource, string audioClipName, bool isLoop)
        {
            AddAudioSource(audioSource);
            audioSource.clip = MasterControl.Instance.resourceCtrl.LoadAudioClip(audioClipName);
            audioSource.loop = isLoop;
            audioSource.Play();
        }
        public  void AddAudioSource(AudioSource audioSource)
        {
            if (AudioSources.Contains(audioSource))
            {
                return;
            }
            AudioSources.Add(audioSource);
        }
        public void RemoveAudioSource(AudioSource audioSource)
        {
           if( AudioSources.Contains(audioSource))
            {
                AudioSources.Remove(audioSource);
            }
            else
            {
                Debug.LogError($"RemoveAudioSource  function audioSource is null.");
            }
        }
    }
}