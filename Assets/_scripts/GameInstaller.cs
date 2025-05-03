using UnityEngine;
using WordSlide;
using Zenject;

public class GameInstaller : MonoInstaller
{
	public override void InstallBindings()
	{
		Container.Bind<IDictionaryService>().
		To<DictionaryService>()
		.AsSingle();

		Container.Bind<IDictionaryImporterService>().
		To<DictionaryImporterService>()
		.AsSingle();

		Container.Bind<IWordFinderService>().
		To<WordFinderService>()
		.AsSingle();
	}
}
