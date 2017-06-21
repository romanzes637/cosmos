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
			Function.mul [Function.Name.ExponentialMap] (ref value, a);
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
			Function.mul [Function.Name.LogisticGrowth] (ref value, a, b, dt);
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
			Function.mul [Function.Name.DelayedGrowth] (ref value, a, b, pValue, dt);
			pValue = pv;
		}
	}

	//	public class ResourcePopulation : Population
	//	{
	//		public float previousValue;
	//		public float deltaTimeConsumed;
	//		public float deltaTimeProduced;
	//		public float allProduced;
	//		public float allConsumed;
	//		public float deltaTimeGrowth;
	//		public float allGrowth;
	//		public List<Resource> resources;
	//		public List<int> resourcesFunctionsIdxs;
	//		public List<float> resourcesProductions;
	//		public List<float> resourcesPersons;
	//		public float growthA;
	//		public float consumeMean;
	//		public float consumeDev;
	//
	//		public ResourcePopulation (float value, float consumeMean = 1, float consumeDev = 1f / 3, float growthA = 1) : base (value)
	//		{
	//			previousValue = value;
	//			resources = new List<Resource> ();
	//			resourcesFunctionsIdxs = new List<int> ();
	//			resourcesProductions = new List<float> ();
	//			resourcesPersons = new List<float> ();
	//			this.growthA = growthA;
	//			this.consumeMean = consumeMean;
	//			this.consumeDev = consumeDev;
	//		}
	//
	//		public void AddResource (Resource resource, Function function, float nPersons)
	//		{
	//			resources.Add (resource);
	//			resourcesFunctionsIdxs.Add (resource.AddFunction (function, nPersons));
	//			resourcesProductions.Add (0);
	//			resourcesPersons.Add (nPersons);
	//		}
	//
	//		public void UpdateResourceFunction (int resourceIdx, Function function)
	//		{
	//			resources [resourceIdx].UpdateFunction (resourcesFunctionsIdxs [resourceIdx], function);
	//		}
	//
	//		public void UpdateResourcePersons (int resourceIdx, float nPersons)
	//		{
	//			resourcesPersons [resourceIdx] = nPersons;
	//			resources [resourceIdx].UpdatePersons (resourcesFunctionsIdxs [resourceIdx], nPersons);
	//		}
	//
	//		public void UpdateResources ()
	//		{
	//			for (int i = 0; i < resources.Count; i++) {
	//				UpdateResourcePersons (i, (float)value / resources.Count);
	//			}
	//		}
	//
	//		public override void Update (float deltaTime)
	//		{
	//			deltaTimeProduced = 0;
	//			for (int i = 0; i < resources.Count; i++) {
	//				deltaTimeProduced += -resources [i].GetDeltaTimeValue (resourcesFunctionsIdxs [i]);
	//			}
	//			allProduced += deltaTimeProduced;
	//			deltaTimeConsumed = Function.ret[Function.Name.GaussianProduct](consumeMean, consumeDev, value, deltaTime);
	//			allConsumed += deltaTimeConsumed;
	//			allGrowth += growthFunction.Call ();
	//			previousValue = value;
	//			Function.mul[Function.Name.ResourceGrowth](ref value, growthA, deltaTimeConsumed, deltaTimeProduced, deltaTime);
	//			deltaTimeGrowth = value - previousValue;
	//			if (value < 1) {
	//				value = 0;
	//			}
	//		}
	//	}
}