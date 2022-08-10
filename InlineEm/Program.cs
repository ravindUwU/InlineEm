namespace InlineEm;

using InlineEm.Lib;
using MimeKit;
using System;
using System.IO;

public class Program
{
	public static void Main(string[] args)
	{
		var input = @"C:\Users\rliy0003\Downloads\inlineem\email.mhtml";

		var message = MimeMessage.Load(input);
		var doc = Inliner.Inline(message);

		var output = $"{Path.GetDirectoryName(input)}{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(input)}.html";
		doc.Save(output);

		Console.WriteLine("Done");
	}
}
