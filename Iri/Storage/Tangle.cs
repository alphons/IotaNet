using System;
using System.Collections.Generic;

using IotaNet.Iri.Helpers;
using IotaNet.Iri.Utils;

namespace IotaNet.Iri.Storage
{
	/**
	 * Created by paul on 3/3/17 for iri.
	 */
	public class Tangle
	{
		private static Logger log = LoggerFactory.getLogger<Tangle>();

		private List<PersistenceProvider> persistenceProviders = new List<PersistenceProvider>();

		public void addPersistenceProvider(PersistenceProvider provider)
		{
			this.persistenceProviders.Add(provider);
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

			this.persistenceProviders.ForEach(x => x.shutdown());
			this.persistenceProviders.Clear();
		}

		public Persistable load<T>(Indexable index)
		{
			Persistable Out = null;
			foreach (PersistenceProvider provider in this.persistenceProviders)
			{
				if ((Out = provider.get<T>(index)) != null)
				{
					break;
				}
			}
			return Out;
		}

		public Boolean saveBatch(List<Pair<Indexable, Persistable>> models)
		{
			bool exists = false;
			foreach (PersistenceProvider provider in persistenceProviders)
			{
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
			foreach (PersistenceProvider provider in persistenceProviders)
			{
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

		public void delete<T>(Indexable index)
		{
			foreach (PersistenceProvider provider in persistenceProviders)
			{
				provider.delete<T>(index);
			}
		}

		public Pair<Indexable, Persistable> getLatest<T, U>()//(T model, U index)
		{
			Pair<Indexable, Persistable> latest = null;
			foreach (PersistenceProvider provider in persistenceProviders)
			{
				if (latest == null)
				{
					latest = provider.latest<T,U>();
				}
			}
			return latest;
		}

		public Boolean update(Persistable model, Indexable index, String item)
		{
			bool success = false;
			foreach (PersistenceProvider provider in this.persistenceProviders)
			{
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

		public HashSet<Indexable> keysWithMissingReferences<T, U>(T modelClass, U referencedClass)
		{
			HashSet<Indexable> output = null;
			foreach (PersistenceProvider provider in this.persistenceProviders)
			{
				output = provider.keysWithMissingReferences(modelClass, referencedClass);
				if (output != null && output.Count > 0)
				{
					break;
				}
			}
			return output;
		}

		public HashSet<Indexable> keysStartingWith<T>(T modelClass, byte[] value)
		{
			HashSet<Indexable> output = null;
			foreach (PersistenceProvider provider in this.persistenceProviders)
			{
				output = provider.keysStartingWith(modelClass, value);
				if (output.Count != 0)
				{
					break;
				}
			}
			return output;
		}

		public Boolean exists<T>(T modelClass, Indexable hash)
		{
			foreach (PersistenceProvider provider in this.persistenceProviders)
			{
				if (provider.exists(modelClass, hash))
					return true;
			}
			return false;
		}

		public Boolean maybeHas<T>(Indexable index)
		{
			foreach (PersistenceProvider provider in this.persistenceProviders)
			{
				if (provider.mayExist<T>(index)) return true;
			}
			return false;
		}

		public long getCount<T>(T modelClass)
		{
			long value = 0;
			foreach (PersistenceProvider provider in this.persistenceProviders)
			{
				if ((value = provider.count(modelClass)) != 0)
				{
					break;
				}
			}
			return value;
		}

		public Persistable find<T>(byte[] key)
		{
			Persistable Out = null;
			foreach (PersistenceProvider provider in this.persistenceProviders)
			{
				if ((Out = provider.seek<T>(key)) != null)
				{
					break;
				}
			}
			return Out;
		}

		public Pair<Indexable, Persistable> next<T>(Indexable index)
		{
			Pair<Indexable, Persistable> latest = null;
			foreach (PersistenceProvider provider in persistenceProviders)
			{
				if (latest == null)
				{
					latest = provider.next<T>(index);
				}
			}
			return latest;
		}

		public Pair<Indexable, Persistable> previous<T>(Indexable index)
		{
			Pair<Indexable, Persistable> latest = null;
			foreach (PersistenceProvider provider in persistenceProviders)
			{
				if (latest == null)
				{
					latest = provider.previous<T>(index);
				}
			}
			return latest;
		}

		public Pair<Indexable, Persistable> getFirst<T, U>()
		{
			Pair<Indexable, Persistable> latest = null;
			foreach (PersistenceProvider provider in persistenceProviders)
			{
				if (latest == null)
				{
					latest = provider.first<T,U>();
				}
			}
			return latest;
		}

		public void clearColumn<T>(T column)
		{
			foreach (PersistenceProvider provider in persistenceProviders)
			{
				provider.clear(column);
			}
		}

		public void clearMetadata<T>(T column)
		{
			foreach (PersistenceProvider provider in persistenceProviders)
			{
				provider.clearMetadata(column);
			}
		}

		/*
		public bool merge(Persistable model, Indexable index)
		{
			bool exists = false;
			foreach(PersistenceProvider provider in persistenceProviders)
			{
				if(exists)
				{
					provider.save(model, index);
				}
				else
				{
					exists = provider.merge(model, index);
				}
			}
			return exists;
		}
		*/

	}
}

