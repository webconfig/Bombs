using System;

namespace Model
{
	public static class IdGenerater
	{
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long AppId { private get; set; }

		private static ushort value;

		public static long GenerateId()
		{
			long time = Convert.ToInt64((DateTime.UtcNow - epoch).TotalSeconds);

			return (AppId << 48) + (time << 16) + ++value;
		}
	}
}