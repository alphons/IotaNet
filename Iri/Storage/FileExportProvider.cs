using System;
using System.Collections.Generic;

using IotaNet.Iri.Helpers;
using IotaNet.Iri.Model;
using IotaNet.Iri.Utils;
using System.IO;
using System.Text;

using IotaNet.Iri.Controllers;

namespace IotaNet.Iri.Storage
{
	/**
	 * Created by paul on 4/18/17.
	 */
	public class FileExportProvider : PersistenceProvider
	{

		private static Logger log = LoggerFactory.getLogger<FileExportProvider>();

		public void init()
		{

		}

		public bool isAvailable()
		{
			return false;
		}

		public void shutdown()
		{

		}

		public bool save(Persistable model, Indexable index)
		{
			return false;
		}

		public void delete<T>(T model, Indexable index)
		{

		}

		public bool update(Persistable model, Indexable index, String item)
		{

			if (model is Transaction)
			{
				Transaction transaction = ((Transaction)model);
				if (item.Contains("sender"))
				{
					try
					{
						StreamWriter writer;
						var path = Path.Combine("export", getFileNumber() + ".tx");
						writer = new StreamWriter(path, false, Encoding.UTF8);
						writer.WriteLine(index.ToString());
						writer.WriteLine(Converter.trytes(trits(transaction)));
						writer.WriteLine(transaction.sender);
						if (item == "height")
						{
							writer.WriteLine("Height: " + transaction.height);
						}
						else
						{
							writer.WriteLine("Height: ");
						}
						writer.Close();
						return true;
					}
					catch (FileNotFoundException e)
					{
						log.error("File export failed", e);
					}
					catch (Exception e)
					{
						log.error("Transaction load failed. ", e);
					}
					finally
					{

					}
				}
			}
			return false;
		}

		public bool exists<T>(T model, Indexable key)
		{
			return false;
		}


		public Pair<Indexable, Persistable> latest<T, U>()
		{
			return null;
		}




		public HashSet<Indexable> keysWithMissingReferences<T, U>(T modelClass, U other)
		{
			return null;
		}


		public Persistable get<T>(T model, Indexable index)
		{
			return null;
		}


		public bool mayExist<T>(T model, Indexable index)
		{
			return false;
		}


		public long count<T>(T model)
		{
			return 0;
		}


		public HashSet<Indexable> keysStartingWith<T>(T modelClass, byte[] value)
		{
			return null;
		}


		public Persistable seek<T>(T model, byte[] key)
		{
			return null;
		}


		public Pair<Indexable, Persistable> next<T>(T model, Indexable index)
		{
			return null;
		}


		public Pair<Indexable, Persistable> previous<T>(T model, Indexable index)
		{
			return null;
		}


		public Pair<Indexable, Persistable> first<T, U>(T model, U indexModel)
		{
			return null;
		}

		public bool merge(Persistable model, Indexable index)
		{
			return false;
		}


		public bool saveBatch(List<Pair<Indexable, Persistable>> models)
		{
			return false;
		}


		public void clear<T>(T column)
		{

		}


		public void clearMetadata<T>(T column)
		{

		}

		private static long lastFileNumber = 0L;
		private static Object locker = new Object();

		public static long getFileNumber()
		{
			long now = DateTimeOffset.Now.ToUnixTimeMilliseconds() * 1000;
			lock (locker)
			{
				if (now <= lastFileNumber)
				{
					return ++lastFileNumber;
				}
				lastFileNumber = now;
			}
			return now;
		}

		int[] trits(Transaction transaction)
		{
			int[] _trits = new int[TransactionViewModel.TRINARY_SIZE];
			if (transaction._bytes != null)
			{
				Converter.getTrits(transaction._bytes, _trits);
			}
			return _trits;
		}

	}
}
