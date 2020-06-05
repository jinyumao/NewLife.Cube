﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NewLife.Cube.Entity;
using NewLife.Web;
using XCode.Membership;
using System.Net.Http;
using NewLife.Log;
using NewLife.Model;
using NewLife.Reflection;
using NewLife.Security;
using XCode;
#if __CORE__
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using IHttpRequest = Microsoft.AspNetCore.Http.HttpRequest;
#else
using IHttpRequest = System.Web.HttpRequestBase;
using HttpRequest = System.Web.HttpRequest;
#endif

namespace NewLife.Cube.Web
{
    /// <summary>单点登录提供者</summary>
    public class SsoProvider
    {
        #region 属性
        /// <summary>用户管理提供者</summary>
        public IManageProvider Provider { get; set; }

        /// <summary>重定向地址。~/Sso/LoginInfo</summary>
        public String RedirectUrl { get; set; }

        /// <summary>登录成功后跳转地址。~/Admin</summary>
        public String SuccessUrl { get; set; }

        /// <summary>本地登录检查地址。~/Admin/User/Login</summary>
        public String LoginUrl { get; set; }

        /// <summary>已登录用户</summary>
        public IManageUser Current => Provider.Current;
        #endregion

        #region 构造
        /// <summary>实例化</summary>
        public SsoProvider()
        {
            Provider = ManageProvider.Provider;
            RedirectUrl = "~/Sso/LoginInfo";
            SuccessUrl = "~/Admin";
            LoginUrl = "~/Admin/User/Login";
        }
        #endregion

        #region 方法
        /// <summary>获取OAuth客户端</summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual OAuthClient GetClient(String name) => OAuthClient.Create(name);

        /// <summary>获取返回地址</summary>
        /// <param name="request">请求对象</param>
        /// <param name="referr">是否使用引用</param>
        /// <returns></returns>
        public virtual String GetReturnUrl(IHttpRequest request, Boolean referr)
        {
            var url = request.Get("r");
            if (url.IsNullOrEmpty()) url = request.Get("redirect_uri");
            if (url.IsNullOrEmpty() && referr)
            {
#if __CORE__
                url = request.Headers["Referer"].FirstOrDefault() + "";
#else
                url = request.UrlReferrer + "";
#endif
            }
            if (!url.IsNullOrEmpty() && url.StartsWithIgnoreCase("http"))
            {
                var baseUri = request.GetRawUrl();

                var uri = new Uri(url);
                if (uri != null && uri.Host.EqualIgnoreCase(baseUri.Host)) url = uri.PathAndQuery;
            }

            return url;
        }

        /// <summary>获取回调地址</summary>
        /// <param name="request"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public virtual String GetRedirect(IHttpRequest request, String returnUrl = null)
        {
            if (returnUrl.IsNullOrEmpty()) returnUrl = request.Get("r");
            // 过滤环回重定向
            if (!returnUrl.IsNullOrEmpty() && returnUrl.StartsWithIgnoreCase("/Sso/Login")) returnUrl = null;

            var uri = RedirectUrl.AsUri(request.GetRawUrl()) + "";

            return uri.AppendReturn(returnUrl);
        }

        /// <summary>获取连接信息</summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public virtual UserConnect GetConnect(OAuthClient client)
        {
            var openid = client.OpenID;
            if (openid.IsNullOrEmpty()) openid = client.UserName;

            // 根据OpenID找到用户绑定信息
            var uc = UserConnect.FindByProviderAndOpenID(client.Name, openid);
            if (uc == null) uc = new UserConnect { Provider = client.Name, OpenID = openid };

            return uc;
        }

