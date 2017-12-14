
namespace IotaNet.Iri.Model
{
	/**
	 * Created by paul on 5/15/17.
	 */
	public class Bundle : Hashes
	{

		public Bundle(Hash hash)
		{
			set.Add(hash);
		}

		public Bundle()
		{

		}
	}
}
