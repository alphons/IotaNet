using System.Text;
using System.Linq;
using System.Collections.Generic;

using IotaNet.Iri.Storage;

namespace IotaNet.Iri.Model
{
	/**
	 * Created by paul on 3/8/17 for iri.
	 */
	public class Hashes : Persistable
	{

		public HashSet<Hash> set = new HashSet<Hash>();
		private static byte[] delimiter = Encoding.ASCII.GetBytes(",");

		public byte[] bytes()
		{
			//return set.parallelStream()
			//		.map(Hash::bytes)
			//		.reduce((a, b)->ArrayUtils.addAll(ArrayUtils.add(a, delimiter), b))
			//		.orElse(new byte[0]);
			return set
				.Select(x => x.bytes())
				.Aggregate((a,b) => a.Concat(delimiter).Concat(b).ToArray())
				.ToArray<byte>();
		}

		public void read(byte[] bytes)
		{
			if (bytes != null)
			{
				set = new HashSet<Hash>();// (bytes.length / (1 + Hash.SIZE_IN_BYTES) + 1);
				for (int i = 0; i < bytes.Length; i += 1 + Hash.SIZE_IN_BYTES)
				{
					set.Add(new Hash(bytes, i, Hash.SIZE_IN_BYTES));
				}
			}
		}

		public byte[] metadata()
		{
			return new byte[0];
		}

		public void readMetadata(byte[] bytes)
		{

		}

		public bool merge()
		{
			return true;
		}
	}
}

