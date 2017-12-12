using System;
using System.Text;
using System.Collections.Generic;

using IotaNet.Iri.Log;
using IotaNet.Iri.IniFile;

namespace IotaNet.Iri.Conf
{
	public class Configuration
	{
		private Ini ini;
		private Preferences prefs;

		private Logger log = LoggerFactory.getLogger<Configuration>();

		private Dictionary<String, String> conf = new Dictionary<string, string>();

		public enum DefaultConfSettings
		{
			CONFIG,
			PORT,
			API_HOST,
			UDP_RECEIVER_PORT,
			TCP_RECEIVER_PORT,
			TESTNET,
			DEBUG,
			REMOTE_LIMIT_API,
			REMOTE_AUTH,
			NEIGHBORS,
			IXI_DIR,
			DB_PATH,
			DB_LOG_PATH,
			DB_CACHE_SIZE,
			P_REMOVE_REQUEST,
			P_DROP_TRANSACTION,
			P_SELECT_MILESTONE_CHILD,
			P_SEND_MILESTONE,
			P_REPLY_RANDOM_TIP,
			P_PROPAGATE_REQUEST,
			MAIN_DB, EXPORT, // exports transaction trytes to filesystem
			SEND_LIMIT,
			MAX_PEERS,
			DNS_REFRESHER_ENABLED,
			COORDINATOR,
			REVALIDATE,
			RESCAN_DB,
			MIN_RANDOM_WALKS,
			MAX_RANDOM_WALKS,
			MAX_FIND_TRANSACTIONS,
			MAX_REQUESTS_LIST,
			MAX_GET_TRYTES,
			MAX_BODY_LENGTH,
			MAX_DEPTH,
			MAINNET_MWM,
			TESTNET_MWM,
			ZMQ_ENABLED,
			ZMQ_PORT,
			ZMQ_IPC,
			ZMQ_THREADS,
		}

		public Configuration()
		{
			// defaults
			conf.Add(DefaultConfSettings.PORT.ToString(), "14600");
			conf.Add(DefaultConfSettings.API_HOST.ToString(), "localhost");
			conf.Add(DefaultConfSettings.UDP_RECEIVER_PORT.ToString(), "14600");
			conf.Add(DefaultConfSettings.TCP_RECEIVER_PORT.ToString(), "15600");
			conf.Add(DefaultConfSettings.TESTNET.ToString(), "false");
			conf.Add(DefaultConfSettings.DEBUG.ToString(), "false");
			conf.Add(DefaultConfSettings.REMOTE_LIMIT_API.ToString(), "");
			conf.Add(DefaultConfSettings.REMOTE_AUTH.ToString(), "");
			conf.Add(DefaultConfSettings.NEIGHBORS.ToString(), "");
			conf.Add(DefaultConfSettings.IXI_DIR.ToString(), "ixi");
			conf.Add(DefaultConfSettings.DB_PATH.ToString(), "mainnetdb");
			conf.Add(DefaultConfSettings.DB_LOG_PATH.ToString(), "mainnet.log");
			conf.Add(DefaultConfSettings.DB_CACHE_SIZE.ToString(), "100000"); //KB
			conf.Add(DefaultConfSettings.CONFIG.ToString(), "iota.ini");
			conf.Add(DefaultConfSettings.P_REMOVE_REQUEST.ToString(), "0.01");
			conf.Add(DefaultConfSettings.P_DROP_TRANSACTION.ToString(), "0.0");
			conf.Add(DefaultConfSettings.P_SELECT_MILESTONE_CHILD.ToString(), "0.7");
			conf.Add(DefaultConfSettings.P_SEND_MILESTONE.ToString(), "0.02");
			conf.Add(DefaultConfSettings.P_REPLY_RANDOM_TIP.ToString(), "0.66");
			conf.Add(DefaultConfSettings.P_PROPAGATE_REQUEST.ToString(), "0.01");
			conf.Add(DefaultConfSettings.MAIN_DB.ToString(), "rocksdb");
			conf.Add(DefaultConfSettings.EXPORT.ToString(), "false");
			conf.Add(DefaultConfSettings.SEND_LIMIT.ToString(), "-1.0");
			conf.Add(DefaultConfSettings.MAX_PEERS.ToString(), "0");
			conf.Add(DefaultConfSettings.DNS_REFRESHER_ENABLED.ToString(), "true");
			conf.Add(DefaultConfSettings.REVALIDATE.ToString(), "false");
			conf.Add(DefaultConfSettings.RESCAN_DB.ToString(), "false");
			conf.Add(DefaultConfSettings.MAINNET_MWM.ToString(), "14");
			conf.Add(DefaultConfSettings.TESTNET_MWM.ToString(), "13");

			// Pick a number based on best performance
			conf.Add(DefaultConfSettings.MIN_RANDOM_WALKS.ToString(), "5");
			conf.Add(DefaultConfSettings.MAX_RANDOM_WALKS.ToString(), "27");
			// Pick a milestone depth number depending on risk model
			conf.Add(DefaultConfSettings.MAX_DEPTH.ToString(), "15");

			conf.Add(DefaultConfSettings.MAX_FIND_TRANSACTIONS.ToString(), "100000");
			conf.Add(DefaultConfSettings.MAX_REQUESTS_LIST.ToString(), "1000");
			conf.Add(DefaultConfSettings.MAX_GET_TRYTES.ToString(), "10000");
			conf.Add(DefaultConfSettings.MAX_BODY_LENGTH.ToString(), "1000000");
			conf.Add(DefaultConfSettings.ZMQ_ENABLED.ToString(), "false");
			conf.Add(DefaultConfSettings.ZMQ_PORT.ToString(), "5556");
			conf.Add(DefaultConfSettings.ZMQ_IPC.ToString(), "ipc://iri");
			conf.Add(DefaultConfSettings.ZMQ_THREADS.ToString(), "2");
		}

		public bool init()
		{
			var confFile = Configuration.DefaultConfSettings.CONFIG.ToString();
			if (System.IO.File.Exists(confFile))
			{
				ini = new Ini(confFile);
				prefs = new IniPreferences(ini);
				return true;
			}
			return false;
		}

		public String getIniValue(String k)
		{
			if (ini != null)
			{
				return prefs.node("IRI").get(k, null);
			}
			return null;
		}

		private String getConfValue(String k)
		{
			String value = getIniValue(k);
			return value == null ? conf[k] : value;
		}

		public String allSettings()
		{
			StringBuilder settings = new StringBuilder();
			foreach (var t in conf.Keys)
			{
				settings.Append("Set '" + t + "'\t -> " + getConfValue(t) + Environment.NewLine);
			}
			return settings.ToString();
		}

		public void put(String k, String v)
		{
			log.debug("Setting {0} with {1}", k, v);
			conf[k] = v;
		}

		public void put(DefaultConfSettings d, String v)
		{
			log.debug("Setting {0} with {1}", d.ToString(), v);
			conf[d.ToString()] = v;
		}

		private String get(String k)
		{
			return getConfValue(k);
		}

		public float floating(String k)
		{
			return float.Parse(getConfValue(k));
		}

		public double doubling(String k)
		{
			return Double.Parse(getConfValue(k));
		}

		private int integer(String k)
		{
			return int.Parse(getConfValue(k));
		}

		private bool booling(String k)
		{
			return Boolean.Parse(getConfValue(k));
		}

		public String get(DefaultConfSettings d)
		{
			return get(d.ToString());
		}

		public int integer(DefaultConfSettings d)
		{
			return integer(d.ToString());
		}

		public bool booling(DefaultConfSettings d)
		{
			return booling(d.ToString());
		}
	}
}
