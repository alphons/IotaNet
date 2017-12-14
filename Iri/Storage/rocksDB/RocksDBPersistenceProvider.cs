using IotaNet.Iri.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotaNet.Iri.Storage.rocksDB
{
	/**
	 * Created by paul on 3/2/17 for iri.
	 */
	public class RocksDBPersistenceProvider : PersistenceProvider
	{


		private static Logger log = LoggerFactory.getLogger<RocksDBPersistenceProvider>();
		private static int BLOOM_FILTER_BITS_PER_KEY = 10;

		private List<String> columnFamilyNames = Arrays.asList(
				new String(RocksDB.DEFAULT_COLUMN_FAMILY),
				"transaction",
				"transaction-metadata",
				"milestone",
				"stateDiff",
				"address",
				"approvee",
				"bundle",
				"tag"
		);
		private String dbPath;
		private String logPath;
		private int cacheSize;

		private ColumnFamilyHandle transactionHandle;
		private ColumnFamilyHandle transactionMetadataHandle;
		private ColumnFamilyHandle milestoneHandle;
		private ColumnFamilyHandle stateDiffHandle;
		private ColumnFamilyHandle addressHandle;
		private ColumnFamilyHandle approveeHandle;
		private ColumnFamilyHandle bundleHandle;
		private ColumnFamilyHandle tagHandle;

		private List<ColumnFamilyHandle> transactionGetList;

		private AtomicReference<Map<T, ColumnFamilyHandle>> classTreeMap = new AtomicReference<>();
		private AtomicReference<Map<T, ColumnFamilyHandle>> metadataReference = new AtomicReference<>();

		private SecureRandom seed = new SecureRandom();

		private RocksDB db;
		private DBOptions options;
		private BloomFilter bloomFilter;
		private boolean available;

		public RocksDBPersistenceProvider(String dbPath, String logPath, int cacheSize)
		{
			this.dbPath = dbPath;
			this.logPath = logPath;
			this.cacheSize = cacheSize;

		}

		public void init()
		{
			log.info("Initializing Database Backend... ");
			initDB(dbPath, logPath);
			initClassTreeMap();
			available = true;
			log.info("RocksDB persistence provider initialized.");
		}

		public bool isAvailable()
		{
			return this.available;
		}


		private void initClassTreeMap()
		{
			Map < Class <?>, ColumnFamilyHandle > classMap = new HashMap<>();
			classMap.put(Transaction.class, transactionHandle);
        classMap.put(Milestone.class, milestoneHandle);
        classMap.put(StateDiff.class, stateDiffHandle);
        classMap.put(Address.class, addressHandle);
        classMap.put(Approvee.class, approveeHandle);
        classMap.put(Bundle.class, bundleHandle);
        classMap.put(Tag.class, tagHandle);
        classTreeMap.set(classMap);

        Map<Class<?>, ColumnFamilyHandle> metadataHashMap = new HashMap<>();
		metadataHashMap.put(Transaction.class, transactionMetadataHandle);
        metadataReference.set(metadataHashMap);

        /*
        counts.put(Transaction.class, getCountEstimate(Transaction.class));
        counts.put(Milestone.class, getCountEstimate(Milestone.class));
        counts.put(StateDiff.class, getCountEstimate(StateDiff.class));
        counts.put(Hashes.class, getCountEstimate(Hashes.class));
        */
    }

	public void shutdown()
	{
		if (db != null) db.close();
		options.close();
		bloomFilter.close();
	}

	public bool save(Persistable thing, Indexable index)
{
	ColumnFamilyHandle handle = classTreeMap.get().get(thing.getClass());
	/*
	if( !db.keyMayExist(handle, index.bytes(), new StringBuffer()) ) {
		counts.put(thing.getClass(), counts.get(thing.getClass()) + 1);
	}
	*/
	db.put(handle, index.bytes(), thing.bytes());
	ColumnFamilyHandle referenceHandle = metadataReference.get().get(thing.getClass());
	if (referenceHandle != null)
	{
		db.put(referenceHandle, index.bytes(), thing.metadata());
	}
	return true;
	}

	public void delete<T>(T model, Indexable index)
	{
		/*
		if( db.keyMayExist(classTreeMap.get().get(model), index.bytes(), new StringBuffer()) ) {
			counts.put(model, counts.get(model) + 1);
		}
		*/
		db.delete(classTreeMap.get().get(model), index.bytes());
	}

	public boolean exists<T>(T model, Indexable key)
	{
		ColumnFamilyHandle handle = classTreeMap.get().get(model);
		return handle != null && db.get(handle, key.bytes()) != null;
	}

	public Dictionary<Indexable, Persistable> latest<T, U>(T model, U indexModel)
	{
		Indexable index;
		Persistable Object;
		RocksIterator iterator = db.newIterator(classTreeMap.get().get(model));
		iterator.seekToLast();
		if (iterator.isValid())
		{
			Object = (Persistable)model.newInstance();
			index = (Indexable)indexModel.newInstance();
			index.read(iterator.key());
			Object.read(iterator.value());
			ColumnFamilyHandle referenceHandle = metadataReference.get().get(model);
			if (referenceHandle != null)
			{
				Object.readMetadata(db.get(referenceHandle, iterator.key()));
			}
		}
		else
		{
			Object = null;
			index = null;
		}
		iterator.close();
		return new Pair<Indexable, Persistable>(index, Object);
	}

	public HashSet<Indexable> keysWithMissingReferences<T, U>(T model, U other)
	{
		ColumnFamilyHandle handle = classTreeMap.get().get(model);
		ColumnFamilyHandle otherHandle = classTreeMap.get().get(other);
		RocksIterator iterator = db.newIterator(handle);
		HashSet<Indexable> indexables = new HashSet<Indexable>();
		for (iterator.seekToFirst(); iterator.isValid(); iterator.next())
		{
			if (db.get(otherHandle, iterator.key()) == null)
			{
				indexables.add(new Hash(iterator.key()));
			}
		}
		iterator.close();
		return indexables;
	}

	public Persistable get<T>(T model, Indexable index)
	{
		Persistable Object = (Persistable)model.newInstance();
		Object.read(db.get(classTreeMap.get().get(model), index == null ? new byte[0] : index.bytes()));
		ColumnFamilyHandle referenceHandle = metadataReference.get().get(model);
		if (referenceHandle != null)
		{
			Object.readMetadata(db.get(referenceHandle, index == null ? new byte[0] : index.bytes()));
		}
		return Object;
	}


	public bool mayExist<T>(T model, Indexable index)
	{
		ColumnFamilyHandle handle = classTreeMap.get().get(model);
		return db.keyMayExist(handle, index.bytes(), new StringBuilder());
	}

	public long count(Class<?> model)
{
        return getCountEstimate(model);
	//return counts.get(model);
}

private long getCountEstimate(Class<?> model) 
{
	ColumnFamilyHandle handle = classTreeMap.get().get(model);
	return db.getLongProperty(handle, "rocksdb.estimate-num-keys");
	}

	@Override

	public Set<Indexable> keysStartingWith(Class<?> modelClass, byte[] value)
{
	RocksIterator iterator;
	ColumnFamilyHandle handle = classTreeMap.get().get(modelClass);
	Set<Indexable> keys = new HashSet<>();
	if (handle != null)
	{
		iterator = db.newIterator(handle);
		try
		{
			iterator.seek(new Hash(value, 0, value.length).bytes());
			for (;
				iterator.isValid() && Arrays.equals(Arrays.copyOf(iterator.key(), value.length), value);
				iterator.next())
			{
				keys.add(new Hash(iterator.key()));
			}
		}
		finally
		{
			iterator.close();
		}
	}
	return keys;
}


	public Persistable seek(Class<?> model, byte[] key) 
{
	Set<Indexable> hashes = keysStartingWith(model, key);
	Indexable out;
        if(hashes.size() == 1) {
            out = (Indexable)hashes.toArray()[0];
	} else if (hashes.size() > 1) {
            out = (Indexable)hashes.toArray()[seed.nextInt(hashes.size())];
	} else {
            out = null;
	}
        return get(model, out);
	}

	

	public Pair<Indexable, Persistable> next(Class<?> model, Indexable index)
{
	RocksIterator iterator = db.newIterator(classTreeMap.get().get(model));
	 Persistable object;
	 Indexable indexable;
	iterator.seek(index.bytes());
	iterator.next();
	if (iterator.isValid())
	{
		object = (Persistable)model.newInstance();
		indexable = index.getClass().newInstance();
		indexable.read(iterator.key());
		object.read(iterator.value());
		ColumnFamilyHandle referenceHandle = metadataReference.get().get(model);
		if (referenceHandle != null)
		{
			object.readMetadata(db.get(referenceHandle, iterator.key()));
		}
	}
	else
	{
		object = null;
		indexable = null;
	}
	iterator.close();
	return new Pair<>(indexable, object);
	}

	

	public Pair<Indexable, Persistable> previous(Class<?> model, Indexable index)
{
	RocksIterator iterator = db.newIterator(classTreeMap.get().get(model));
	 Persistable object;
	 Indexable indexable;
	iterator.seek(index.bytes());
	iterator.prev();
	if (iterator.isValid())
	{
		object = (Persistable)model.newInstance();
		object.read(iterator.value());
		indexable = (Indexable)index.getClass().newInstance();
		indexable.read(iterator.key());
		ColumnFamilyHandle referenceHandle = metadataReference.get().get(model);
		if (referenceHandle != null)
		{
			object.readMetadata(db.get(referenceHandle, iterator.key()));
		}
	}
	else
	{
		object = null;
		indexable = null;
	}
	iterator.close();
	return new Pair<>(indexable, object);
	}

	

	public Pair<Indexable, Persistable> first(Class<?> model, Class<?> index)
{
	RocksIterator iterator = db.newIterator(classTreeMap.get().get(model));
	 Persistable object;
	 Indexable indexable;
	iterator.seekToFirst();
	if (iterator.isValid())
	{
		object = (Persistable)model.newInstance();
		object.read(iterator.value());
		indexable = (Indexable)index.newInstance();
		indexable.read(iterator.key());
		ColumnFamilyHandle referenceHandle = metadataReference.get().get(model);
		if (referenceHandle != null)
		{
			object.readMetadata(db.get(referenceHandle, iterator.key()));
		}
	}
	else
	{
		object = null;
		indexable = null;
	}
	iterator.close();
	return new Pair<>(indexable, object);
	}


	public bool merge(Persistable model, Indexable index)
{
	boolean exists = mayExist(model.getClass(), index);
	db.merge(classTreeMap.get().get(model.getClass()), index.bytes(), model.bytes());
	return exists;
	}

	

	public boolean saveBatch(List<Pair<Indexable, Persistable>> models) 
{
	WriteBatch writeBatch = new WriteBatch();
WriteOptions writeOptions = new WriteOptions();
        for(Pair<Indexable, Persistable> entry: models) {
            Indexable key = entry.low;
Persistable value = entry.hi;
ColumnFamilyHandle handle = classTreeMap.get().get(value.getClass());
ColumnFamilyHandle referenceHandle = metadataReference.get().get(value.getClass());
            if(value.merge()) {
                writeBatch.merge(handle, key.bytes(), value.bytes());
            } else {
                writeBatch.put(handle, key.bytes(), value.bytes());
            }
            if(referenceHandle != null) {
                writeBatch.put(referenceHandle, key.bytes(), value.metadata());
            }
        }
        db.write(writeOptions, writeBatch);
        writeBatch.close();
        writeOptions.close();
        return true;
    }

   
	public void clear(Class<?> column) 
{
	flushHandle(classTreeMap.get().get(column));
	}

	

	public void clearMetadata(Class<?> column) 
{
	flushHandle(metadataReference.get().get(column));
	}


	private void flushHandle(ColumnFamilyHandle handle) 
{
	List<byte[]> itemsToDelete = new ArrayList<>();
        RocksIterator iterator = db.newIterator(handle);
        for(iterator.seekToLast(); iterator.isValid(); iterator.prev()) {
            itemsToDelete.add(iterator.key());
        }
        iterator.close();
        if(itemsToDelete.size() > 0) {
            log.info("Flushing flags. Amount to delete: " + itemsToDelete.size());
        }
        for(byte[] itemToDelete: itemsToDelete) {
            db.delete(handle, itemToDelete);
        }
    }


    
	public boolean update(Persistable thing, Indexable index, String item)
{
	ColumnFamilyHandle referenceHandle = metadataReference.get().get(thing.getClass());
	if (referenceHandle != null)
	{
		db.put(referenceHandle, index.bytes(), thing.metadata());
	}
	return false;
	}


	public void createBackup(String path) 
{
	Env env;
	BackupableDBOptions backupableDBOptions;
	BackupEngine backupEngine;
	env = Env.getDefault();
	backupableDBOptions = new BackupableDBOptions(path);
        try {
            backupEngine = BackupEngine.open(env, backupableDBOptions);
            backupEngine.createNewBackup(db, true);
            backupEngine.close();
        } finally {
            env.close();
            backupableDBOptions.close();
        }
    }

    public void restoreBackup(String path, String logPath) 
{
	Env env;
	BackupableDBOptions backupableDBOptions;
	BackupEngine backupEngine;
	env = Env.getDefault();
	backupableDBOptions = new BackupableDBOptions(path);
backupEngine = BackupEngine.open(env, backupableDBOptions);

		shutdown();
        try( RestoreOptions restoreOptions = new RestoreOptions(false)){
            backupEngine.restoreDbFromLatestBackup(path, logPath, restoreOptions);
        } finally {
            backupEngine.close();
        }
        backupableDBOptions.close();
        env.close();

		initDB(path, logPath);
    }

    private void initDB(String path, String logPath) 
{
        try {
		RocksDB.loadLibrary();
	} catch(Exception e) {
		if (SystemUtils.IS_OS_WINDOWS)
		{
			log.error("Error loading RocksDB library. " +
					"Please ensure that " +
					"Microsoft Visual C++ 2015 Redistributable Update 3 " +
					"is installed and updated");
		}
		throw e;
	}
	Thread.yield();

	File pathToLogDir = Paths.get(logPath).toFile();
	if (!pathToLogDir.exists() || !pathToLogDir.isDirectory())
	{
		pathToLogDir.mkdir();
	}

	RocksEnv.getDefault()
			.setBackgroundThreads(Runtime.getRuntime().availableProcessors() / 2, RocksEnv.FLUSH_POOL)
			.setBackgroundThreads(Runtime.getRuntime().availableProcessors() / 2, RocksEnv.COMPACTION_POOL)
	/*
			.setBackgroundThreads(Runtime.getRuntime().availableProcessors())
	*/
	;

	options = new DBOptions()
			.setCreateIfMissing(true)
			.setCreateMissingColumnFamilies(true)
			.setDbLogDir(logPath)
			.setMaxLogFileSize(SizeUnit.MB)
			.setMaxManifestFileSize(SizeUnit.MB)
			.setMaxOpenFiles(10000)
			.setMaxBackgroundCompactions(1)
	/*
	.setBytesPerSync(4 * SizeUnit.MB)
	.setMaxTotalWalSize(16 * SizeUnit.MB)
	*/
	;
	options.setMaxSubcompactions(Runtime.getRuntime().availableProcessors());

	bloomFilter = new BloomFilter(BLOOM_FILTER_BITS_PER_KEY);
	PlainTableConfig plainTableConfig = new PlainTableConfig();
	BlockBasedTableConfig blockBasedTableConfig = new BlockBasedTableConfig().setFilter(bloomFilter);
	blockBasedTableConfig
			.setFilter(bloomFilter)
			.setCacheNumShardBits(2)
			.setBlockSizeDeviation(10)
			.setBlockRestartInterval(16)
			.setBlockCacheSize(cacheSize * SizeUnit.KB)
			.setBlockCacheCompressedNumShardBits(10)
			.setBlockCacheCompressedSize(32 * SizeUnit.KB)
	/*
	.setHashIndexAllowCollision(true)
	.setCacheIndexAndFilterBlocks(true)
	*/
	;
	options.setAllowConcurrentMemtableWrite(true);

	MemTableConfig hashSkipListMemTableConfig = new HashSkipListMemTableConfig()
			.setHeight(9)
			.setBranchingFactor(9)
			.setBucketCount(2 * SizeUnit.MB);
	MemTableConfig hashLinkedListMemTableConfig = new HashLinkedListMemTableConfig().setBucketCount(100000);
	MemTableConfig vectorTableConfig = new VectorMemTableConfig().setReservedSize(10000);
	MemTableConfig skipListMemTableConfig = new SkipListMemTableConfig();


	MergeOperator mergeOperator = new StringAppendOperator();
	ColumnFamilyOptions columnFamilyOptions = new ColumnFamilyOptions()
			.setMergeOperator(mergeOperator)
			.setTableFormatConfig(blockBasedTableConfig)
			.setMaxWriteBufferNumber(2)
			.setWriteBufferSize(2 * SizeUnit.MB)
			/*
			.setCompactionStyle(CompactionStyle.UNIVERSAL)
			.setCompressionType(CompressionType.SNAPPY_COMPRESSION)
			*/
			;
	//columnFamilyOptions.setMemTableConfig(hashSkipListMemTableConfig);

	List<ColumnFamilyHandle> familyHandles = new ArrayList<>();
	//List<ColumnFamilyDescriptor> familyDescriptors = columnFamilyNames.stream().map(name -> new ColumnFamilyDescriptor(name.getBytes(), columnFamilyOptions)).collect(Collectors.toList());
	//familyDescriptors.add(0, new ColumnFamilyDescriptor(RocksDB.DEFAULT_COLUMN_FAMILY, new ColumnFamilyOptions()));

	List<ColumnFamilyDescriptor> columnFamilyDescriptors = columnFamilyNames.stream().map(name-> new ColumnFamilyDescriptor(name.getBytes(), columnFamilyOptions)).collect(Collectors.toList());
	//fillMissingColumns(columnFamilyDescriptors, familyHandles, path);
	db = RocksDB.open(options, path, columnFamilyDescriptors, familyHandles);
	db.enableFileDeletions(true);

	fillmodelColumnHandles(familyHandles);
	}



	private void fillmodelColumnHandles(List<ColumnFamilyHandle> familyHandles)
{
        int i = 0;
	transactionHandle = familyHandles.get(++i);
	transactionMetadataHandle = familyHandles.get(++i);
	milestoneHandle = familyHandles.get(++i);
	stateDiffHandle = familyHandles.get(++i);
	addressHandle = familyHandles.get(++i);
	approveeHandle = familyHandles.get(++i);
	bundleHandle = familyHandles.get(++i);
	tagHandle = familyHandles.get(++i);
	//hashesHandle = familyHandles.get(++i);

	for (; ++i < familyHandles.size();)
	{
		db.dropColumnFamily(familyHandles.get(i));
	}

	transactionGetList = new ArrayList<>();
	for (i = 1; i < 5; i++)
	{
		transactionGetList.add(familyHandles.get(i));
	}
	}

	

	private interface MyFunction<T, R>
{
	R apply(T t) throws Exception;
}


	private interface IndexFunction<T>
{
	void apply(T t);
}


	private interface DoubleFunction<T, I>
{
	void apply(T t, I i);
}


	private interface MyRunnable<R>
{
	R run();
}
private void fillMissingColumns(List<ColumnFamilyDescriptor> familyDescriptors, List<ColumnFamilyHandle> familyHandles, String path)
{
	List<ColumnFamilyDescriptor> columnFamilies = RocksDB.listColumnFamilies(new Options().setCreateIfMissing(true), path)
                .stream()
                .map(b -> new ColumnFamilyDescriptor(b, new ColumnFamilyOptions()))
                .collect(Collectors.toList());
columnFamilies.add(0, familyDescriptors.get(0));
        List<ColumnFamilyDescriptor> missingFromDatabase = familyDescriptors.stream().filter(d->columnFamilies.stream().filter(desc-> new String(desc.columnFamilyName()).equals(new String(d.columnFamilyName()))).toArray().length == 0).collect(Collectors.toList());
List<ColumnFamilyDescriptor> missingFromDescription = columnFamilies.stream().filter(d->familyDescriptors.stream().filter(desc-> new String(desc.columnFamilyName()).equals(new String(d.columnFamilyName()))).toArray().length == 0).collect(Collectors.toList());
        if (missingFromDatabase.size() != 0) {
            missingFromDatabase.remove(familyDescriptors.get(0));
            db = RocksDB.open(options, path, columnFamilies, familyHandles);
            for (ColumnFamilyDescriptor description : missingFromDatabase) {

				addColumnFamily(description.columnFamilyName(), db);
            }
            db.close();
        }
        if (missingFromDescription.size() != 0) {
            missingFromDescription.forEach(familyDescriptors::add);
        }
    }

	private void addColumnFamily(byte[] familyName, RocksDB db)
	{
		ColumnFamilyHandle columnFamilyHandle = db.createColumnFamily(

				   new ColumnFamilyDescriptor(familyName,

						   new ColumnFamilyOptions()));

		assert(columnFamilyHandle != null);
	}

}
}

