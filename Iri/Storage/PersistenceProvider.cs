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
		void delete<T>(T model, Indexable index);

		bool update(Persistable model, Indexable index, String item);

		bool exists<T>(T model, Indexable key);

		Pair<Indexable, Persistable> latest<T, U>();

		HashSet<Indexable> keysWithMissingReferences<T,U>();

		Persistable get<T>(T model, Indexable index);

		bool mayExist<T>(T model, Indexable index);

		long count<T>(T model);

		HashSet<Indexable> keysStartingWith<T>(T modelClass, byte[] value);

		Persistable seek<T>(T model, byte[] key);

		Pair<Indexable, Persistable> next<T>(T model, Indexable index);
		Pair<Indexable, Persistable> previous<T>(T model, Indexable index);

		Pair<Indexable, Persistable> first<T,U>();

		bool saveBatch(List<Pair<Indexable, Persistable>> models);

		void clear<T>(T column);
		void clearMetadata<T>(T column);

		//bool merge(Persistable model, Indexable index);
	}
}
