using System;
using System.Collections.Generic;

namespace Cosmos
{
	public delegate float FunctionRet (params float[] args);
	public delegate void FunctionRef (ref float value, params float[] args);

	public static class Function
	{
		public enum Name
		{
			DelayedGrowth,
			DelayedMap,
			ExponentialGrowth,
			ExponentialMap,
			GaussianProduction,
			GaussianProductionWithMeanFilter,
			GaussianProductionWithMeanOneFilter,
			GaussianRandom,
			GaussianRandomWithMeanFilter,
			GaussianRandomWithMeanOneFilter,
			LogisticGrowth,
			LogisticMap,
			Migration,
			ProdConsGrowth,
			Production
		}

		public static Dictionary<Name, FunctionRet> rtr;
		public static Dictionary<Name, FunctionRef> rfr;

		static Function ()
		{
			rtr = new Dictionary<Name, FunctionRet> ();
			rtr.Add (Name.GaussianRandom, (float[] args) => {
				// value = GaussianValue(mean, dev), args: mean, dev
				double u1, u2, w, z1;
				// double z2;
				do {
					u1 = 2.0 * Global.random.NextDouble () - 1.0;
					u2 = 2.0 * Global.random.NextDouble () - 1.0;
					w = u1 * u1 + u2 * u2;
				} while (w >= 1.0);
				w = Math.Sqrt ((-2.0 * Math.Log (w)) / w);
				z1 = u1 * w;
				// z2 = x2 * w;
				return args [0] + args [1] * (float)z1;
			});
			rtr.Add (Name.GaussianRandomWithMeanFilter, (float[] args) => {
				// value = GaussianValue(mean, dev) in range (0, 2*mean], if mean > 0
				// value = GaussianValue(mean, dev) in range [2*mean, 0), if mean < 0
				// value = 0 otherwise
				// args: mean, dev
				float value = rtr [Name.GaussianRandom] (args [0], args [1]);
				if (args [0] > 0) {
					while (value < 0 || value > 2 * args [0]) {
						value = rtr [Name.GaussianRandom] (args [0], args [1]);
					}
				} else if (args [0] < 0) {
					while (value < 2 * args [0] || value > 0) {
						value = rtr [Name.GaussianRandom] (args [0], args [1]);
					}
				}
				return value;
			});
			rtr.Add (Name.GaussianRandomWithMeanOneFilter, (float[] args) => {
				// value = GaussianValue(mean, dev) in range [1, 2*mean-1], if mean >= 1
				// value = GaussianValue(mean, dev) in range [2*mean+1, -1], if mean =< -1
				// value = 0 otherwise
				// args: mean, dev
				float value = rtr [Name.GaussianRandom] (args [0], args [1]);
				if (args [0] > 1) {
					while (value < 1 || value > 2 * args [0] - 1) {
						value = rtr [Name.GaussianRandom] (args [0], args [1]);
					}
				} else if (args [0] < -1) {
					while (value < 2 * args [0] + 1 || value > -1) {
						value = rtr [Name.GaussianRandom] (args [0], args [1]);
					}
				} else if (args [0] == 1 || args [0] == -1) {
					value = args [0];
				}
				return value;
			});
			rtr.Add (Name.Production, (float[] args) => {
				// value = rate(per person per time)*npersons*dt, args: rate, npersons, dt
				return args [0] * args [1] * args [2];
			});
			rtr.Add (Name.GaussianProduction, (float[] args) => {
				// value = GR(mean, dev)*npersons*dt, args: mean, dev, npersons, dt
				return rtr [Name.Production] (rtr [Name.GaussianRandom] (args [0], args [1]), args [2], args [3]);
			});
			rtr.Add (Name.GaussianProductionWithMeanFilter, (float[] args) => {
				// value = GR(mean, dev)*npersons*dt, args: mean, dev, npersons, dt
				return rtr [Name.Production] (rtr [Name.GaussianRandomWithMeanFilter] (args [0], args [1]), args [2], args [3]);
			});
			rtr.Add (Name.GaussianProductionWithMeanOneFilter, (float[] args) => {
				// value = GR(mean, dev)*npersons*dt, args: mean, dev, npersons, dt
				return rtr [Name.Production] (rtr [Name.GaussianRandomWithMeanOneFilter] (args [0], args [1]), args [2], args [3]);
			});
			rtr.Add (Name.Migration, (float[] args) => {
				// value = a*x*(ec/ic-1)*dt
				// args: a, x, ec, ic, dt (ec - external consumed resources rate per person, 
				// ic - internal consumed resources rate per person)
				return args [0] * args [1] * (args [2] / args [3] - 1) * args [4];
			});

			rfr = new Dictionary<Name, FunctionRef> ();
			rfr.Add (Name.DelayedGrowth, (ref float value, float[] args) => {
				// (Hutchinson 1948) value += dx = a*x*(1-px/b)*dt, args: a, b, px, dt ( b - max population, px - previous x)
				value += args [0] * value * (1 - args [2] / args [1]) * args [3];
			});
			rfr.Add (Name.DelayedMap, (ref float value, float[] args) => {
				// (Hutchinson 1948) x(i+1) = a*x(i)*(1-x(i-1)), where x in [0, 1], args: a, x(i-1)
				value *= args [0] * (1 - args [1]);
			});
			rfr.Add (Name.ExponentialGrowth, (ref float value, float[] args) => {
				// (Malthus 1798 Fibonacci? 1202) value += dx = a*x*dt, args: a, dt
				value += args [0] * value * args [1];
			});
			rfr.Add (Name.ExponentialMap, (ref float value, float[] args) => {
				// (Malthus 1798 Fibonacci? 1202) x(i+1) = a*x(i), args: a
				value *= args [0];
			});
			rfr.Add (Name.LogisticGrowth, (ref float value, float[] args) => {
				// (Verhulst 1845, 1847) value += dx = a*x*(1-x/b)*dt, args: a, b, dt (b - max population)
				value += args [0] * value * (1 - value / args [1]) * args [2];
			});
			rfr.Add (Name.LogisticMap, (ref float value, float[] args) => {
				// (Verhulst 1845, 1847) x(i+1) = a*x(i)*(1-x(i)), where x in [0, 1]}, args: a
				value *= args [0] * (1 - value);
			});
			rfr.Add (Name.ProdConsGrowth, (ref float value, float[] args) => {
				// value += dx = a*x*(p/c-1)*dt, if (p > c);
				// value -= dx = a*x*(c/p-1)*dt, if (c > p);
				// args: a, p, c, dt (p - produced resources, c - consumed resources)
				float delta = args [1] - args [2];
				if (delta > 0) {
					value += args [0] * value * (args [1] / args [2] - 1) * args [3]; 
				} else if (delta < 0) {
					value -= args [0] * value * (args [2] / args [1] - 1) * args [3];
				}
			});
		}
	}
}
