#pragma warning disable 0649
using System;
using System.Collections.Generic;

namespace wadder.png
{
	class PNG
	{
		public byte[] header;
		public List<Chunk> chunks;
		public byte[] data;

		public PNG(byte[] data)
		{
			this.data = data;
			this.chunks = new List<Chunk>();
			header = new byte[8];
			for(int i = 0; i < 8; i++)
			{
				header[i] = data[i];
			}
			int p = 8;
			while(p < data.Length)
			{
				byte[] l = new byte[4];
				Array.Copy(data, p, l, 0, 4);
				if (BitConverter.IsLittleEndian) Array.Reverse(l);
				int length = BitConverter.ToInt32(l, 0);

				byte[] r = new byte[length+12];
				Array.Copy(data, p, r, 0, length + 12);
				if(r.Length > 0) chunks.Add(new Chunk(r));
				p += length + 12;
			}
		}
		public byte[] RewriteData()
		{
			List<byte> data = new List<byte>();
			foreach(byte b in header) data.Add(b);

			foreach (Chunk chunk in chunks)
			{
				byte[] d = chunk.GetData();
				foreach (byte b in d) data.Add(b);
			}

			this.data = data.ToArray();
			return this.data;
		}
		public void AddOffset(int x, int y)
		{
			int p = -1;
			bool exists = false;
			for(int i = 0; i < chunks.Count; i++)
			{
				if(chunks[i].type == "IHDR")
				{
					p = i;
				}
				else if(chunks[i].type == "grAb")
				{
					p = i;
					exists = true;
					break;
				}
			}
			if (p == -1) return;

			Chunk c = new Chunk("grAb");
			byte[] bx = BitConverter.GetBytes(x);
			byte[] by = BitConverter.GetBytes(y);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bx);
				Array.Reverse(by);
			}
			c.data = new byte[8];
			for (int j = 0; j < 4; j++)
			{
				c.data[j] = bx[j];
				c.data[j + 4] = by[j];
			}
			if (exists)
				chunks[p] = c;
			else
				chunks.Insert(p + 1, c);
		}
	}
}
