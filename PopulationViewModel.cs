using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopulationViewModel : MonoBehaviour
{
	public Game game;

	public Cosmos.GaussProdConsResMigTechPopulation population { get; private set; }

	public enum Strategy
	{
		Player,
		MaxGrowth,
		MaxGrowthMaxConsume
	}

	public Strategy str;
	public float initialValue;
	public float a;
	public float mA;
	public float tA;
	public float pA;
	public float pMean;
	public float pDev;
	public float cMean;
	public float cDev;
	public float rMean;
	public float rDev;

	public float Value { get { return population.value; } set { population.value = value; } }

	public float PValue { get { return population.pValue; } set { population.pValue = value; } }

	public float CMean { get { return population.cMean; } set { population.cMean = value; } }

	public float PMean { get { return population.pMean; } set { population.pMean = value; } }

	public float RMean { get { return population.rMean; } set { population.rMean = value; } }

	public float Researchers { get { return population.researchers; } set { population.researchers = value; } }

	public float Technology { get { return population.technology; } set { population.technology = value; } }

	public float Workers { get { return population.workers [0]; } set { population.workers [0] = value; } }

	public float Resources { get { return population.resources [0].value; } set { population.resources [0].value = value; } }

	LineRenderer lr;
	List<Vector3> ps;
	float time;

	void UpdateDeltaTime ()
	{
		time += game.deltaTime;
		ps.Add (new Vector3 (time, Value));
		lr.positionCount = ps.Count;
		lr.SetPositions (ps.ToArray ());
	}

	void UpdateTime ()
	{
		if (str == Strategy.Player) {
		} else if (str == Strategy.MaxGrowth) {
			SetWorkers (0, Value);
		} else if (str == Strategy.MaxGrowthMaxConsume) {
			SetWorkers (0, Value);
			float maxCMean = CMean;
			foreach (KeyValuePair<int, PopulationViewModel> entry in game.populations) {
				if (entry.Value.CMean > maxCMean) {
					maxCMean = entry.Value.CMean;
				}
			}
			CMean = maxCMean;
		}
	}

	// Use this for initialization
	void Awake ()
	{
		population = new Cosmos.GaussProdConsResMigTechPopulation (initialValue, a, pMean, pDev, cMean, cDev, mA, tA, pA, rMean, rDev);

		time = 0;
		ps = new List<Vector3> ();
		ps.Add (new Vector3 (time, Value));
		lr = GetComponent<LineRenderer> ();

		Color color = new Color ((float)Cosmos.Global.random.NextDouble (), (float)Cosmos.Global.random.NextDouble (), (float)Cosmos.Global.random.NextDouble ());
		lr.startColor = color;
		lr.endColor = color;
		lr.positionCount = ps.Count;
		lr.SetPositions (ps.ToArray ());

		Game.onDeltaTimeChanged += UpdateDeltaTime;
		Game.onTimeChanged += UpdateTime;
	}

	void Update ()
	{
		population.a = a;
		population.pDev = pDev;
		population.cDev = cDev;
		population.mA = mA;
		population.tA = tA;
		population.pA = pA;
		population.rDev = rDev;
	}

	public void Growth (float dt)
	{
		population.Update (dt);
	}

	public void Migrate (float dt)
	{
		population.Migrate (dt);
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
