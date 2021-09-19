using Amplitude.Framework;
using Amplitude.UI.Renderers;
using UnityEngine;

namespace Amplitude.UI
{
	public class UIPerformanceAlertFeedback : UIRenderer
	{
		[SerializeField]
		private Color criticalColor = (HtmlColor)"#ed6d50ff";

		[SerializeField]
		private Color warningColor = (HtmlColor)"#fcb13eff";

		[SerializeField]
		private int easingDuration = 50;

		protected override void Render(UIPrimitiveDrawer drawer)
		{
		}
	}
}
