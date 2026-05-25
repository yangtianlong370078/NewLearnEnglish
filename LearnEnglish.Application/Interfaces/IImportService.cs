namespace LearnEnglish.Application.Interfaces
{
    /// <summary>
    /// 数据导入服务接口
    /// </summary>
    public interface IImportService
    {
        /// <summary>
        /// 从 Excel 文件导入课程数据
        /// </summary>
        /// <returns>成功导入的记录数</returns>
        Task<int> ImportFromExcelAsync(int userId, int courseId, Stream fileStream, string fileName);
    }
}
