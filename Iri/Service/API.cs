using IotaNet.Iri.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotaNet.Iri.Service
{
	public class API
	{

		private static Logger log = LoggerFactory.getLogger<API>();
    private IXI ixi;

    private Undertow server;

		private  Gson gson = new GsonBuilder().create();
		private volatile PearlDiver pearlDiver = new PearlDiver();

		private  AtomicInteger counter = new AtomicInteger(0);

		private Pattern trytesPattern = Pattern.compile("[9A-Z]*");

		private  static int HASH_SIZE = 81;
		private  static int TRYTES_SIZE = 2673;

		private  static long MAX_TIMESTAMP_VALUE = (3 ^ 27 - 1) / 2;

		private  int minRandomWalks;
		private  int maxRandomWalks;
		private  int maxFindTxs;
		private  int maxRequestList;
		private  int maxGetTrytes;
		private  int maxBodyLength;
		private  static String overMaxErrorMessage = "Could not complete request";
		private  static String invalidParams = "Invalid parameters";

		private  static char ZERO_LENGTH_ALLOWED = 'Y';
		private  static char ZERO_LENGTH_NOT_ALLOWED = 'N';
		private Iota instance;

		public API(Iota instance, IXI ixi)
		{
			this.instance = instance;
			this.ixi = ixi;
			minRandomWalks = instance.configuration.integer(DefaultConfSettings.MIN_RANDOM_WALKS);
			maxRandomWalks = instance.configuration.integer(DefaultConfSettings.MAX_RANDOM_WALKS);
			maxFindTxs = instance.configuration.integer(DefaultConfSettings.MAX_FIND_TRANSACTIONS);
			maxRequestList = instance.configuration.integer(DefaultConfSettings.MAX_REQUESTS_LIST);
			maxGetTrytes = instance.configuration.integer(DefaultConfSettings.MAX_GET_TRYTES);
			maxBodyLength = instance.configuration.integer(DefaultConfSettings.MAX_BODY_LENGTH);
		}

		public void init()
		{
			 int apiPort = instance.configuration.integer(DefaultConfSettings.PORT);
		 String apiHost = instance.configuration.string (DefaultConfSettings.API_HOST);

        log.debug("Binding JSON-REST API Undertow server on {}:{}", apiHost, apiPort);

        server = Undertow.builder().addHttpListener(apiPort, apiHost)
	                .setHandler(path().addPrefixPath("/", addSecurity(new HttpHandler()
		{
			
	
					public void handleRequest( HttpServerExchange exchange) throws Exception {
				HttpString requestMethod = exchange.getRequestMethod();
				if (Methods.OPTIONS.equals(requestMethod))
				{
					String allowedMethods = "GET,HEAD,POST,PUT,DELETE,TRACE,OPTIONS,CONNECT,PATCH";
					//return list of allowed methods in response headers
					exchange.setStatusCode(StatusCodes.OK);
					exchange.getResponseHeaders().put(Headers.CONTENT_TYPE, MimeMappings.DEFAULT_MIME_MAPPINGS.get("txt"));
					exchange.getResponseHeaders().put(Headers.CONTENT_LENGTH, 0);
					exchange.getResponseHeaders().put(Headers.ALLOW, allowedMethods);
					exchange.getResponseHeaders().put(new HttpString("Access-Control-Allow-Origin"), "*");
					exchange.getResponseHeaders().put(new HttpString("Access-Control-Allow-Headers"), "Origin, X-Requested-With, Content-Type, Accept, X-IOTA-API-Version");
					exchange.getResponseSender().close();
					return;
				}

				if (exchange.isInIoThread())
				{
					exchange.dispatch(this);
					return;
				}
				processRequest(exchange);
			}
		}))).build();
		server.start();
    }

	private void processRequest( HttpServerExchange exchange) 
	{
		 ChannelInputStream cis = new ChannelInputStream(exchange.getRequestChannel());
	exchange.getResponseHeaders().put(Headers.CONTENT_TYPE, "application/json");

	 long beginningTime = System.currentTimeMillis();
	 String body = IOUtils.toString(cis, StandardCharsets.UTF_8);
         AbstractResponse response;

        if (!exchange.getRequestHeaders().contains("X-IOTA-API-Version")) {
            response = ErrorResponse.create("Invalid API Version");
        } else if (body.length() > maxBodyLength) {
            response = ErrorResponse.create("Request too long");
        } else {
            response = process(body, exchange.getSourceAddress());
        }

		sendResponse(exchange, response, beginningTime);
    }

    private AbstractResponse process( String requestString, InetSocketAddress sourceAddress) 
{

        try {

		 Map<String, Object> request = gson.fromJson(requestString, Map.class);
            if (request == null) {
                return ExceptionResponse.create("Invalid request payload: '" + requestString + "'");
            }

             String command = (String) request.get("command");
            if (command == null) {
                return ErrorResponse.create("COMMAND parameter has not been specified in the request.");
            }

            if (instance.configuration.string (DefaultConfSettings.REMOTE_LIMIT_API).contains(command) &&
                    !sourceAddress.getAddress().isLoopbackAddress()) {
                return AccessLimitedResponse.create("COMMAND " + command + " is not available on this node");
            }

            log.debug("# {} -> Requesting command '{}'", counter.incrementAndGet(), command);

            switch (command) {

                case "addNeighbors": {
                    List<String> uris = getParameterAsList(request, "uris", 0);
log.debug("Invoking 'addNeighbors' with {}", uris);
                    return addNeighborsStatement(uris);
                }
                case "attachToTangle": {
                     Hash trunkTransaction  = new Hash(getParameterAsStringAndValidate(request,"trunkTransaction", HASH_SIZE));
                     Hash branchTransaction = new Hash(getParameterAsStringAndValidate(request,"branchTransaction", HASH_SIZE));
                     int minWeightMagnitude = getParameterAsInt(request, "minWeightMagnitude");

 List<String> trytes = getParameterAsList(request, "trytes", TRYTES_SIZE);

List<String> elements = attachToTangleStatement(trunkTransaction, branchTransaction, minWeightMagnitude, trytes);
                    return AttachToTangleResponse.create(elements);
                }
                case "broadcastTransactions": {

					broadcastTransactionStatement(getParameterAsList(request,"trytes", TRYTES_SIZE));
                    return AbstractResponse.createEmptyResponse();
                }
                case "findTransactions": {
                    return findTransactionStatement(request);
                }
                case "getBalances": {
                     List<String> addresses = getParameterAsList(request, "addresses", HASH_SIZE);
 int threshold = getParameterAsInt(request, "threshold");
                    return getBalancesStatement(addresses, threshold);
                }
                case "getInclusionStates": {
                    if (invalidSubtangleStatus()) {
                        return ErrorResponse
								.create("This operations cannot be executed: The subtangle has not been updated yet.");
                    }
                     List<String> transactions = getParameterAsList(request, "transactions", HASH_SIZE);
 List<String> tips = getParameterAsList(request, "tips", HASH_SIZE);

                    return getNewInclusionStateStatement(transactions, tips);
                }
                case "getNeighbors": {
                    return getNeighborsStatement();
                }
                case "getNodeInfo": {
                    String name = instance.configuration.booling(Configuration.DefaultConfSettings.TESTNET) ? IRI.TESTNET_NAME : IRI.MAINNET_NAME;
                    return GetNodeInfoResponse.create(name, IRI.VERSION, Runtime.getRuntime().availableProcessors(),
                            Runtime.getRuntime().freeMemory(), System.getProperty("java.version"), Runtime.getRuntime().maxMemory(),
                            Runtime.getRuntime().totalMemory(), instance.milestone.latestMilestone, instance.milestone.latestMilestoneIndex,
                            instance.milestone.latestSolidSubtangleMilestone, instance.milestone.latestSolidSubtangleMilestoneIndex,
                            instance.node.howManyNeighbors(), instance.node.queuedTransactionsSize(),
                            System.currentTimeMillis(), instance.tipsViewModel.size(),
                            instance.transactionRequester.numberOfTransactionsToRequest());
                }
                case "getTips": {
                    return getTipsStatement();
                }
                case "getTransactionsToApprove": {
                    if (invalidSubtangleStatus()) {
                        return ErrorResponse
								.create("This operations cannot be executed: The subtangle has not been updated yet.");
                    }

                     int depth = getParameterAsInt(request, "depth");
 String reference = request.containsKey("reference") ? getParameterAsStringAndValidate(request,"reference", HASH_SIZE) : null;
                    int numWalks = request.containsKey("numWalks") ? getParameterAsInt(request, "numWalks") : 1;
                    if(numWalks<minRandomWalks) {
                        numWalks = minRandomWalks;
                    }

                     Hash[] tips = getTransactionToApproveStatement(depth, reference, numWalks);
                    if(tips == null) {
                        return ErrorResponse.create("The subtangle is not solid");
                    }
                    return GetTransactionsToApproveResponse.create(tips[0], tips[1]);
                }
                case "getTrytes": {
                     List<String> hashes = getParameterAsList(request, "hashes", HASH_SIZE);
                    return getTrytesStatement(hashes);
                }

                case "interruptAttachingToTangle": {
                    pearlDiver.cancel();
                    return AbstractResponse.createEmptyResponse();
                }
                case "removeNeighbors": {
                    List<String> uris = getParameterAsList(request, "uris", 0);
log.debug("Invoking 'removeNeighbors' with {}", uris);
                    return removeNeighborsStatement(uris);
                }

                case "storeTransactions": {
                    try {

						storeTransactionStatement(getParameterAsList(request,"trytes", TRYTES_SIZE));
                    } catch (RuntimeException e) {
                        //transaction not valid
                        return ErrorResponse.create("Invalid trytes input");
                    }
                }
                case "getMissingTransactions": {
					//TransactionRequester.instance().rescanTransactionsToRequest();
					synchronized(instance.transactionRequester)
{
	List<String> missingTx = Arrays.stream(instance.transactionRequester.getRequestedTransactions())
			.map(Hash::toString)
			.collect(Collectors.toList());
	return GetTipsResponse.create(missingTx);
}
                }
                default: {
                    AbstractResponse response = ixi.processCommand(command, request);
                    return response == null ?
							ErrorResponse.create("Command [" + command + "] is unknown") :

							response;
                }
            }

        } catch ( ValidationException e) {
            log.info("API Validation failed: " + e.getLocalizedMessage());
            return ErrorResponse.create(e.getLocalizedMessage());
        } catch ( Exception e) {
            log.error("API Exception: ", e);
            return ExceptionResponse.create(e.getLocalizedMessage());
        }
    }

    private int getParameterAsInt(Map<String, Object> request, String paramName) 
{
	validateParamExists(request, paramName);
	 int result;
        try {
		result = ((Double)request.get(paramName)).intValue();
	} catch (ClassCastException e) {
		throw new ValidationException("Invalid " + paramName + " input");
	}
        return result;
}

private String getParameterAsStringAndValidate(Map<String, Object> request, String paramName, int size) 
{
	validateParamExists(request, paramName);
	String result = (String) request.get(paramName);
	validateTrytes(paramName, size, result);
	return result;
	}


	private void validateTrytes(String paramName, int size, String result) 
{
        if (!validTrytes(result,size,ZERO_LENGTH_NOT_ALLOWED)) {
		throw new ValidationException("Invalid " + paramName + " input");
	}
}

private void validateParamExists(Map<String, Object> request, String paramName)
{
        if (!request.containsKey(paramName)) {
		throw new ValidationException(invalidParams);
	}
}

private List<String> getParameterAsList(Map<String, Object> request, String paramName, int size) 
{
	validateParamExists(request, paramName);
	 List<String> paramList = (List<String>) request.get(paramName);
	if (paramList.size() > maxRequestList)
	{
		throw new ValidationException(overMaxErrorMessage);
	}

	if (size > 0)
	{
		//validate
		for ( String param : paramList)
		{
			validateTrytes(paramName, size, param);
		}
	}

	return paramList;

	}


	public boolean invalidSubtangleStatus()
{
	return (instance.milestone.latestSolidSubtangleMilestoneIndex == Milestone.MILESTONE_START_INDEX);
}

private AbstractResponse removeNeighborsStatement(List<String> uris) 
{
	 AtomicInteger numberOfRemovedNeighbors = new AtomicInteger(0);

        for ( String uriString : uris) {
             URI uri = new URI(uriString);

            if ("udp".equals(uri.getScheme()) || "tcp".equals(uri.getScheme())) {
                log.info("Removing neighbor: "+uriString);
                if (instance.node.removeNeighbor(uri,true)) {
                    numberOfRemovedNeighbors.incrementAndGet();
                }
            }
            else {
                return ErrorResponse.create("Invalid uri scheme");
            }
        }
        return RemoveNeighborsResponse.create(numberOfRemovedNeighbors.get());
    }

    private synchronized AbstractResponse getTrytesStatement(List<String> hashes) 
{
	 List<String> elements = new LinkedList<>();
        for ( String hash : hashes) {
             TransactionViewModel transactionViewModel = TransactionViewModel.fromHash(instance.tangle, new Hash(hash));
            if (transactionViewModel != null) {
                elements.add(Converter.trytes(transactionViewModel.trits()));
            }
        }
        if (elements.size() > maxGetTrytes){
            return ErrorResponse.create(overMaxErrorMessage);
        }
        return GetTrytesResponse.create(elements);
    }

    private static int counter_getTxToApprove = 0;
public static int getCounter_getTxToApprove()
{
	return counter_getTxToApprove;
}
public static void incCounter_getTxToApprove()
{
	counter_getTxToApprove++;
}

private static long ellapsedTime_getTxToApprove = 0L;
public static long getEllapsedTime_getTxToApprove()
{
	return ellapsedTime_getTxToApprove;
}
public static void incEllapsedTime_getTxToApprove(long ellapsedTime)
{
	ellapsedTime_getTxToApprove += ellapsedTime;
}

public synchronized Hash[] getTransactionToApproveStatement( int depth,  String reference,  int numWalks)
{
        int tipsToApprove = 2;
	Hash []
	tips = new Hash[tipsToApprove];
         SecureRandom random = new SecureRandom();
 int randomWalkCount = numWalks > maxRandomWalks || numWalks < 1 ? maxRandomWalks : numWalks;
Hash referenceHash = null;
        if(reference != null) {
            referenceHash = new Hash(reference);
            if(!TransactionViewModel.exists(instance.tangle, referenceHash)) {
                referenceHash = null;
            }
        }
        for(int i = 0; i<tipsToApprove; i++) {
            tips[i] = instance.tipsManager.transactionToApprove(referenceHash, tips[0], depth, randomWalkCount, random);
            if (tips[i] == null) {
                return null;
            }
        }
        API.incCounter_getTxToApprove();
        if ((getCounter_getTxToApprove() % 100) == 0 ) {
            String sb = "Last 100 getTxToApprove consumed " +
					API.getEllapsedTime_getTxToApprove() / 1000000000L +
					" seconds processing time.";
log.info(sb);
            counter_getTxToApprove = 0;
            ellapsedTime_getTxToApprove = 0L;
        }
        return tips;
    }

    private synchronized AbstractResponse getTipsStatement() 
{
        return GetTipsResponse.create(instance.tipsViewModel.getTips().stream().map(Hash::toString).collect(Collectors.toList()));
}

public void storeTransactionStatement( List<String> trys)
{
        for ( String trytes : trys) {
		 TransactionViewModel transactionViewModel = instance.transactionValidator.validate(Converter.trits(trytes),
				instance.transactionValidator.getMinWeightMagnitude());
		if (transactionViewModel.store(instance.tangle))
		{
			transactionViewModel.setArrivalTime(System.currentTimeMillis() / 1000L);
			instance.transactionValidator.updateStatus(transactionViewModel);
			transactionViewModel.updateSender("local");
			transactionViewModel.update(instance.tangle, "sender");
		}
	}
}

private AbstractResponse getNeighborsStatement()
{
	return GetNeighborsResponse.create(instance.node.getNeighbors());
}

private AbstractResponse getNewInclusionStateStatement( List<String> trans,  List<String> tps) 
{
	 List<Hash> transactions = trans.stream().map(Hash::new).collect(Collectors.toList());
 List<Hash> tips = tps.stream().map(Hash::new).collect(Collectors.toList());
int numberOfNonMetTransactions = transactions.size();
 int[] inclusionStates = new int[numberOfNonMetTransactions];

List<Integer> tipsIndex = new LinkedList<>();
        {
            for(Hash hash: tips) {
                TransactionViewModel tx = TransactionViewModel.fromHash(instance.tangle, hash);
                if (tx.getType() != TransactionViewModel.PREFILLED_SLOT) {
                    tipsIndex.add(tx.snapshotIndex());
                }
            }
        }
        int minTipsIndex = tipsIndex.stream().reduce((a, b)->a < b ? a : b).orElse(0);
        if(minTipsIndex > 0) {
            int maxTipsIndex = tipsIndex.stream().reduce((a, b)->a > b ? a : b).orElse(0);
            for(Hash hash: transactions) {
                TransactionViewModel transaction = TransactionViewModel.fromHash(instance.tangle, hash);
                if(transaction.getType() == TransactionViewModel.PREFILLED_SLOT || transaction.snapshotIndex() == 0) {
                    inclusionStates[transactions.indexOf(transaction.getHash())] = -1;
                } else if(transaction.snapshotIndex() > maxTipsIndex) {
                    inclusionStates[transactions.indexOf(transaction.getHash())] = -1;
                } else if(transaction.snapshotIndex() < maxTipsIndex) {
                    inclusionStates[transactions.indexOf(transaction.getHash())] = 1;
                }
            }
        }

        Set<Hash> analyzedTips = new HashSet<>();
Map<Integer, Set<Hash>> sameIndexTips = new HashMap<>();
Map<Integer, Set<Hash>> sameIndexTransactions = new HashMap<>();
Map<Integer, Queue<Hash>> nonAnalyzedTransactionsMap = new HashMap<>();
        for ( Hash tip : tips) {
            TransactionViewModel transactionViewModel = TransactionViewModel.fromHash(instance.tangle, tip);
            if (transactionViewModel.getType() == TransactionViewModel.PREFILLED_SLOT){
                return ErrorResponse.create("One of the tips absents");
            }
            sameIndexTips.putIfAbsent(transactionViewModel.snapshotIndex(), new HashSet<>());
            sameIndexTips.get(transactionViewModel.snapshotIndex()).add(tip);
nonAnalyzedTransactionsMap.putIfAbsent(transactionViewModel.snapshotIndex(), new LinkedList<>());
            nonAnalyzedTransactionsMap.get(transactionViewModel.snapshotIndex()).offer(tip);
        }
        for(int i = 0; i<inclusionStates.length; i++) {
            if(inclusionStates[i] == 0) {
                TransactionViewModel transactionViewModel = TransactionViewModel.fromHash(instance.tangle, transactions.get(i));
sameIndexTransactions.putIfAbsent(transactionViewModel.snapshotIndex(), new HashSet<>());
                sameIndexTransactions.get(transactionViewModel.snapshotIndex()).add(transactionViewModel.getHash());
            }
        }
        for(Map.Entry<Integer, Set<Hash>> entry: sameIndexTransactions.entrySet()) {
            if(!exhaustiveSearchWithinIndex(nonAnalyzedTransactionsMap.get(entry.getKey()), analyzedTips, transactions, inclusionStates, entry.getValue().size(), entry.getKey())) {
                return ErrorResponse.create("The subtangle is not solid");
            }
        }
         boolean[] inclusionStatesBoolean = new boolean[inclusionStates.length];
        for(int i = 0; i<inclusionStates.length; i++) {
            inclusionStatesBoolean[i] = inclusionStates[i] == 1;
        }
        {
            return GetInclusionStatesResponse.create(inclusionStatesBoolean);
        }
    }
    private boolean exhaustiveSearchWithinIndex(Queue<Hash> nonAnalyzedTransactions, Set<Hash> analyzedTips, List<Hash> transactions, int[] inclusionStates, int count, int index) throws Exception
{
	Hash pointer;
	MAIN_LOOP:
        while ((pointer = nonAnalyzedTransactions.poll()) != null) {


		if (analyzedTips.add(pointer))
		{

			 TransactionViewModel transactionViewModel = TransactionViewModel.fromHash(instance.tangle, pointer);
			if (transactionViewModel.snapshotIndex() == index)
			{
				if (transactionViewModel.getType() == TransactionViewModel.PREFILLED_SLOT)
				{
					return false;
				}
				else
				{
					for (int i = 0; i < inclusionStates.length; i++)
					{

						if (inclusionStates[i] < 1 && pointer.equals(transactions.get(i)))
						{
							inclusionStates[i] = 1;
							if (--count <= 0)
							{
								break MAIN_LOOP;
							}
						}
					}
					nonAnalyzedTransactions.offer(transactionViewModel.getTrunkTransactionHash());
					nonAnalyzedTransactions.offer(transactionViewModel.getBranchTransactionHash());
				}
			}
		}
	}
        return true;
}

private synchronized AbstractResponse findTransactionStatement( Map<String, Object> request) 
{
	 Set<Hash> foundTransactions =  new HashSet<>();
        boolean containsKey = false;

 Set<Hash> bundlesTransactions = new HashSet<>();
        if (request.containsKey("bundles")) {
             HashSet<String> bundles = getParameterAsSet(request, "bundles", HASH_SIZE);
            for ( String bundle : bundles) {
                bundlesTransactions.addAll(BundleViewModel.load(instance.tangle, new Hash(bundle)).getHashes());
            }
            foundTransactions.addAll(bundlesTransactions);
            containsKey = true;
        }

         Set<Hash> addressesTransactions = new HashSet<>();
        if (request.containsKey("addresses")) {
             HashSet<String> addresses = getParameterAsSet(request, "addresses", HASH_SIZE);
            for ( String address : addresses) {
                addressesTransactions.addAll(AddressViewModel.load(instance.tangle, new Hash(address)).getHashes());
            }
            foundTransactions.addAll(addressesTransactions);
            containsKey = true;
        }

         Set<Hash> tagsTransactions = new HashSet<>();
        if (request.containsKey("tags")) {
             HashSet<String> tags = getParameterAsSet(request, "tags", 0);
            for (String tag : tags) {
                tag = padTag(tag);
tagsTransactions.addAll(TagViewModel.load(instance.tangle, new Hash(tag)).getHashes());
            }
            foundTransactions.addAll(tagsTransactions);
            containsKey = true;
        }

         Set<Hash> approveeTransactions = new HashSet<>();

        if (request.containsKey("approvees")) {
             HashSet<String> approvees = getParameterAsSet(request, "approvees", HASH_SIZE);
            for ( String approvee : approvees) {
                approveeTransactions.addAll(TransactionViewModel.fromHash(instance.tangle, new Hash(approvee)).getApprovers(instance.tangle).getHashes());
            }
            foundTransactions.addAll(approveeTransactions);
            containsKey = true;
        }

        if (!containsKey) {
            throw new ValidationException(invalidParams);
        }

        //Using multiple of these input fields returns the intersection of the values.
        if (request.containsKey("bundles")) {
            foundTransactions.retainAll(bundlesTransactions);
        }
        if (request.containsKey("addresses")) {
            foundTransactions.retainAll(addressesTransactions);
        }
        if (request.containsKey("tags")) {
            foundTransactions.retainAll(tagsTransactions);
        }
        if (request.containsKey("approvees")) {
            foundTransactions.retainAll(approveeTransactions);
        }
        if (foundTransactions.size() > maxFindTxs){
            return ErrorResponse.create(overMaxErrorMessage);
        }

         List<String> elements = foundTransactions.stream()
				.map(Hash::toString)
				.collect(Collectors.toCollection(LinkedList::new));

        return FindTransactionsResponse.create(elements);
    }

    private String padTag(String tag) 
{
        while (tag.length() < HASH_SIZE) {
		tag += Converter.TRYTE_ALPHABET.charAt(0);
	}
        if (tag.equals(Hash.NULL_HASH.toString())) {
		throw new ValidationException("Invalid tag input");
	}
        return tag;
}

private HashSet<String> getParameterAsSet(Map<String, Object> request, String paramName, int size) 
{

	HashSet<String> result = getParameterAsList(request,paramName,size).stream().collect(Collectors.toCollection(HashSet::new));
        if (result.contains(Hash.NULL_HASH.toString())) {
            throw new ValidationException("Invalid " + paramName + " input");
        }
        return result;
    }

    public void broadcastTransactionStatement( List<String> trytes2)
{
	for ( String tryte : trytes2)
	{
		//validate PoW - throws exception if invalid
		 TransactionViewModel transactionViewModel = instance.transactionValidator.validate(Converter.trits(tryte), instance.transactionValidator.getMinWeightMagnitude());
		//push first in line to broadcast
		transactionViewModel.weightMagnitude = Curl.HASH_LENGTH;
		instance.node.broadcast(transactionViewModel);
	}
}

private AbstractResponse getBalancesStatement( List<String> addrss,  int threshold)
{

        if (threshold <= 0 || threshold > 100) {
		return ErrorResponse.create("Illegal 'threshold'");
	}

	 List<Hash> addresses = addrss.stream().map(address -> (new Hash(address)))
                .collect(Collectors.toCollection(LinkedList::new));

         Map<Hash, Long> balances = new HashMap<>();
 int index;

		synchronized(Snapshot.latestSnapshotSyncObject)
{
	index = instance.latestSnapshot.index();
	for ( Hash address : addresses)
	{
		balances.put(address,
				instance.latestSnapshot.getState().containsKey(address) ?
						instance.latestSnapshot.getState().get(address) : Long.valueOf(0));
	}
}

 Hash milestone = instance.milestone.latestSolidSubtangleMilestone;
         int milestoneIndex = instance.milestone.latestSolidSubtangleMilestoneIndex;


Set<Hash> analyzedTips = new HashSet<>();

 Queue<Hash> nonAnalyzedTransactions = new LinkedList<>(Collections.singleton(milestone));
Hash hash;
        while ((hash = nonAnalyzedTransactions.poll()) != null) {

            if (analyzedTips.add(hash)) {

                 TransactionViewModel transactionViewModel = TransactionViewModel.fromHash(instance.tangle, hash);

                if(transactionViewModel.snapshotIndex() == 0 || transactionViewModel.snapshotIndex() > index) {
                    if (transactionViewModel.value() != 0) {

                         Hash address = transactionViewModel.getAddressHash();
                         Long balance = balances.get(address);
                        if (balance != null) {

                            balances.put(address, balance + transactionViewModel.value());
                        }
                    }
                    nonAnalyzedTransactions.offer(transactionViewModel.getTrunkTransactionHash());
                    nonAnalyzedTransactions.offer(transactionViewModel.getBranchTransactionHash());
                }
            }
        }
         List<String> elements = addresses.stream().map(address->balances.get(address).toString())
				.collect(Collectors.toCollection(LinkedList::new));

        return GetBalancesResponse.create(elements, milestone, milestoneIndex);
    }

    private static int counter_PoW = 0;
public static int getCounter_PoW()
{
	return counter_PoW;
}
public static void incCounter_PoW()
{
	API.counter_PoW++;
}

private static long ellapsedTime_PoW = 0L;
public static long getEllapsedTime_PoW()
{
	return ellapsedTime_PoW;
}
public static void incEllapsedTime_PoW(long ellapsedTime)
{
	ellapsedTime_PoW += ellapsedTime;
}

public List<String> attachToTangleStatement( Hash trunkTransaction,  Hash branchTransaction,
															   int minWeightMagnitude,  List<String> trytes)
{
	 List<TransactionViewModel> transactionViewModels = new LinkedList<>();

	Hash prevTransaction = null;
	pearlDiver = new PearlDiver();

	for ( String tryte : trytes)
	{
		long startTime = System.nanoTime();
		long timestamp = System.currentTimeMillis();
		try
		{
			 int[] transactionTrits = Converter.trits(tryte);
			//branch and trunk
			System.arraycopy((prevTransaction == null ? trunkTransaction : prevTransaction).trits(), 0,
					transactionTrits, TransactionViewModel.TRUNK_TRANSACTION_TRINARY_OFFSET,
					TransactionViewModel.TRUNK_TRANSACTION_TRINARY_SIZE);
			System.arraycopy((prevTransaction == null ? branchTransaction : trunkTransaction).trits(), 0,
					transactionTrits, TransactionViewModel.BRANCH_TRANSACTION_TRINARY_OFFSET,
					TransactionViewModel.BRANCH_TRANSACTION_TRINARY_SIZE);

			//attachment fields: tag and timestamps
			//tag - copy the obsolete tag to the attachment tag field only if tag isn't set.
			if (Arrays.stream(transactionTrits, TransactionViewModel.TAG_TRINARY_OFFSET, TransactionViewModel.TAG_TRINARY_OFFSET + TransactionViewModel.TAG_TRINARY_SIZE).allMatch(s->s == 0))
			{
				System.arraycopy(transactionTrits, TransactionViewModel.OBSOLETE_TAG_TRINARY_OFFSET,
					transactionTrits, TransactionViewModel.TAG_TRINARY_OFFSET,
					TransactionViewModel.TAG_TRINARY_SIZE);
			}

			Converter.copyTrits(timestamp, transactionTrits, TransactionViewModel.ATTACHMENT_TIMESTAMP_TRINARY_OFFSET,
					TransactionViewModel.ATTACHMENT_TIMESTAMP_TRINARY_SIZE);
			Converter.copyTrits(0, transactionTrits, TransactionViewModel.ATTACHMENT_TIMESTAMP_LOWER_BOUND_TRINARY_OFFSET,
					TransactionViewModel.ATTACHMENT_TIMESTAMP_LOWER_BOUND_TRINARY_SIZE);
			Converter.copyTrits(MAX_TIMESTAMP_VALUE, transactionTrits, TransactionViewModel.ATTACHMENT_TIMESTAMP_UPPER_BOUND_TRINARY_OFFSET,
					TransactionViewModel.ATTACHMENT_TIMESTAMP_UPPER_BOUND_TRINARY_SIZE);

			if (!pearlDiver.search(transactionTrits, minWeightMagnitude, 0))
			{
				transactionViewModels.clear();
				break;
			}
			//validate PoW - throws exception if invalid
			 TransactionViewModel transactionViewModel = instance.transactionValidator.validate(transactionTrits, instance.transactionValidator.getMinWeightMagnitude());

			transactionViewModels.add(transactionViewModel);
			prevTransaction = transactionViewModel.getHash();
		}
		finally
		{
			API.incEllapsedTime_PoW(System.nanoTime() - startTime);
			API.incCounter_PoW();
			if ((API.getCounter_PoW() % 100) == 0)
			{
				String sb = "Last 100 PoW consumed " +
						API.getEllapsedTime_PoW() / 1000000000L +
						" seconds processing time.";
				log.info(sb);
				counter_PoW = 0;
				ellapsedTime_PoW = 0L;
			}
		}
	}

	 List<String> elements = new LinkedList<>();
	for (int i = transactionViewModels.size(); i-- > 0;)
	{
		elements.add(Converter.trytes(transactionViewModels.get(i).trits()));
	}
	return elements;
}

private AbstractResponse addNeighborsStatement( List<String> uris)
{

        int numberOfAddedNeighbors = 0;
        for ( String uriString : uris) {
		 URI uri = new URI(uriString);

		if ("udp".equals(uri.getScheme()) || "tcp".equals(uri.getScheme()))
		{
			log.info("Adding neighbor: " + uriString);
			// 3rd parameter true if tcp, 4th parameter true (configured tethering)
			 Neighbor neighbor;
			switch (uri.getScheme())
			{
				case "tcp":
					neighbor = new TCPNeighbor(new InetSocketAddress(uri.getHost(), uri.getPort()), true);
					break;
				case "udp":
					neighbor = new UDPNeighbor(new InetSocketAddress(uri.getHost(), uri.getPort()), instance.node.getUdpSocket(), true);
					break;
				default:
					return ErrorResponse.create("Invalid uri scheme");
			}
			if (!instance.node.getNeighbors().contains(neighbor))
			{
				instance.node.getNeighbors().add(neighbor);
				numberOfAddedNeighbors++;
			}
		}
		else
		{
			return ErrorResponse.create("Invalid uri scheme");
		}
	}
        return AddedNeighborsResponse.create(numberOfAddedNeighbors);
}

private void sendResponse( HttpServerExchange exchange,  AbstractResponse res,  long beginningTime)

			throws IOException
{
	res.setDuration((int) (System.currentTimeMillis() - beginningTime));
	 String response = gson.toJson(res);

        if (res instanceof ErrorResponse) {
		exchange.setStatusCode(400); // bad request
	} else if (res instanceof AccessLimitedResponse) {
		exchange.setStatusCode(401); // api method not allowed
	} else if (res instanceof ExceptionResponse) {
		exchange.setStatusCode(500); // internal error
	}

	setupResponseHeaders(exchange);

	ByteBuffer responseBuf = ByteBuffer.wrap(response.getBytes(StandardCharsets.UTF_8));
	exchange.setResponseContentLength(responseBuf.array().length);
	StreamSinkChannel sinkChannel = exchange.getResponseChannel();
	sinkChannel.getWriteSetter().set(channel-> {
		if (responseBuf.remaining() > 0)
			try
			{
				sinkChannel.write(responseBuf);
				if (responseBuf.remaining() == 0)
				{
					exchange.endExchange();
				}
			}
			catch (IOException e)
			{
				log.error("Lost connection to client - cannot send response");
				exchange.endExchange();
				sinkChannel.getWriteSetter().set(null);
			}
		else
		{
			exchange.endExchange();
		}
	});
	sinkChannel.resumeWrites();
	}


	private boolean validTrytes(String trytes, int length, char zeroAllowed)
{
	if (trytes.length() == 0 && zeroAllowed == ZERO_LENGTH_ALLOWED)
	{
		return true;
	}
	if (trytes.length() != length)
	{
		return false;
	}
	Matcher matcher = trytesPattern.matcher(trytes);
	return matcher.matches();
}

private static void setupResponseHeaders( HttpServerExchange exchange)
{
	 HeaderMap headerMap = exchange.getResponseHeaders();
	headerMap.add(new HttpString("Access-Control-Allow-Origin"), "*");
	headerMap.add(new HttpString("Keep-Alive"), "timeout=500, max=100");
}

private HttpHandler addSecurity( HttpHandler toWrap)
{
	String credentials = instance.configuration.string(DefaultConfSettings.REMOTE_AUTH);
	if (credentials == null || credentials.isEmpty()) return toWrap;

	 Map<String, char[]> users = new HashMap<>(2);
	users.put(credentials.split(":")[0], credentials.split(":")[1].toCharArray());

	IdentityManager identityManager = new MapIdentityManager(users);
	HttpHandler handler = toWrap;
	handler = new AuthenticationCallHandler(handler);
	handler = new AuthenticationConstraintHandler(handler);
	 List<AuthenticationMechanism> mechanisms = Collections.singletonList(new BasicAuthenticationMechanism("Iota Realm"));
	handler = new AuthenticationMechanismsHandler(handler, mechanisms);
	handler = new SecurityInitialHandler(AuthenticationMode.PRO_ACTIVE, identityManager, handler);
	return handler;
}

public void shutDown()
{
	if (server != null)
	{
		server.stop();
	}
}
}
}
