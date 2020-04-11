#pragma warning disable 0649
using System.Collections.Generic;

namespace wadder
{
	struct Project
	{
		public string output;
		public string type;
		public bool compress;
		public File[] lumps;
		public Dictionary<string, string> defaultLumpIO;
	}
}
