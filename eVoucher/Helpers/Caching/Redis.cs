using Microsoft.Extensions.Configuration;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.Helpers.Caching
{
    public class Redis
    {
        private IConfiguration configuration;
        public Redis(IConfiguration config)
        {
            configuration = config;
        }
        public bool SetRedisValue(string key, string value)
        {
            var result = false;
            try
            {
                var client = RedisClient();
                client.Db = Get_RedisDB();
                if (client.Exists(key) > 0)
                {
                    result = client.SetValueIfExists(key, value);
                }
                else
                {
                    result = client.SetValueIfNotExists(key, value);
                }
            }
            catch (Exception)
            {               
            }

            return result;
        }
        public string GetValue(string key)
        {

            var response = "";
            try
            {
                var client = RedisClient();
                client.Db = Get_RedisDB();
                response = client.GetValue(key);
            }
            catch (Exception)
            {
            }
            return response;
        }
        public bool Remove(string key)
        {
            var client = RedisClient();
            client.Db = Get_RedisDB();
            return client.Remove(key);
        }
        public long Exists(string key)
        {
            long response = 0;
            try
            {
                var client = RedisClient();
                response = client.Exists(key);
            }
            catch (Exception)
            {
               
            }
            return response;
        }
        public string CheckAndGetCache(string key)
        {
            var response = "";
            try
            {
                var client = RedisClient();
                client.Db = Get_RedisDB();
                if (client.Exists(key) > 0)
                {
                    response = client.GetValue(key);
                }
            }
            catch (Exception)
            {
             
            }
            
            return response;
        }
        public List<string> GetALL()
        {
            var result = new List<string>();
            try
            {
                var client = RedisClient();
                client.Db = Get_RedisDB();
                result = client.GetAllKeys();
            }
            catch (Exception)
            {
                               
            }
            return result;
        }
        public void RemoveAll()
        {
            try
            {
                var client = RedisClient();
                client.Db = Get_RedisDB();
                client.FlushAll();
            }
            catch (Exception)
            {
            }
        }

        #region Validations and helper
        public RedisClient RedisClient()
        {
            return new RedisClient(
                  configuration["RedisIP"],
                  int.Parse(configuration["RedisPort"]),
                  configuration["RedisPassword"]);
        }

        public long Get_RedisDB()
        {
            var index = configuration["RedisDB"];
            var index_long = long.Parse(index);
            return index_long;
        }
        #endregion
    }
}
