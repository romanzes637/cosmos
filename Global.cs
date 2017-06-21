using System.Collections.Generic;
using System;

namespace Cosmos
{
	

	public static class Global
	{
		public static Random random;
//		public static Dictionary<int, object> functions;
//		static int functionCnt;
//		public static List<object> populations;
//		public static List<object> resources;

		static Global ()
		{
			random = new Random ();
//			functions = new Dictionary<int, object> ();
//			functionCnt = 0;
//			populations = new List<object> ();
//			resources = new List<object> ();
		}

		public static Random GetRandom ()
		{
			return new Random (random.Next ());
		}

//		public static int AddFunction (object function)
//		{
//			functions.Add (functionCnt, function);
//			functionCnt += 1;
//			return functionCnt - 1;
//		}
//
//		public static object GetFunction (int idx)
//		{
//			return functions [idx];
//		}
//
//		public static void UpdateFunction (int idx, object function)
//		{
//			functions [idx] = function;
//		}
//
//		public static void RemoveFunction (int idx)
//		{
//			functions.Remove (idx);
//		}
	}
}

