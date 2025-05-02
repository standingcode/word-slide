using UnityEngine;

[CreateAssetMenu(fileName = "SettingsScriptable", menuName = "Scriptable WordSlide/SettingsScriptable")]
public class SettingsScriptable : ScriptableObject
{
	[Header("Main Settings")]
	[SerializeField]
	private int columns = 10;
	public int Columns => columns;

	[SerializeField]
	private int rows = 10;
	public int Rows => rows;

	[SerializeField]
	private int minimumWordLength = 3;
	public int MinimumWordLength => minimumWordLength;

	[Header("Sizing")]
	[SerializeField]
	private float tilePaddingRatio = 0.1f;
	public float TilePaddingRatio => tilePaddingRatio;

	[SerializeField]
	private float minimumMarginFromBoardAsRatio = 0.05f;
	public float MinimumMarginFromBoardAsRatio => minimumMarginFromBoardAsRatio;
}
