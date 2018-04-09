using UnityEngine;

namespace Motive.Unity.Utilities
{
    /// <summary>
    /// Helper methods for identifying the type of device the app is running on.
    /// </summary>
	public class ApplicationHelper
	{
		public static bool IsMobile
		{
			get 
			{
				return (UnityEngine.Application.platform == RuntimePlatform.Android ||
				        UnityEngine.Application.platform == RuntimePlatform.IPhonePlayer ||
				        UnityEngine.Application.platform == RuntimePlatform.WP8Player ||
				        UnityEngine.Application.platform == RuntimePlatform.BlackBerryPlayer);
			}
		}

		public static bool IsIOS
		{
			get 
			{
				return (UnityEngine.Application.platform == RuntimePlatform.IPhonePlayer);
			}
		}

		public static bool IsAndroid
		{
			get
			{
				return (UnityEngine.Application.platform == RuntimePlatform.Android);
			}
		}

		public static bool IsWinPhone
		{
			get
			{
				return (UnityEngine.Application.platform == RuntimePlatform.WP8Player);
			}
		}
	}
}

