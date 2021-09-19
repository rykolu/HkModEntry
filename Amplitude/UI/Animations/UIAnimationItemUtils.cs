using System;

namespace Amplitude.UI.Animations
{
	public static class UIAnimationItemUtils
	{
		public static IUIAnimationItemSerializable Create(Type implementation, UIAnimationItemParams parameters)
		{
			if (implementation == null)
			{
				return null;
			}
			IUIAnimationItemSerializable obj = (IUIAnimationItemSerializable)Activator.CreateInstance(implementation);
			obj.Parameters = parameters;
			return obj;
		}

		public static IUIAnimationItem Create(Type implementation)
		{
			return Activator.CreateInstance(implementation) as IUIAnimationItem;
		}

		public static IUIAnimationItem Clone(IUIAnimationItem other)
		{
			IUIAnimationItemSerializable obj = (IUIAnimationItemSerializable)Activator.CreateInstance(other.GetType());
			obj.Parameters = ((IUIAnimationItemSerializable)other).Parameters;
			return obj as IUIAnimationItem;
		}
	}
}
