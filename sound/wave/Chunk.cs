using System;
using System.Text;

namespace wadder.wave
{
	class Chunk
	{
		public string id;
		public int size;
		public byte[] data; 
		public Chunk(string id, int size)
		{
			this.id = id;
			this.size = size;
			data = new byte[size];
		}
		public Chunk(byte[] data, int index = 0)
		{
			id = Encoding.ASCII.GetString(data, 0+index, 4);
			size = BitConverter.ToInt32(data, 4+index);
			this.data = new byte[size];
			for(int i = 0; i < size; i++) this.data[i] = data[i+8+index];
		}
	}
}
