using System.Linq;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Regions;
using Silverlight.Helper.UserControls;
using System.Windows;

namespace Silverlight.Helper.General
{
	public static class HelpContents
	{
		public static void DisplayHelp(string message,IRegionManager regionManager,bool help)
		{
			HelpText raisedText;
			IRegion region = regionManager.Regions[Constants.RegionHelp];
			if (region.Views.Count() == 0)
			{
				raisedText = new HelpText();
				raisedText.TextSize = 12;
				raisedText.ShadowOpacity = 0.5;
				raisedText.TextWeight = FontWeights.Bold;
				region.Add(raisedText);
			}
			else
			{
				raisedText = region.Views.FirstOrDefault() as HelpText;
			}
			if (raisedText != null)
			{
				string controlName = help ? "DisplayHelp" : "DisplayAuthor";
				TextBlock textBlock = raisedText.LayoutRoot.FindName(controlName) as TextBlock;
				if (textBlock != null)
					textBlock.Text = message;
			}
		}
	}
}
