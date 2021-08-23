using System.Collections.Generic;

namespace Caregivr
{
	public class VariantDictionary : Dictionary<string, Variant> 
	{
		public string ReturnDataFromIdentifier(string inputString)
		{
			try
			{
				VariantDictionary dict = this;
				string[] parts = inputString.Split('.');
				string dataText = "";

				for (int i = 0; i < parts.Length; i++)
				{
					string key = parts[i];
					if (i == parts.Length - 1)
					{
						dataText = dict[key].AsString();
					}
					else
					{
						dict = dict[key].AsData();
					}
				}
				return dataText;
			} catch (System.Exception)
			{
				return "";
			}
		}
	}
}
