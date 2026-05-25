namespace LearnEnglish.Application.Interfaces
{
    /// <summary>
    /// 收藏服务接口
    /// </summary>
    public interface IFavoriteService
    {
        /// <summary>
        /// 收藏/取消收藏单词
        /// </summary>
        Task SetCollectAsync(int userId, int lexiconId, int isCollect);

        /// <summary>
        /// 获取用户收藏单词数量
        /// </summary>
        Task<int> GetFavoriteCountAsync(int userId);
    }
}
