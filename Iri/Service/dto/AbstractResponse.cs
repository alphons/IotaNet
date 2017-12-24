using System;

namespace IotaNet.Iri.Service.dto
{
	public abstract class AbstractResponse
	{
		private static class Emptyness : AbstractResponse { }

		private int duration;

		public String toString()
		{
			return ToStringBuilder.reflectionToString(this, ToStringStyle.MULTI_LINE_STYLE);
		}

		public int hashCode()
		{
			return HashCodeBuilder.reflectionHashCode(this, false);
		}

		public bool equals(Object obj)
		{
			return EqualsBuilder.reflectionEquals(this, obj, false);
		}

		public int getDuration()
		{
			return duration;
		}

		public void setDuration(int duration)
		{
			this.duration = duration;
		}

		public static AbstractResponse createEmptyResponse()
		{
			return new Emptyness();
		}
	}
}
