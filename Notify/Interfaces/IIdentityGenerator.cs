namespace Notify.Interfaces
{
    public interface IIdentityGenerator
    {
        string GetRandomString(int length);
        string GetIDentityString(int length = 4);
    }
}
