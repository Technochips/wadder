using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using wadder.png;
using wadder.wad;

namespace wadder
{
	class Wadder
	{
		static int Main(string[] args)
		{
			Console.WriteLine("Wadder - the cool wad compiler");
			Dictionary<string, object> arg = new Dictionary<string, object>();
			{
				for (int i = 1; i < args.Length; i++)
				{
					string s = args[i];
					if(s[0] == '-')
					{
						if(i > 1) arg[args[i - 1].Substring(1)] = true;
					}
					else
					{
						if (i > 1)
						{
							int j;
							if (int.TryParse(args[i], out j)) arg[args[i - 1].Substring(1)] = j;
							else arg[args[i - 1].Substring(1)] = args[i];
						}
					}
				}
			}
			int r = Load(args[0], arg);

			Console.ReadKey();
			return r;
		}
		static int Load(string projectFile, Dictionary<string, object> arg)
		{
			string projectFolder = Path.GetDirectoryName(projectFile);
			Console.WriteLine("Loading project...");

			Project project = JsonConvert.DeserializeObject<Project>(System.IO.File.ReadAllText(projectFile));
			if (arg.ContainsKey("output")) project.output = (string)arg["output"];

			if (project.output == null)
			{
				Console.WriteLine("Output file missing.");
				return 1;
			}
			Console.WriteLine(project.lumps.Length + " lumps found.");
			WAD wad = new WAD(project.type == null ? "IWAD" : project.type);
			for(int i = 0; i < project.lumps.Length; i++)
			{
				try
				{
					if (project.lumps[i].file != null) project.lumps[i].data = System.IO.File.ReadAllBytes(Path.Combine(projectFolder, project.lumps[i].file));
					else if (project.lumps[i].data == null) project.lumps[i].data = new byte[0];
					if (project.lumps[i].output == null) project.lumps[i].output = project.lumps[i].input;

					Lump lump = new Lump();
					lump.name = project.lumps[i].name;
					lump.data = project.lumps[i].data;
					if(project.lumps[i].output == "png" && project.lumps[i].args.Length > 0)
					{
						PNG png = new PNG(lump.data);
						png.AddOffset(Convert.ToInt32(project.lumps[i].args[0]), Convert.ToInt32(project.lumps[i].args[1]));
						lump.data = png.RewriteData();
					}
					wad.AddLump(lump);
					Console.WriteLine("Lump " + project.lumps[i].name + " processed.");
				}
				catch(IOException e)
				{
					Console.WriteLine(e);
					Console.WriteLine("Couldn't read file \"" + project.lumps[i].file + "\". Try again?");
					Console.ReadKey(true);
					i--;
				}
			}
			wad.Save(Path.Combine(projectFolder, project.output));
			Console.WriteLine("Saved at " + Path.Combine(projectFolder, project.output).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
			return 0;
		}
	}
}
