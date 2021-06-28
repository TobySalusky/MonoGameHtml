using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;

namespace Testing // TODO: multiple definitions of word (verb vs noun)
{
	public static class Scraper { 
		public static ScrapingBrowser browser = new ScrapingBrowser();

		public static string[] GetSynonyms(string word) {
			string url = $"https://www.thesaurus.com/browse/{word}";
			try {
				HtmlAgilityPack.HtmlNode html = GetHtml(url);
			
				var synonyms = new List<string>();
			
				var best = html.CssSelect(".css-1kg1yv8");
				var okay = html.CssSelect(".css-1gyuw4i");
				var eh = html.CssSelect(".css-1n6g4vv");
			
				synonyms.AddRange(best.Select(node => node.InnerText));
				synonyms.AddRange(okay.Select(node => node.InnerText));
				synonyms.AddRange(eh.Select(node => node.InnerText));

				return synonyms.ToArray();
			} catch (Exception e) {
				Console.WriteLine($"Failed to get data from: {url}");
				return new string[0];
			}
		}

		public static HtmlAgilityPack.HtmlNode GetHtml(string url) {
			WebPage webpage = browser.NavigateToPage(new Uri(url));
			return webpage.Html;
		}
	}
}
