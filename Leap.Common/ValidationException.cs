namespace Leap.Common;

public class ValidationException(string message) : InvalidOperationException(message);
