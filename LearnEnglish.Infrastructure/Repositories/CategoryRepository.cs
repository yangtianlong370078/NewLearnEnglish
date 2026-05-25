using Dapper;
using LearnEnglish.Domain.Entities;
using LearnEnglish.Infrastructure.Data;

namespace LearnEnglish.Infrastructure.Repositories
{
    /// <summary>
    /// 分类 Repository 实现
    /// </summary>
    public class CategoryRepository : DapperRepository<Category>, ICategoryRepository
    {
        protected override string TableName => "category";

        public CategoryRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public new async Task<Category?> GetByIdAsync(int id)
        {
            return await base.GetByIdAsync(id);
        }

        public new async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await base.GetAllAsync();
        }
    }
}
