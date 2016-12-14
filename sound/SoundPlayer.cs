using UnityEngine;

using System.Collections.Generic;

namespace lpesign
{
    //todo
    //run 중엔 public 변수가 아니라
    //내부적으로 생성한 palette 와 categorymap 이 보여지는게 좋을 것 같다.
    public class SoundPlayer : MonoBehaviour
    {
        [System.Serializable]
        public class SoundCategory
        {
            public string name;
            public List<AudioClip> clips;
        }

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

        [Header("PALETTE")]
        public AudioClip[] basicClips;
        public SoundCategory[] categoryList;
        #endregion

        AudioSource _BGMChannel;
        List<AudioSource> _SFXChannels;

        Dictionary<string, AudioClip> _soundMap;
        Dictionary<string, SoundCategory> _categoryMap;

        void Awake()
        {
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

            //create PALETTE
            _soundMap = new Dictionary<string, AudioClip>();
            _categoryMap = new Dictionary<string, SoundCategory>();

            Initialize(basicClips, categoryList);
        }

        public void Initialize(AudioClip[] basicClips, SoundCategory[] categoryList)
        {
            Clear();

            this.basicClips = basicClips;
            this.categoryList = categoryList;

            AddSound(basicClips);
            AddSound(categoryList);
        }

        void RegisterSound(string key, AudioClip sound)
        {
            _soundMap.Add(key, sound);
        }

        void AddSound(AudioClip[] sounds)
        {
            if (sounds == null) return;

            foreach (AudioClip clip in sounds)
            {
                if (clip == null) continue;

                RegisterSound(clip.name, clip);
            }
        }

        void AddSound(SoundCategory[] categories)
        {
            if (categories == null) return;

            foreach (SoundCategory category in categories)
            {
                _categoryMap.Add(category.name, category);

                foreach (AudioClip clip in category.clips)
                {
                    if (clip == null) continue;

                    string clipName = clip.name;
                    string fullName = category.name + "/" + clipName;

                    if (_soundMap.ContainsKey(fullName))
                    {
                        int i = 0;
                        while (_soundMap.ContainsKey(fullName))
                        {
                            i++;
                            clipName = clip.name + i;
                            fullName = category.name + "/" + clipName;
                        }
                    }

                    _soundMap.Add(fullName, clip);
                }
            }
        }

        public void Clear()
        {
            if (basicClips != null) basicClips = null;
            if (categoryList != null) categoryList = null;

            _soundMap.Clear();
            _categoryMap.Clear();

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

            var clip = FindClip(name);
            return PlayBGM(FindClip(name), volume, pitch);
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

            var clip = FindClip(name);
            return PlaySFX(clip, volume, pitch, position);
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

        public AudioClip FindClip(string name)
        {
            AudioClip clip = null;

            if (name.Contains("/"))
            {
                //split category and name
                string[] chunks = name.Split('/');
                var categoryName = chunks[0];
                var clipName = chunks[1];

                if (_categoryMap.ContainsKey(categoryName) &&
                    (clipName == "Random" || clipName == "*"))
                {
                    var relativeCategory = _categoryMap[categoryName];
                    var ranidx = (int)Random.Range(0, relativeCategory.clips.Count);
                    clip = relativeCategory.clips[ranidx];
                    return clip;
                }
            }

            if (_soundMap.ContainsKey(name))
            {
                clip = _soundMap[name];
            }

            return clip;
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