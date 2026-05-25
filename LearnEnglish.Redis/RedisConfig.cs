using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Diagnostics.SymbolStore;
namespace LearnEnglish.Redis
{
    public class RedisConfig
    {
        #region Redis配置


        //配置实体
        private readonly RedisOption _option;
        //Redis连接字符串
        private readonly ConcurrentDictionary<string, ConnectionMultiplexer> _connections;

        //在初始化中注入配置实体，以及实例化Redis连接
        public RedisConfig(RedisOption option)
        {
            _option = option;
            _connections = new ConcurrentDictionary<string, ConnectionMultiplexer>();
        }

        //根据配置文件实例化Redis连接
        private ConnectionMultiplexer GetConnection()
        {
            return _connections.GetOrAdd(_option.InstanceName,
                p => ConnectionMultiplexer.Connect(_option.Connection));
        }

        //根据配置文件创建数据库
        public IDatabase GetDatabase()
        {
            return GetConnection().GetDatabase(_option.DefaultDb);
        }

        #endregion Redis配置


        #region Redis中的一些API
        /// <summary>
        /// 设置key的过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool Expire(string key, TimeSpan time)
        {
            try
            {
                GetDatabase().KeyExpire(key, time);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public TimeSpan GetExpire(string key)
        {
            try
            {
                // ReSharper disable once PossibleInvalidOperationException
                TimeSpan time = GetDatabase().KeyTimeToLive(key).Value;
                return time;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        /// <summary>
        /// 写入缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Set(string key, RedisValue value)
        {
            bool result = false;
            try
            {
                result = GetDatabase().StringSet(key, value);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return result;
        }

        /// <summary>
        /// 写入缓存Hash
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fieId"></param>
        /// <param name="value"></param>
        public void SaveHashSet(string key,string field, string value)
        {
            try
            {
                GetDatabase().HashSet(key, field, value);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// 获取某一条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public RedisValue GetHash(string key,string field)
        {
            var result = GetDatabase().HashGet(key,field);
            return result;
        }

        /// <summary>
        /// 获key对应的所有数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public HashEntry[] GetAllHash(string key)
        {
           return GetDatabase().HashGetAll(key);
        }

        /// <summary>
        /// /删除某条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public bool DeleteHash(string key,string field)
        {
            return GetDatabase().HashDelete(key, field);
        }

        /// <summary>
        /// 写入缓存HashSet
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public void SetHashSet(string key, HashEntry[] value)
        {
           
            try
            {
               GetDatabase().HashSet(key, value);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// 写入缓存并设置过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expireTime"></param>
        /// <returns></returns>
        public bool Set(string key, string value, TimeSpan expireTime)
        {
            Console.WriteLine($"过期时间:{expireTime}");
            bool result = false;
            try
            {
                result = GetDatabase().StringSet(key, value);
                GetDatabase().KeyExpire(key, expireTime);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return result;
        }

        public RedisValue Get(string key)
        {
            return key == null ? "" : GetDatabase().StringGet(key);
        }

        public RedisValue HashGet(string hashKey, string key)
        {
            return key == null ? "" : GetDatabase().HashGet(hashKey, key);;
        }
        

        /// <summary>
        /// 递增
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public long Incr(string key, long value)
        {
            return GetDatabase().StringIncrement(key, value);
        }

        public long Incr(string key)
        {
            return GetDatabase().StringIncrement(key);
        }

        /// <summary>
        /// 递减
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public long Decr(string key, long value)
        {
            return GetDatabase().StringIncrement(key, value);
        }

        public long Decr(string key)
        {
            return GetDatabase().StringDecrement(key);
        }

        public RedisValue Hget(string key, string item)
        {
            return GetDatabase().HashGet(key, item);
        }

        /// <summary>
        /// 加锁，如果锁定成功，就去执行方法
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expire"></param>
        /// <returns></returns>
        public bool TryGetLock(string key, string value, TimeSpan expire)
        {
            return GetDatabase().LockTake(key, value, expire);
        }

        /// <summary>
        /// 解锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool LockRelease(string key, string value)
        {
            return GetDatabase().LockRelease(key, value);
        }

        public bool Remove(string key)
        {
            return GetDatabase().KeyDelete(key);
        }

        #endregion Redis中的一些API
    }
}