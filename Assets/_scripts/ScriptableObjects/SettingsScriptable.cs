using UnityEngine;

[CreateAssetMenu(fileName = "SettingsScriptable", menuName = "Scriptable WordSlide/SettingsScriptable")]
public class SettingsScriptable : ScriptableObject
{
	[Header("Board")]
	private static int columns = 8;
	public static int Columns => columns;

	private static int rows = 8;
	public static int Rows => rows;

	private static float tilePaddingRatio = 0.1f;
	public static float TilePaddingRatio => tilePaddingRatio;

	private static float minimumMarginFromBoardAsRatio = 0.05f;
	public static float MinimumMarginFromBoardAsRatio => minimumMarginFromBoardAsRatio;

	[Header("Game logic")]

	private static int minimumWordLength = 3;
	public static int MinimumWordLength => minimumWordLength;

	[Header("User experience")]

	private static float ratioOfOverlapToSwapTile = 0.7f;
	public static float RatioOfOverlapToSwapTile => ratioOfOverlapToSwapTile;


	[Header("Appearance")]
	private static float gravitySpeed = 5f;
	public static float GravitySpeed => gravitySpeed;

	private static float tileMovementSpeed = 10f;
	public static float TileMovementSpeed => tileMovementSpeed;

}
