using IotaNet.Iri.Storage;
using IotaNet.Iri.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotaNet.Iri.Model
{
	/**
	 * Created by paul on 4/11/17.
	 */
	public class Milestone : Persistable
	{
		public IntegerIndex index;
		public Hash hash;

		public byte[] bytes()
		{
			return index.bytes().Concat(hash.bytes()).ToArray();
		}

		public void read(byte[] bytes)
		{
			if (bytes != null)
			{
				index = new IntegerIndex(Serializer.getInteger(bytes));
				hash = new Hash(bytes, sizeof(int), Hash.SIZE_IN_BYTES);
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
			return false;
		}
	}
}
