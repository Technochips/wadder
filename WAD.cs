using System;
using System.Collections.Generic;
using System.IO;

namespace wadder
{
	class WAD
	{
		public string type;
		public List<Lump> lumps;
		public int lumpsSize = 0;

		public List<byte> header;
		public List<byte> data;
		public List<byte> directory;

		public WAD(string type)
		{
			this.type = type;
			lumps = new List<Lump>();
			data = new List<byte>();
			directory = new List<byte>();
		}
		public void resetHeader()
		{
			header = new List<byte>();

			for (int i = 0; i < 4; i++)
			{
				header.Add(Convert.ToByte(type[i]));
			}

			byte[] bytes = BitConverter.GetBytes(lumps.Count);
			if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
			for(int i = 0; i < 4; i++) header.Add(bytes[i]);

			bytes = BitConverter.GetBytes(lumpsSize+12);
			if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
			for (int i = 0; i < 4; i++) header.Add(bytes[i]);
		}
		public void addLump(Lump lump)
		{
			lump.offset = lumpsSize + 12;
			lumps.Add(lump);
			lumpsSize += lump.data.Length;

			byte[] bytes = BitConverter.GetBytes(lump.offset);
			if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
			for (int i = 0; i < 4; i++) directory.Add(bytes[i]);

			bytes = BitConverter.GetBytes(lump.data.Length);
			if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
			for (int i = 0; i < 4; i++) directory.Add(bytes[i]);

			for (int i = 0; i < 8; i++)
			{
				if (i >= lump.name.Length)
				{
					directory.Add(0);
					continue;
				}
				directory.Add(Convert.ToByte(lump.name[i]));
			}

			foreach (byte b in lump.data) data.Add(b);
		}
		public void save(string where)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(where));
			BinaryWriter file = new BinaryWriter(System.IO.File.OpenWrite(where));
			resetHeader();
			file.Write(header.ToArray());
			file.Write(data.ToArray());
			file.Write(directory.ToArray());
			file.Close();
		}
	}
}
