using UnityEngine;

[CreateAssetMenu(fileName = "SettingsScriptable", menuName = "Scriptable WordSlide/SettingsScriptable")]
public class SettingsScriptable : ScriptableObject
{
	[Header("Main Settings")]
	private static int columns = 8;
	public static int Columns => columns;

	private static int rows = 8;
	public static int Rows => rows;

	private static int minimumWordLength = 4;
	public static int MinimumWordLength => minimumWordLength;

	[Header("Sizing")]
	private static float tilePaddingRatio = 0.1f;
	public static float TilePaddingRatio => tilePaddingRatio;

	private static float minimumMarginFromBoardAsRatio = 0.05f;
	public static float MinimumMarginFromBoardAsRatio => minimumMarginFromBoardAsRatio;

	[Header("Other")]
	private static float gravitySpeed = 5f;
	public static float GravitySpeed => gravitySpeed;

}
