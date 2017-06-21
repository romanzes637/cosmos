using System;
using System.Collections.Generic;

namespace Cosmos
{
	public delegate float FunctionRet (params float[] args);
	public delegate void FunctionSum (ref float value, params float[] args);
	public delegate void FunctionMul (ref float value, params float[] args);


	//	public class ExponentialGrowth : IFunction
	//	{
	//		// args: a, x, dt
	//		public float Call (params float[] args)
	//		{
	//			return args [0] * args [1] * args [2];
	//		}
	//
	//		// args: a, dt
	//		public void CallSum (ref float value, params float[] args)
	//		{
	//			value += args [0] * value * args [1];
	//		}
	//
	//		// args: a, dt
	//		public void CallMul (ref float value, params float[] args)
	//		{
	//			value *= 1 + args [0] * args [1];
	//		}

	public static class Function
	{
		public enum Name
		{
			DelayedGrowth,
			ExponentialGrowth,
			ExponentialMap,
			GaussianProduction,
			LogisticGrowth,
			LogisticMap,
			ResourceGrowth,
			}

		;

		public static Dictionary<Name, FunctionRet> ret;
		public static Dictionary<Name, FunctionSum> sum;
		public static Dictionary<Name, FunctionMul> mul;

		static Function ()
		{
			// (Malthus 1798 Fibonacci? 1202) dx = a*x*dt
			// (Malthus 1798 Fibonacci? 1202) x(i+1) = a*x(i)
			// (Verhulst 1845, 1847) x(i+1) = a*x(i)*(1-x(i)), where x in [0, 1]}
			// (Verhulst 1845, 1847) dx = a*x*(1-x/b)*dt
			// (Hutchinson 1948) x(i+1) = a*x(i)*(1-x(i-1)), where x in [0, 1]
			// (Hutchinson 1948) dx = a*x*(1-c/b)*dt

			// Resource Growth
			// dx = a*x*(c/b-1)*dt, if (c > b)
			// dx = -a*x*(b/c-1)*dt, if (b > c)
			// dx = 0, if (b == c)
			// b - consumed resources, c - produced resources

			ret = new Dictionary<Name, FunctionRet> ();
			ret.Add (Name.DelayedGrowth, (float[] args) => {
				// (Hutchinson 1948) dx = a*x*(1-px/b)*dt, args: a, b, px, x, dt
				return args [0] * args [3] * (1 - args [2] / args [1]) * args [4];
			});
			ret.Add (Name.GaussianProduction, (float[] args) => {
		    // y = GaussianValue(mean, dev)*npersons*dt, args: mean, dev, npersons, dt
//			double u1 = 1.0 - rand.NextDouble ();
//			double u2 = 1.0 - rand.NextDouble ();
//			double z1 = Math.Sqrt (-2.0 * Math.Log (u1)) * Math.Cos (2.0 * Math.PI * u2);
//			double z2 = Math.Sqrt (-2.0 * Math.Log (u1)) * Math.Sin (2.0 * Math.PI * u2);
//			float value = mean + dev * (float)z1;
				double u1, u2, w;
				do {
					u1 = 2.0 * Global.random.NextDouble () - 1.0;
					u2 = 2.0 * Global.random.NextDouble () - 1.0;
					w = u1 * u1 + u2 * u2;
				} while (w >= 1.0);
				w = Math.Sqrt ((-2.0 * Math.Log (w)) / w);
				float z1 = (float)(u1 * w);
//				float z2 = (float)x2 * w;
				return (args [0] + args [1] * z1) * args [2] * args [3];
			});

			sum = new Dictionary<Name, FunctionSum> ();
			sum.Add (Name.DelayedGrowth, (ref float value, float[] args) => {
				// (Hutchinson 1948) dx = a*x*(1-px/b)*dt, args: a, b, px, dt
				value += args [0] * value * (1 - args [2] / args [1]) * args [3];
			});

			mul = new Dictionary<Name, FunctionMul> ();
			mul.Add (Name.DelayedGrowth, (ref float value, float[] args) => {
				// (Hutchinson 1948) dx = a*x*(1-px/b)*dt, args: a, b, px, dt
				value *= 1 + args [0] * (1 - args [2] / args [1]) * args [3]; 
			});
			mul.Add (Name.ExponentialGrowth, (ref float value, float[] args) => {
				// (Malthus 1798 Fibonacci? 1202) dx = a*x*dt, args: a, dt
				value *= 1 + args [0] * args [1];
			});
			mul.Add (Name.ExponentialMap, (ref float value, float[] args) => {
				// (Malthus 1798 Fibonacci? 1202) x(i+1) = a*x(i), args: a
				value *= args [0];
			});
			mul.Add (Name.LogisticGrowth, (ref float value, float[] args) => {
				// (Verhulst 1845, 1847) dx = a*x*(1-x/b)*dt, args: a, b, dt
				value *= 1 + args [0] * (1 - value / args [1]) * args [2];
			});
			mul.Add (Name.ResourceGrowth, (ref float value, float[] args) => {
				// args: a, b, c, dt
				float delta = args [2] - args [1];
				if (delta > 0) {
					value *= 1 + args [0] * (args [2] / args [1] - 1) * args [3]; 
				} else if (delta < 0) {
					value *= 1 + -args [0] * (args [1] / args [2] - 1) * args [3];
				}
			});
		}
	}
		
