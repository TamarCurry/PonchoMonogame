using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Poncho;
using Poncho.Interfaces;
using System.Collections.Generic;
using System.Linq;

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
			public int elapsed;
			public int fadeTime;
			public int panTime;
			public int pitchTime;
			public int timeUntilPlayAgain;
			public bool stopIfMuted;
			public float startVolume;
			public float targetVolume;
			public float startPan;
			public float targetPan;
			public float startPitch;
			public float targetPitch;
			public string name;
			public SoundEffectInstance sound;
			
			private VolumeSetting[] _settings;
			
			// --------------------------------------------------------------
			public AudioInstance(VolumeSetting[] settings)
			{
				_settings = settings;
			}
			
			// --------------------------------------------------------------
			public void Update()
			{
				if(sound == null) return;
				elapsed += App.deltaTimeMs;
				sound.Volume = GetValue(startVolume, targetVolume, fadeTime)*_settings.Sum(s => s.level);
				sound.Pan = GetValue(startPan, targetPan, panTime);
				sound.Pitch = GetValue(startPitch, targetPitch, pitchTime);

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
			
			// --------------------------------------------------------------
			private float GetValue(float start, float target, int time)
			{
				float min = start < target ? start : target;
				float max = start < target ? target : start;
				float percent = elapsed < time ? (elapsed*1f/time) : 1;
				return min + ((max - min)*percent);
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
		public void LoadAudio(string path, string name)
		{
			GetAudio(path, name);
		}
		
		// --------------------------------------------------------------
		public void PlayMusic(string path, string name, float volume=1, float endVolume=1, int fadeTimeMs=0,  float pan=0, float pitch=0)
		{
			GetAudio(path, name);
			PlayMusic(name, volume, endVolume, fadeTimeMs, pan, pitch);
		}
		
		// --------------------------------------------------------------
		public void PlayMusic(string name, float volume=1, float endVolume=1, int fadeTimeMs=0,  float pan=0, float pitch=0)
		{
			PlayAudio(name, pan, pitch, AudioType.MUSIC_IN, 0, volume, endVolume, fadeTimeMs);
		}
		
		// --------------------------------------------------------------
		public void CrossfadeMusic(string path, string name, int fadeTimeMs, float endVolume = 1, float pan = 0, float pitch = 0)
		{
			LoadAudio(path, name);
			CrossfadeMusic(name, fadeTimeMs, endVolume, pan, pitch);
		}

		// --------------------------------------------------------------
		public void CrossfadeMusic(string name, int fadeTimeMs, float endVolume=1, float pan=0, float pitch=0)
		{
			if(_activeAudio[0].sound == null) {
				PlayMusic(name, 0, endVolume, fadeTimeMs, pan, pitch);
			}
			else
			{
				StopSoundAt(1);
				
				AudioInstance a = _activeAudio[0];
				AudioInstance b = _activeAudio[1];

				a.stopIfMuted = true;
				b.stopIfMuted = false;

				if(a.sound != null)
				{
					a.fadeTime = fadeTimeMs;
					a.startVolume = a.sound.Volume;
					a.targetVolume = 0;
					a.sound.IsLooped = false;
				}

				_activeAudio[0] = b;
				_activeAudio[1] = a;

				PlayAudio(name, pan, pitch, AudioType.MUSIC_IN, 0, 0, endVolume, fadeTimeMs);
			}
		}

		// --------------------------------------------------------------
		public void PlaySfx(string path, string name, float volume=1, float endVolume=1, int fadeTimeMs=0,  float pan=0, float pitch=0, int timeUntilPlayAgain=100)
		{
			GetAudio(path, name);
			PlaySfx(name, volume, endVolume, fadeTimeMs, pan, pitch, timeUntilPlayAgain);
		}
		
		// --------------------------------------------------------------
		public void PlaySfx(string name, float volume=1, float endVolume=1, int fadeTimeMs=0,  float pan=0, float pitch=0, int timeUntilPlayAgain=100)
		{
			PlayAudio(name, pan, pitch, AudioType.SFX, timeUntilPlayAgain, volume, endVolume, fadeTimeMs);
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
		private void PlayAudio(string name, float pan, float pitch, AudioType type, int timeUntilPlayAgain, float startVolume, float endVolume, int fadeTimeMs)
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
					else if ( _activeAudio[i].name == name && _activeAudio[i].timeUntilPlayAgain >= App.time)
					{
						return;
					}
				}
			}

			SoundEffect audio = GetAudio(name);
			if( audio == null || index < 0 || index >= _activeAudio.Length ) return;
			SoundEffectInstance instance = audio.CreateInstance();
			instance.Volume = startVolume;
			instance.Pan = pan;
			instance.Pitch = pitch;
			instance.IsLooped = type == AudioType.MUSIC_IN;
			StopSoundAt(index);
			_activeAudio[index].elapsed = 0;
			_activeAudio[index].fadeTime = fadeTimeMs > 0 ? fadeTimeMs : 0;
			_activeAudio[index].startVolume = startVolume;
			_activeAudio[index].targetVolume = endVolume;
			_activeAudio[index].name = name;
			_activeAudio[index].sound = instance;
			_activeAudio[index].timeUntilPlayAgain = App.time + timeUntilPlayAgain;
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
