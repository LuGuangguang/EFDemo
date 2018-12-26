using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Data;
using System.Linq.Dynamic;
using EntityFramework.Extensions;
using System.Reflection;
using System.Data.Entity.Infrastructure;
using WindowsFormsApp3;
//using MySql.Data.MySqlClient;
/************************************************
◇作者： LowKeyC  需要引用这个程序集:EntityFramework.Extended.6.1.0.168
◇说明： 实现EF通用的CRUD通用的接口  
◇版本号：V1.0   
◇创建日期：2017年6月22日 星期四
*****************************************************/
namespace EFCommon.SqlHelp
{
    public class Repository : IRepository, IDisposable
    {

        private static DbContext _DbContextHandle = new TestEntities();//此处进行调用EF的DBContent 的实体类或者通过工厂化模式来进行调用。

        public Repository()
        {
            _DbContextHandle = new TestEntities();
        }
        /// <summary>
        /// 添加一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Entity"></param>
        /// <returns></returns>
        public int Add<T>(T Entity) where T : class
        {

            using (TransactionScope Ts = new TransactionScope(TransactionScopeOption.Required))
            {
                try
                {
                    _DbContextHandle.Set<T>().Add(Entity);
                    int Count = _DbContextHandle.SaveChanges();
                    Ts.Complete();
                    return Count;
                }

                catch (Exception)
                {
                    Ts.Complete();
                    return 0;
                }

            }

        }

        /// <summary>
        /// 批量的插入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Entity"></param>
        /// <returns></returns>
        public int AddRange<T>(List<T> Entity) where T : class
        {
            using (TransactionScope Ts = new TransactionScope(TransactionScopeOption.Required))
            {
                _DbContextHandle.Set<T>().AddRange(Entity);
                int Count = _DbContextHandle.SaveChanges();
                Ts.Complete();
                return Count;
            }
        }

        /// <summary>
        /// 根据查询条件进行删除对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereLambda">查询条件</param>
        /// <returns></returns>
        public int Delete<T>(Expression<Func<T, bool>> whereLambda) where T : class
        {
            using (TransactionScope Ts = new TransactionScope(TransactionScopeOption.Required))
            {
                int count = 0;
                //var EntityModel = _DbContextHandle.Set<T>().Where(whereLambda).FirstOrDefault();
                var EntityModel = _DbContextHandle.Set<T>().Where(whereLambda).ToList();
                foreach (var item in EntityModel)
                {
                    _DbContextHandle.Set<T>().Remove(item);
                    int Count = _DbContextHandle.SaveChanges();
                    count++;
                }
                Ts.Complete();
                //if (EntityModel != null)
                //{
                //    _DbContextHandle.Set<T>().Remove(EntityModel);
                //    int Count = _DbContextHandle.SaveChanges();
                //    Ts.Complete();
                //    return Count > 0;
                //}
                return count;
            }
        }


        /// <summary>
        /// 删除单个对象的实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Entity">实体对象</param>
        /// <returns></returns>
        public bool Delete<T>(T Entity) where T : class
        {
            using (TransactionScope Ts = new TransactionScope(TransactionScopeOption.Required))
            {
                _DbContextHandle.Set<T>().Attach(Entity);
                _DbContextHandle.Set<T>().Remove(Entity);
                int Count = _DbContextHandle.SaveChanges();
                Ts.Complete();
                return Count > 0;
            }
        }

        /// <summary>
        /// 批量的进行更新数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Entity"></param>
        /// <returns></returns>
        public bool Update<T>(List<T> Entity) where T : class
        {
            int Count = 0;
            using (TransactionScope Ts = new TransactionScope(TransactionScopeOption.Required))
            {
                if (Entity != null)
                {
                    foreach (var items in Entity)
                    {
                        var EntityModel = _DbContextHandle.Entry(Entity);
                        _DbContextHandle.Set<T>().Attach(items);
                        EntityModel.State = EntityState.Modified;
                    }

                }
                Count = _DbContextHandle.SaveChanges();
                Ts.Complete();
            }

            return Count > 0;
        }


        /// <summary>
        /// 进行修改单个实体对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Entity">实体对象</param>
        /// <returns></returns>
        public bool Update<T>(T Entity) where T : class
        {
            using (TransactionScope Ts = new TransactionScope())
            {
                var EntityModel = _DbContextHandle.Entry<T>(Entity);
                _DbContextHandle.Set<T>().Attach(Entity);
                EntityModel.State = EntityState.Modified;
                int Count = _DbContextHandle.SaveChanges();
                Ts.Complete();
                return Count > 0;
            }
        }

