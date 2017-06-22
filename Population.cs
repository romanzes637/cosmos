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

		public bool CheckExtinction ()
		{
			if (value < 1) {
				value = 0;
				return true;
			}
			return false;
		}
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
		public float pValue;

		public GaussProdConsPopulation (float value, float a, float pMean, float pDev, float cMean, float cDev) : base (value)
		{
			this.a = a;
			this.pMean = pMean;
			this.pDev = pDev;
			this.cMean = cMean;
			this.cDev = cDev;
			pValue = value;
		}

		public override void Update (float dt)
		{
			pValue = value;
			Function.rfr [Function.Name.ProdConsGrowth] (ref value, a, 
				Function.rtr [Function.Name.GaussianProduction] (pMean, pDev, value, dt), 
				Function.rtr [Function.Name.GaussianProduction] (cMean, cDev, value, dt), dt);
			CheckExtinction ();
		}
	}

	public class GaussProdConsResPopulation : GaussProdConsPopulation
	{
		public List<Resource> resources;
		public List<float> workers;

		public GaussProdConsResPopulation (float value, float a, float pMean, float pDev, float cMean, float cDev) : base (value, a, pMean, pDev, cMean, cDev)
		{
			resources = new List<Resource> ();
			workers = new List<float> ();
		}

		public float WorkersCount ()
		{
			float nw = 0;
			foreach (float w in workers) {
				nw += w;
			}
			return nw;
		}

		public void SetWorkers (int idx, float n)
		{
			float delta = n - workers [idx];
			if (WorkersCount () + delta <= value) {
				workers [idx] = n;
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

		public void UpdateWorkers ()
		{
			float nw = WorkersCount ();
			if (nw > value) {
				float delta = nw - value;
				for (int i = 0; i < workers.Count; i++) {
					if (workers [i] >= delta) {
						workers [i] -= delta;
						break;
					} else {
						delta -= workers [i];
						workers [i] = 0;
					}
				}
			}
		}

		public override void Update (float dt)
		{
			UpdateWorkers ();
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
			// Consumtion
			consumed = Function.rtr [Function.Name.GaussianProductionWithMeanOneFilter] (cMean, cDev, value, dt);
			// Growth
			pValue = value;
			Function.rfr [Function.Name.ProdConsGrowth] (ref value, a, produced, consumed, dt);
			CheckExtinction ();
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

	public class GaussProdConsResMigPopulation : GaussProdConsResPopulation
	{
		public float mA;
		public float cRate;
		public float nEmigrants;
		public Dictionary<int, GaussProdConsResMigPopulation> populations;

		public GaussProdConsResMigPopulation (float value, float a, float pMean, float pDev, float cMean, float cDev, float mA) : base (value, a, pMean, pDev, cMean, cDev)
		{
			this.mA = mA;
			cRate = 0;
			nEmigrants = 0;
			populations = new Dictionary<int, GaussProdConsResMigPopulation> ();
		}

		public void AddPopulation (int idx, GaussProdConsResMigPopulation p)
		{
			populations.Add (idx, p);
		}

		public void RemovePopulation (int idx)
		{
			populations.Remove (idx);
		}

		public override void Update (float dt)
		{
			base.Update (dt);
			cRate = consumed / pValue;
		}

		public void Migrate (float dt)
		{
			nEmigrants = 0;
			// Migration
			foreach (KeyValuePair<int, GaussProdConsResMigPopulation> entry in populations) {
				float ecRate = entry.Value.consumed / entry.Value.pValue;
				if (ecRate > cRate) {
					float emigrants = Function.rtr [Function.Name.Migration] (mA, value, ecRate, cRate, dt);
					if (emigrants < value) {
						entry.Value.value += emigrants;
						value -= emigrants;
						nEmigrants += emigrants;
					} else {
						entry.Value.value += value;
						nEmigrants = value;
						value = 0;
						break;
					}
				}
			}
		}
	}
}