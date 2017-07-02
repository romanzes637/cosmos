using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopulationSliders : MonoBehaviour
{
	public Game game;
	public int populationIdx;
	PopulationViewModel pop;
	public Slider workersSlider;
	public Text workersText;
	public Slider cMeanSlider;
	public Text cMeanText;
	public Slider researchSlider;
	public Text researchText;
	public Slider researchersSlider;
	public Text researchersText;

	// Use this for initialization
	void Start ()
	{
		pop = game.GetPopulation (populationIdx);
		workersSlider.onValueChanged.AddListener (delegate {
			SetWorkersValue ();
		});
		cMeanSlider.onValueChanged.AddListener (delegate {
			SetCMeanValue ();
		});
		researchSlider.onValueChanged.AddListener (delegate {
			SetResearchValue ();
		});
		researchersSlider.onValueChanged.AddListener (delegate {
			SetResearchersValue ();
		});
		UpdateLimits ();
		workersSlider.value = pop.Workers;
		cMeanSlider.value = pop.CMean;
		researchSlider.value = pop.RMean;
		researchersSlider.value = pop.Researchers;
		Game.onTimeChanged += UpdateLimits;
	}

	void SetCMeanValue ()
	{
		pop.CMean = cMeanSlider.value;
		cMeanText.text = string.Format ("Consume: {0:F}", cMeanSlider.value);
	}

	void SetResearchValue ()
	{
		pop.RMean = researchSlider.value;
		researchText.text = string.Format ("Research: {0:F}", researchSlider.value);
	}

	void SetWorkersValue ()
	{
		pop.Workers = workersSlider.value;
		workersText.text = string.Format ("Workers: {0:F}", workersSlider.value);
	}

	void SetResearchersValue ()
	{
		pop.Researchers = researchersSlider.value;
		researchersText.text = string.Format ("Researchers: {0:F}", researchersSlider.value);
	}

	void UpdateLimits ()
	{
		workersSlider.minValue = 0;
		workersSlider.maxValue = pop.Value;
		cMeanSlider.minValue = 1;
		cMeanSlider.maxValue = pop.PMean;
		researchSlider.minValue = 0;
		researchSlider.maxValue = pop.PMean;
		researchersSlider.minValue = 0;
		researchersSlider.maxValue = pop.Value - pop.Workers;
	}
}
