using Humper.Base;

namespace Humper
{
	public class Collision : ICollision
	{
		public Collision()
		{
		}

		public Box Box { get; set; }

		public Box Other
        {
            get
            {
                if (this.Hit == null)
                {
                    return null;
                }
                else
                {
                    return this.Hit.Box;
                }
            }
        }

		public RectangleF Origin { get; set; }

		public RectangleF Goal { get; set; }

		public Hit Hit { get; set; }

		public bool HasCollided
        {
            get { return this.Hit != null; }
        }
	}
}

