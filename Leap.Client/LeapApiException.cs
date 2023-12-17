namespace Leap.Client;

public class LeapApiException(string message, Exception inner) : Exception(message, inner);
