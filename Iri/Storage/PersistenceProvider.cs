using System;
using System.Collections.Generic;

using IotaNet.Iri.Utils;

namespace IotaNet.Iri.Storage
{
	/**
	 * Created by paul on 3/2/17 for iri.
	 */
	public interface PersistenceProvider
	{
		void init();
		bool isAvailable();
		void shutdown();
		bool save(Persistable model, Indexable index);
		void delete<T>(Indexable index);

		bool update(Persistable model, Indexable index, String item);

		bool exists<T>(T model, Indexable key);

		Pair<Indexable, Persistable> latest<T, U>();

		HashSet<Indexable> keysWithMissingReferences<T,U>();

		Persistable get<T>(Indexable index);

		bool mayExist<T>(Indexable index);

		long count<T>(T model);

		HashSet<Indexable> keysStartingWith<T>(T modelClass, byte[] value);

		Persistable seek<T>(byte[] key);

		Pair<Indexable, Persistable> next<T>(Indexable index);
		Pair<Indexable, Persistable> previous<T>(Indexable index);

		Pair<Indexable, Persistable> first<T,U>();

		bool saveBatch(List<Pair<Indexable, Persistable>> models);

		void clear<T>(T column);
		void clearMetadata<T>(T column);

		//bool merge(Persistable model, Indexable index);
	}
}
