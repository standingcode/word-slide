using UnityEngine;

[CreateAssetMenu(fileName = "SettingsScriptable", menuName = "Scriptable WordSlide/SettingsScriptable")]
public class SettingsScriptable : ScriptableObject
{
	[Header("Board")]
	public int Columns = 8;

	public int Rows = 8;

	public float TilePaddingRatio = 0.1f;

	public float MinimumMarginFromBoardAsRatio = 0.05f;

	[Header("Game logic")]

	public int MinimumWordLength = 4;

	[Header("User experience")]

	public float RatioOfOverlapToSwapTile = 0.7f;


	[Header("Appearance")]
	public float GravitySpeed = 5f;

	public float TileMovementSpeed = 10f;
}
