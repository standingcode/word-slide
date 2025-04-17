using UnityEngine;
using WordSlide;
using Zenject;

public class GameInstaller : MonoInstaller
{
	public override void InstallBindings()
	{
		Container.Bind<IWordManager>().
		To<WordManager>()
		.AsSingle();
	}
}
