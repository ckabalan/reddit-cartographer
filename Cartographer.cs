using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Drawing2D;

namespace RedditCartographer {
	class Cartographer {
		private const double ATTRACTION_CONSTANT = 0.1;		// spring constant
		private const double REPULSION_CONSTANT = 100;	// charge constant
		private const double DEFAULT_DAMPING = 0.5;
		private const int DEFAULT_SPRING_LENGTH = 50;
		private const int DEFAULT_MAX_ITERATIONS = 500;

		private Surveyor m_Surveyor;
		private int m_Width;
		private int m_Height;

		// http://en.wikipedia.org/wiki/Force-based_layout

		public Cartographer(Surveyor Surveyor, int Width, int Height) {
			Console.WriteLine("Cartographer Initialized!");
			m_Surveyor = Surveyor;
			m_Width = Width;
			m_Height = Height;
		}

		public void RemoveOrphans() {
			foreach (KeyValuePair<string, SubReddit> Node in m_Surveyor.m_SubReddits) {
				SubReddit curSubReddit = (SubReddit)Node.Value;
				foreach (KeyValuePair<string, SubReddit> NodeSearch in m_Surveyor.m_SubReddits) {
					SubReddit curSubRedditSearch = (SubReddit)NodeSearch.Value;

				}
			}
		}

		public void DrawMap() {
			RemoveOrphans();
			AssignRandomLocations();
			int i = 0;
			GenerateImage(String.Format("Output\\Out_{0:0000}.bmp", i));
			for (i = 1; i < 2000; i++) {
				ApplyForces(DEFAULT_DAMPING, DEFAULT_SPRING_LENGTH, DEFAULT_MAX_ITERATIONS, true);
				if (i % 25 == 0) {
					GenerateImage(String.Format("Output\\Out_{0:0000}.bmp", i));
				}
			}
		}

		private void AssignRandomLocations() {
			Console.WriteLine("Assigning Random Locations...");
			Random random = new Random();
			foreach (KeyValuePair<string, SubReddit> Node in m_Surveyor.m_SubReddits) {
				SubReddit curSubReddit = (SubReddit)Node.Value;
				curSubReddit.Location = new Point(
					random.Next(Convert.ToInt32(m_Width * 0.05), Convert.ToInt32(m_Width * 0.95)),
					random.Next(Convert.ToInt32(m_Height * 0.05), Convert.ToInt32(m_Height * 0.95))
				);
			}
		}

		private void GenerateImage(string Output) {
			Console.WriteLine("Generating Image... " + Output);
			SolidBrush SolidBlackBrush = new SolidBrush(Color.Black);
			Pen BlackPen = new Pen(Color.Black, 1);
			Bitmap tempBmp = new Bitmap(m_Width, m_Height);
			Graphics tempGfx = Graphics.FromImage(tempBmp);
			tempGfx.TextRenderingHint = TextRenderingHint.AntiAlias;
			tempGfx.SmoothingMode = SmoothingMode.AntiAlias;
			Point PointOne;
			Point PointTwo;
			SizeF TextSize;
			Rectangle LogicalBounds = GetDiagramBounds();
			double Scale = 1;
			if (LogicalBounds.Width > LogicalBounds.Height) {
				if (LogicalBounds.Width != 0) { Scale = (double)Math.Min(2000, 2000) / (double)LogicalBounds.Width; }
			} else {
				if (LogicalBounds.Height != 0) { Scale = (double)Math.Min(2000, 2000) / (double)LogicalBounds.Height; }
			}
			foreach (KeyValuePair<string, SubReddit> Node in m_Surveyor.m_SubReddits) {
				SubReddit curSubReddit = (SubReddit)Node.Value;
				tempGfx.DrawString("/r/" + curSubReddit.Name, new Font("Arial", 12), SolidBlackBrush, ScalePoint(curSubReddit.Location, Scale));
				foreach (string RelatedSubReddit in curSubReddit.GetRelatedSubReddits()) {
					if (m_Surveyor.m_SubReddits.ContainsKey(RelatedSubReddit)) {
						TextSize = tempGfx.MeasureString("/r/" + curSubReddit.Name, new Font("Arial", 12));
						PointOne = ScalePoint(new Point(
							Convert.ToInt32(curSubReddit.Location.X + (TextSize.Width / 2)),
							Convert.ToInt32(curSubReddit.Location.Y + (TextSize.Height / 2))
						), Scale);
						TextSize = tempGfx.MeasureString("/r/" + RelatedSubReddit, new Font("Arial", 12));
						PointTwo = ScalePoint(new Point(
							Convert.ToInt32(m_Surveyor.m_SubReddits[RelatedSubReddit].Location.X + (TextSize.Width / 2)),
							Convert.ToInt32(m_Surveyor.m_SubReddits[RelatedSubReddit].Location.Y + (TextSize.Height / 2))
						), Scale);
						tempGfx.DrawLine(BlackPen, PointOne, PointTwo);
					}
				}
			}
			tempBmp.Save(Output);
		}

