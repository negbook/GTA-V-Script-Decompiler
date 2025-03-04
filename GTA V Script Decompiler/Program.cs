﻿using System;
using System.IO;
using System.Windows.Forms;

namespace Decompiler
{
	static class Program
	{
		public static NativeFile nativefile;
		public static x64NativeFile x64nativefile;
		internal static Ini.IniFile Config;
		public static Object ThreadLock;
		public static int ThreadCount;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			ThreadLock = new object();
			Config = new Ini.IniFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini"));
			if (!File.Exists(Config.path))
			{
				Config.IniWriteValue("Base", "IntStyle", "int");
				Config.IniWriteBool("Base", "Show_Array_Size", false);
				Config.IniWriteBool("Base", "Reverse_Hashes", true);
				Config.IniWriteBool("Base", "Declare_Variables", true);
				Config.IniWriteBool("Base", "Shift_Variables", true);
				Config.IniWriteBool("View", "Show_Nat_Namespace", false);
				Config.IniWriteBool("Base", "Show_Func_Pointer", false);
				Config.IniWriteBool("Base", "Use_MultiThreading", true);
				Config.IniWriteBool("Base", "Include_Function_Position", false);
				Config.IniWriteBool("Base", "Uppercase_Natives", false);
				Config.IniWriteBool("Base", "Hex_Index", false);
				Config.IniWriteBool("View", "Line_Numbers", true);
			}
			Find_Show_Array_Size();
			Find_Reverse_Hashes();
			Find_Declare_Variables();
			Find_Shift_Variables();
			Find_Show_Func_Pointer();
			Find_Use_MultiThreading();
			Find_IncFuncPos();
			Find_Nat_Namespace();
			Find_Hex_Index();
			Find_Upper_Natives();
			//Build NativeFiles from Directory if file exists, if not use the files in the resources
			string path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
				"natives.dat");
			if (File.Exists(path))
				nativefile = new NativeFile(File.OpenRead(path));
			else
				nativefile = new NativeFile(new MemoryStream(Properties.Resources.natives));


			path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
				"x64natives.dat");
			if (File.Exists(path))
				x64nativefile = new x64NativeFile(File.OpenRead(path));
			else
				x64nativefile = new x64NativeFile(new MemoryStream(Properties.Resources.x64natives));

			ScriptFile.npi = new NativeParamInfo();
			ScriptFile.hashbank = new Hashes();

			if (args.Length == 0)
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new MainForm());
			}
			else
			{
				DateTime Start = DateTime.Now;
				string ext = Path.GetExtension(args[0]);
				if (ext == ".full") //handle openIV exporting pc scripts as *.ysc.full
				{
					ext = Path.GetExtension(Path.GetFileNameWithoutExtension(args[0]));
				}
				ScriptFile fileopen;
				Console.WriteLine("Decompiling " + args[0] + "...");
				try
				{
					fileopen = new ScriptFile(File.OpenRead(args[0]), ext != ".ysc");
				}
				catch (Exception ex)
				{
					Console.WriteLine("Error decompiling script " + ex.Message);
					return;
				}
				Console.WriteLine("Decompiled in " + (DateTime.Now - Start).ToString());
				fileopen.Save(File.OpenWrite(args[0] + ".lua"), true);
				Console.WriteLine("Extracing native table...");
				StreamWriter fw = new StreamWriter(File.OpenWrite(args[0] + " native table.txt"));
				foreach (ulong nat in fileopen.X64NativeTable._nativehash)
				{
					string temps = nat.ToString("X");
					while (temps.Length < 16)
						temps = "0" + temps;
					fw.WriteLine(temps);
				}
				fw.Flush();
				fw.Close();
				Console.WriteLine("All done & saved!");
			}
		}

		public enum IntType
		{
			_int,
			_uint,
			_hex
		}

		public static IntType Find_getINTType()
		{
			string s = Program.Config.IniReadValue("Base", "IntStyle").ToLower();
			if (s.StartsWith("int")) return _getINTType = IntType._int;
			else if (s.StartsWith("uint")) return _getINTType = IntType._uint;
			else if (s.StartsWith("hex")) return _getINTType = IntType._hex;
			else
			{
				Program.Config.IniWriteValue("Base", "IntStyle", "int");
				return _getINTType = IntType._int;
			}
		}

		private static IntType _getINTType = IntType._int;

		public static IntType getIntType
		{
			get { return _getINTType; }
		}

		public static bool Find_Show_Array_Size()
		{
			return _Show_Array_Size = Program.Config.IniReadBool("Base", "Show_Array_Size", true);
		}

		private static bool _Show_Array_Size = false;

		public static bool Find_Reverse_Hashes()
		{
			return _Reverse_Hashes = Program.Config.IniReadBool("Base", "Reverse_Hashes", true);
		}

		private static bool _Reverse_Hashes = false;

		public static bool Reverse_Hashes
		{
			get { return _Reverse_Hashes; }
		}

		public static bool Show_Array_Size
		{
			get { return _Show_Array_Size; }
		}

		public static bool Find_Declare_Variables()
		{
			return _Declare_Variables = Program.Config.IniReadBool("Base", "Declare_Variables", true);
		}

		private static bool _Declare_Variables = false;

		public static bool Declare_Variables
		{
			get { return _Declare_Variables; }
		}

		public static bool Find_Shift_Variables()
		{
			return _Shift_Variables = Program.Config.IniReadBool("Base", "Shift_Variables", true);
		}

		private static bool _Shift_Variables = false;

		public static bool Shift_Variables
		{
			get { return _Shift_Variables; }
		}

		public static bool Find_Use_MultiThreading()
		{
			return _Use_MultiThreading = Program.Config.IniReadBool("Base", "Use_MultiThreading", false);
		}

		private static bool _Use_MultiThreading = false;

		public static bool Use_MultiThreading
		{
			get { return _Use_MultiThreading; }
		}


		public static bool Find_IncFuncPos()
		{
			return _IncFuncPos = Program.Config.IniReadBool("Base", "Include_Function_Position", false);
		}

		private static bool _IncFuncPos = false;

		public static bool IncFuncPos
		{
			get { return _IncFuncPos; }
		}


		public static bool Find_Show_Func_Pointer()
		{
			return _Show_Func_Pointer = Program.Config.IniReadBool("Base", "Show_Func_Pointer", false);
		}

		private static bool _Show_Func_Pointer = false;

		public static bool Show_Func_Pointer
		{
			get { return _Show_Func_Pointer; }
		}

		public static bool Find_Nat_Namespace()
		{
			return _Show_Nat_Namespace = Program.Config.IniReadBool("Base", "Show_Nat_Namespace", false);
		}

		private static bool _Show_Nat_Namespace = false;

		public static bool Show_Nat_Namespace
		{
			get { return _Show_Nat_Namespace; }
		}

		public static bool Find_Hex_Index()
		{
			return _Hex_Index = Program.Config.IniReadBool("Base", "Hex_Index", false);
		}

		private static bool _Hex_Index = false;

		public static bool Hex_Index
		{
			get { return _Hex_Index; }
		}

		public static bool Find_Upper_Natives()
		{
			return _upper_Natives = Program.Config.IniReadBool("Base", "Uppercase_Natives", false);
		}

		private static bool _upper_Natives = false;

		public static bool Upper_Natives
		{
			get { return _upper_Natives; }
		}

	}
}