        /// <summary>
        /// 批量的修改
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="WhereLambda"></param>
        /// <param name="UpdateLambda"></param>
        /// <returns></returns>
        //public bool Update<T>(Expression<Func<T, bool>> WhereLambda, Expression<Func<T, T>> UpdateLambda) where T : class
        //{
        //    _DbContextHandle.Set<T>().Where(WhereLambda).Update<T>(UpdateLambda);
        //    return _DbContextHandle.SaveChanges() > 0;
        //}


        /// <summary>
        /// 查询条件进行修改
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">要修改的实体模型</param>
        /// <param name="WhereLambda">查询条件</param>
        /// <param name="ModifiedProNames">要修改的字段名称(可以搭配GetMemberName方法使用）</param>
        /// <returns></returns>
        public int Update<T>(T model, Expression<Func<T, bool>> WhereLambda, params string[] ModifiedProNames) where T : class
        {
            try
            {
                //查询要修改的数据
                List<T> ListModifing = _DbContextHandle.Set<T>().Where(WhereLambda).ToList();
                Type t = typeof(T);
                List<PropertyInfo> ProInfos = t.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();
                Dictionary<string, PropertyInfo> DitProList = new Dictionary<string, PropertyInfo>();
                ProInfos.ForEach(p =>
                {
                    if (ModifiedProNames.Contains(p.Name))
                    {
                        DitProList.Add(p.Name, p);
                    }
                });

                if (DitProList.Count <= 0)
                {
                    throw new Exception("指定修改的字段名称有误或为空");
                }

                foreach (var item in DitProList)
                {
                    PropertyInfo proInfo = item.Value;
                    object newValue = proInfo.GetValue(model, null);
                    //批量进行修改相互对应的属性
                    foreach (T oModel in ListModifing)
                    {
                        proInfo.SetValue(oModel, newValue, null);//设置其中新的值
                    }
                }
                return _DbContextHandle.SaveChanges();
            }
            catch (Exception)
            {

                return 0;
            }
        }
        /// <summary>
        /// 获得属性字段等的名称的名称
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="memberExpression"></param>
        /// <returns></returns>
        public static string GetMemberName<T>(Expression<Func<T>> memberExpression)
        {
            MemberExpression expressionBody = (MemberExpression)memberExpression.Body;
            return expressionBody.Member.Name;
        }

        /// <summary>
        /// 释放缓存
        /// </summary>
        public void Dispose()
        {
            _DbContextHandle.Dispose();
        }

        /// <summary>
        /// 查询单个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ID">主键ID</param>
        /// <returns></returns>
        public T FindByID<T>(dynamic ID) where T : class
        {
            return _DbContextHandle.Set<T>().Find(ID) ?? null;
        }


        /// <summary>
        /// 获取全部数据的列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Order">排序</param>
        /// <returns></returns>
        public List<T> GetAll<T>(string Order = null) where T : class
        {
            return Order != null ? _DbContextHandle.Set<T>().OrderBy(Order).ToList() ?? null : _DbContextHandle.Set<T>().ToList() ?? null;
        }

        /// <summary>
        ///根据查询条件进行查询列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="WhereLambda"></param>
        /// <returns></returns>
        public List<T> GetAllQuery<T>(Expression<Func<T, bool>> WhereLambda = null) where T : class
        {
            return WhereLambda != null ? _DbContextHandle.Set<T>().Where(WhereLambda).ToList() ?? null : _DbContextHandle.Set<T>().ToList() ?? null;
        }

        /// <summary>
        ///判断对象是否存在
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="WhereLambda"></param>
        /// <returns></returns>
        public bool GetAny<T>(Expression<Func<T, bool>> WhereLambda = null) where T : class
        {
            return WhereLambda != null ? _DbContextHandle.Set<T>().Where(WhereLambda).Any() : _DbContextHandle.Set<T>().Any();
        }

        /// <summary>
        /// 获取查询条件的记录数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="WhereLambda"></param>
        /// <returns></returns>
        public int GetCount<T>(Expression<Func<T, bool>> WhereLambda = null) where T : class
        {
            return WhereLambda != null ? _DbContextHandle.Set<T>().Where(WhereLambda).Count() : _DbContextHandle.Set<T>().Count();
        }


