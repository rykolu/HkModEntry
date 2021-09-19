using System;
using UnityEngine;

namespace Amplitude.UI
{
	[Serializable]
	public struct PathControlPoint
	{
		public Vector2 Position;

		public Vector2 LeftHandle;

		public Vector2 RightHandle;

		public bool IndependantHandles;

		public Vector2 LeftHandlePosition
		{
			get
			{
				return Position + LeftHandle;
			}
			set
			{
				LeftHandle = value - Position;
			}
		}

		public Vector2 RightHandlePosition
		{
			get
			{
				if (IndependantHandles)
				{
					return Position + RightHandle;
				}
				return Position - LeftHandle;
			}
			set
			{
				if (IndependantHandles)
				{
					RightHandle = value - Position;
				}
				else
				{
					LeftHandle = Position - value;
				}
			}
		}
	}
}
