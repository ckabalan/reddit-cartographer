using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace RedditCartographer {
	class SubReddit {
		private List<string> m_RelatedSubReddits;
		private int m_Subscribers;
		private string m_Title;
		private string m_Name;
		private Point m_Location;
		private Point m_NextLocation;
		private int m_X;
		private int m_Y;
		private Vector m_Velocity;

		public double Mass {
			get { return 1; }
		}

		public Point Location {
			get { return m_Location; }
			set { m_Location = value; }
		}

		public Point NextLocation {
			get { return m_NextLocation; }
			set { m_NextLocation = value; }
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

		public int Xp {
			get { return m_X; }
		}

		public int Yp {
			get { return m_Y; }
		}

		public Vector Velocity {
			get { return m_Velocity; }
			set { m_Velocity = value; }
		}

		public SubReddit() {
			m_RelatedSubReddits = new List<string>();
		}

		public void AddRelatedSubReddit(string Name) {
			m_RelatedSubReddits.Add(Name);
		}

		public List<string> GetRelatedSubReddits() {
			return m_RelatedSubReddits;
		}

		public bool IsRelatedTo(SubReddit OtherReddit) {
			foreach (string curSubReddit in m_RelatedSubReddits) {
				if (curSubReddit == OtherReddit.Name) {
					return true;
				}
			}
			foreach (string curSubReddit in OtherReddit.GetRelatedSubReddits()) {
				if (curSubReddit == this.Name) {
					return true;
				}
			}
			return false;
		}

	}
}
