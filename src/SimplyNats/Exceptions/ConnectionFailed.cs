using System;

namespace SimplyNats.Exceptions
{
  public class ConnectionFailed : AggregateException
  {
    public ConnectionFailed(Exception innerException) : base(innerException) 
    {
    }
  }
}