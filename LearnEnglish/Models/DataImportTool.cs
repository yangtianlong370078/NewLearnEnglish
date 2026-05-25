using MySql.Data.MySqlClient;
using System.Reflection;
using System.Text;

namespace LearnEnglish.Models
{
    /// <summary>
    /// Mysql大文件导入工具类
    /// </summary>
    public class DataImportTool
    {
        /// <summary>
        /// 数据库连接
        /// </summary>
        //MySqlConnection _connection = null;
        //MySqlTransaction _transaction=null;



        /// <summary>
        /// 属性信息
        /// </summary>
        List<TypeInfo> _typeInfos = null;
        public DataImportTool()
        {

            _typeInfos = new List<TypeInfo>();


        }

        /// <summary>
        ///  批量导入数据
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="_connection">连接必传</param>
        /// <param name="_transaction">有事务必传，无事务传null</param>
        /// <param name="datas">数据</param>
        /// <param name="tableName">表名</param>
        /// <param name="isAddTable">是不创建表</param>
        /// <returns></returns>
        public int InsertDatas<T>(MySqlConnection _connection, ref MySqlTransaction _transaction, List<T> datas, string tableName, bool isAddTable = false) where T : class
        {
            bool isTransaction = false;
            if (_transaction != null)
            {
                try
                {
                    isTransaction = true;
                    _transaction.Commit();
                }
                catch
                {
                }
                finally
                {
                    _transaction = null;
                }
            }
            MySqlTransaction transaction = _connection.BeginTransaction();

            Type type = typeof(T);
            List<PropertyInfo> fields = new List<PropertyInfo>();

            // 循环获取属性名称和属性类型
            foreach (var field in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var keyAttribute = field.GetCustomAttribute<KeyAttribute>();

                var isEffectiveAttribute = field.GetCustomAttribute<IsEffectiveAttribute>();
                if (isEffectiveAttribute == null || isEffectiveAttribute.IsEffectiveIs)
                {
                    var typeInfo = new TypeInfo
                    {
                        Name = field.Name,
                        //Value = field.PropertyType.IsValueType ? Activator.CreateInstance(field.PropertyType) : "''",
                        KeyName = keyAttribute != null ? keyAttribute.KeyName : string.Empty,
                        PropertyType = field.PropertyType,
                    };

                    MapCsharpTypeToMysql(typeInfo);
                    _typeInfos.Add(typeInfo);
                    fields.Add(field);
                }
            }

            try
            {
                //创建表
                if (isAddTable)
                {
                    StringBuilder sql = new StringBuilder();
                    sql.Append("CREATE TEMPORARY TABLE ");
                    sql.Append(tableName);
                    sql.Append(" (");

                    bool isFirst = true;
                    var keyNames = new List<string>();
                    foreach (var item in _typeInfos)
                    {
                        if (!isFirst)
                            sql.Append(", ");
                        isFirst = false;

                        string propertyName = item.Name;
                        Type propertyType = item.PropertyType;
                        string sqlType = item.SqlType;

                        sql.Append($"`{propertyName}` {sqlType}");

                        if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            sql.Append(" NULL");
                        }
                        else
                        {
                            //  sql.Append(" NOT NULL");

                            sql.Append($" DEFAULT {item.Value} ");
                        }
                        if (!string.IsNullOrWhiteSpace(item.KeyName))
                        {
                            keyNames.Add(item.KeyName);
                        }

                    }

                    foreach (var item in keyNames)
                    {
                        sql.Append($" ,KEY `key_{item}` (`{item}`) USING BTREE ");
                    }

                    sql.Append(") ENGINE=InnoDB CHARSET=utf8;");

                    var createsql = sql.ToString();

                    using (MySqlCommand cmd = new MySqlCommand(createsql, _connection, transaction))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                //插入数据
                string InsertSql = $"INSERT INTO {tableName} ({string.Join(",", _typeInfos.Select(a => string.Format("`{0}`", a.Name)))}) VALUES ({string.Join(",", _typeInfos.Select(a => string.Format("@{0}", a.Name)))})";
                using (MySqlCommand cmd = new MySqlCommand(InsertSql, _connection, transaction))
                {

                    foreach (var field in _typeInfos)
                    {
                        cmd.Parameters.AddWithValue($"@{field.Name}", field.Value);
                    }

                    int rowsAffected = 0;
                    foreach (var book in datas)
                    {
                        foreach (var field in fields)
                        {
                            cmd.Parameters[$"@{field.Name}"].Value = field.GetValue(book);
                        }

                        rowsAffected += cmd.ExecuteNonQuery();

                        // 根据需要调整批处理大小
                        if (rowsAffected % 2000 == 0)
                        {
                            transaction.Commit();
                            transaction = _connection.BeginTransaction();
                            cmd.Transaction = transaction;
                        }
                    }
                    // 提交剩余的事务
                    transaction.Commit();

                    return datas.Count();
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                transaction = null;
                if (isTransaction)
                {
                    _transaction = _connection.BeginTransaction();
                }
            }
        }

        private static void MapCsharpTypeToMysql(TypeInfo typeInfo)
        {
            Type type = typeInfo.PropertyType;
            if (type == typeof(int))
            {
                typeInfo.SqlType = "INT";
                typeInfo.Value = 0;
            }
            if (type == typeof(string))
            {
                typeInfo.SqlType = "VARCHAR(255)";
                typeInfo.Value = "''";
            }// Adjust the length as needed  
            if (type == typeof(DateTime))
            {
                typeInfo.SqlType = "DATETIME";
                typeInfo.Value = DateTime.MinValue;
            }
            if (type == typeof(bool))
            {
                typeInfo.SqlType = "TINYINT(1)";
                typeInfo.Value = 0;
            }
            if (type == typeof(decimal))
            {
                typeInfo.SqlType = "decimal(18,2)";
                typeInfo.Value = 0.00;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                //  MapCsharpTypeToMysql(Nullable.GetUnderlyingType(type)); // Handle nullable types  
            }

            // Add more mappings as needed for other types  
            // throw new NotImplementedException($"Type {type.Name} is not mapped to a MySQL type.");
        }

    }
    /// <summary>
    /// 属性类型信息
    /// </summary>
    public class TypeInfo
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 属性类型默认值
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 索引字段
        /// </summary>
        public string KeyName { get; set; }

        /// <summary>
        /// 字段类型
        /// </summary>
        public Type PropertyType { get; set; }

        /// <summary>
        /// mysql字段类型
        /// </summary>
        public string SqlType { get; set; }
    }
}
