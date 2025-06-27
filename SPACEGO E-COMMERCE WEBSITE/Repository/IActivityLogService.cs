namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public interface IActivityLogService
    {
         Task LogAsync(string userId, string userName, string actionType, string tableName, string objectId, string description);

    }
}
