using System.Collections.Generic;
using System;

namespace Cosmos
{
	public abstract class Population
	{
		public float value;

		public Population (float value)
		{
			this.value = value;
		}

		public abstract void Update (float dt);
	}

	public class ExponentialMapPopualtion : Population
	{
		public float a;

		public ExponentialMapPopualtion (float value, float a) : base (value)
		{
			this.a = a;
		}

		public override void Update (float dt)
		{
			Function.rfr [Function.Name.ExponentialMap] (ref value, a);
		}
	}

	public class LogisticPopulation : Population
	{
		public float a;
		public float b;

		public LogisticPopulation (float value, float a, float b) : base (value)
		{
			this.a = a;
			this.b = b;
		}

		public override void Update (float dt)
		{
			Function.rfr [Function.Name.LogisticGrowth] (ref value, a, b, dt);
		}
	}

	public class DelayedPopulation : Population
	{
		public float pValue;
		public float a;
		public float b;

		public DelayedPopulation (float value, float a, float b) : base (value)
		{
			this.a = a;
			this.b = b;
			pValue = value;
		}

		public override void Update (float dt)
		{
			float pv = value;
			Function.rfr [Function.Name.DelayedGrowth] (ref value, a, b, pValue, dt);
			pValue = pv;
		}
	}

	public class GaussProdConsPopulation : Population
	{
		public float a;
		public float produced;
		public float consumed;
		public float pMean;
		public float pDev;
		public float cMean;
		public float cDev;

		public GaussProdConsPopulation (float value, float a, float pMean, float pDev, float cMean, float cDev) : base (value)
		{
			this.a = a;
			this.pMean = pMean;
			this.pDev = pDev;
			this.cMean = cMean;
			this.cDev = cDev;
		}

		public override void Update (float dt)
		{
			Function.rfr [Function.Name.ProdConsGrowth] (ref value, a, 
				Function.rtr [Function.Name.GaussianProduction] (pMean, pDev, value, dt), 
				Function.rtr [Function.Name.GaussianProduction] (cMean, cDev, value, dt), dt);
			if (value < 1) {
				value = 0;
			}
		}
	}

	public class GaussProdConsResPopulation : GaussProdConsPopulation
	{
		public List<Resource> resources;
		public List<float> workers;
		public float nWorkers;

		public GaussProdConsResPopulation (float value, float a, float pMean, float pDev, float cMean, float cDev) : base (value, a, pMean, pDev, cMean, cDev)
		{
			resources = new List<Resource> ();
			workers = new List<float> ();
			nWorkers = 0;
		}

		public void SetWorkers (int idx, float n)
		{
			float delta = n - workers [idx];
			if (nWorkers + delta <= value) {
				workers [idx] = n;
				nWorkers += delta;
			}
		}

		public void AddResource (Resource r)
		{
			resources.Add (r);
			workers.Add (0);
		}

		public void RemoveResource (int idx)
		{
			resources.RemoveAt (idx);
			workers.RemoveAt (idx);
		}

		public override void Update (float dt)
		{
			// Check nWorkers < value
			if (nWorkers > value) {
				float delta = (nWorkers - value);
				for (int i = 0; i < workers.Count; i++) {
					if (workers [i] > 0) {
						if (workers [i] >= delta) {
							workers [i] -= delta;
							nWorkers -= delta;
							break;
						} else {
							nWorkers -= workers [i];
							delta -= workers [i];
							workers [i] = 0;
						}
					}
				}
			}
			// Production
			produced = 0;
			for (int i = 0; i < resources.Count; i++) {
				float p = Function.rtr [Function.Name.GaussianProductionWithMeanOneFilter] (pMean, pDev, workers [i], dt);
				resources [i].value -= p;
				if (resources [i].value > 0) {
					produced += p;
				} else {
					produced += p + resources [i].value;
					resources [i].value = 0;
					RemoveResource (i);
				}
			}
			// Growth
			Function.rfr [Function.Name.ProdConsGrowth] (ref value, a, produced, 
				Function.rtr [Function.Name.GaussianProductionWithMeanOneFilter] (cMean, cDev, value, dt), dt);
			if (value < 1) {
				value = 0;
			}
		}
	}

	public class Resource
	{
		public float value;

		public Resource (float value)
		{
			this.value = value;
		}
	}
}