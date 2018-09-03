﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Microgame Assets/TouhouSort/Traits")]
public class TouhouSortTraits : MicrogameTraits
{
	public TouhouSortSorter.Category[] categories;
	[HideInInspector]
	public TouhouSortSorter.Category category;

	public override string localizedCommand { 
		get { return string.Format(TextHelper.getLocalizedText("microgame." + microgameId + ".command", command),
			TextHelper.getLocalizedText("microgame.TouhouSort." + category.name, category.name)); }
	}

	public override void onAccessInStage(string microgameId, int difficulty)
	{
		base.onAccessInStage(microgameId, difficulty);
		category = categories[Random.Range(0, categories.Length)];
	}
}
