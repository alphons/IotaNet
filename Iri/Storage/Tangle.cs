using IotaNet.Iri.Helpers;
using IotaNet.Iri.Storage;
using IotaNet.Iri.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotaNet.Iri.Storage
{
	/**
	 * Created by paul on 3/3/17 for iri.
	 */
	public class Tangle
	{
		private static Logger log = LoggerFactory.getLogger<Tangle>();

		private List<PersistenceProvider> persistenceProviders = new ArrayList<>();

		public void addPersistenceProvider(PersistenceProvider provider)
		{
			this.persistenceProviders.add(provider);
		}

		public void init()
		{
			foreach (PersistenceProvider provider in this.persistenceProviders)
			{
				provider.init();
			}
		}


		public void shutdown()
		{
			log.info("Shutting down Tangle Persistence Providers... ");

			this.persistenceProviders.forEach(PersistenceProvider::shutdown);
			this.persistenceProviders.clear();
		}

		public Persistable load(Class<?> model, Indexable index) throws Exception
		{
			Persistable out = null;
            foreach(PersistenceProvider provider in this.persistenceProviders) {
		if ((out = provider.get(model, index)) != null) {
			break;
		}
			}
            return out;
		}

public Boolean saveBatch(List<Pair<Indexable, Persistable>> models)
{
	bool exists = false;
	foreach (PersistenceProvider provider in persistenceProviders) {
		if (exists)
		{
			provider.saveBatch(models);
		}
		else
		{
			exists = provider.saveBatch(models);
		}
	}
	return exists;
}
public Boolean save(Persistable model, Indexable index) 
{
	bool exists = false;
            foreach(PersistenceProvider provider in persistenceProviders) {
		if (exists)
		{
			provider.save(model, index);
		}
		else
		{
			exists = provider.save(model, index);
		}
	}
            return exists;
}

public void delete(Class<?> model, Indexable index)
{
            foreach(PersistenceProvider provider in persistenceProviders) {
		provider.delete(model, index);
	}
}

public Pair<Indexable, Persistable> getLatest(Class<?> model, Class<?> index) throws Exception
{
	Pair<Indexable, Persistable> latest = null;
            for(PersistenceProvider provider: persistenceProviders) {
		if (latest == null)
		{
			latest = provider.latest(model, index);
		}
	}
            return latest;
}

public Boolean update(Persistable model, Indexable index, String item)
{
	bool success = false;
            foreach(PersistenceProvider provider in this.persistenceProviders) {
		if (success)
		{
			provider.update(model, index, item);
		}
		else
		{
			success = provider.update(model, index, item);
		}
	}
            return success;
}

public Set<Indexable> keysWithMissingReferences(Class<?> modelClass, Class<?> referencedClass) 
{
	Set<Indexable> output = null;
            foreach(PersistenceProvider provider in this.persistenceProviders) {
		output = provider.keysWithMissingReferences(modelClass, referencedClass);
		if (output != null && output.size() > 0)
		{
			break;
		}
	}
            return output;
}

public Set<Indexable> keysStartingWith(Class<?> modelClass, byte[] value)
{
	Set<Indexable> output = null;
	foreach (PersistenceProvider provider in this.persistenceProviders)
	{
		output = provider.keysStartingWith(modelClass, value);
		if (output.size() != 0)
		{
			break;
		}
	}
	return output;
}

public Boolean exists(Class<?> modelClass, Indexable hash) 
{
            foreach(PersistenceProvider provider in this.persistenceProviders) {
		if (provider.exists(modelClass, hash)) return true;
	}
            return false;
}

public Boolean maybeHas(Class<?> model, Indexable index) 
{
            foreach(PersistenceProvider provider in this.persistenceProviders) {
		if (provider.mayExist(model, index)) return true;
	}
            return false;
}

public Long getCount(Class<?> modelClass) 
{
            long value = 0;
            foreach(PersistenceProvider provider in this.persistenceProviders) {
		if ((value = provider.count(modelClass)) != 0)
		{
			break;
		}
	}
            return value;
}

public Persistable find(Class<?> model, byte[] key) 
{
	Persistable out = null;
            foreach (PersistenceProvider provider in this.persistenceProviders) {
		if ((out = provider.seek(model, key)) != null) {
			break;
		}
	}
            return out;
}

public Pair<Indexable, Persistable> next(Class<?> model, Indexable index) 
{
	Pair<Indexable, Persistable> latest = null;
            foreach(PersistenceProvider provider in persistenceProviders) {
		if (latest == null)
		{
			latest = provider.next(model, index);
		}
	}
            return latest;
}

public Pair<Indexable, Persistable> previous(Class<?> model, Indexable index) 
{
	Pair<Indexable, Persistable> latest = null;
            foreach(PersistenceProvider provider in persistenceProviders) {
		if (latest == null)
		{
			latest = provider.previous(model, index);
		}
	}
            return latest;
}

public Pair<Indexable, Persistable> getFirst(Class<?> model, Class<?> index) 
{
	Pair<Indexable, Persistable> latest = null;
            foreach(PersistenceProvider provider in persistenceProviders) {
		if (latest == null)
		{
			latest = provider.first(model, index);
		}
	}
            return latest;
}

public void clearColumn(Class<?> column) 
{
        foreach(PersistenceProvider provider in persistenceProviders) {
		provider.clear(column);
	}
}

public void clearMetadata(Class<?> column)
{
        foreach(PersistenceProvider provider in persistenceProviders) {
		provider.clearMetadata(column);
	}
}

    /*
    public boolean merge(Persistable model, Indexable index) throws Exception {
        boolean exists = false;
        for(PersistenceProvider provider: persistenceProviders) {
            if(exists) {
                provider.save(model, index);
            } else {
                exists = provider.merge(model, index);
            }
        }
        return exists;
    }
    */
}

