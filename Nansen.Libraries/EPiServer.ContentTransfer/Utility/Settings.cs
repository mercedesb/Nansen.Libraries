using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentTransfer.Utility
{
	public static class Settings
	{
		public static string MASTER_LANGUAGE
		{
			get
			{
				return Properties.Settings.Default.MASTER_LANGUAGE;
			}
		}

		public static bool ALWAYS_CREATE_MASTER_LANGUAGE
		{
			get
			{
				return Properties.Settings.Default.ALWAYS_CREATE_MASTER_LANGUAGE;
			}
		}
	}
}
