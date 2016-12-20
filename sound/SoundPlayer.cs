using UnityEngine;

using System.Text;
using System.Collections.Generic;

namespace lpesign
{
    public class SoundPlayer : MonoBehaviour
    {
        #region inspector
        [Header("BGM")]
        [SerializeField]
        bool _enableBGM = true;
        [Range(0, 2)]
        public float bgmMultiplier = 1f;

        [Header("SFX")]
        [SerializeField]
        bool _enableSFX = true;
        [Range(0, 2)]
        public float sfxMultiplier = 1f;
        public int maxChannels = 5;
        #endregion

        AudioSource _BGMChannel;
        List<AudioSource> _SFXChannels;

        Dictionary<string, SoundSchema> _soundMap;

        SoundList _soundList;

        StringBuilder _sb;

        void Awake()
        {
            _soundMap = new Dictionary<string, SoundSchema>();
            _sb = new StringBuilder();

            //create channel
            _BGMChannel = GameObjectUtil.Create<AudioSource>("BGM_Channel", transform);
            _BGMChannel.playOnAwake = false;
            _BGMChannel.loop = true;

            _SFXChannels = new List<AudioSource>();
            for (int i = 0; i < maxChannels; ++i)
            {
                AudioSource ch = GameObjectUtil.Create<AudioSource>("SFX_Channel" + i, transform);
                ch.playOnAwake = false;
                _SFXChannels.Add(ch);
            }

            _soundList = GetComponent<SoundList>();
            if (_soundList != null) SetSoundList(_soundList);

        }

        public void Clear()
        {
            _soundList = null;
            _soundMap.Clear();

            if (_BGMChannel.isPlaying) _BGMChannel.Stop();
            _BGMChannel.clip = null;

            for (int i = 0; i < maxChannels; ++i)
            {
                var ch = _SFXChannels[i];
                if (ch.isPlaying) ch.Stop();
                ch.clip = null;
                ch.transform.localPosition = Vector3.zero;
            }
        }

        public void SetSoundList(SoundList soundList)
        {
            if (soundList == null) return;

            Clear();

            _soundList = soundList;

            foreach (var sound in _soundList.basic)
            {
                AddSound(sound);
            }

            foreach (var soundGroup in _soundList.groups)
            {
                AddSoundGroup(soundGroup);
            }
        }

        void AddSound(SoundSchema sound)
        {
            if (sound == null) return;

            RegisterSound(sound.Name, sound);
        }

        void AddSoundGroup(SoundGroup soundGroup)
        {
            Debug.Log("Add SoundGroup : " + soundGroup.Name);

            RegisterSound(soundGroup.Name, soundGroup);

            foreach (SoundSchema sound in soundGroup.sounds)
            {
                _sb.Append(soundGroup.Name);
                _sb.Append(SoundGroup.SEPARATOR);
                _sb.Append(sound.Name);
                var name = _sb.ToString();
                _sb.Clear();

                RegisterSound(name, sound);
            }
        }

        void RegisterSound(string key, SoundSchema sound)
        {
            Debug.Log("RegisterSound > " + key);
            _soundMap.Add(key, sound);
        }

        SoundSchema FindSound(string name)
        {
            if (_soundMap.ContainsKey(name)) return _soundMap[name];
            else return null;
        }

        void Update()
        {
            if (Time.timeScale == 1f) return;

            float pitch = Mathf.Lerp(0.4f, 1f, Mathf.InverseLerp(0.5f, 1f, Time.timeScale));
            for (int i = 0; i < maxChannels; ++i)
            {
                var source = _SFXChannels[i];
                if (source.isPlaying == false) continue;
                source.pitch = pitch;
            }
        }

        //언제나 하나의 bgm만을 재생
        public AudioSource PlayBGM(string name, float volume = 1f, float pitch = 1f)
        {
            if (string.IsNullOrEmpty(name)) return null;

            return PlayBGM(FindSound(name), volume, pitch);
        }

        public AudioSource PlayBGM(SoundSchema sound, float volume = 1f, float pitch = 1f)
        {
            return PlayBGM(sound.Clip, volume, pitch);
        }

        public AudioSource PlayBGM(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return null;
            if (_enableBGM == false) return null;
            if (_BGMChannel.clip == clip) return null;

            if (_BGMChannel.isPlaying) _BGMChannel.Stop();

            _BGMChannel.clip = clip;
            _BGMChannel.pitch = pitch;
            _BGMChannel.volume = volume * bgmMultiplier;
            _BGMChannel.Play();

            return _BGMChannel;
        }

        public void PauseBGM()
        {
            _BGMChannel.Pause();
        }
        public void ResumeBGM()
        {
            _BGMChannel.UnPause();
        }
        public void StopBGM()
        {
            _BGMChannel.Stop();
        }

        public AudioSource PlaySFX(string name, float volume = 1f, float pitch = 1f)
        {
            return PlaySFX(name, volume, pitch, Vector3.zero);
        }

        public AudioSource PlaySFX(string name, float volume, float pitch, Vector3 position)
        {
            if (string.IsNullOrEmpty(name)) return null;

            SoundSchema sound = FindSound(name);
            return PlaySFX(sound.Clip, volume, pitch, position);
        }

        public AudioSource PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            return PlaySFX(clip, volume, pitch, Vector3.zero);
        }

        public AudioSource PlaySFX(AudioClip clip, float volume, float pitch, Vector3 position)
        {
            if (clip == null) return null;
            if (_enableSFX == false) return null;
            if (maxChannels == 0) return null;

            AudioSource ch = null;
            bool canPlay = false;

            for (int i = 0; i < maxChannels; ++i)
            {
                ch = _SFXChannels[i];
                if (ch.isPlaying == false)
                {
                    canPlay = true;
                    break;
                }
            }

            if (canPlay == false) ch = _SFXChannels[0];
            if (ch == null) return null;

            ch.transform.position = position;
            ch.clip = clip;
            ch.pitch = pitch;
            ch.volume = volume * sfxMultiplier;
            ch.Play();

            return ch;
        }

        public bool bgmMute
        {
            set
            {
                _BGMChannel.mute = value;
            }
        }

        public bool sfxMute
        {
            set
            {
                foreach (AudioSource s in _SFXChannels)
                {
                    s.mute = value;
                }
            }
        }
    }
}