using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(Text))]
public class Tickers : MonoBehaviour
{
	public Game game;
	public int populationIdx;
	Text text;
	List<string> tickers;
	List<float> pValues;
	List<float> values;
	string up = "\u25b2";
	string down = "\u25bc";
	string unchaged = "\u25ac";

	// Use this for initialization
	void Start ()
	{
		text = GetComponent<Text> ();
		UpdateText ();
		Game.onTimeChanged += UpdateText;
	}

	void UpdateText ()
	{
		text.text = "";
		tickers = new List<string> ();
		if (game.time > 0) {
			pValues = values;
		} else {
			pValues = new List<float> ();
		}
		values = new List<float> ();
		PopulationViewModel pop = game.GetPopulation (populationIdx);
		tickers.Add ("POP");
		values.Add (pop.Value);
		tickers.Add ("CONS");
		values.Add (pop.CMean);
		tickers.Add ("PROD");
		values.Add (pop.PMean);
		tickers.Add ("RES");
		values.Add (pop.RMean);
		tickers.Add ("NRES");
		values.Add (pop.Researchers);
		tickers.Add ("TECH");
		values.Add (pop.Technology);
		tickers.Add ("WORK");
		values.Add (pop.Workers);
		tickers.Add ("RESR");
		values.Add (pop.Resources);
		if (game.time == 0) {
			pValues.Add (pop.Value);
			pValues.Add (pop.CMean);
			pValues.Add (pop.PMean);
			pValues.Add (pop.RMean);
			pValues.Add (pop.Researchers);
			pValues.Add (pop.Technology);
			pValues.Add (pop.Workers);
			pValues.Add (pop.Resources);
		}
		for (int i = 0; i < tickers.Count; i++) {
			float delta = values [i] - pValues [i];
			if (delta > 0) {
				text.text += string.Format ("{0}{1}{2}+{3} ", tickers [i], up, values [i], delta);
			} else if (delta < 0) {
				text.text += string.Format ("{0}{1}{2}{3} ", tickers [i], down, values [i], delta);
			} else {
				text.text += string.Format ("{0}{1}{2} ", tickers [i], unchaged, values [i]);
			}
		}
	}
}
