namespace InlineEm.Lib;

using HtmlAgilityPack;
using MimeKit;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Inliner
{
	public static async Task<HtmlDocument> InlineAsync(MimeMessage message)
	{
		var messageParts = message.BodyParts
			.Where((p) => p.ContentId != null)
			.ToDictionary((p) => p.ContentId);

		var doc = new HtmlDocument();
		doc.LoadHtml(message.HtmlBody);

		var imgNodes = doc.DocumentNode.Descendants()
			.Where((n) => n.NodeType == HtmlNodeType.Element && n.Name == "img");

		foreach (var node in imgNodes)
		{
			if (node.Attributes["src"] is HtmlAttribute srcAttribute)
			{
				var src = srcAttribute.Value;
				if (src.StartsWith("cid:"))
				{
					var id = String.Join(':', src.Split(':').Skip(1));

					if (messageParts.ContainsKey(id) && messageParts[id] is MimePart part)
					{
						using var stream = new MemoryStream();
						await part.Content.WriteToAsync(stream);

						var base64 = Encoding.UTF8.GetString(stream.ToArray());
						node.SetAttributeValue("src", $"data:{part.ContentType.MimeType};base64,{base64}");
					}
				}
			}
		}

		return doc;
	}
}
