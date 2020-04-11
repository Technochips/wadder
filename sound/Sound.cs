using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wadder
{
	class Sound
	{
		public double[,] soundData;
		public ushort numChannels;
		public uint sampleRate;
		public double GetMonoSound(int i)
		{
			double d = 0;
			for (int j = 0; j < soundData.GetLength(1); j++) d += soundData[i, j];
			return d/soundData.GetLength(1);
		}
		public Sound GetGenericSound()
		{
			Sound sound = new Sound();
			sound.soundData = soundData;
			sound.sampleRate = sampleRate;
			return sound;
		}
	}
}
