using System.Threading.Tasks;
using UnityEngine;
using WordSlide;
using Zenject;

public class MainMenuManager : MonoBehaviour
{
	[SerializeField]
	private GameObject gamePrefab;

	[SerializeField]
	private GameStateEventHandler gameStateEventHandler;

	[SerializeField]
	private ClickEventHandler clickEventHandler;

	[SerializeField]
	private TileEventHandler tileEventHandler;

	public static MainMenuManager Instance { get; private set; }

	private IDictionaryService _dictionaryService;
	private IWordFinderService _wordFinderService;

	[Inject]
	public async Task Construct(IDictionaryService dictionaryService, IWordFinderService wordFinderService)
	{
		_dictionaryService = dictionaryService;
		_wordFinderService = wordFinderService;

		await _dictionaryService.LoadDictionary("english");
		await _dictionaryService.LoadCharacterSet("english");
	}

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this);
			return;
		}
		Instance = this;
	}

	void Start()
	{
		var mainBoardGameObject = Instantiate(gamePrefab);

		// TODO: Add different PlayManager based on the game mode.
		var playManager = mainBoardGameObject.AddComponent<PlayManagerClassic>();

		// Initialize the PlayManager with the services
		playManager.Initialize(_dictionaryService, _wordFinderService, gameStateEventHandler, clickEventHandler, tileEventHandler);
	}
}
