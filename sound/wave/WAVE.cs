using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wadder.wave
{
	class WAVE : Sound
	{
		public short audioFormat;
		public int byteRate;
		public short blockAlign;
		public short bitsPerSample;

		public WAVE(byte[] data)
		{
			Chunk riff = new Chunk(data);
			if (riff.id != "RIFF") throw new Exception("Invalid file, not RIFF");
			if (Encoding.ASCII.GetString(riff.data, 0, 4) != "WAVE") throw new Exception("Invalid sound, not WAVE");
			Chunk fmt = new Chunk(riff.data, 4);
			if (fmt.id != "fmt ") throw new Exception("Invalid sound, no fmt");
			audioFormat = BitConverter.ToInt16(fmt.data, 0);
			if (audioFormat != 1) throw new Exception("Cannot open compressed sound files, must be PCM");
			numChannels = BitConverter.ToUInt16(fmt.data, 2);
			sampleRate = BitConverter.ToUInt32(fmt.data, 4);
			byteRate = BitConverter.ToInt32(fmt.data, 8);
			blockAlign = BitConverter.ToInt16(fmt.data, 12);
			bitsPerSample = BitConverter.ToInt16(fmt.data, 14);
			Chunk sdata = new Chunk(riff.data, 28);
			if (sdata.id != "data") throw new Exception("Invalid sound, no data");
			soundData = new double[sdata.size/blockAlign, numChannels];
			for(int i = 0; i < sdata.size; i += blockAlign)
			{
				int bc = blockAlign / numChannels;
				for (int k = 0; k < numChannels; k++)
				{
					bool neg = false;
					for (int j = bc-1; j >= 0; j--)
					{
						int b = sdata.data[i + j + (k * bc)]*(j==0?1:j*256);
						if (j == bc - 1 && (0x80 & sdata.data[i + j + (k * bc)]) > 0) neg = true;
						if (neg) b = (byte)~b;
						soundData[i / blockAlign,k] += b/Math.Pow(2,bitsPerSample);
					}
					if (neg) soundData[i / blockAlign, k] *= -1;
				}
			}
		}
	}
}
