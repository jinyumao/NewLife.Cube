﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using NewLife.Data;
using XCode;
using XCode.Membership;

namespace NewLife.School.Entity
{
    /// <summary>学生</summary>
    public partial class Student : Entity<Student>
    {
        #region 对象操作
        static Student()
        {
            // 累加字段
            //Meta.Factory.AdditionalFields.Add(__.Logins);

            // 过滤器 UserModule、TimeModule、IPModule
            Meta.Modules.Add<UserModule>();
            Meta.Modules.Add<TimeModule>();
            Meta.Modules.Add<IPModule>();

            // 设置从键缓存
            var sc = Meta.SingleCache;
            sc.FindSlaveKeyMethod = k => Find(_.Name == k);
            sc.GetSlaveKeyMethod = e => e.Name;
        }

        /// <summary>验证数据，通过抛出异常的方式提示验证失败。</summary>
        /// <param name="isNew">是否插入</param>
        public override void Valid(Boolean isNew)
        {
            // 如果没有脏数据，则不需要进行任何处理
            if (!HasDirty) return;
        }

        ///// <summary>首次连接数据库时初始化数据，仅用于实体类重载，用户不应该调用该方法</summary>
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //protected override void InitData()
        //{
        //    // InitData一般用于当数据表没有数据时添加一些默认数据，该实体类的任何第一次数据库操作都会触发该方法，默认异步调用
        //    if (Meta.Session.Count > 0) return;

        //    if (XTrace.Debug) XTrace.WriteLine("开始初始化Student[学生]数据……");

        //    var entity = new Student();
        //    entity.ID = 0;
        //    entity.ClassID = 0;
        //    entity.Name = "abc";
        //    entity.Sex = 0;
        //    entity.Age = 0;
        //    entity.Mobile = "abc";
        //    entity.Address = "abc";
        //    entity.CreateUserID = 0;
        //    entity.CreateTime = DateTime.Now;
        //    entity.CreateIP = "abc";
        //    entity.UpdateUserID = 0;
        //    entity.UpdateTime = DateTime.Now;
        //    entity.UpdateIP = "abc";
        //    entity.Remark = "abc";
        //    entity.Insert();

        //    if (XTrace.Debug) XTrace.WriteLine("完成初始化Student[学生]数据！");
        //}

        ///// <summary>已重载。基类先调用Valid(true)验证数据，然后在事务保护内调用OnInsert</summary>
        ///// <returns></returns>
        //public override Int32 Insert()
        //{
        //    return base.Insert();
        //}

        ///// <summary>已重载。在事务保护范围内处理业务，位于Valid之后</summary>
        ///// <returns></returns>
        //protected override Int32 OnDelete()
        //{
        //    return base.OnDelete();
        //}
        #endregion

        #region 扩展属性
        /// <summary>班级</summary>
        [XmlIgnore]
        //[ScriptIgnore]
        public Class Class => Extends.Get(nameof(Class), k => Class.FindByID(ClassID));

        /// <summary>班级</summary>
        [XmlIgnore]
        //[ScriptIgnore]
        [DisplayName("班级")]
        [Map(__.ClassID, typeof(Class), "ID")]
        public String ClassName => Class?.Name;
        #endregion

        #region 扩展查询
        /// <summary>根据编号查找</summary>
        /// <param name="id">编号</param>
        /// <returns>实体对象</returns>
        public static Student FindByID(Int32 id)
        {
            if (id <= 0) return null;

            // 实体缓存
            if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.ID == id);

            // 单对象缓存
            return Meta.SingleCache[id];

            //return Find(_.ID == id);
        }

        /// <summary>根据名称查找</summary>
        /// <param name="name"></param>
        /// <returns>学生对象</returns>
        public static Student FindByName(String name)
        {
            if (name.IsNullOrEmpty()) return null;

            // 单对象缓存
            return Meta.SingleCache.GetItemWithSlaveKey(name) as Student;

            //return Find(_.Name == name);
        }

        /// <summary>根据班级查找</summary>
        /// <param name="classid">班级</param>
        /// <returns>实体列表，没有数据时返回空列表而不是null</returns>
        public static IList<Student> FindAllByClassID(Int32 classid)
        {
            if (classid <= 0) return new List<Student>();

            return FindAll(_.ClassID == classid);
        }
        #endregion

        #region 高级查询
        public static IList<Student> Search(Int32 classid, String key, PageParameter page)
        {
            var exp = new WhereExpression();
            if (classid > 0) exp &= _.ClassID == classid;
            if (!key.IsNullOrEmpty()) exp &= _.Name.Contains(key);

            return FindAll(exp, page);
        }

        public static IList<Student> Search(SexKinds sex, String className, PageParameter page)
        {
            var exp = new WhereExpression();
            if (sex >= SexKinds.未知) exp &= _.Sex == sex;
            if (!className.IsNullOrEmpty()) exp &= _.ClassID.In(Class.FindSQLWithKey(Class._.Name == className));

            return FindAll(exp, page);
        }
        #endregion

        #region 业务操作
        #endregion
    }
}