using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedditCartographer {
	class Cartographer {
		private Surveyor m_Surveyor;

		// http://en.wikipedia.org/wiki/Force-based_layout

		public Cartographer(Surveyor Surveyor) {
			Console.WriteLine("Cartographer Initialized!");
			m_Surveyor = Surveyor;
		}
	}
}