	// Resource Growth
	// dx = a*x*(c/b-1)*dt, if (c > b)
	// dx = -a*x*(b/c-1)*dt, if (b > c)
	// dx = 0, if (b == c)
	// b - consumed resources, c - produced resources
	//	public class ResourceGrowth2 : Function
	//	{
	//		public ResourceGrowth2 (float a = 0, float b = 0, float c = 0, float x = 0, float dt = 0) : base (5)
	//		{
	//			args [0] = a;
	//			args [1] = b;
	//			args [2] = c;
	//			args [3] = x;
	//			args [4] = dt;
	//		}
	//
	//		public override float Call ()
	//		{
	//			float delta = args [2] - args [1];
	//			if (delta > 0) {
	//				return args [0] * args [3] * (args [2] / args [1] - 1) * args [4];
	//			} else if (delta < 0) {
	//				return -args [0] * args [3] * (args [1] / args [2] - 1) * args [4];
	//			} else {
	//				return 0;
	//			}
	//		}
	//
	//		public override void CallSum (ref float value)
	//		{
	//			float delta = args [2] - args [1];
	//			if (delta > 0) {
	//				value += args [0] * value * (args [2] / args [1] - 1) * args [4];
	//			} else if (delta < 0) {
	//				value += -args [0] * value * (args [1] / args [2] - 1) * args [4];
	//			}
	//		}
	//
	//		public override void CallMul (ref float value)
	//		{
	//			float delta = args [2] - args [1];
	//			if (delta > 0) {
	//				value *= 1 + args [0] * (args [2] / args [1] - 1) * args [4];
	//			} else if (delta < 0) {
	//				value *= 1 + -args [0] * (args [1] / args [2] - 1) * args [4];
	//			}
	//		}
	//	}

