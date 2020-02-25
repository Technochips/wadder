using System;
using System.IO;
using Newtonsoft.Json;

namespace Wadder
{
	class Wadder
	{
		static int Main(string[] args)
		{
			Console.WriteLine("Wadder - the cool wad compiler");
			int r = Load(args[0]);
			Console.ReadKey();
			return r;
		}
		static int Load(string projectFile)
		{
			string projectFolder = Path.GetDirectoryName(projectFile);
			Console.WriteLine("Loading project...");
			Project project = JsonConvert.DeserializeObject<Project>(File.ReadAllText(projectFile));
			if (project.output == null)
			{
				Console.WriteLine("Output file missing.");
				return 1;
			}
			Console.WriteLine(Path.Combine(projectFolder, project.output));
			return 0;
		}
	}
}
