using System.Collections.Generic;
using System;

namespace Cosmos
{
	public static class Global
	{
		public static Random random;

		static Global ()
		{
			random = new Random ();
		}
	}
}

