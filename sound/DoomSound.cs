using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wadder.sound
{
	class DoomSound : Sound
	{
		public DoomSound(Sound sound)
		{
			soundData = sound.soundData;
			sampleRate = sound.sampleRate;
		}
		public byte[] Save()
		{
			if (sampleRate > ushort.MaxValue) throw new Exception("Sample rate too high");
			byte[] r = new byte[40+soundData.GetLength(0)];
			r[0] = 3;
			r[1] = 0;
			int a = 2;
			{
				byte[] b_sampleRate = BitConverter.GetBytes((ushort)sampleRate);
				if (!BitConverter.IsLittleEndian) Array.Reverse(b_sampleRate);
				for(int i = 0; i < b_sampleRate.Length; i++) r[i+a] = b_sampleRate[i];
				a += b_sampleRate.Length;
			}
			{
				byte[] b_sampleCount = BitConverter.GetBytes((uint)soundData.GetLength(0) + 32);
				if (!BitConverter.IsLittleEndian) Array.Reverse(b_sampleCount);
				for(int i = 0; i < b_sampleCount.Length; i++) r[i+a] = b_sampleCount[i];
				a += b_sampleCount.Length;
			}
			for(int i = 0; i < soundData.GetLength(0); i++)
			{
				r[i+a+16] = (byte)((GetMonoSound(i)*127)+128);
				if (i == 0 || i == soundData.GetLength(0) - 1)
				{
					for (int j = 0; j < 16; j++)
					{
						r[i + j + a + (i == 0 ? 0 : 17)] = r[i + a + 16]; //i like to make confusing lines
					}
				}
			}
			return r;
		}
	}
}
