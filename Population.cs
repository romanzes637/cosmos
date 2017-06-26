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

		public virtual void SetWorkers (int idx, float n)
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

		public virtual void CheckPopulationSums ()
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

		public virtual void Produce (float dt)
		{
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
		}

		public override void Update (float dt)
		{
			// Check Workers
			CheckPopulationSums ();
			// Production
			Produce (dt);
			// Consumtion
			consumed = Function.rtr [Function.Name.GaussianProductionWithMeanOneFilter] (cMean, cDev, value, dt);
			// Growth
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
		public Dictionary<int, GaussProdConsResMigPopulation> populations;
		public float pValue;

		public GaussProdConsResMigPopulation (float value, float a, float pMean, float pDev, float cMean, float cDev, float mA) : base (value, a, pMean, pDev, cMean, cDev)
		{
			this.mA = mA;
			populations = new Dictionary<int, GaussProdConsResMigPopulation> ();
			pValue = value;
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
			pValue = value;
			base.Update (dt);
		}

		public void Migrate (float dt)
		{
			foreach (KeyValuePair<int, GaussProdConsResMigPopulation> entry in populations) {
				float iConsRate = consumed / pValue;
				float eConsRate = entry.Value.consumed / entry.Value.pValue;
				if (eConsRate > iConsRate) {
					float emigrants = Function.rtr [Function.Name.Migration] (mA, value, eConsRate, iConsRate, dt);
					if (emigrants < value) {
						entry.Value.value += emigrants;
						value -= emigrants;
					} else {
						entry.Value.value += value;
						value = 0;
						break;
					}
				}
			}
		}
	}

	public class GaussProdConsResMigTechPopulation : GaussProdConsResMigPopulation
	{
		public float technology;
		public float pTechnology;
		public float tA;
		public float researchers;
		public float pA;
		public float rMean;
		public float rDev;
		public float rValue;
		public float prValue;

		public GaussProdConsResMigTechPopulation (float value, float a, float pMean, float pDev, float cMean, float cDev, float mA, float tA, float pA, float rMean, float rDev) : base (value, a, pMean, pDev, cMean, cDev, mA)
		{
			technology = 1;
			pTechnology = technology;
			this.tA = tA;
			this.pA = pA;
			this.rMean = rMean;
			this.rDev = rDev;
			rValue = 0;
			prValue = 0;
		}

		public override void CheckPopulationSums ()
		{
			float sum = WorkersCount () + researchers;
			if (sum > value) {
				float delta = sum - value;
				if (researchers >= delta) {
					researchers -= delta;
				} else {
					delta -= researchers;
					researchers = 0;
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
		}

		public override void Produce (float dt)
		{
			base.Produce (dt);
			prValue = rValue;
			rValue = Function.rtr [Function.Name.GaussianProductionWithMeanOneFilter] (rMean, rDev, researchers, dt);
//			float spentResources = Function.rtr [Function.Name.Production] (rMean, researchers, dt);
			float spentResources = Function.rtr [Function.Name.Production] (rMean, 1, dt);
			if (spentResources <= produced) {
				produced -= spentResources;
			} else {
				rValue *= produced / spentResources;
				spentResources = produced;
				produced = 0;
			}
			Research (dt);
		}

		public void Research (float dt)
		{
			pTechnology = technology;
			Function.rfr [Function.Name.TechnologyGrowth] (ref technology, tA, dt, rValue);
			CheckTechMin ();
			Function.rfr [Function.Name.ProductionRateByTechnologyGrowth] (ref pMean, pA, technology, pTechnology, dt);
		}

		public bool CheckTechMin ()
		{
			if (technology < 1) {
				technology = 1;
				return true;
			}
			return false;
		}

	}
}