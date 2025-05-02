using UnityEngine;

[CreateAssetMenu(fileName = "SettingsScriptable", menuName = "Scriptable WordSlide/SettingsScriptable")]
public class SettingsScriptable : ScriptableObject
{
	[SerializeField]
	private int columns = 10;
	public int Columns => columns;

	[SerializeField]
	private int rows = 10;
	public int Rows => rows;

	[SerializeField]
	private int minimumWordLength = 3;
	public int MinimumWordLength => minimumWordLength;
}
