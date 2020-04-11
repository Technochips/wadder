using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using wadder.png;
using wadder.sound;
using wadder.wad;
using wadder.wave;

namespace wadder
{
	class Wadder
	{
		static int Main(string[] args)
		{
			Console.WriteLine("Wadder - the cool wad compiler");
			Dictionary<string, object> arg = new Dictionary<string, object>();
			{
				for (int i = 1; i <= args.Length; i++)
				{
					if(i == args.Length)
					{
						if (i > 1) arg[args[i - 1].Substring(1)] = true;
						continue;
					}
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

			Dictionary<string, string> defaultLumpIO = new Dictionary<string, string>()
			{
				{"wave","lmp"}
			};

			Project project = JsonConvert.DeserializeObject<Project>(System.IO.File.ReadAllText(projectFile));
			if (arg.ContainsKey("output")) project.output = (string)arg["output"];
			if (arg.ContainsKey("compress")) project.compress = (bool)arg["compress"];
			if (project.defaultLumpIO != null) foreach(KeyValuePair<string, string> entry in project.defaultLumpIO) defaultLumpIO[entry.Key] = entry.Value;

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
					if (project.lumps[i].output == null && project.lumps[i].input != null)
					{
						project.lumps[i].output = defaultLumpIO.ContainsKey(project.lumps[i].input) ? defaultLumpIO[project.lumps[i].input] : project.lumps[i].input;
					}

					Lump lump = new Lump();
					lump.name = project.lumps[i].name;
					lump.data = project.lumps[i].data;
					if (project.lumps[i].input != project.lumps[i].output)
					{
						if (project.lumps[i].input == "wave")
						{
							Sound wave = new WAVE(lump.data);
							if (project.lumps[i].output == "lmp")
							{
								lump.data = new DoomSound(wave.GetGenericSound()).Save();
							}
						}
					}
					if(project.lumps[i].output == "png" && project.lumps[i].args != null && project.lumps[i].args.Length > 0)
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
			int uncompressedsize = wad.lumpsSize;
			if(project.compress)
			{
				Console.WriteLine("Compression enabled");
				/*Console.WriteLine("Calculating data edge difference...");
				int[,] comp = new int[wad.lumps.Count,wad.lumps.Count];
				for(int i = 0; i < wad.lumps.Count; i++) //after writing this i no longer understand what this does but it kinda works so
				{
					for (int j = 0; j < wad.lumps.Count; j++)
					{
						if (i == j) continue;
						int mlength = Math.Max(wad.lumps[i].data.Length, wad.lumps[j].data.Length);
						for(int k = 0; k < mlength; k++)
						{
							int c = 0;
							byte ib;
							byte jb;
							bool done = false;
							do
							{
								if(c >= wad.lumps[j].data.Length || k + c >= wad.lumps[i].data.Length)
								{
									if(k+c >= wad.lumps[i].data.Length) done = true;
									break;
								}
								ib = wad.lumps[i].data[k+c];
								jb = wad.lumps[j].data[c];

								if (ib == jb) c++;
								else c = 0;
							}
							while (ib == jb);
							if (done)
							{
								comp[i, j] = c;
								break;
							}
						}
					}
				}*/

				Console.WriteLine("Checking duplicates...");

				List<int> lumporder = new List<int>();
				int[] duplicate = new int[wad.lumps.Count*2];
				{
					int duplicateCount = 0;
					for (int i = 0; i < wad.lumps.Count; i++)
					{
						duplicate[i * 2] = -1;
						duplicate[(i * 2) + 1] = -1;
					}
					for (int i = 0; i < wad.lumps.Count; i++)
					{
						if (wad.lumps[i].data.Length == 0) continue;
						if (duplicate[i * 2] >= 0) continue;
						for (int j = i == 0 ? i+1 : i-1; j < wad.lumps.Count; j += j < i ? -1 : 1)
						{
							if (j < 0) j = i;
							if (duplicate[j*2] >= 0) continue;
							if (wad.lumps[j].data.Length == 0) continue;
							if (i == j) continue;
							//if (comp[i, j] == comp[j, i] && wad.lumps[i].data.Length == wad.lumps[j].data.Length && comp[i, j] == wad.lumps[i].data.Length)
							if (wad.lumps[j].data.Length <= wad.lumps[i].data.Length)
							{
								int dif = wad.lumps[i].data.Length - wad.lumps[j].data.Length;
								dif = dif == 0 ? 1 : dif;
								for (int k = 0; k < dif; k++)
								{
									int l;
									for (l = 0; l < wad.lumps[j].data.Length; l++) if (wad.lumps[i].data[l+k] != wad.lumps[j].data[l]) break;
									if (l == wad.lumps[j].data.Length)
									{
										duplicate[j*2] = i;
										duplicate[(j*2)+1] = k;
										duplicateCount++;
									}
								}
							}
						}
					}
					for (int i = 0; i < wad.lumps.Count; i++)
					{
						if (duplicate[i * 2] == -1) lumporder.Add(i);
					}
					Console.WriteLine(duplicateCount + " duplicates found.");
				}

				/*Console.WriteLine("Reordering lumps...");

				lumporder.Sort((x,y) =>
				{
					return comp[y,x] - comp[x, y];
				});*/


				Console.WriteLine("Rewriting...");

				List<byte> newlumps = new List<byte>();
				int lumpsSize = 0;
				int offset = 12;
				for(int i = 0; i < lumporder.Count; i++)
				{
					Lump lump = wad.lumps[lumporder[i]];
					if (i == 0)
					{
						lump.offset = offset;
						wad.lumps[lumporder[i]] = lump;
						foreach (byte b in lump.data)
						{
							newlumps.Add(b);
							lumpsSize++;
						}
						continue;
					}
					else
					{
						int magic = 0;// comp[lumporder[i-1], lumporder[i]];
						offset += wad.lumps[lumporder[i - 1]].data.Length - magic;
						lump.offset = offset;
						wad.lumps[lumporder[i]] = lump;
						for (int j = magic; j < wad.lumps[lumporder[i]].data.Length; j++)
						{
							newlumps.Add(wad.lumps[lumporder[i]].data[j]);
							lumpsSize++;
						}
					}
				}
				for (int i = 0; i < duplicate.Length/2; i++)
				{
					if (duplicate[i*2] >= 0)
					{
						Lump duplump = wad.lumps[i];
						duplump.offset = wad.lumps[duplicate[i*2]].offset+ duplicate[(i*2)+1];
						wad.lumps[i] = duplump;
					}
				}
				wad.data = newlumps;
				wad.lumpsSize = lumpsSize;
			}
			Console.WriteLine("Saving...");
			wad.Save(Path.Combine(projectFolder, project.output));
			int totalsize = wad.lumpsSize + wad.header.Count + wad.directory.Count;
			Console.WriteLine(totalsize + " bytes");
			if (project.compress)
			{
				int old = wad.header.Count + wad.directory.Count + uncompressedsize;
				Console.WriteLine("Compared to " + old + " bytes uncompressed, that's a " + ((float)totalsize / old * 100) + "% size difference!");
			}
			Console.WriteLine("Saved at " + Path.Combine(projectFolder, project.output).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
			return 0;
		}
	}
}
