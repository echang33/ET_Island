/*
 * Readme.cs
 * Team members: Ethan Chang, Ryan Wu, Steven Tan
 * Purpose: Unity template asset that stores the project readme icon, title, and help sections.
 */

using System;
using UnityEngine;

public class Readme : ScriptableObject {
	[Header("Readme Appearance")]
	public Texture2D icon;
	public float iconMaxWidth = 128f;
	public string title;
	[Header("Readme Content")]
	public Section[] sections;
	[Header("Editor Setup State")]
	[Tooltip("Tracks whether the template editor layout has already been loaded.")]
	public bool loadedLayout;
	
	[Serializable]
	public class Section {
		[Header("Section Content and Optional Link")]
		public string heading, text, linkText, url;
	}
}