        /// <summary>登录成功</summary>
        /// <param name="client">OAuth客户端</param>
        /// <param name="context">服务提供者。可用于获取HttpContext成员</param>
        /// <param name="uc">用户链接</param>
        /// <returns></returns>
        public virtual String OnLogin(OAuthClient client, IServiceProvider context, UserConnect uc)
        {
            // 强行绑定，把第三方账号强行绑定到当前已登录账号
            var forceBind = false;
#if __CORE__
            var httpContext = context.GetService<IHttpContextAccessor>().HttpContext;
            var req = httpContext.Request;
            var ip = httpContext.GetUserHost();
#else
            var req = context.GetService<HttpRequest>();
            var httpContext = req.RequestContext.HttpContext;
            var ip = httpContext.GetUserHost();
#endif
            //if (req != null) forceBind = req.Get("sso_action").EqualIgnoreCase("bind");
            if (req != null) forceBind = req.Get("state").EndsWithIgnoreCase("_bind");

            // 可能因为初始化顺序的问题，导致前面没能给Provider赋值
            var prv = Provider;
            if (prv == null) prv = Provider = ManageProvider.Provider;

            // 检查绑定，新用户的uc.UserID为0
            var user = prv.FindByID(uc.UserID);
            if (forceBind || user == null || !uc.Enable) user = OnBind(uc, client);

            // 填充昵称等数据
            Fill(client, user);

            if (user is IAuthUser user3)
            {
                user3.Logins++;
                user3.LastLogin = DateTime.Now;
                user3.LastLoginIP = ip;
                //user3.Save();
                //(user3 as IEntity).Update();
            }
            if (user is IEntity entity) entity.Update();

            try
            {
                uc.UpdateTime = DateTime.Now;
                uc.Save();
            }
            catch (Exception ex)
            {
                //为了防止某些特殊数据导致的无法正常登录，把所有异常记录到日志当中。忽略错误
                XTrace.WriteException(ex);
            }

            // 写日志
            var log = LogProvider.Provider;
            log?.WriteLog(typeof(UserX), "SSO登录", true, $"[{user}]从[{client.Name}]的[{client.UserName}]登录", user.ID, user + "");

            if (!user.Enable) throw new InvalidOperationException($"用户[{user}]已禁用！");

            // 登录成功，保存当前用户
            //prv.Current = user;
            prv.SetCurrent(user, context);
            // 单点登录不要保存Cookie，让它在Session过期时请求认证中心
            //prv.SaveCookie(user);
            var set = Setting.Current;
            if (set.SessionTimeout > 0)
            {
                var expire = TimeSpan.FromSeconds(set.SessionTimeout);
#if __CORE__
                prv.SaveCookie(user, expire, httpContext);
#else
                prv.SaveCookie(user, expire, httpContext.ApplicationInstance.Context);
#endif
            }

            return SuccessUrl;
        }

        /// <summary>填充用户，登录成功并获取用户信息之后</summary>
        /// <param name="client"></param>
        /// <param name="user"></param>
        protected virtual void Fill(OAuthClient client, IManageUser user)
        {
            client.Fill(user);

            var dic = client.Items;
            // 用户信息
            if (dic != null && user is UserX user2)
            {
                if (user2.Code.IsNullOrEmpty()) user2.Code = client.UserCode;
                if (user2.Mobile.IsNullOrEmpty()) user2.Mobile = client.Mobile;
                if (user2.Mail.IsNullOrEmpty()) user2.Mail = client.Mail;

                if (user2.Sex == SexKinds.未知 && dic.TryGetValue("sex", out var sex)) user2.Sex = (SexKinds)sex.ToInt();
                if (user2.Remark.IsNullOrEmpty()) user2.Remark = client.Detail;

                var set = Setting.Current;

                // 使用认证中心的角色
                if (set.UseSsoRole)
                {
                    var roleId = GetRole(dic, true);
                    if (roleId > 0)
                    {
                        user2.RoleID = roleId;

                        var ids = GetRoles(client.Items, true).ToList();
                        if (ids.Contains(roleId)) ids.Remove(roleId);
                        if (ids.Count == 0)
                            user2.RoleIDs = null;
                        else
                            user2.RoleIDs = "," + ids.OrderBy(e => e).Join() + ",";
                    }
                }
                // 使用本地角色
                if (user2.RoleID <= 0 && !set.DefaultRole.IsNullOrEmpty())
                    user2.RoleID = Role.GetOrAdd(set.DefaultRole).ID;

                // 头像。有可能是相对路径，需要转为绝对路径
                var av = client.Avatar;
                if (av != null && av.StartsWith("/") && client.Server.StartsWithIgnoreCase("http"))
                    av = new Uri(new Uri(client.Server), av) + "";

                if (user2.Avatar.IsNullOrEmpty())
                    user2.Avatar = av;
                // 本地头像，如果不存在，也要更新
                else if (user2.Avatar.StartsWith("/Sso/Avatar/"))
                {
                    var av2 = Setting.Current.AvatarPath.CombinePath(user2.ID + ".png").GetBasePath();
                    if (!File.Exists(av2))
                    {
                        LogProvider.Provider?.WriteLog(user.GetType(), "更新头像", true, $"{user2.Avatar} => {av}", user.ID, user + "");

                        user2.Avatar = av;
                    }
                }

                // 下载远程头像到本地，Avatar还是保存远程头像地址
                if (user2.Avatar.StartsWithIgnoreCase("http") && !set.AvatarPath.IsNullOrEmpty()) Task.Run(() => FetchAvatar(user, av));
            }
        }