		/// <summary>
		/// Runs the force-directed layout algorithm on this Diagram, using the specified parameters.
		/// </summary>
		/// <param name="Damping">Value between 0 and 1 that slows the motion of the nodes during layout.</param>
		/// <param name="SpringLength">Value in pixels representing the length of the imaginary springs that run along the connectors.</param>
		/// <param name="MaxIterations">Maximum number of iterations before the algorithm terminates.</param>
		/// <param name="Deterministic">Whether to use a random or deterministic layout.</param>
		public void ApplyForces(double Damping, int SpringLength, int MaxIterations, bool Deterministic) {
			// random starting positions can be made deterministic by seeding System.Random with a constant
			Random rnd = Deterministic ? new Random(0) : new Random();
			double totalDisplacement = 0;
			foreach (KeyValuePair<string, SubReddit> NodeAKVP in m_Surveyor.m_SubReddits) {
				SubReddit NodeA = (SubReddit)NodeAKVP.Value;
				// express the node's current position as a vector, relative to the origin
				Vector currentPosition = new Vector(CalcDistance(Point.Empty, NodeA.Location), GetBearingAngle(Point.Empty, NodeA.Location));
				Vector netForce = new Vector(0, 0);
				// determine repulsion between nodes
				foreach (KeyValuePair<string, SubReddit> NodeBKVP in m_Surveyor.m_SubReddits) {
					SubReddit NodeB = (SubReddit)NodeBKVP.Value;
					if (NodeB != NodeA) {
						netForce += CalcRepulsionForce(NodeA, NodeB);
					}
				}
				// determine attraction caused by connections
				foreach (string RelatedSubReddit in NodeA.GetRelatedSubReddits()) {
					if (m_Surveyor.m_SubReddits.ContainsKey(RelatedSubReddit)) {
						netForce += CalcAttractionForce(NodeA, m_Surveyor.m_SubReddits[RelatedSubReddit], SpringLength);
					}
				}
				//foreach (Node parent in mNodes) {
				//	if (parent.Connections.Contains(NodeA)) netForce += CalcAttractionForce(NodeA, parent, springLength);
				//}
				// apply net force to node velocity
				NodeA.Velocity = (NodeA.Velocity + netForce) * Damping;
				// apply velocity to node position
				NodeA.NextLocation = (currentPosition + NodeA.Velocity).ToPoint();
			}
			foreach (KeyValuePair<string, SubReddit> NodeAKVP in m_Surveyor.m_SubReddits) {
				SubReddit NodeA = (SubReddit)NodeAKVP.Value;
				totalDisplacement += CalcDistance(NodeA.Location, NodeA.NextLocation);
				NodeA.Location = NodeA.NextLocation;
			}
		}

		/// <summary>
		/// Calculates the attraction force between two connected nodes, using the specified spring length.
		/// </summary>
		/// <param name="x">The node that the force is acting on.</param>
		/// <param name="y">The node creating the force.</param>
		/// <param name="springLength">The length of the spring, in pixels.</param>
		/// <returns>A Vector representing the attraction force.</returns>
		private Vector CalcAttractionForce(SubReddit x, SubReddit y, double springLength) {
			int proximity = Math.Max(CalcDistance(x.Location, y.Location), 1);
			// Hooke's Law: F = -kx
			double force = ATTRACTION_CONSTANT * Math.Max(proximity - springLength, 0);
			double angle = GetBearingAngle(x.Location, y.Location);
			return new Vector(force, angle);
		}

		/// <summary>
		/// Calculates the distance between two points.
		/// </summary>
		/// <param name="a">The first point.</param>
		/// <param name="b">The second point.</param>
		/// <returns>The pixel distance between the two points.</returns>
		public static int CalcDistance(Point a, Point b) {
			double xDist = (a.X - b.X);
			double yDist = (a.Y - b.Y);
			return (int)Math.Sqrt(Math.Pow(xDist, 2) + Math.Pow(yDist, 2));
		}

		/// <summary>
		/// Calculates the repulsion force between any two nodes in the diagram space.
		/// </summary>
		/// <param name="x">The node that the force is acting on.</param>
		/// <param name="y">The node creating the force.</param>
		/// <returns>A Vector representing the repulsion force.</returns>
		private Vector CalcRepulsionForce(SubReddit x, SubReddit y) {
			int proximity = Math.Max(CalcDistance(x.Location, y.Location), 1);
			// Coulomb's Law: F = k(Qq/r^2)
			double force = -(REPULSION_CONSTANT / Math.Pow(proximity, 2));
			double angle = GetBearingAngle(x.Location, y.Location);
			return new Vector(force, angle);
		}

