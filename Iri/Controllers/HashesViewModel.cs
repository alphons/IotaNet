using System;
using System.Collections.Generic;

using IotaNet.Iri.Model;
using IotaNet.Iri.Storage;

namespace IotaNet.Iri.Controllers
{
	/**
	 * Created by paul on 5/6/17.
	 */
	public interface HashesViewModel
	{
		bool store(Tangle tangle);
		int size();
		bool addHash(Hash theHash);
		Indexable getIndex();
		HashSet<Hash> getHashes();
		void delete(Tangle tangle);

		HashesViewModel next(Tangle tangle);
	}
}
