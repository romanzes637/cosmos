using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopulationViewModel : MonoBehaviour
{
	public Cosmos.GaussProdConsResMigPopulation population { get; private set; }

	public float initialValue;
	public float a;
	public float pMean;
	public float pDev;
	public float cMean;
	public float cDev;
	public float cRate;
	public float mA;
	public float value;
	public float nEmigrants;
	public List<float> workers;
	public List<Cosmos.Resource> resources;

	LineRenderer lr;
	List<Vector3> ps;
	float time;

	// Use this for initialization
	void Awake ()
	{
		population = new Cosmos.GaussProdConsResMigPopulation (initialValue, a, pMean, pDev, cMean, cDev, mA);
		value = population.value;
		workers = population.workers;
		resources = population.resources;

		time = 0;
		ps = new List<Vector3> ();
		ps.Add (new Vector3 (time, population.value));
		lr = GetComponent<LineRenderer> ();

		Color color = new Color ((float)Cosmos.Global.random.NextDouble(), (float)Cosmos.Global.random.NextDouble(), (float)Cosmos.Global.random.NextDouble());
		lr.startColor = color;
		lr.endColor = color;
		lr.positionCount = ps.Count;
		lr.SetPositions (ps.ToArray ());
	}
	
	// Update is called once per frame
	void Update ()
	{
		population.a = a;
		population.pMean = pMean;
		population.pDev = pDev;
		population.cMean = cMean;
		population.cDev = cDev;
		population.mA = mA;
		value = population.value;
		cRate = population.cRate;
		nEmigrants = population.nEmigrants;
	}

	public void UpdatePopulation (float dt)
	{
		population.Update (dt);
	}

	public void Migrate (float dt)
	{
		population.Migrate (dt);
	}

	public void UpdatePlot (float dt)
	{
		time += dt;
		ps.Add (new Vector3 (time, population.value));
		lr.positionCount = ps.Count;
		lr.SetPositions (ps.ToArray ());
	}

	public void AddResource (Cosmos.Resource r)
	{
		population.AddResource (r);
	}

	public void AddPopulation (int idx, Cosmos.GaussProdConsResMigPopulation p)
	{
		population.AddPopulation (idx, p);
	}

	public void RemovePopulation (int idx)
	{
		population.RemovePopulation (idx);
	}

	public void SetWorkers (int idx, float n)
	{
		population.SetWorkers (idx, n);
	}

	public bool CheckExtinction ()
	{
		return population.CheckExtinction ();
	}
}
