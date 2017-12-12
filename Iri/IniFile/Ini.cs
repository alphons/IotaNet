using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotaNet.Iri.IniFile
{
	public class Ini
	{
		public Ini(string ConfFile)
		{

		}
	}

	public class IniPreferences : Preferences
	{
		public IniPreferences(Ini ini)
		{

		}
	}

	public class Preferences
	{
		public Node node(string name)
		{
			return new Node();
		}
	}

	public class Node
	{
		public string get(string name, object c)
		{
			return null;
		}
	}
}
