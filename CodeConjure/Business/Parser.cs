﻿using static CodeConjure.Utils;

namespace CodeConjure
{
	internal class Parser
	{
		internal string rootClassName = "";
		internal string rootNamespace = "";
		internal string templateName = "";
		internal ClassStructure? rootClass;
		internal Settings? settings;
		internal List<string> classes = [];

		internal Parser(Settings settings)
		{
			this.settings = settings;
		}

		internal void Parse(string filePath)
		{
			var lines = ReadFile(filePath)
				.Where(p => !string.IsNullOrEmpty(p) && !p.Trim().StartsWith("//"))
				.ToArray();

			VerifyHeader(lines);
			GetRootNamespaceAndClass(lines);

			rootClass = ClassStructure.BuildStructure(rootClassName, ParseClasses(lines));
		}

		private Dictionary<string, List<Declaration>> ParseClasses(string[] lines)
		{
			string currentClassPath = rootClassName;

			classes.Add(currentClassPath);

			Dictionary<string, List<Declaration>> declarations = [];
			declarations[currentClassPath] = [];
			Declaration? previousDec = null;
			for (int i = 2; i < lines.Length; i++)
			{
				var line = lines[i];

				if (line.StartsWith('[') && line.EndsWith(']'))
				{
					//new sub-class
					currentClassPath = line.Replace("[", "").Replace("]", "").Trim();
					classes.Add(currentClassPath);
					declarations[currentClassPath] = [];
					previousDec = null;

					continue;
				}

				if (line.StartsWith('\t'))
				{
					if (previousDec == null)
					{
						throw new Exception("Attempting to add extras before declaration.");
					}

					// parse line of code
					// append to previous declaration
					line = line.Replace('\t', ' ').Trim();
					previousDec.extras.Add(line);
				}
				else
				{
					Declaration dec = new();
					dec.parseDeclaration(line);
					declarations[currentClassPath].Add(dec);

					previousDec = dec;
				}
			}

			return declarations;
		}

		private void GetRootNamespaceAndClass(string[] lines)
		{
			string rootClassLine = lines[1];
			string rootDeclaration = rootClassLine.Replace('[', ' ').Replace(']', ' ').Trim();
			if (rootClassLine.Contains('.'))
			{
				int n = rootDeclaration.LastIndexOf('.');
				rootNamespace = (settings?.BaseNamespace ?? "") + rootDeclaration[..n];
				rootClassName = rootDeclaration[(n + 1)..];
			}
			else
			{
				if (!String.IsNullOrEmpty(settings?.BaseNamespace))
				{
					rootNamespace = settings.BaseNamespace[..(settings.BaseNamespace.Length - 1)];
				}
				else
				{
					rootNamespace = "NOT_SET";
				}

				rootClassName = rootDeclaration;
			}
		}

		private void VerifyHeader(string[] lines)
		{
			if (lines.Length <= 2)
			{
				throw new Exception("This file is empty.");
			}

			if (lines[0].StartsWith('@'))
			{
				templateName = lines[0].Replace("@", "").Trim();
			}

			if (string.IsNullOrEmpty(templateName))
			{
				throw new Exception("No template declaration found. Usage example: @base will load template_base.txt.");
			}

			if (!lines[1].StartsWith('[') || !lines[1].EndsWith(']') || lines[1].Contains(':'))
			{
				throw new Exception("No root class declaration found. Usage example: [Person] will create a Person root class.");
			}
		}
	}
}
