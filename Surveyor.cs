using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace RedditCartographer {
	class Surveyor {
		public Dictionary<string, SubReddit> m_SubReddits;

		public Surveyor() {
			m_SubReddits = new Dictionary<string, SubReddit>();
			Console.WriteLine("Surveyor Initialized!");
		}

		public void ProcessTopSubReddits(int NumberOfSubReddits) {
			string NextSubRedditListURL = "http://www.reddit.com/reddits/";
			for (int i = 0; i < NumberOfSubReddits; i++) {
				// Get the HTML for the page, process it, and return the URL for the next page.
				NextSubRedditListURL = ProcessPage(NextSubRedditListURL);
			}
		}

		private string ProcessPage(string URL) {
			Console.WriteLine("Surveyor - Retreiving: " + URL);
			HtmlNode.ElementsFlags.Remove("form");
			HtmlDocument Page = new HtmlDocument();
			Page.LoadHtml(GetPageHTML(URL));
			SubReddit tempSR;
			// For every SubReddit "Box" on the reddits page
			foreach (HtmlNode EntryNode in Page.DocumentNode.SelectNodes("//div[@class='entry likes' or @class='entry unvoted']")) {
				tempSR = new SubReddit();
				// Get the Title and Name
				foreach (HtmlNode Node in EntryNode.SelectNodes("p[@class='titlerow']/a[@class='title']")) {
					tempSR.Title = Node.InnerText;
					tempSR.Name = Node.Attributes["href"].Value.Replace("http://www.reddit.com/r/", "").Replace("/", "").ToLower();
					break;
				}
				// Get the Related SubReddits
				HtmlNodeCollection tempHNC = EntryNode.SelectNodes("div[@class='description']/form/div[@class='usertext-body']/div[@class='md']//a[@href]");
				if (tempHNC != null) {
					foreach (HtmlNode MDNode in tempHNC) {
						string Ahref = MDNode.Attributes["href"].Value;
						// Match if it has a "/r/" in it and does NOT contain a "+"
						if (Regex.IsMatch(Ahref, "\\/r\\/", RegexOptions.IgnoreCase) && !Ahref.Contains("+")) {
							// Cut out everything before /r/
							Ahref = Ahref.Substring(Ahref.LastIndexOf("/r/"));
							// Cut out the trailing slash if it exists
							if (Ahref.Substring(Ahref.Length - 1) == "/") {
								Ahref = Ahref.Substring(0, Ahref.Length - 1);
							}
							// If there are no slashes other than the two in "/r/" then this is an
							// actual SubReddit, add it to the list.
							if ((Ahref.Split('/').Length - 1) == 2) {
								tempSR.AddRelatedSubReddit(Ahref.Replace("/r/", "").ToLower());
							}
						}
					}
				}
				// Get the Subscribers
				foreach (HtmlNode Node in EntryNode.SelectNodes("p[@class='tagline']/span[@class='score dislikes']/span[@class='number']")) {
					tempSR.Subscribers = Convert.ToInt32(Node.InnerText.Replace(",", ""));
					break;
				}
				Console.WriteLine("Added: /r/{0} ({1:0,0} Subscribers)", tempSR.Name, tempSR.Subscribers);
				m_SubReddits[tempSR.Name] = tempSR;
			}
			// Get and return the URL to the next page
			foreach (HtmlNode EntryNode in Page.DocumentNode.SelectNodes("//p[@class='nextprev']/a[@rel='nofollow next']")) {
				return EntryNode.Attributes["href"].Value;
			}
			// Wasn't able to find the URL, so...?
			return null;
		}

		private string GetPageHTML(string URL) {
			StringBuilder strBuilder = new StringBuilder();
			byte[] byteBuffer = new byte[8192];
			Console.Write("Creating Request... ");
			WebRequest tempWR = WebRequest.Create(URL);
			tempWR.Proxy = new WebProxy();
			HttpWebRequest request = (HttpWebRequest)tempWR;
			Console.WriteLine("Receiving Response...");
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			Stream resStream = response.GetResponseStream();
			string tempString = null;
			int count = 0;
			do {
				count = resStream.Read(byteBuffer, 0, byteBuffer.Length);
				if (count != 0) {
					tempString = Encoding.ASCII.GetString(byteBuffer, 0, count);
					strBuilder.Append(tempString);
				}
			}
			while (count > 0);
			return strBuilder.ToString();
		}

	}
}
