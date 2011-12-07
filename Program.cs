using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedditCartographer {
	class Program {
		static void Main(string[] args) {
			PrintWelcome();
			Surveyor Surveyor = new Surveyor();
			Cartographer Cartographer = new Cartographer(Surveyor);
			Surveyor.ProcessTopSubReddits(10);
			Console.WriteLine("Execution Finished. Press ENTER to quit!");
			Console.ReadLine();
		}

		private static void PrintWelcome() {
			Console.WriteLine("Welcome to the Reddit Cartographer!");
			Console.WriteLine("-----------------------------------");
		}

	}
}
