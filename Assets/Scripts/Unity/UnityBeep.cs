using Chip8Sharp.Sound;
using System;
using UnityEngine;

namespace Chip8Sharp.Unity
{
    [RequireComponent(typeof(AudioSource))]
    public class UnityBeep : MonoBehaviour, IBeep
    {
        private AudioSource _audioSource;

        public void Beep()
        {
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();

                if (_audioSource == null)
                {
                    throw new Exception("Audio Source Component is NULL");
                }
            }

            _audioSource.Play();
        }
    }
}

