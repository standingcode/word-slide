using UnityEngine;
using WordSlide;
using Zenject;

public class GameInstaller : MonoInstaller
{
	public override void InstallBindings()
	{
		Container.Bind<IDictionaryManager>().
		To<DictionaryManager>()
		.AsSingle();

		Container.Bind<IDictionaryImporter>().
		To<DictionaryImporter>()
		.AsSingle();
	}
}
