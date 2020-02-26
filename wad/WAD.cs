using System;
using System.Collections.Generic;
using System.IO;

namespace wadder.wad
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
		public void ResetHeader()
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
		public void ResetDirectory()
		{
			directory = new List<byte>();
			foreach(Lump lump in lumps)
			{
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
			}
		}
		public void AddLump(Lump lump)
		{
			lump.offset = lump.data.Length > 0 ? lumpsSize + 12 : 0;
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
		public void Save(string where)
		{
			ResetHeader();
			ResetDirectory();
			Directory.CreateDirectory(Path.GetDirectoryName(where));
			BinaryWriter file = new BinaryWriter(System.IO.File.Open(where,FileMode.Create));
			file.Write(header.ToArray());
			file.Write(data.ToArray());
			file.Write(directory.ToArray());
			file.Close();
		}
	}
}
