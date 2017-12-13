using System.IO;
namespace IotaNet.Iri.Helpers
{
	public static class Extensions
	{
		public static void put(this MemoryStream ms, byte[] data)
		{
			ms.Write(data, 0, data.Length);
		}
	}
}
