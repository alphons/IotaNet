using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IotaNet.Iri.Storage;
using IotaNet.Iri.Utils;

namespace IotaNet.Iri.Model
{
	/**
	 * Created by paul on 5/6/17.
	 */
	public class StateDiff : Persistable
	{

		public Dictionary<Hash, long> state;

		public byte[] bytes()
		{
			return state.entrySet().parallelStream()
					.map(entry->ArrayUtils.addAll(entry.getKey().bytes(), Serializer.serialize(entry.getValue())))
					.reduce(ArrayUtils::addAll)
					.orElse(new byte[0]);
		}
		public void read(byte[] bytes)
		{
			int i;
			state = new Dictionary<Hash, long>();
			if (bytes != null)
			{
				for (i = 0; i < bytes.Length; i += Hash.SIZE_IN_BYTES + sizeof(long))
				{
					state.Add(new Hash(bytes, i, Hash.SIZE_IN_BYTES),
							Serializer.getLong(Arrays.copyOfRange(bytes, i + Hash.SIZE_IN_BYTES, i + Hash.SIZE_IN_BYTES + sizeof(long))));
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
			return false;
		}
	}
}

