using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Poncho;
using Poncho.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Poncho.Audio;

namespace PonchoMonogame
{
	internal class MonogameAudio : IAppAudio
	{
		private enum AudioType
		{
			MUSIC_IN,
			MUSIC_OUT,
			SFX
		}

		private enum AudioAction
		{
			PAUSE,
			STOP,
			RESUME
		}

		private class AudioInstance
		{
			public bool stopIfMuted;
			public float startVolume;
			public float targetVolume;
			public float startPan;
			public float targetPan;
			public float startPitch;
			public float targetPitch;
			public string name;
			public SoundEffectInstance sound;

			private VolumeSetting[] _volumeSettings;
			
			// --------------------------------------------------------------
			public AudioSettings settings { get; private set; }

			// --------------------------------------------------------------
			public AudioInstance(VolumeSetting[] volumeSettings)
			{
				_volumeSettings = volumeSettings;
				settings = new AudioSettings();
			}
			
			// --------------------------------------------------------------
			public void Update()
			{
				if(sound == null) return;
				settings.Update();
				sound.Volume = settings.volume*_volumeSettings.Sum(s => s.level);
				sound.Pan = settings.pan;
				sound.Pitch = settings.pitch;

				if (stopIfMuted && sound.Volume < 0.01)
				{
					sound.Stop(true);
				}

				if (sound.State == SoundState.Stopped)
				{
					sound.Dispose();
					sound = null;
				}
			}
		}

		private class VolumeSetting
		{
			private float _level;

			public float level
			{
				get { return _level; }
				set { if (value > 0) _level = value; }
			}

			public VolumeSetting()
			{
				level = 1;
			}
		}
		
		private VolumeSetting _musicVolume;
		private VolumeSetting _sfxVolume;
		private VolumeSetting _globalVolume;

		private ContentManager _content;
		private AudioInstance[] _activeAudio;
		private Dictionary<string, SoundEffect> _audio;

		public float musicVolume { get { return _musicVolume.level; } set { _musicVolume.level = value; } }
		public float sfxVolume { get { return _sfxVolume.level; } set { _sfxVolume.level = value; } }
		public float globalVolume { get { return _globalVolume.level; } set { _globalVolume.level = value; } }
		
		// --------------------------------------------------------------
		public MonogameAudio(ContentManager content)
		{
			_content = content;
			_audio = new Dictionary<string, SoundEffect>();
			_activeAudio = new AudioInstance[32];
			_musicVolume = new VolumeSetting();
			_sfxVolume = new VolumeSetting();
			_globalVolume = new VolumeSetting();
			VolumeSetting[] sfxVolume = {_globalVolume, _sfxVolume};
			VolumeSetting[] musicVolume = {_globalVolume, _musicVolume};

			for ( int i = 0; i < _activeAudio.Length; ++i )
			{
				_activeAudio[i] = new AudioInstance( i < 2 ? musicVolume : sfxVolume );
			}
		}
		
		// --------------------------------------------------------------
		public void Update()
		{
			for( int i = 0; i < _activeAudio.Length; ++i )
			{
				_activeAudio[i].Update();
			}
		}
		
		// --------------------------------------------------------------
		public void PauseSfx()
		{
			HandleAction(AudioAction.PAUSE, 2, _activeAudio.Length);
		}

		// --------------------------------------------------------------
		public void PauseMusic()
		{
			HandleAction(AudioAction.PAUSE, 0, 2);
		}

		// --------------------------------------------------------------
		public void PauseAll()
		{
			HandleAction(AudioAction.PAUSE, 0, _activeAudio.Length);
		}

		// --------------------------------------------------------------
		public void ResumeMusic()
		{
			HandleAction(AudioAction.RESUME, 0, 2);
		}
		
		// --------------------------------------------------------------
		public void ResumeSfx()
		{
			HandleAction(AudioAction.RESUME, 2, _activeAudio.Length);
		}
		
		// --------------------------------------------------------------
		public void ResumeAll()
		{
			HandleAction(AudioAction.RESUME, 0, _activeAudio.Length);
		}

		// --------------------------------------------------------------
		public void StopMusic()
		{
			HandleAction(AudioAction.STOP, 0, 2);
		}
		
		// --------------------------------------------------------------
		public void StopSfx()
		{
			HandleAction(AudioAction.STOP, 2, _activeAudio.Length);
		}
		
