namespace Humper
{
	using Base;
	using System.Collections.Generic;
	using System.Linq;

	public class Movement
	{
		public Movement()
		{
			this.Hits = new List<Hit>();
		}

		public List<Hit> Hits { get; set; }

		public bool HasCollided { get { return this.Hits.Any(); } }

		public RectangleF Origin { get; set; }

		public RectangleF Destination { get; set; }

		public RectangleF Goal { get; set; }
	}
}

