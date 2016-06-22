using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Poncho;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonchoMonogame
{
	internal class MonogameAudio
	{
		private class AudioInstance
		{
			public uint timeUntilPlayAgain;
			public float fadeSpeed;
			public float targetVolume;
			public string name;
			public SoundEffectInstance sound;

			public void Update(float deltaTime)
			{
				if(sound == null) return;

				if(sound.Volume != targetVolume)
				{
					if (fadeSpeed == 0)
					{
						sound.Volume = targetVolume;
					}
					else if(sound.Volume < targetVolume)
					{
						sound.Volume += fadeSpeed * deltaTime;
						if(sound.Volume > targetVolume) {
							sound.Volume = targetVolume;
							targetVolume = 0;
						}
					}
					else
					{
						sound.Volume -= fadeSpeed * deltaTime;
						if(sound.Volume < targetVolume) {
							sound.Volume = targetVolume;
							targetVolume = 0;
						}
					}
				}
			}
		}
		
		private ContentManager _content;
		private AudioInstance[] _activeAudio;
		private Dictionary<string, SoundEffect> _audio;
		
		// --------------------------------------------------------------
		public MonogameAudio(ContentManager content)
		{
			_content = content;
			_activeAudio = new AudioInstance[32];
			
			for ( int i = 0; i < _activeAudio.Length; ++i )
			{
				_activeAudio[i] = new AudioInstance();
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
		public void LoadAndPlaySfx(string path, string name, float volume, float pan)
		{

		}

		// --------------------------------------------------------------
		public void PlaySfx(string name, float volume=1, float endVolume=1, ushort fadeTimeMs=0,  float pan=0, float pitch=0, ushort timeUntilPlayAgain=100)
		{
			PlayAudio(name, pan, pitch, -1, false, timeUntilPlayAgain, volume, endVolume, fadeTimeMs);
		}

		// --------------------------------------------------------------
		public void Update()
		{
			for( int i = 0; i < _activeAudio.Length; ++i )
			{
				if(_activeAudio[i].sound == null) continue;
				
				_activeAudio[i].Update(App.deltaTime);

				if( i > 0 && _activeAudio[i].sound.State == SoundState.Stopped)
				{
					_activeAudio[i].sound.Dispose();
					_activeAudio[i].sound = null;
				}
			}
		}
		
		// --------------------------------------------------------------
		public void PauseSfx()
		{

		}

		// --------------------------------------------------------------
		public void PauseMusic()
		{

		}

		// --------------------------------------------------------------
		public void PauseAll()
		{

		}

		// --------------------------------------------------------------
		public void ResumeMusic()
		{

		}
		
		// --------------------------------------------------------------
		public void ResumeSfx()
		{

		}
		
		// --------------------------------------------------------------
		public void ResumeAll()
		{

		}

		// --------------------------------------------------------------
		public void StopMusic()
		{

		}
		
		// --------------------------------------------------------------
		public void StopSfx()
		{

		}
		
		// --------------------------------------------------------------
		public void StopAll()
		{

		}

		// --------------------------------------------------------------
		public void PlayMusic(string name, float volume=1, float endVolume=1, ushort fadeTimeMs=0,  float pan=0, float pitch=0)
		{
			PlayAudio(name, pan, pitch, 0, true, 0, volume, endVolume, fadeTimeMs);
		}

		// --------------------------------------------------------------
		public void CrossfadeMusic(string name, ushort fadeTimeMs, float endVolume=1, float pan=0, float pitch=0)
		{
			if(_activeAudio[0].sound == null) {
				PlayMusic(name, 0, endVolume, fadeTimeMs, pan, pitch);
			}
			else
			{
				_activeAudio[1].sound?.Stop(true);
				_activeAudio[1].sound?.Dispose();
				
				AudioInstance a = _activeAudio[0];
				AudioInstance b = _activeAudio[1];

				_activeAudio[0] = b;
				_activeAudio[1] = a;
				PlayAudio(name, pan, pitch, 0, true, 0, 0, endVolume, fadeTimeMs);
				a.fadeSpeed = fadeTimeMs > 0 ? 1000 / fadeTimeMs : 0;
				a.targetVolume = 0;
			}
		}

		// --------------------------------------------------------------
		private void PlayAudio(string name, float pan, float pitch, int index, bool isMusic, uint timeUntilPlayAgain, float startVolume, float endVolume, ushort fadeTimeMs)
		{
			if ( index > -1 )
			{
				index = 0;
				_activeAudio[index]?.sound?.Stop(true);
				_activeAudio[0].sound?.Dispose();
			}
			else {
				for( int i = 1; i < _activeAudio.Length; ++i ) // first index is reserved for music, so skip it
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
			if( audio == null || index < 0 ) return;
			SoundEffectInstance instance = audio.CreateInstance();
			instance.Volume = startVolume;
			instance.Pan = pan;
			instance.Pitch = pitch;
			instance.IsLooped = isMusic;
			_activeAudio[index].name = name;
			_activeAudio[index].sound = instance;
			_activeAudio[index].targetVolume = endVolume;
			_activeAudio[index].fadeSpeed = Math.Abs(endVolume - startVolume) / fadeTimeMs;
			_activeAudio[index].timeUntilPlayAgain = (uint)App.time + timeUntilPlayAgain;
		}

	}
}
