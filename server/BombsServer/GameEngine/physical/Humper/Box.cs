namespace Humper
{
	using System;
	using System.Linq;
	using Base;
	using Responses;

	public class Box
	{
		#region Constructors 

		public Box(World world, float x, float y, float width, float height)
		{
			this.world = world;
			this.bounds = new RectangleF(x, y, width, height);
		}

		#endregion

		#region Fields

		private World world;

		private RectangleF bounds;

		#endregion

		#region Properties

		public RectangleF Bounds
		{
			get { return bounds; } 
		}

		public object Data { get; set; }

		public float Height
        {
            get { return Bounds.Height; }
        }

		public float Width
        {
            get { return Bounds.Width; }
        }

		public float X
        {
            get { return Bounds.X; }
        }

		public float Y
        {
            get { return Bounds.Y; }
        }

        #endregion

        #region Movements
        public Movement Move(float x, float y, Func<ICollision, CollisionResponses> filter)
        {
            var movement = world.Simulate(this, x, y, (col) =>
            {
                if (col.Hit == null)
                    return null;

                return CollisionResponse.Create(col, filter(col));
            });
            this.bounds.X = movement.Destination.X;
            this.bounds.Y = movement.Destination.Y;
            this.world.Update(this, movement.Origin);
            return movement;
        }
        #endregion

        #region Tags

        private Enum tags;

		public Box AddTags(params Enum[] newTags)
		{
			foreach (var tag in newTags)
			{
				this.AddTag(tag);
			}

			return this;
		}

		public Box RemoveTags(params Enum[] newTags)
		{
			foreach (var tag in newTags)
			{
				this.RemoveTag(tag);
			}

			return this;
		}

		private void AddTag(Enum tag)
		{
			if (tags == null)
			{
				tags = tag;
			}
			else
			{
				var t = this.tags.GetType();
				var ut = Enum.GetUnderlyingType(t);

				if (ut != typeof(ulong))
					this.tags = (Enum)Enum.ToObject(t, Convert.ToInt64(this.tags) | Convert.ToInt64(tag));
				else
					this.tags = (Enum)Enum.ToObject(t, Convert.ToUInt64(this.tags) | Convert.ToUInt64(tag));
			}
		}

		private void RemoveTag(Enum tag)
		{
			if (tags != null)
			{
				var t = this.tags.GetType();
				var ut = Enum.GetUnderlyingType(t);

				if (ut != typeof(ulong))
					this.tags = (Enum)Enum.ToObject(t, Convert.ToInt64(this.tags) & ~Convert.ToInt64(tag));
				else
					this.tags = (Enum)Enum.ToObject(t, Convert.ToUInt64(this.tags) & ~Convert.ToUInt64(tag));
			}
		}

		public bool HasTag(params Enum[] values)
		{
			return (tags != null) && values.Any((value) => HasFlag(this.tags,value));
		}

		public bool HasTags(params Enum[] values)
		{
			return (tags != null) && values.All((value) => HasFlag(this.tags, value));
		}

        #endregion

        public bool HasFlag(Enum e1, Enum e2)
        {
            long num =Convert.ToInt64(e2);
            long num2 = Convert.ToInt64(e1);
            return (num2 & num) == num;

        }
    }
}