		///// <summary>
		///// Draws the diagram using GDI+, centering and scaling within the specified bounds.
		///// </summary>
		///// <param name="graphics">GDI+ Graphics surface.</param>
		///// <param name="bounds">Bounds in which to draw the diagram.</param>
		//public void Draw(Graphics graphics, Rectangle bounds) {
		//	Point center = new Point(bounds.X + (bounds.Width / 2), bounds.Y + (bounds.Height / 2));
		//
		//	// determine the scaling factor
		//	Rectangle logicalBounds = GetDiagramBounds();
		//	double scale = 1;
		//	if (logicalBounds.Width > logicalBounds.Height) {
		//		if (logicalBounds.Width != 0) scale = (double)Math.Min(bounds.Width, bounds.Height) / (double)logicalBounds.Width;
		//	} else {
		//		if (logicalBounds.Height != 0) scale = (double)Math.Min(bounds.Width, bounds.Height) / (double)logicalBounds.Height;
		//	}
		//
		//	// draw all of the connectors first
		//	foreach (Node node in mNodes) {
		//		Point source = ScalePoint(node.Location, scale);
		//
		//		// connectors
		//		foreach (Node other in node.Connections) {
		//			Point destination = ScalePoint(other.Location, scale);
		//			node.DrawConnector(graphics, center + (Size)source, center + (Size)destination, other);
		//		}
		//	}
		//
		//	// then draw all of the nodes
		//	foreach (Node node in mNodes) {
		//		Point destination = ScalePoint(node.Location, scale);
		//
		//		Size nodeSize = node.Size;
		//		Rectangle nodeBounds = new Rectangle(center.X + destination.X - (nodeSize.Width / 2), center.Y + destination.Y - (nodeSize.Height / 2), nodeSize.Width, nodeSize.Height);
		//		node.DrawNode(graphics, nodeBounds);
		//	}
		//}

		/// <summary>
		/// Calculates the bearing angle from one point to another.
		/// </summary>
		/// <param name="start">The node that the angle is measured from.</param>
		/// <param name="end">The node that creates the angle.</param>
		/// <returns>The bearing angle, in degrees.</returns>
		private double GetBearingAngle(Point start, Point end) {
			Point half = new Point(start.X + ((end.X - start.X) / 2), start.Y + ((end.Y - start.Y) / 2));

			double diffX = (double)(half.X - start.X);
			double diffY = (double)(half.Y - start.Y);

			if (diffX == 0) diffX = 0.001;
			if (diffY == 0) diffY = 0.001;

			double angle;
			if (Math.Abs(diffX) > Math.Abs(diffY)) {
				angle = Math.Tanh(diffY / diffX) * (180.0 / Math.PI);
				if (((diffX < 0) && (diffY > 0)) || ((diffX < 0) && (diffY < 0))) angle += 180;
			} else {
				angle = Math.Tanh(diffX / diffY) * (180.0 / Math.PI);
				if (((diffY < 0) && (diffX > 0)) || ((diffY < 0) && (diffX < 0))) angle += 180;
				angle = (180 - (angle + 90));
			}

			return angle;
		}

		/// <summary>
		/// Determines the logical bounds of the diagram. This is used to center and scale the diagram when drawing.
		/// </summary>
		/// <returns>A System.Drawing.Rectangle that fits exactly around every node in the diagram.</returns>
		private Rectangle GetDiagramBounds() {
			int minX = Int32.MaxValue, minY = Int32.MaxValue;
			int maxX = Int32.MinValue, maxY = Int32.MinValue;
			foreach (KeyValuePair<string, SubReddit> NodeAKVP in m_Surveyor.m_SubReddits) {
				SubReddit NodeA = (SubReddit)NodeAKVP.Value;
				if (NodeA.Location.X < minX) minX = NodeA.Location.X;
				if (NodeA.Location.X > maxX) maxX = NodeA.Location.X;
				if (NodeA.Location.Y < minY) minY = NodeA.Location.Y;
				if (NodeA.Location.Y > maxY) maxY = NodeA.Location.Y;
			}
			return Rectangle.FromLTRB(minX - 100, minY - 100, maxX + 100, maxY + 100);
		}

		/// <summary>
		/// Applies a scaling factor to the specified point, used for zooming.
		/// </summary>
		/// <param name="point">The coordinates to scale.</param>
		/// <param name="scale">The scaling factor.</param>
		/// <returns>A System.Drawing.Point representing the scaled coordinates.</returns>
		private Point ScalePoint(Point point, double scale) {
			return new Point((int)((double)point.X * scale), (int)((double)point.Y * scale));
		}

	}
}