	//	// (Malthus 1798 Fibonacci? 1202) x(i+1) = a*x(i)
	//	public class ExponentialMap : IFunction
	//	{
	//		// args: a, x(i)
	//		public float Call (params float[] args)
	//		{
	//			return args [0] * args [1];
	//		}
	//
	//		// args: a
	//		public void CallSum (ref float value, params float[] args)
	//		{
	//			value += value * (args [0] - 1);
	//		}
	//
	//		// args: a
	//		public void CallMul (ref float value, params float[] args)
	//		{
	//			value *= args [0];
	//		}
	//	}
	//
	//	// (Verhulst 1845, 1847) x(i+1) = a*x(i)*(1-x(i)), where x in [0, 1]
	//	public class LogisticMap : IFunction
	//	{
	//		// args: a, x(i)
	//		public float Call (params float[] args)
	//		{
	//			return args [0] * args [1] * (1 - args [1]);
	//		}
	//
	//		// args: a
	//		public void CallSum (ref float value, params float[] args)
	//		{
	//			value += value * (args [0] * (1 - value) + 1);
	//		}
	//
	//		// args: a
	//		public void CallMul (ref float value, params float[] args)
	//		{
	//			value *= args [0] * (1 - value);
	//		}
	//
	//		// args: a, x(i)
	//		public double Call (params double[] args)
	//		{
	//			return args [0] * args [1] * (1 - args [1]);
	//		}
	//
	//		// args: a
	//		public void CallSum (ref double value, params double[] args)
	//		{
	//			value += value * (args [0] * (1 - value) + 1);
	//		}
	//
	//		// args: a
	//		public void CallMul (ref double value, params double[] args)
	//		{
	//			value *= args [0] * (1 - value);
	//		}
	//	}
	//
	//	// (Hutchinson 1948) x(i+1) = a*x(i)*(1-x(i-1)), where x in [0, 1]
	//	public class DelayedMap : IFunction
	//	{
	//		// args: a, x(i), x(i-1)
	//		public float Call (params float[] args)
	//		{
	//			return args [0] * args [1] * (1 - args [2]);
	//		}
	//
	//		// args: a, x(i-1)
	//		public void CallSum (ref float value, params float[] args)
	//		{
	//			value += value * (args [0] * (1 - args [1]) + 1);
	//		}
	//
	//		// args: a, x(i-1)
	//		public void CallMul (ref float value, params float[] args)
	//		{
	//			value *= args [0] * (1 - args [1]);
	//		}
	//
	//		// args: a, x(i), x(i-1)
	//		public double Call (params double[] args)
	//		{
	//			return args [0] * args [1] * (1 - args [2]);
	//		}
	//
	//		// args: a, x(i-1)
	//		public void CallSum (ref double value, params double[] args)
	//		{
	//			value += value * (args [0] * (1 - args [1]) + 1);
	//		}
	//
	//		// args: a, x(i-1)
	//		public void CallMul (ref double value, params double[] args)
	//		{
	//			value *= args [0] * (1 - args [1]);
	//		}
	//	}
	//
	//	// (Malthus 1798 Fibonacci? 1202) dx = a*x*dt
	//	public class ExponentialGrowth : IFunction
	//	{
	//		// args: a, x, dt
	//		public float Call (params float[] args)
	//		{
	//			return args [0] * args [1] * args [2];
	//		}
	//
	//		// args: a, dt
	//		public void CallSum (ref float value, params float[] args)
	//		{
	//			value += args [0] * value * args [1];
	//		}
	//
	//		// args: a, dt
	//		public void CallMul (ref float value, params float[] args)
	//		{
	//			value *= 1 + args [0] * args [1];
	//		}
	//
	//		// args: a, x, dt
	//		public double Call (params double[] args)
	//		{
	//			return args [0] * args [1] * args [2];
	//		}
	//
	//		// args: a, dt
	//		public void CallSum (ref double value, params double[] args)
	//		{
	//			value += args [0] * value * args [1];
	//		}
	//
	//		// args: a, dt
	//		public void CallMul (ref double value, params double[] args)
	//		{
	//			value *= 1 + args [0] * args [1];
	//		}
	//	}
	//
	//	// (Verhulst 1845, 1847) dx = a*x*(1-x/b)*dt
	//	public class LogisticGrowth : IFunction
	//	{
	//		// args: a, b, x, dt
	//		public float Call (params float[] args)
	//		{
	//			return args [0] * args [2] * (1 - args [2] / args [1]) * args [3];
	//		}
	//
	//		// args: a, b, dt
	//		public void CallSum (ref float value, params float[] args)
	//		{
	//			value += args [0] * value * (1 - value / args [1]) * args [2];
	//		}
	//
	//		// args: a, b, dt
	//		public void CallMul (ref float value, params float[] args)
	//		{
	//			value *= 1 + args [0] * (1 - value / args [1]) * args [2];
	//		}
	//
	//		// args: a, b, x, dt
	//		public double Call (params double[] args)
	//		{
	//			return args [0] * args [2] * (1 - args [2] / args [1]) * args [2];
	//		}
	//
	//		// args: a, b, dt
	//		public void CallSum (ref double value, params double[] args)
	//		{
	//			value += args [0] * value * (1 - value / args [1]) * args [2];
	//		}
	//
	//		// args: a, b, dt
	//		public void CallMul (ref double value, params double[] args)
	//		{
	//			value *= 1 + args [0] * (1 - value / args [1]) * args [2];
	//		}
	//	}

	//	// f(mean, dev) = GaussianValue(mean, dev)
	//	public class GaussianRandom : IFunction
	//	{
	//		Random random;
	//		float mean;
	//		float dev;
	//
	//		public GaussianRandom (Random random, float mean = 0, float dev = 1)
	//		{
	//			this.random = random;
	//			this.mean = mean;
	//			this.dev = dev;
	//		}
	//
	//		public virtual float Call (params float[] args)
	//		{
	////			double u1 = 1.0 - rand.NextDouble ();
	////			double u2 = 1.0 - rand.NextDouble ();
	////			double z1 = Math.Sqrt (-2.0 * Math.Log (u1)) * Math.Cos (2.0 * Math.PI * u2);
	//////			double z2 = Math.Sqrt (-2.0 * Math.Log (u1)) * Math.Sin (2.0 * Math.PI * u2);
	////			float value = mean + dev * (float)z1;
	//			double u1, u2, w, z1;
	////			double z2;
	//			do {
	//				u1 = 2.0 * random.NextDouble () - 1.0;
	//				u2 = 2.0 * random.NextDouble () - 1.0;
	//				w = u1 * u1 + u2 * u2;
	//			} while (w >= 1.0);
	//			w = Math.Sqrt ((-2.0 * Math.Log (w)) / w);
	//			z1 = u1 * w;
	////			z2 = x2 * w;
	//			return mean + dev * (float)z1;
	//		}
	//
	//		public virtual void CallSum (ref float value, params float[] args)
	//		{
	//			double u1, u2, w, z1;
	////			double z2;
	//			do {
	//				u1 = 2.0 * random.NextDouble () - 1.0;
	//				u2 = 2.0 * random.NextDouble () - 1.0;
	//				w = u1 * u1 + u2 * u2;
	//			} while (w >= 1.0);
	//			w = Math.Sqrt ((-2.0 * Math.Log (w)) / w);
	//			z1 = u1 * w;
	////			z2 = x2 * w;
	//			value += (mean + dev * (float)z1);
	//		}
	//
	//		public virtual void CallMul (ref float value, params float[] args)
	//		{
	//		}
	//	}
}