        /// <summary>
        /// 获取单条的记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="WhereLambda"></param>
        /// <returns></returns>
        public T GetFristDefault<T>(Expression<Func<T, bool>> WhereLambda = null) where T : class
        {
            return WhereLambda != null ? _DbContextHandle.Set<T>().Where(WhereLambda).FirstOrDefault() ?? null : _DbContextHandle.Set<T>().FirstOrDefault() ?? null;
        }


        /// <summary>
        /// 查询对象的转化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="WhereLambda"></param>
        /// <returns></returns>
        public List<T> GetSelect<T>(Expression<Func<T, bool>> WhereLambda) where T : class
        {
            return _DbContextHandle.Set<T>().Where(WhereLambda).ToList() ?? null;
        }

        /// <summary>
        ///根据查询条件进行分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="PageIndex">当前页</param>
        /// <param name="PageSize">每页的大小</param>
        /// <param name="TotalCount">总记录数</param>
        /// <param name="ordering">排序条件</param>
        /// <param name="WhereLambda">查询条件</param>
        /// <returns></returns>
        public List<T> Pagination<T>(int PageIndex, int PageSize, out int TotalCount, string Ordering, Expression<Func<T, bool>> WhereLambda = null) where T : class
        {
            //分页的时候一定要注意 Order 一定在Skip 之前
            var QueryList = _DbContextHandle.Set<T>().OrderBy(Ordering);
            if (WhereLambda != null)
            {
                QueryList = QueryList.Where(WhereLambda);
            }

            TotalCount = QueryList.Count();
            return QueryList.Skip(PageSize * (PageIndex - 1)).Take(PageSize).ToList() ?? null;
        }

        /// <summary>
        ///根据查询条件进行分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="PageIndex">当前页</param>
        /// <param name="PageSize">每页的大小</param>
        /// <param name="TotalCount">总记录数</param>
        /// <param name="OrderBy">排序条件</param>
        /// <param name="WhereLambda">查询的条件</param>
        /// <param name="IsOrder"></param>
        /// <returns></returns>
        public List<T> Pagination<T, TKey>(int PageIndex, int PageSize, out int TotalCount, Expression<Func<T, TKey>> OrderBy, Expression<Func<T, bool>> WhereLambda = null, bool IsOrder = true) where T : class
        {
            //分页的时候一定要注意 Order一定在Skip 之前
            IQueryable<T> QueryList = IsOrder == true ? _DbContextHandle.Set<T>().OrderBy(OrderBy) : _DbContextHandle.Set<T>().OrderByDescending(OrderBy);

            if (WhereLambda != null)
            {
                QueryList = QueryList.Where(WhereLambda);
            }

            TotalCount = QueryList.Count();
            return QueryList.Skip(PageSize * (PageIndex - 1)).Take(PageSize).ToList() ?? null;
        }


        /// <summary>
        /// 执行存储过程的SQL 语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Sql">执行的SQL语句</param>
        /// <param name="Parms">SQL 语句的参数</param>
        /// <param name="CmdType"></param>
        /// <returns></returns>
        //public List<T> QueryPro<T>(string Sql, List<MySqlParameter> Parms, CommandType CmdType = CommandType.Text) where T : class
        //{
        //    //进行执行存储过程
        //    if (CmdType == CommandType.StoredProcedure)
        //    {
        //        StringBuilder paraNames = new StringBuilder();
        //        foreach (var item in Parms)
        //        {
        //            paraNames.Append($" @{item},");
        //        }
        //        Sql = paraNames.Length > 0 ? $"exec {Sql} {paraNames.ToString().Trim(',')}" : $"exec {Sql} ";
        //    }
        //    return _DbContextHandle.Set<T>().SqlQuery(Sql, Parms.ToArray()).ToList();
        //}


        /// <summary>
        /// 进行回滚
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RollBackChanges<T>() where T : class
        {
            var Query = _DbContextHandle.ChangeTracker.Entries().ToList();

            Query.ForEach(p => p.State = EntityState.Unchanged);
        }

        public bool Update<T>(Expression<Func<T, bool>> WhereLambda, Expression<Func<T, T>> UpdateLambda) where T : class
        {
            throw new NotImplementedException();
        }
    }
}