        /// <summary>绑定用户，用户未有效绑定或需要强制绑定时</summary>
        /// <param name="uc"></param>
        /// <param name="client"></param>
        public virtual IManageUser OnBind(UserConnect uc, OAuthClient client)
        {
            var prv = Provider;
            var mode = "";

            // 如果未登录，需要注册一个
            var user = prv.Current;
            if (user == null)
            {
                var set = Setting.Current;
                if (!set.AutoRegister) throw new InvalidOperationException("绑定要求本地已登录！");

                // 先找用户名，如果存在，就加上提供者前缀，直接覆盖
                var name = client.UserName;
                if (name.IsNullOrEmpty()) name = client.NickName;
                if (!name.IsNullOrEmpty())
                {
                    // 强制绑定本地用户时，没有前缀
                    if (set.ForceBindUser)
                    {
                        mode = "UserName";
                        user = prv.FindByName(name);
                    }
                    else
                    {
                        mode = "Provider-UserName";
                        name = client.Name + "_" + name;
                        user = prv.FindByName(name);
                    }
                }

                // 匹配Code
                if (user == null && set.ForceBindUserCode)
                {
                    mode = "UserCode";
                    if (!client.UserCode.IsNullOrEmpty()) user = UserX.FindByCode(client.UserCode);
                }

                // 匹配Mobile
                if (user == null && set.ForceBindUserMobile)
                {
                    mode = "UserMobile";
                    if (!client.Mobile.IsNullOrEmpty()) user = UserX.FindByMobile(client.Mobile);
                }

                // 匹配Mail
                if (user == null && set.ForceBindUserMail)
                {
                    mode = "UserMail";
                    if (!client.Mail.IsNullOrEmpty()) user = UserX.FindByMail(client.Mail);
                }

                // QQ、微信 等不返回用户名
                if (user == null && name.IsNullOrEmpty())
                {
                    // OpenID和AccessToken不可能同时为空
                    var openid = client.OpenID;
                    if (openid.IsNullOrEmpty()) openid = client.AccessToken;

                    // 过长，需要随机一个较短的
                    var num = openid.GetBytes().Crc();

                    mode = "OpenID-Crc";
                    name = client.Name + "_" + num.ToString("X8");
                    user = prv.FindByName(name);
                }

                if (user == null)
                {
                    mode = "Register";

                    // 新注册用户采用魔方默认角色
                    var rid = Role.GetOrAdd(set.DefaultRole).ID;
                    //if (rid == 0 && client.Items.TryGetValue("roleid", out var roleid)) rid = roleid.ToInt();
                    //if (rid <= 0) rid = GetRole(client.Items, rid < -1);

                    // 注册用户，随机密码
                    user = prv.Register(name, Rand.NextString(16), rid, true);
                    //if (user is UserX user2) user2.RoleIDs = GetRoles(client.Items, rid < -2).Join();
                }
            }

            uc.UserID = user.ID;
            uc.Enable = true;

            // 写日志
            var log = LogProvider.Provider;
            log?.WriteLog(typeof(UserX), "绑定", true, $"[{user}]依据[{mode}]绑定到[{client.Name}]的[{client.UserName}]", user.ID, user + "");

            return user;
        }

        /// <summary>注销</summary>
        /// <returns></returns>
        public virtual void Logout() => Provider?.Logout();
        #endregion

        #region 服务端
        /// <summary>获取访问令牌</summary>
        /// <param name="sso"></param>
        /// <param name="client_id"></param>
        /// <param name="client_secret"></param>
        /// <param name="code"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public virtual Object GetAccessToken(OAuthServer sso, String client_id, String client_secret, String code, String ip)
        {
            var token = sso.GetToken(client_id, client_secret, code);

            return new
            {
                access_token = token,
                expires_in = sso.Expire,
                scope = "basic,UserInfo",
            };
        }

        /// <summary>获取用户信息</summary>
        /// <param name="sso"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual IManageUser GetUser(OAuthServer sso, String token)
        {
            var username = sso.Decode(token);

            var user = Provider?.FindByName(username);
            // 两级单点登录可能因缓存造成查不到用户
            if (user == null) user = UserX.Find(UserX._.Name == username);

            return user;
        }

