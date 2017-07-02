using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeText : MonoBehaviour
{
	public Game game;
	Text text;

	void UpdateText ()
	{
		text.text = game.time.ToString ();
	}

	// Use this for initialization
	void Start ()
	{
		text = GetComponent<Text> ();
		text.text = game.time.ToString ();
		Game.onTimeChanged += UpdateText;
	}
}
