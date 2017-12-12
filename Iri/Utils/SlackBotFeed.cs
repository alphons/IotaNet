using System;
using System.IO;
using System.Text;
using System.Net;

namespace IotaNet.Iri.Utils
{
	public class SlackBotFeed
	{
		public static string reportToSlack(String message)
		{
			try
			{
				String request = "token="
					   + WebUtility.UrlEncode("<botToken>") + "&channel="
					   + WebUtility.UrlEncode("#botbox") + "&text=" + WebUtility.UrlEncode(message) + "&as_user=true";

				var data = Encoding.UTF8.GetBytes(request);

				var connection = (HttpWebRequest)WebRequest.Create("https://slack.com/api/chat.postMessage");
				connection.ContentType = "application/x-www-form-urlencoded";
				connection.Method = "POST";
				connection.ContentLength = data.Length;
				using (var stream = connection.GetRequestStream())
				{
					stream.Write(data, 0, data.Length);
				}

				var response = (HttpWebResponse)connection.GetResponse();

				var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

				return responseString;
			}
			catch (Exception e)
			{
				return "Error: " + e.Message;
			}

		}
	}
}


