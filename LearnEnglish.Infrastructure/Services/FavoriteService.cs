using LearnEnglish.Application.Interfaces;
using LearnEnglish.Infrastructure.Repositories;

namespace LearnEnglish.Infrastructure.Services
{
    /// <summary>
    /// 收藏服务实现
    /// </summary>
    public class FavoriteService : IFavoriteService
    {
        private readonly IMyLexiconRepository _myLexiconRepository;

        public FavoriteService(IMyLexiconRepository myLexiconRepository)
        {
            _myLexiconRepository = myLexiconRepository;
        }

        /// <inheritdoc/>
        public async Task SetCollectAsync(int userId, int lexiconId, int isCollect)
        {
            await _myLexiconRepository.SetCollectAsync(userId, lexiconId, isCollect);
        }

        /// <inheritdoc/>
        public async Task<int> GetFavoriteCountAsync(int userId)
        {
            return await _myLexiconRepository.GetFavoriteCountAsync(userId);
        }
    }
}
