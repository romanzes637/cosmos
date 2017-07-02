using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
	public delegate void ChangedTimeHandler ();

	public static event ChangedTimeHandler onTimeChanged;
	public static event ChangedTimeHandler onDeltaTimeChanged;

	public float time;
	public float deltaTime;
	public float timeStep;

	public void NextTime ()
	{
		float cnt = 0;
		while (cnt < timeStep) {
			cnt += deltaTime;
			UpdateGame (deltaTime);
			if (onDeltaTimeChanged != null) {
				onDeltaTimeChanged ();
			}
		}
		time += timeStep;
		if (onTimeChanged != null) {
			onTimeChanged ();
		}
	}

	public PopulationViewModel population;
	public int nPopulations;
	public int range;
	public Dictionary<int, PopulationViewModel> populations;
	public Dictionary<int, Cosmos.Resource> resources;
	public float globalPopulation;
	public List<float> emigrations;
	public List<float> growths;
	public List<float> values;

	// Use this for initialization
	void Awake ()
	{
		populations = new Dictionary<int, PopulationViewModel> ();
		emigrations = new List<float> ();
		growths = new List<float> ();
		values = new List<float> ();
		resources = new Dictionary<int, Cosmos.Resource> ();
		for (int i = 0; i < nPopulations; i++) {
			populations.Add (i, Instantiate (population,
				transform.position + new Vector3 (
					Cosmos.Global.random.Next (-range, range), 
					Cosmos.Global.random.Next (-range, range), 
					Cosmos.Global.random.Next (-range, range)),
				Quaternion.identity, transform));
			emigrations.Add (0);
			growths.Add (0);
			values.Add (0);
			resources.Add (i, new Cosmos.Resource (1000000));
			populations [i].AddResource (resources [i]);
			populations [i].SetWorkers (0, populations [i].Value);
			if (i != 0) {
				populations [i].str = PopulationViewModel.Strategy.MaxGrowthMaxConsume;
			} else {
				populations [i].str = PopulationViewModel.Strategy.Player;
			}
		}

		for (int i = 0; i < nPopulations; i++) {
			for (int j = 0; j < nPopulations; j++) {
				if (j != i) {
					populations [i].AddPopulation (j, populations [j].population);
				}
			}
		}
	}

	public PopulationViewModel GetPopulation (int idx)
	{
		return populations [idx];
	}

	public void UpdateGame (float time)
	{
		foreach (KeyValuePair<int, PopulationViewModel> entry in populations) {
			entry.Value.Growth (deltaTime);
			growths [entry.Key] = entry.Value.Value - entry.Value.PValue;
		}
		foreach (KeyValuePair<int, PopulationViewModel> entry in populations) {
			emigrations [entry.Key] = entry.Value.Value;
			entry.Value.Migrate (deltaTime);
		}
		List<int> removeKeys = new List<int> ();
		globalPopulation = 0;
		foreach (KeyValuePair<int, PopulationViewModel> entry in populations) {
			bool ext = entry.Value.CheckExtinction ();
			emigrations [entry.Key] -= entry.Value.Value;
			values [entry.Key] = entry.Value.Value;
			globalPopulation += entry.Value.Value;
			if (ext) {
				removeKeys.Add (entry.Key);
			}
		}
		foreach (int key in removeKeys) {
			foreach (KeyValuePair<int, PopulationViewModel> entry in populations) {
				entry.Value.RemovePopulation (key);
			}
			Destroy (populations [key].gameObject);
			populations.Remove (key);
		}
	}
}
