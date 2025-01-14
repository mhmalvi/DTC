namespace DTCBillingSystem.Core.Interfaces
{
    public interface IHttpContextService
    {
        string GetClientIpAddress();
        string GetUserAgent();
        string GetRequestPath();
        string GetRequestMethod();
        string GetCorrelationId();
    }
} 