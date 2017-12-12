using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		boolean save(Persistable model, Indexable index);
		void delete(Class<?> model, Indexable index);

		boolean update(Persistable model, Indexable index, String item);

		boolean exists(Class<?> model, Indexable key);

		Pair<Indexable, Persistable> latest(Class<?> model, Class<?> indexModel);

		Set<Indexable> keysWithMissingReferences(Class<?> modelClass, Class<?> otherClass);

		Persistable get(Class<?> model, Indexable index);

		boolean mayExist(Class<?> model, Indexable index);

		long count(Class<?> model);

		Set<Indexable> keysStartingWith(Class<?> modelClass, byte[] value);

		Persistable seek(Class<?> model, byte[] key);

		Pair<Indexable, Persistable> next(Class<?> model, Indexable index);
		Pair<Indexable, Persistable> previous(Class<?> model, Indexable index);

		Pair<Indexable, Persistable> first(Class<?> model, Class<?> indexModel);

		boolean saveBatch(List<Pair<Indexable, Persistable>> models);

		void clear(Class<?> column);
		void clearMetadata(Class<?> column);
	}
}
