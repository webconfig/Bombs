namespace Humper
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Base;
	using Responses;

	public class World : IWorld
	{
		public World(float width, float height, float cellSize = 64)
		{
			var iwidth = (int)Math.Ceiling(width / cellSize);
			var iheight = (int)Math.Ceiling(height / cellSize);

			this.grid = new Grid(iwidth, iheight, cellSize);
		}

		public RectangleF Bounds
        {
            get { return new RectangleF(0, 0, this.grid.Width, this.grid.Height); }
        }

		#region Boxes

		private Grid grid;

		public Box Create(float x, float y, float width, float height)
		{
			var box = new Box(this, x, y, width, height);
			this.grid.Add(box);
			return box;
		}

		public IEnumerable<Box> Find(float x, float y, float w, float h)
		{
			x = Math.Max(0, Math.Min(x, this.Bounds.Right - w));
			y = Math.Max(0, Math.Min(y, this.Bounds.Bottom - h));

			return this.grid.QueryBoxes(x, y, w, h);
		}

		public IEnumerable<Box> Find(RectangleF area)
		{
			return this.Find(area.X, area.Y, area.Width, area.Height);
		}

		public bool Remove(Box box)
		{
			return this.grid.Remove(box);
		}

		public void Update(Box box, RectangleF from)
		{
			this.grid.Update(box, from);
		}

		#endregion

		#region Hits

		public Hit Hit(Vector2 point, IEnumerable<Box> ignoring = null)
		{
			var boxes = this.Find(point.X, point.Y, 0, 0);

			if (ignoring != null)
			{
				boxes = boxes.Except(ignoring);
			}

			foreach (var other in boxes)
			{
				var hit = Humper.Hit.Resolve(point, other);

				if (hit != null)
				{
					return hit;
				}
			}

			return null;
		}

		public Hit Hit(Vector2 origin, Vector2 destination, IEnumerable<Box> ignoring = null)
		{
			var min = Vector2.Min(origin, destination);
			var max = Vector2.Max(origin, destination);

			var wrap = new RectangleF(min, max - min);
			var boxes = this.Find(wrap.X, wrap.Y, wrap.Width, wrap.Height);

			if (ignoring != null)
			{
				boxes = boxes.Except(ignoring);
			}

			Hit nearest = null;

			foreach (var other in boxes)
			{
				var hit = Humper.Hit.Resolve(origin, destination, other);

				if (hit != null && (nearest == null || hit.IsNearest(nearest,origin)))
				{
					nearest = hit;
				}
			}

			return nearest;
		}

		public Hit Hit(RectangleF origin, RectangleF destination, IEnumerable<Box> ignoring = null)
		{
			var wrap = new RectangleF(origin, destination);
			var boxes = this.Find(wrap.X, wrap.Y, wrap.Width, wrap.Height);

			if (ignoring != null)
			{
				boxes = boxes.Except(ignoring);
			}

			Hit nearest = null;

			foreach (var other in boxes)
			{
				var hit = Humper.Hit.Resolve(origin, destination, other);

				if (hit != null && (nearest == null || hit.IsNearest(nearest, origin.Location)))
				{
					nearest = hit;
				}
			}

			return nearest;
		}

		#endregion

		
        #region Movements

		public Movement Simulate(Box box, float x, float y, Func<ICollision, ICollisionResponse> filter)
		{
			var origin = box.Bounds;
			var destination = new RectangleF(x, y, box.Width, box.Height);

			var hits = new List<Hit>();

			var result = new Movement()
			{
				Origin = origin,
				Goal = destination,
				Destination = this.Simulate(hits, new List<Box>() { box }, box, origin, destination, filter),
				Hits = hits,
			};

			return result;
		}

		private RectangleF Simulate(List<Hit> hits, List<Box> ignoring, Box box, RectangleF origin, RectangleF destination, Func<ICollision, ICollisionResponse> filter)
		{
			var nearest = this.Hit(origin, destination, ignoring);
				
			if (nearest != null)
			{
				hits.Add(nearest);

				var impact = new RectangleF(nearest.Position, origin.Size);
				var collision = new Collision() { Box = box, Hit = nearest, Goal = destination, Origin = origin };
				var response = filter(collision);

				if (response != null && destination != response.Destination)
				{
					ignoring.Add(nearest.Box);
					return this.Simulate(hits, ignoring, box, impact, response.Destination, filter);
				}
			}

			return destination;
		}

		#endregion

		#region Diagnostics

		public void DrawDebug(float x, float y, float w, float h, Action<float, float, float, float, float> drawCell, Action<Box> drawBox, Action<string, float, float, float> drawString)
		{
			// Drawing boxes
			var boxes = this.grid.QueryBoxes(x, y, w, h);
			foreach (var box in boxes)
			{
				drawBox(box);
			}

			// Drawing cells
			var cells = this.grid.QueryCells(x, y, w, h);
            int i = 0;
			foreach (var cell in cells)
			{
				var count = cell.Count();
				var alpha = count > 0 ? 1f : 0.4f;
				drawCell(cell.Bounds.X, cell.Bounds.Y, cell.Bounds.Width, cell.Bounds.Height, alpha);
                drawString(i.ToString(), cell.Bounds.X, cell.Bounds.Y+ cell.Bounds.Height, alpha);
                drawString(count.ToString(), cell.Bounds.Center.X, cell.Bounds.Center.Y,alpha);
                i++;
			}
		}

		#endregion
	}
    public delegate void Action<in T1, in T2, in T3, in T4, in T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
}

