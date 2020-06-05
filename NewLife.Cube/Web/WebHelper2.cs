﻿using System;
using XCode;
using System.IO;
#if __CORE__
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NewLife.Collections;
using NewLife.Cube.Extensions;
using NewLife.Serialization;
using Microsoft.AspNetCore.Http.Extensions;
#else
using System.Web;
#endif

namespace NewLife.Cube
{
    /// <summary>Web助手</summary>
    public static class WebHelper2
    {
        #region 兼容处理
#if __CORE__
        /// <summary>获取请求值</summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static String Get(this HttpRequest request, String key) => request.GetRequestValue(key);

        /// <summary>获取Session值</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(this ISession session, String key) where T : class
        {
            if (!session.TryGetValue(key, out var buf)) return default(T);

            return buf.ToStr().ToJsonEntity<T>();
        }

        /// <summary>获取Session值</summary>
        /// <param name="session"></param>
        /// <param name="key"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static Object Get(this ISession session, String key, Type targetType)
        {
            if (!session.TryGetValue(key, out var buf)) return null;

            var rs = buf.ToStr().ToJsonEntity(targetType);
            if (rs is IEntity entity && entity.HasDirty) entity.Dirtys.Clear();

            return rs;
        }

        /// <summary>设置Session值</summary>
        /// <param name="session"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Set(this ISession session, String key, Object value) => session.Set(key, value?.ToJson().GetBytes());

        /// <summary>获取用户主机</summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static String GetUserHost(this HttpContext context)
        {
            var request = context.Request;

            var str = "";
            if (str.IsNullOrEmpty()) str = request.Headers["HTTP_X_FORWARDED_FOR"];
            if (str.IsNullOrEmpty()) str = request.Headers["X-Real-IP"];
            if (str.IsNullOrEmpty()) str = request.Headers["X-Forwarded-For"];
            if (str.IsNullOrEmpty()) str = request.Headers["REMOTE_ADDR"];
            //if (str.IsNullOrEmpty()) str = request.Headers["Host"];
            if (str.IsNullOrEmpty())
            {
                var addr = context.Connection?.RemoteIpAddress;
                if (addr != null)
                {
                    if (addr.IsIPv4MappedToIPv6) addr = addr.MapToIPv4();
                    str = addr + "";
                }
            }

            return str;
        }

        /// <summary>返回请求字符串和表单的名值字段，过滤空值和ViewState，同名时优先表单</summary>
        public static IDictionary<String, String> Params
        {
            get
            {
                var ctx = NewLife.Web.HttpContext.Current;
                if (ctx.Items["Params"] is IDictionary<String, String> dic) return dic;

                var req = ctx.Request;
                var nvss = new[]
                {
                    req.Query,
                    req.HasFormContentType ? (IEnumerable<KeyValuePair<String, StringValues>>) req.Form : new List<KeyValuePair<String, StringValues>>()
                };

                // 这里必须用可空字典，否则直接通过索引查不到数据时会抛出异常
                dic = new NullableDictionary<String, String>(StringComparer.OrdinalIgnoreCase);
                foreach (var nvs in nvss)
                {
                    foreach (var item in nvs)
                    {
                        if (item.Key.IsNullOrWhiteSpace()) continue;
                        if (item.Key.StartsWithIgnoreCase("__VIEWSTATE")) continue;

                        // 空值不需要
                        var value = item.Value.ToString();
                        if (value.IsNullOrWhiteSpace())
                        {
                            // 如果请求字符串里面有值而后面表单为空，则抹去
                            if (dic.ContainsKey(item.Key)) dic.Remove(item.Key);
                            continue;
                        }

                        // 同名时优先表单
                        dic[item.Key] = value.Trim();
                    }
                }
                ctx.Items["Params"] = dic;

                return dic;
            }
        }

        /// <summary>获取Linux发行版名称</summary>
        /// <returns></returns>
        public static String GetLinuxName()
        {
            var fr = "/etc/redhat-release";
            var dr = "/etc/debian-release";
            if (File.Exists(fr))
                return File.ReadAllText(fr).Trim();
            else if (File.Exists(dr))
                return File.ReadAllText(dr).Trim();
            else
            {
                var sr = "/etc/os-release";
                if (File.Exists(sr)) return File.ReadAllText(sr).SplitAsDictionary("=", "\n", true)["PRETTY_NAME"].Trim();
            }

            return null;
        }
#else
        /// <summary>获取请求值</summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static String Get(this HttpRequest request, String key) => request[key];

        /// <summary>获取请求值</summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static String Get(this HttpRequestBase request, String key) => request[key];

        /// <summary>获取Session值</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(this HttpSessionStateBase session, String key) => (T)session[key];

        /// <summary>设置Session值</summary>
        /// <param name="session"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Set(this HttpSessionStateBase session, String key, Object value) => session[key] = value;

        ///// <summary>获取用户主机</summary>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //public static String GetUserHost(this HttpRequest request) => request.UserHostAddress;

        /// <summary>获取用户主机</summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static String GetUserHost(this HttpContextBase context)
        {
            var request = context.Request;

            var str = "";
            if (str.IsNullOrEmpty()) str = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (str.IsNullOrEmpty()) str = request.ServerVariables["X-Real-IP"];
            if (str.IsNullOrEmpty()) str = request.ServerVariables["X-Forwarded-For"];
            if (str.IsNullOrEmpty()) str = request.ServerVariables["REMOTE_ADDR"];
            if (str.IsNullOrEmpty()) str = request.UserHostName;
            if (str.IsNullOrEmpty()) str = request.UserHostAddress;

            return str;
        }
        
        /// <summary>确定指定的 HTTP 请求是否为 AJAX 请求</summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Boolean IsAjaxRequest(this HttpRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (request["X-Requested-With"] == "XMLHttpRequest") return true;
            if (request.Headers?["X-Requested-With"] == "XMLHttpRequest") return true;

            return false;
        }
#endif
        #endregion
    }
}