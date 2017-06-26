using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
	public PopulationViewModel population;
	public int nPopulations;
	public int range;
	public Dictionary<int, PopulationViewModel> populations;
	public Dictionary<int, Cosmos.Resource> resources;
	public float dt;
	public float globalPopulation;
	public List<float> emigrations;
	public List<float> growths;
	public List<float> values;

	// Use this for initialization
	void Start ()
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
			resources.Add (i, new Cosmos.Resource (10000000));
			populations [i].AddResource (resources [i]);
			populations [i].SetWorkers (0, populations [i].GetValue ());
		}

		for (int i = 0; i < nPopulations; i++) {
			for (int j = 0; j < nPopulations; j++) {
				if (j != i) {
					populations [i].AddPopulation (j, populations [j].population);
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		foreach (KeyValuePair<int, PopulationViewModel> entry in populations) {
			entry.Value.UpdatePopulation (dt);
			growths [entry.Key] = entry.Value.GetValue () - entry.Value.GetPreviousValue ();
		}
		foreach (KeyValuePair<int, PopulationViewModel> entry in populations) {
			emigrations [entry.Key] = entry.Value.GetValue ();
			entry.Value.Migrate (dt);
		}

		List<int> removeKeys = new List<int> ();
		globalPopulation = 0;
		foreach (KeyValuePair<int, PopulationViewModel> entry in populations) {
			bool ext = entry.Value.CheckExtinction ();
			emigrations [entry.Key] -= entry.Value.GetValue ();
			values [entry.Key] = entry.Value.GetValue ();
			globalPopulation += entry.Value.GetValue ();
			entry.Value.UpdatePlot (dt);
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
