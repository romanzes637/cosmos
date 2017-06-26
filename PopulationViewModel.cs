using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopulationViewModel : MonoBehaviour
{
	public Cosmos.GaussProdConsResMigTechPopulation population { get; private set; }

	public float initialValue;
	public float a;
	public float pMean;
	public float pDev;
	public float cMean;
	public float cDev;
	public float cRate;
	public float mA;
	public float tA;
	public float pA;
	public float rMean;
	public float rDev;
	public float rValue;
	public float prValue;

	public List<float> workers;
	public List<Cosmos.Resource> resources;
	public float researchers;
	public float researchers2;

	public float value;
	public float technology;
	public float pValue;
	public float pTechnology;


	LineRenderer lr;
	List<Vector3> ps;
	float time;

	// Use this for initialization
	void Awake ()
	{
		population = new Cosmos.GaussProdConsResMigTechPopulation (initialValue, a, pMean, pDev, cMean, cDev, mA, tA, pA, rMean, rDev);
		workers = population.workers;
		resources = population.resources;
		researchers = population.researchers;
		value = GetValue ();
		pValue = population.pValue;
		pTechnology = population.pTechnology;

		time = 0;
		ps = new List<Vector3> ();
		ps.Add (new Vector3 (time, GetValue ()));
		lr = GetComponent<LineRenderer> ();

		Color color = new Color ((float)Cosmos.Global.random.NextDouble (), (float)Cosmos.Global.random.NextDouble (), (float)Cosmos.Global.random.NextDouble ());
		lr.startColor = color;
		lr.endColor = color;
		lr.positionCount = ps.Count;
		lr.SetPositions (ps.ToArray ());
	}
	
	// Update is called once per frame
	void Update ()
	{
		population.a = a;
//		population.pMean = pMean;
		population.pDev = pDev;
		population.cMean = cMean;
		population.cDev = cDev;
		population.mA = mA;
		population.tA = tA;
		population.pA = pA;
		population.rMean = rMean;
		population.rDev = rDev;
		population.researchers = researchers;
	}

	public float GetValue ()
	{
		return population.value;
	}

	public float GetPreviousValue ()
	{
		return population.pValue;
	}

	public void UpdatePopulation (float dt)
	{
//		population.SetWorkers (0, GetValue ());
		population.Update (dt);
	}

	public void Migrate (float dt)
	{
		cRate = population.consumed / population.pValue;
		population.Migrate (dt);
	}

	public void UpdatePlot (float dt)
	{
		pMean = population.pMean;
		rValue = population.rValue;
		prValue = population.prValue;
		pValue = population.pValue;
		technology = population.technology;
		pTechnology = population.pTechnology;
		researchers2 = population.researchers;
		value = GetValue ();
		time += dt/1000;
		ps.Add (new Vector3 (time, GetValue ()));
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
