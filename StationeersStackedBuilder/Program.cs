using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Reflection;

internal class Program
{
	public static Version Version = Assembly.GetExecutingAssembly().GetName().Version;

	public static string FolderDir = "";

	private static void Main()
	{
		string[] userInput = new string[16];
		FolderDir = AppDomain.CurrentDomain.BaseDirectory;

		Console.WriteLine($"StationeersStackManager {Version} by decxi - Use 'help' for a list of commands.");
		while (true)
		{
			try
			{
				Console.Write("\n>> ");
				userInput[0] = string.Empty;
				userInput[0] = Console.ReadLine().Trim();

				if (Regex.IsMatch(userInput[0], @"[\w-]+"))
				{
					MatchCollection matches = Regex.Matches(userInput[0], @"\w+");
					for (int i = 0; i < matches.Count; i++) userInput[i + 1] = matches[i].Value;
					userInput[1] = userInput[1].ToLower();
				}
				else
				{
					continue;
				}

				Console.WriteLine();

				if (userInput[1] == "exit")
					return;
				else if (userInput[1] == "help")
				{
					Console.WriteLine("StationeersStackManager commands:");
					Console.WriteLine("\thelp: Show all commands");
					Console.WriteLine("\tinfo: Get info about StationeersStackManager");
					Console.WriteLine("\tbuild [folderName]: Build a stack configuration from a specific folder");
					Console.WriteLine("\tbuildall: Build stack configurations from all folders");
					Console.WriteLine("\tclear: Clears the console");
					Console.WriteLine("\texit: Close the program");
				}
				else if (userInput[1] == "buildall")
				{
					int i = 0;
					foreach (string directory in Directory.GetDirectories(FolderDir))
					{
						BuildConfig(Path.GetFileName(directory));
						i++;
					}
					Console.WriteLine($"{i} config(s) built successfuly. ");
				}
				else if (userInput[1] == "build")
				{
					if (!string.IsNullOrEmpty(userInput[2]))
					{
						BuildConfig(userInput[2]);
					}
					else
					{
						Console.WriteLine("Please enter a valid folder name.");
					}
				}
				else if (userInput[1] == "info")
				{
					Console.WriteLine($"StationeersStackManager {Version} by decxi - Use 'help' for a list of commands.");
				}
				else if (userInput[1] == "clear")
				{
					Console.Clear();
					Console.WriteLine($"StationeersStackManager {Version} by decxi - Use 'help' for a list of commands.");
				}
				else
				{
					Console.WriteLine($"'{userInput[1]}' isn't a valid command. Use 'help' for a list of commands.");
				}
			}
			catch (Exception e)
			{
				Console.WriteLine($"ERROR: {e.Message}");
			}
		}
	}

	public static void BuildConfig(string folderName)
	{
		string configDir = $@"{FolderDir}\{folderName}";
		if (!Directory.Exists(configDir))
		{
			Console.WriteLine($"Config folder '{configDir}' doesn't exist.");
			return;
		}
		Console.WriteLine($"Folder '{folderName}' found, building config...");

		using (XmlWriter writer = XmlWriter.Create($@"{FolderDir}\{folderName}.xml", new XmlWriterSettings
		{
			Indent = true,
			IndentChars = "\t",
			NewLineChars = "\n",
			NewLineHandling = NewLineHandling.Replace
		}))
		{
			writer.WriteStartDocument();
			writer.WriteStartElement("GameData");
			writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
			writer.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
			writer.WriteStartElement("ThingMods");

			foreach (string file in Directory.GetFiles(configDir, "*.txt", SearchOption.AllDirectories))
			{
				if (Path.GetFileNameWithoutExtension(file).StartsWith("_")) {
					Console.WriteLine($"\t{Path.GetFileName(file)} starts with '_', skipping");
					continue;
				}
				Console.WriteLine($"\tFound file '{Path.GetFileName(file)}', including in build...");
				string[] lines = File.ReadAllLines(file);
				string stackSize = "";
				for (int i = 0; i < lines.Length; i++)
				{
					lines[i] = (lines[i].IndexOf("//") != -1) ? lines[i].Substring(0, lines[i].IndexOf("//")).Trim() : lines[i];
					lines[i] = lines[i].Trim();
					if (string.IsNullOrEmpty(lines[i])) continue;
					if (int.TryParse(lines[i], out int number))
					{
						stackSize = number.ToString();
						Console.WriteLine($"\t\tSetting stacksize: {stackSize}");
					}
					else
					{
						if (!string.IsNullOrEmpty(stackSize))
						{
							writer.WriteStartElement("ThingModData");
							writer.WriteAttributeString("xsi", "type", null, "StackableModData");
							writer.WriteElementString("PrefabName", lines[i]);
							writer.WriteElementString("MaxQuantity", stackSize);
							writer.WriteEndElement();
							Console.WriteLine($"\t\t\t{lines[i]}");
						}
						else
						{
							Console.WriteLine($"\t\tERROR: Tried to set stack size of {lines[i]}, but stack size hasn't been declared, skipping");
						}
					}
				}
			}
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
		Console.WriteLine($"Config '{folderName}.xml' created successfuly!\n");
	}
}