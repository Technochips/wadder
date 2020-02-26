#pragma warning disable 0649
using Force.Crc32;
using System;
using System.Text;

namespace wadder.png
{
	class Chunk
	{
		public string type;
		public byte[] data;

		public Chunk(byte[] data)
		{
			byte[] r = new byte[4];
			Array.Copy(data, 0, r, 0, 4);
			if (BitConverter.IsLittleEndian) Array.Reverse(r);
			int length = BitConverter.ToInt32(r, 0);

			type = Encoding.ASCII.GetString(data, 4, 4);
			this.data = new byte[length];
			for(int i = 0; i < length; i++)
			{
				this.data[i] = data[i+8];
			}
		}
		public Chunk(string type)
		{
			this.type = type;
		}

		public byte[] CalculateCRC()
		{
			byte[] input = new byte[data.Length + 4];

			byte[] type = Encoding.ASCII.GetBytes(this.type);
			for (int i = 0; i < 4; i++) input[i] = type[i];

			for(int i = 0; i < data.Length; i++) input[i + 4] = data[i];

			byte[] r = BitConverter.GetBytes(Crc32Algorithm.Compute(input));
			Array.Reverse(r);
			return r;
		}
		public byte[] GetData()
		{
			byte[] input = new byte[data.Length + 12];
			//length
			byte[] a = BitConverter.GetBytes(data.Length);
			if (BitConverter.IsLittleEndian) Array.Reverse(a);
			for (int i = 0; i < 4; i++) input[i] = a[i];
			//type
			a = Encoding.ASCII.GetBytes(type);
			for (int i = 0; i < 4; i++) input[i+4] = a[i];
			//data
			for (int i = 0; i < data.Length; i++) input[i+8] = data[i];
			//crc
			a = CalculateCRC();
			for(int i = 0; i < 4; i++) input[i+8+data.Length] = a[i];

			return input;
		}
	}
}
