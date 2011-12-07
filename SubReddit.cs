using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedditCartographer {
	class SubReddit {
		private List<string> m_RelatedSubReddits;
		private int m_Subscribers;
		private string m_Title;
		private string m_Name;
		private string m_URL;

		public SubReddit() {
			m_RelatedSubReddits = new List<string>();
		}

		public string Name {
			get { return m_Name; }
			set { m_Name = value; }
		}

		public string Title {
			get { return m_Title; }
			set { m_Title = value; }
		}

		public int Subscribers {
			get { return m_Subscribers; }
			set { m_Subscribers = value; }
		}

		public void AddRelatedSubReddit(string Name) {
			m_RelatedSubReddits.Add(Name);
		}

		public List<string> GetRelatedSubReddits() {
			return m_RelatedSubReddits;
		}

	}
}