        /// <summary>获取用户信息</summary>
        /// <param name="sso"></param>
        /// <param name="token"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual Object GetUserInfo(OAuthServer sso, String token, IManageUser user)
        {
            if (user is UserX user2)
                return new
                {
                    userid = user.ID,
                    username = user.Name,
                    nickname = user.NickName,
                    sex = user2.Sex,
                    mail = user2.Mail,
                    mobile = user2.Mobile,
                    code = user2.Code,
                    roleid = user2.RoleID,
                    rolename = user2.RoleName,
                    roleids = user2.RoleIDs,
                    rolenames = user2.Roles.Skip(1).Join(",", e => e + ""),
                    avatar = user2.Avatar,
                    detail = user2.Remark,
                };
            else
                return new
                {
                    userid = user.ID,
                    username = user.Name,
                    nickname = user.NickName,
                };
        }
        #endregion

        #region 辅助
        /// <summary>抓取远程头像</summary>
        /// <param name="user"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public virtual Boolean FetchAvatar(IManageUser user, String url = null)
        {
            if (url.IsNullOrEmpty()) url = user.GetValue("Avatar") as String;
            //if (av.IsNullOrEmpty()) throw new Exception("用户头像不存在 " + user);

            // 尝试从用户链接获取头像地址
            if (url.IsNullOrEmpty() || !url.StartsWithIgnoreCase("http"))
            {
                var list = UserConnect.FindAllByUserID(user.ID);
                url = list.OrderByDescending(e => e.UpdateTime)
                    .Where(e => !e.Avatar.IsNullOrEmpty() && e.Avatar.StartsWithIgnoreCase("http"))
                    .FirstOrDefault()?.Avatar;
            }

            if (url.IsNullOrEmpty()) return false;
            if (!url.StartsWithIgnoreCase("http")) return false;

            // 不要扩展名
            var set = Setting.Current;
            var dest = set.AvatarPath.CombinePath(user.ID + ".png").GetBasePath();

            //// 头像是否已存在
            //if (File.Exists(dest)) return false;

            LogProvider.Provider?.WriteLog(user.GetType(), "抓取头像", true, $"{url} => {dest}", user.ID, user + "");

            dest.EnsureDirectory(true);

            try
            {
                var client = new HttpClient();
                var rs = client.GetAsync(url).Result;
                var buf = rs.Content.ReadAsByteArrayAsync().Result;
                File.WriteAllBytes(dest, buf);

                // 更新头像
                user.SetValue("Avatar", "/Sso/Avatar/" + user.ID);
                (user as IEntity)?.Update();

                return true;
            }
            catch (Exception ex)
            {
                XTrace.WriteLine("抓取头像失败，{0}, {1}", user, url);
                XTrace.WriteException(ex);
            }

            return false;
        }

        private Int32 GetRole(IDictionary<String, String> dic, Boolean create)
        {
            // 先找RoleName，再找RoleID
            if (dic.TryGetValue("RoleName", out var name) && !name.IsNullOrEmpty())
            {
                var r = Role.FindByName(name);
                if (r != null) return r.ID;

                if (create)
                {
                    r = new Role { Name = name };
                    r.Insert();
                    return r.ID;
                }
            }

            //// 判断角色有效
            //if (dic.TryGetValue("RoleID", out var rid) && Role.FindByID(rid.ToInt()) != null) return rid.ToInt();

            return 0;
        }

        private Int32[] GetRoles(IDictionary<String, String> dic, Boolean create)
        {
            if (dic.TryGetValue("RoleNames", out var roleNames))
            {
                var names = roleNames.Split(',');
                var rs = new List<Int32>();
                foreach (var item in names)
                {
                    if (item.IsNullOrEmpty()) continue;

                    var r = Role.FindByName(item);
                    if (r != null)
                        rs.Add(r.ID);
                    else if (create)
                    {
                        r = new Role { Name = item };
                        r.Insert();
                        rs.Add(r.ID);
                    }
                }

                if (rs.Count > 0) return rs.ToArray();
            }

            //// 判断角色有效
            //if (dic.TryGetValue("RoleIDs", out var rids)) return rids.SplitAsInt().Where(e => Role.FindByID(e) != null).ToArray();

            return new Int32[0];
        }
        #endregion
    }
}