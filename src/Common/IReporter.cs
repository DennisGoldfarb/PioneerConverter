namespace PioneerConverter.Core.Common;

public interface IReporter
{
    void WriteLine(string message);
    void WriteLine(string format, params object[] args);
}
