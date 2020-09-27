using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Prism.Ioc;
using Prism.Unity;

namespace WpfApp1
{

	public partial class App : PrismApplication
	{
		protected override void RegisterTypes(IContainerRegistry containerRegistry)
		{
			containerRegistry.Register<Services.League>();
		}

		protected override Window CreateShell()
		{

			var w = Container.Resolve<MainWindow>();
			return w;
		}
	}
}
