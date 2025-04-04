﻿using System;
using System.Collections.Generic;
using Milease.Core.Animator;
using Milease.DSL;
using Milease.Enums;
using Milease.Utils;
using UnityEngine;

namespace Minity.Audio
{
    public enum AudioPlayerType
    {
        BGMPlayer, BGSPlayer, SndPlayer
    }
    [AddComponentMenu("")]
    internal class AudioPlayer : MonoBehaviour
    {
        private class PlayerData
        {
            public AudioSource AudioSource;
            public AudioClip TargetClip;
            public float Volume;
            public readonly string VolumeKey;

            public PlayerData(AudioPlayerType type, AudioSource source, float defaultVolume)
            {
                VolumeKey = "Milutools.Audio.Volume." + type;
                Volume = PlayerPrefs.GetFloat(VolumeKey, defaultVolume);
                AudioSource = source;
                AudioSource.volume = Volume;
            }
        }

        private PlayerData[] players;

        private AudioSource GenerateAudioSource(bool loop)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.loop = loop;
            return source;
        }
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            var list = new List<PlayerData>
            {
                new PlayerData(AudioPlayerType.BGMPlayer, GenerateAudioSource(true), 0.5f),
                new PlayerData(AudioPlayerType.BGSPlayer, GenerateAudioSource(true), 0.5f),
                new PlayerData(AudioPlayerType.SndPlayer, GenerateAudioSource(false), 1f)
            };

            players = list.ToArray();
        }

        internal float GetVolume(AudioPlayerType type)
            => players[(int)type].Volume;
        
        internal void SetVolume(AudioPlayerType type, float volume)
        {
            var player = players[(int)type];
            player.Volume = volume;
            player.AudioSource.volume = volume;
            PlayerPrefs.SetFloat(player.VolumeKey, volume);
        }
        
        internal void SwitchClip(AudioPlayerType type, AudioClip clip, bool transition, float startPosition)
        {
            var player = players[(int)type];
            if (transition)
            {
                if (!player.AudioSource.clip)
                {
                    player.TargetClip = clip;
                    player.AudioSource.clip = clip;
                    player.AudioSource.Play();
                    player.AudioSource.time = startPosition;
                    player.AudioSource.MQuadOut(x => x.volume, 1f / 0f.To(player.Volume))
                        .PlayImmediately();
                }
                else
                {
                    player.TargetClip = clip;
                    player.AudioSource.MQuadOut(x => x.volume, 1f / 0f.ToThis())
                        .Then(new Action(() =>
                        {
                            player.AudioSource.clip = player.TargetClip;
                            player.AudioSource.Play();
                            player.AudioSource.time = startPosition;
                        }).AsMileaseKeyEvent())
                        .Then(
                            player.AudioSource.MQuadOut(x => x.volume, 1f / 0f.To(player.Volume))
                        ).PlayImmediately();
                }
            }
            else
            {
                player.TargetClip = clip;
                player.AudioSource.clip = clip;
                player.AudioSource.Play();
                player.AudioSource.time = startPosition;
            }
        }

        internal void PlaySnd(AudioClip clip)
            => players[(int)AudioPlayerType.SndPlayer].AudioSource.PlayOneShot(clip);
    }
}