		// --------------------------------------------------------------
		public void StopAll()
		{
			HandleAction(AudioAction.STOP, 0, _activeAudio.Length);
		}
		
		// --------------------------------------------------------------
		public void LoadAudio(string path, string name)
		{
			GetAudio(path, name);
		}
		
		// --------------------------------------------------------------
		public AudioSettings PlayMusic(string path, string name)
		{
			GetAudio(path, name);
			return PlayMusic(name);
		}
		
		// --------------------------------------------------------------
		public AudioSettings PlayMusic(string name)
		{
			return PlayAudio(name, AudioType.MUSIC_IN);
		}
		
		// --------------------------------------------------------------
		public AudioSettings CrossfadeMusic(string path, string name, int durationMs)
		{
			LoadAudio(path, name);
			return CrossfadeMusic(name, durationMs);
		}

		// --------------------------------------------------------------
		public AudioSettings CrossfadeMusic(string name, int durationMs)
		{
			if(_activeAudio[0].sound == null) {
				return PlayMusic(name);
			}

			StopSoundAt(1);
				
			AudioInstance a = _activeAudio[0];
			AudioInstance b = _activeAudio[1];

			a.stopIfMuted = true;
			b.stopIfMuted = false;

			if(a.sound != null)
			{
				a.settings.fadeTo(0, durationMs);
				a.sound.IsLooped = false;
			}

			_activeAudio[0] = b;
			_activeAudio[1] = a;

			return PlayAudio(name, AudioType.MUSIC_IN).fadeFrom(0, durationMs);
		}

		// --------------------------------------------------------------
		public AudioSettings PlaySfx(string path, string name)
		{
			GetAudio(path, name);
			return PlaySfx(name);
		}
		
		// --------------------------------------------------------------
		public AudioSettings PlaySfx(string name)
		{
			return PlayAudio(name, AudioType.SFX);
		}

		// --------------------------------------------------------------
		private SoundEffect GetAudio(string path, string name)
		{
			SoundEffect audio = null;

			if( !_audio.TryGetValue(name, out audio) )
			{
				audio = _content.Load<SoundEffect>(MonogameFuncs.GetPath(path));
				if(audio == null) return null;
				_audio.Add(name, audio);
			}

			return audio;
		}
		
		// --------------------------------------------------------------
		private SoundEffect GetAudio(string name)
		{
			SoundEffect audio = null;
			_audio.TryGetValue(name, out audio);
			return audio;
		}

		// --------------------------------------------------------------
		private void HandleAction(AudioAction action, int startIndex, int stopIndex)
		{
			for (int i = startIndex; i < stopIndex; ++i )
			{
				AudioInstance instance = _activeAudio[i];
				if(action == AudioAction.PAUSE) instance.sound?.Pause();
				else if (action == AudioAction.RESUME) instance.sound?.Resume();
				else if (action == AudioAction.STOP) StopSoundAt(i);
			}
		}
		
		// --------------------------------------------------------------
		private AudioSettings PlayAudio(string name, AudioType type)
		{
			int index = -1;
			
			if (type == AudioType.MUSIC_IN)
			{
				index = 0;
			}
			else if (type == AudioType.MUSIC_OUT)
			{
				index = 1;
			}
			else
			{
				for( int i = 2; i < _activeAudio.Length; ++i ) // first two indexed are reserved for music, so skip them
				{
					if(_activeAudio[i].sound == null)
					{
						index = i;
					}
					else if ( _activeAudio[i].name == name && _activeAudio[i].settings.timeUntilRepeatMs >= App.time)
					{
						return null;
					}
				}
			}

			SoundEffect audio = GetAudio(name);
			if( audio == null || index < 0 || index >= _activeAudio.Length ) return null;
			SoundEffectInstance instance = audio.CreateInstance();
			instance.IsLooped = type == AudioType.MUSIC_IN;
			StopSoundAt(index);
			_activeAudio[index].settings.Reset();
			_activeAudio[index].settings.id = name;
			_activeAudio[index].name = name;
			_activeAudio[index].sound = instance;
			return _activeAudio[index].settings;
		}
		
		// --------------------------------------------------------------
		private void StopSoundAt(int index)
		{
			if(index < 0 && index >= _activeAudio.Length) return;
			_activeAudio[index].sound?.Stop(true);
			_activeAudio[index].sound?.Dispose();
			_activeAudio[index].sound = null;
		}
	}
}
