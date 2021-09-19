using System.Collections.Generic;
using Amplitude.Framework;

namespace Amplitude.UI
{
	public interface IUIStampsService : IService
	{
		void FindStamps(StaticString key, ref List<UIComponent> result);

		void FindStamps(StaticString key, StaticString tag, ref List<UIComponent> result);

		void FindStamps(StaticString key, StaticString[] tags, ref List<UIComponent> result);

		void FindStamps(int keyGuid, ref List<UIComponent> result);

		void FindStamps(int keyGuid, StaticString tag, ref List<UIComponent> result);

		void FindStamps(int keyGuid, StaticString[] tags, ref List<UIComponent> result);
	}
}
