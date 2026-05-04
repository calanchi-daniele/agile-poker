namespace AgilePoker.Api.Exceptions;

public class InvalidVoteException : Exception
{
    public InvalidVoteException(string message) : base(message)
    {
    }
}