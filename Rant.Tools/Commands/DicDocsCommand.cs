﻿using Rant.Vocabulary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rant.Tools.Commands
{
	[CommandName("tdocs", Description = "Generates a Markdown documentation file all table files in the current directory.")]
	[CommandParam("out", false, "Specifies the output path for the generated file. (Default: ./dictionary.md)")]
	internal class DicDocsCommand : Command
	{
		protected override void OnRun()
		{
			var outputPath = CmdLine.Property("out", Path.Combine(Environment.CurrentDirectory, $"dictionary.md"));
			var tables = Directory.GetFiles(Environment.CurrentDirectory, "*.table", SearchOption.AllDirectories)
				.Select(dir => RantDictionaryTable.FromStream(dir, File.Open(dir, FileMode.Open)))
				.OrderBy(table => table.Name);
			
			using (var writer = new StreamWriter(outputPath))
			{
				foreach(var table in tables)
				{
					writer.WriteLine($"## {table.Name}");
					writer.WriteLine($"**Entries:** {table.EntryCount}\n");
					if (table.HiddenClasses.Any())
						writer.WriteLine($"**Hidden classes:** {table.HiddenClasses.Select(cl => $"`{cl}`").Aggregate((c, n) => $"{c}, {n}")}\n");

					// Write subtype list
					writer.WriteLine($"### Subtypes\n");
					for(int i = 0; i < table.TermsPerEntry; i++)
					{
						writer.WriteLine($"{i + 1}. {table.GetSubtypesForIndex(i).Select(st => $"`{st}`").Aggregate((c, n) => $"{c}, {n}")}");
					}
					writer.WriteLine();

					// Write classes
					writer.WriteLine($"### Classes\n");
					foreach(var cl in table.GetClasses().OrderBy(cl => cl))
					{
						writer.WriteLine($"- `{cl}` ({table.GetEntries().Count(e => e.ContainsClass(cl))})");
					}

					writer.WriteLine();
				}
			}
		}
	}
}
