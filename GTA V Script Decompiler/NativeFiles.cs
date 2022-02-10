using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Decompiler
{
	class NativeFile : Dictionary<uint, string>
	{
		public string nativefromhash(uint hash)
		{
			if (Program.nativefile.ContainsKey(hash))
			{
				return this[hash];
			}
			else
			{
				string temps = hash.ToString("X");
				while (temps.Length < 8)
					temps = "0" + temps;
				return "unk_0x" + temps;
			}
		}
		public Dictionary<string, uint> revmap = new Dictionary<string, uint>();
		public NativeFile(Stream Nativefile) : base()
		{
			StreamReader sr = new StreamReader(Nativefile);
			while (!sr.EndOfStream)
			{
				string line = sr.ReadLine();
				if (line.Length > 1)
				{
					string[] data = line.Split(':');
					if (data.Length != 3)
						continue;
					string val = data[0];	
					if(val.StartsWith("0x"))
					{
						val = val.Substring(2);
					}
					uint value;
					if (uint.TryParse(val, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value))
					{
						if (!ContainsKey(value))
						{
							var n0x = false;
							string nat = (Program.Show_Nat_Namespace ? (data[1] + "::") : "");
							if(Program.Upper_Natives)
							{
								nat = nat.ToUpper();
								if(data[2].StartsWith("_0x"))
								{
									nat +=  data[2].Remove(0) + data[2].Substring(3).ToUpper() ;
									n0x = true;
								}
								else
								{
									nat += data[2].ToUpper();
								}
							}
							else
							{
								nat = nat.ToLower();
								if(data[2].StartsWith("_0x"))
								{
									nat +=  data[2].Remove(0) + data[2].Substring(3).ToLower();
									n0x = true;
								}
								else
								{


									TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
									data[2] = myTI.ToTitleCase(data[2].ToLower());
									nat += data[2];

								}
							}
							if (n0x)
                            {
								nat = "N_0x" + nat.Replace("_", "");
							}
							else
                            {
								nat = nat.Replace("_", "");
							}
							
								Add(value, nat);
							if (!revmap.ContainsKey(nat))
							{
								revmap.Add(nat, value);
							}
							else
							{
								Console.WriteLine(nat);
							}

						}
					}
				}
			}
			sr.Close();
		}
	}
	class x64NativeFile : Dictionary<ulong, string>
	{
		public ulong TranslateHash(ulong hash)
		{
			if (TranslationTable.ContainsKey(hash))
			{
				hash = TranslationTable[hash];
			}
			return hash;
		}
		public string nativefromhash(ulong hash)
		{
			if (Program.x64nativefile.ContainsKey(hash))
			{
				return this[hash];
			}
			else
			{
				string temps = hash.ToString("X");
				while (temps.Length < 8)
					temps = "0" + temps;
				return "unk_0x" + temps;
			}
		}
		public Dictionary<string, ulong> revmap = new Dictionary<string, ulong>();
		public Dictionary<ulong, ulong> TranslationTable = new Dictionary<ulong, ulong>();
		public x64NativeFile(Stream Nativefile)
			: base()
		{
			StreamReader sr = new StreamReader(Nativefile);
			while (!sr.EndOfStream)
			{
				string line = sr.ReadLine();
				if (line.Length > 1)
				{
					string[] data = line.Split(':');
					if (data.Length != 3)
						continue;
					string val = data[0];
					if(val.StartsWith("0x"))
					{
						val = val.Substring(2);
					}
					ulong value;
					if (ulong.TryParse(val, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value))
					{
						string nat = (Program.Show_Nat_Namespace ? (data[1] + "::") : "");
						var n0x = false;
						if(Program.Upper_Natives)
						{
							nat = nat.ToUpper();
							if(data[2].StartsWith("_0x"))
							{
								nat +=  data[2].Remove(0) + data[2].Substring(3).ToUpper();
								n0x = true;
							}
							else
							{
								nat += data[2].ToUpper();
							}
						}
						else
						{
							nat = nat.ToLower();
							if(data[2].StartsWith("_0x"))
							{
								nat +=  data[2].Remove(0) + data[2].Substring(3).ToLower() ;
								n0x = true;
							}
							else
							{
								TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
								data[2] = myTI.ToTitleCase(data[2].ToLower());
								nat += data[2];
							}
						}
						if (n0x)
						{
							nat = "N_0x" + nat.Replace("_", "");
						}
						else
						{
							nat = nat.Replace("_", "");
						}
						if (!ContainsKey(value))
						{
							Add(value, nat);
							if (!revmap.ContainsKey(nat))
							{
								revmap.Add(nat, value);
							}
                            else
                            {
								Console.WriteLine(nat);
                            }
						}
					}
				}
			}
			sr.Close();
			Stream Decompressed = new MemoryStream();
			Stream Compressed = new MemoryStream(Properties.Resources.native_translation);
			DeflateStream deflate = new DeflateStream(Compressed, CompressionMode.Decompress);
			deflate.CopyTo(Decompressed);
			deflate.Dispose();
			Decompressed.Position = 0;
			sr = new StreamReader(Decompressed);
			while (!sr.EndOfStream)
			{
				string line = sr.ReadLine();
				if (line.Length > 1)
				{
					string val = line.Remove(line.IndexOfAny(new char[] { ':', '=' }));
					string nat = line.Substring(val.Length + 1);
					ulong newer;
					ulong older;
					if (ulong.TryParse(val, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out newer))
					{
						if (!ContainsKey(newer))
						{
							if (ulong.TryParse(nat, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out older))
							{
								if (!TranslationTable.ContainsKey(newer))
								{
									TranslationTable.Add(newer, older);
								}
							}
						}
					}
				}
			}
			sr.Close();
		}
	}
}
