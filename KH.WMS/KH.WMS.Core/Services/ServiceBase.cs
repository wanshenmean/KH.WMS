using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Core.Models;
using Magicodes.ExporterAndImporter.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using SqlSugar;
using System.Drawing.Drawing2D;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using WIDESEAWCS_Core.BaseRepository;
using WIDESEAWCS_Core.Const;
using WIDESEAWCS_Core.Enums;
using WIDESEAWCS_Core.Helper;
using WIDESEAWCS_Core.Utilities;

namespace WIDESEAWCS_Core.BaseServices
{
    /// <summary>
    /// 分页数据选项
    /// </summary>
    public class PageDataOptions
    {
        /// <summary>
        /// 当前页码
        /// </summary>
        public int Page { get; set; }
        /// <summary>
        /// 每页显示的行数
        /// </summary>
        public int Rows { get; set; }
        /// <summary>
        /// 总记录数
        /// </summary>
        public int Total { get; set; }
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// 排序列名
        /// </summary>
        public string Sort { get; set; }
        /// <summary>
        /// 排序方式
        /// </summary>
        public string Order { get; set; }
        /// <summary>
        /// 查询条件
        /// </summary>
        public string Wheres { get; set; }
        /// <summary>
        /// 是否导出
        /// </summary>
        public bool Export { get; set; }
        /// <summary>
        /// 其他参数
        /// </summary>
        public object Value { get; set; }
        /// <summary>
        /// 查询条件
        /// </summary>
        public List<SearchParameters> Filter { get; set; }
    }
    //定义一个搜索参数类
    public class SearchParameters
    {
        //搜索参数名称
        public string Name { get; set; }
        //搜索参数值
        public string Value { get; set; }
        //查询类型：LinqExpressionType
        public string DisplayType { get; set; }
    }

    public class SaveModel
    {
        /// <summary>
        /// 主数据
        /// </summary>
        public Dictionary<string, object> MainData { get; set; }
        /// <summary>
        /// 详细数据
        /// </summary>
        public List<Dictionary<string, object>> DetailData { get; set; }
        /// <summary>
        /// 要删除的键
        /// </summary>
        public List<object> DelKeys { get; set; }

        /// <summary>
        /// 从前台传入的其他参数(自定义扩展可以使用)
        /// </summary>
        public object Extra { get; set; }

    }

    public class ServiceBase<TEntity, TRepository> : ServiceFunFilter<TEntity>, IService<TEntity>
         where TEntity : class, new()
         where TRepository : IRepository<TEntity>

    {
        public ServiceBase(TRepository BaseDal)
        {
            this.BaseDal = BaseDal;
        }

        public TRepository BaseDal { get; set; } //通过在子类的构造函数中注入，这里是基类，不用构造函数

        public ISqlSugarClient Db => BaseDal.Db;

        private PropertyInfo[] _propertyInfo { get; set; } = null;
        protected PropertyInfo[] TProperties
        {
            get
            {
                if (_propertyInfo != null)
                {
                    return _propertyInfo;
                }
                _propertyInfo = typeof(TEntity).GetProperties();
                return _propertyInfo;
            }
        }

        public IRepository<TEntity> Repository => BaseDal;

        public virtual PageGridData<TEntity> GetPageData(PageDataOptions options)
        {
            ISugarQueryable<TEntity> sugarQueryable = Db.Queryable<TEntity>();

            ValidatePageOptions(options, ref sugarQueryable);

            List<OrderByModel> orderByModels = new List<OrderByModel>();

            if (OrderByParameters != null)
            {
                foreach (var item in OrderByParameters)
                {
                    OrderByModel orderByModel = new OrderByModel()
                    {
                        FieldName = item.Key,
                        OrderByType = item.Value
                    };
                    orderByModels.Add(orderByModel);
                }
            }
            if (EnableWebOrderBy)
            {
                //获取排序字段
                Dictionary<string, OrderByType> orderbyDic = GetPageDataSort(options, TProperties);

                foreach (var item in orderbyDic)
                {
                    OrderByModel orderByModel = new OrderByModel()
                    {
                        FieldName = item.Key,
                        OrderByType = item.Value
                    };
                    orderByModels.Add(orderByModel);
                }
            }

            int total = 0;
            PageGridData<TEntity> pageGridData = new PageGridData<TEntity>();
            sugarQueryable = sugarQueryable.OrderBy(orderByModels);
            pageGridData.Rows = sugarQueryable.ToPageList(options.Page, options.Rows, ref total);
            pageGridData.Total = total;

            return pageGridData;
        }

        protected void ValidatePageOptions(PageDataOptions options, ref ISugarQueryable<TEntity> sugarQueryable)
        {
            options = options ?? new PageDataOptions();
            List<SearchParameters> searchParametersList = new List<SearchParameters>();
            if (options.Filter != null && options.Filter.Count > 0)
            {
                searchParametersList.AddRange(options.Filter);
            }
            else if (!string.IsNullOrEmpty(options.Wheres))
            {
                try
                {
                    searchParametersList = options.Wheres.DeserializeObject<List<SearchParameters>>();
                    options.Filter = searchParametersList;
                }
                catch { }
            }
            QueryRelativeList?.Invoke(searchParametersList);

            for (int i = 0; i < searchParametersList.Count; i++)
            {
                if (string.IsNullOrEmpty(searchParametersList[i].Value))
                {
                    continue;
                }

                PropertyInfo? property = TProperties.Where(c => c.Name.ToUpper() == searchParametersList[i].Name.ToUpper()).FirstOrDefault();

                if (property == null) continue;

                List<(bool, string, object)> results = property.ValidationValueForDbType(searchParametersList[i].Value.Split(',')).ToList();
                if (results == null || results.Count() == 0)
                {
                    continue;
                }
                for (int j = 0; j < results.Count(); j++)
                {
                    LinqExpressionType expressionType = searchParametersList[i].DisplayType.ToLower().GetLinqCondition();
                    if (expressionType == LinqExpressionType.In)
                    {
                        Expression<Func<TEntity, bool>> expression = GetWhereExpression(property.Name, results.Select(x => x.Item3).ToArray(), null, expressionType);
                        sugarQueryable = sugarQueryable.Where(expression);
                        break;
                    }
                    else
                    {
                        Expression<Func<TEntity, bool>> expression = GetWhereExpression(property.Name, results[j].Item3, null, expressionType);
                        sugarQueryable = sugarQueryable.Where(expression);
                    }
                }
            }
        }

        protected Expression<Func<TEntity, bool>> GetWhereExpression(string propertyName, object propertyValue, ParameterExpression parameter, LinqExpressionType expressionType)
        {
            Type? proType = typeof(TEntity).GetProperty(propertyName)?.PropertyType;
            if (proType == null) return p => true;
            if (propertyValue == null) return p => true;
            string? value = propertyValue?.ToString();
            if (value == null) return p => true;


            parameter = parameter ?? Expression.Parameter(typeof(TEntity), "x");
            //创建节点的属性p=>p.name 属性name
            MemberExpression memberProperty = Expression.PropertyOrField(parameter, propertyName);

            if (expressionType == LinqExpressionType.In)
            {
                if (!(propertyValue is System.Collections.IList list) || list.Count == 0)
                {
                    if (!string.IsNullOrEmpty(propertyValue?.ToString()))
                    {
                        list = propertyValue.ToString().Split(",").ToList();
                    }
                    else
                    {
                        throw new Exception("属性值类型不正确");
                    }
                }
                //

                bool isStringValue = true;
                List<object> objList = new List<object>();

                if (proType.ToString() != "System.String")
                {
                    isStringValue = false;
                    foreach (var values in list)
                    {
                        objList.Add(values?.ToString().ChangeType(proType));
                    }
                    list = objList;
                }

                //if (isStringValue)
                //{
                //    //string 类型的字段，如果值带有'单引号,EF会默认变成''两个单引号
                //    MethodInfo method = typeof(System.Collections.IList).GetMethod("Contains");
                //    //创建集合常量并设置为常量的值
                //    ConstantExpression constantCollection = Expression.Constant(list);
                //    //创建一个表示调用带参数的方法的：new string[]{"1","a"}.Contains("a");
                //    MethodCallExpression methodCall = Expression.Call(constantCollection, method, memberProperty);
                //    return Expression.Lambda<Func<TEntity, bool>>(methodCall, parameter);
                //}
                //非string字段，按上面方式处理报异常Null TypeMapping in Sql Tree
                BinaryExpression body = null;
                foreach (var values in list)
                {
                    ConstantExpression constantExpression = Expression.Constant(values);
                    UnaryExpression unaryExpression = Expression.Convert(memberProperty, constantExpression.Type);

                    body = body == null
                        ? Expression.Equal(unaryExpression, constantExpression)
                        : Expression.OrElse(body, Expression.Equal(unaryExpression, constantExpression));
                }
                return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
            }

            ConstantExpression constant = proType.ToString() == "System.String"
              ? Expression.Constant(propertyValue) : Expression.Constant(value.ChangeType(proType));

            // DateTime只选择了日期的时候自动在结束日期加一天，修复DateTime类型使用日期区间查询无法查询到结束日期的问题
            if ((proType == typeof(DateTime) || proType == typeof(DateTime?)) && expressionType == LinqExpressionType.LessThanOrEqual && value.Length == 10)
            {
                constant = Expression.Constant(Convert.ToDateTime(value).AddDays(1));
            }

            UnaryExpression member = Expression.Convert(memberProperty, constant.Type);
            Expression<Func<TEntity, bool>> expression = p => false;
            switch (expressionType)
            {
                case LinqExpressionType.Equal:
                    expression = Expression.Lambda<Func<TEntity, bool>>(Expression.Equal(member, constant), parameter);
                    break;
                case LinqExpressionType.NotEqual:
                    expression = Expression.Lambda<Func<TEntity, bool>>(Expression.NotEqual(member, constant), parameter);
                    break;
                case LinqExpressionType.GreaterThan:
                    expression = Expression.Lambda<Func<TEntity, bool>>(Expression.GreaterThan(member, constant), parameter);
                    break;
                case LinqExpressionType.LessThan:
                    expression = Expression.Lambda<Func<TEntity, bool>>(Expression.LessThan(member, constant), parameter);
                    break;
                case LinqExpressionType.ThanOrEqual:
                    expression = Expression.Lambda<Func<TEntity, bool>>(Expression.GreaterThanOrEqual(member, constant), parameter);
                    break;
                case LinqExpressionType.LessThanOrEqual:
                    expression = Expression.Lambda<Func<TEntity, bool>>(Expression.LessThanOrEqual(member, constant), parameter);
                    break;
                case LinqExpressionType.Contains:
                case LinqExpressionType.NotContains:
                    MethodInfo? method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                    if (method != null)
                    {
                        constant = Expression.Constant(propertyValue, typeof(string));
                        if (expressionType == LinqExpressionType.Contains)
                        {
                            expression = Expression.Lambda<Func<TEntity, bool>>(Expression.Call(member, method, constant), parameter);
                        }
                        else
                        {
                            expression = Expression.Lambda<Func<TEntity, bool>>(Expression.Not(Expression.Call(member, method, constant)), parameter);
                        }
                    }
                    else
                    {
                        expression = p => true;
                    }
                    break;
                default:
                    expression = p => false;
                    break;
            }
            return expression;
        }

        /// <summary>
        /// 获取排序字段
        /// </summary>
        /// <param name="pageData"></param>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        protected Dictionary<string, OrderByType> GetPageDataSort(PageDataOptions pageData, PropertyInfo[] propertyInfo)
        {
            if (!string.IsNullOrEmpty(pageData.Sort))
            {
                if (pageData.Sort.Contains(","))
                {
                    List<string> sortArr = pageData.Sort.Split(",").Where(x => propertyInfo.Any(p => p.Name == x)).ToList();
                    Dictionary<string, OrderByType> sortDic = new Dictionary<string, OrderByType>();
                    foreach (var item in sortArr)
                    {
                        sortDic[item] = pageData.Order?.ToLower() == OrderByType.Asc.ToString() ? OrderByType.Asc : OrderByType.Desc;
                    }
                    return sortDic;
                }
                else if (propertyInfo.Any(x => x.Name == pageData.Sort.FirstLetterToLower() || x.Name == pageData.Sort.FirstLetterToUpper()))
                {
                    return new Dictionary<string, OrderByType> {
                        {
                            pageData.Sort,pageData.Order?.ToLower() == OrderByType.Asc.ToString().ToLower() ? OrderByType.Asc : OrderByType.Desc
                        } };
                }
            }
            return new Dictionary<string, OrderByType> { { "CreateDate", pageData.Order?.ToLower() == OrderByType.Asc.ToString() ? OrderByType.Asc : OrderByType.Desc } };
        }

        public virtual object GetDetailPage(PageDataOptions pageData)
        {
            Type t = typeof(TEntity);

            if (pageData.Value == null) return new PageGridData<object>(total: 0, null);
            string keyName = t.GetKeyName();
            ////生成查询条件
            //Expression<Func<TEntity, bool>> whereExpression = keyName.CreateExpression<TEntity>(pageData.Value, LinqExpressionType.Equal);
            int totalCount = 0;
            PropertyInfo propertyInfo = t.GetProperties().FirstOrDefault(x => x.GetCustomAttribute<Navigate>() != null);
            if (propertyInfo != null)
            {
                Type detailType = propertyInfo.PropertyType.GetGenericArguments()[0];
                Navigate navigate = propertyInfo.GetCustomAttribute<Navigate>();
                List<ExpandoObject> list = BaseDal.Db.Queryable(detailType.Name, "detail").Where(navigate.GetName(), "=", pageData.Value).ToPageList(pageData.Page, pageData.Rows, ref totalCount);
                return new PageGridData<ExpandoObject>(totalCount, list);
            }
            return new PageGridData<object>(total: 0, null);
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="entity">单个实体</param>
        /// <returns></returns>
        public virtual WebResponseContent AddData(TEntity entity)
        {
            try
            {
                return BaseDal.AddData(entity) > 0 ? WebResponseContent.Instance.OK() : WebResponseContent.Instance.Error();
            }
            catch (Exception ex)
            {
                return WebResponseContent.Instance.Error(ex.Message);
            }
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <returns></returns>
        public virtual WebResponseContent AddData(List<TEntity> entities)
        {
            try
            {
                return BaseDal.AddData(entities) > 0 ? WebResponseContent.Instance.OK() : WebResponseContent.Instance.Error();
            }
            catch (Exception ex)
            {
                return WebResponseContent.Instance.Error(ex.Message);
            }
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="saveModel"></param>
        /// <returns></returns>
        public virtual WebResponseContent AddData(SaveModel saveModel)
        {
            try
            {
                if (saveModel == null || saveModel.MainData == null || saveModel.MainData.Count == 0)//判断参数是否传入
                {
                    return WebResponseContent.Instance.Error("传参错误,参数不能为空");
                }
                string validResult = typeof(TEntity).ValidateDicInEntity(saveModel.MainData, true, TProperties);

                if (!string.IsNullOrEmpty(validResult))
                {
                    return WebResponseContent.Instance.Error(validResult);
                }

                PropertyInfo keyPro = typeof(TEntity).GetKeyProperty();
                if (keyPro == null)
                {
                    return WebResponseContent.Instance.Error("请先设置主键");
                }
                if (keyPro.PropertyType == typeof(Guid))
                {
                    saveModel.MainData.Add(keyPro.Name, Guid.NewGuid());
                }
                else if (keyPro.PropertyType == typeof(int) || keyPro.PropertyType == typeof(long))
                {
                    SugarColumn sugarColumn = keyPro.GetCustomAttribute<SugarColumn>();
                    if (sugarColumn.IsIdentity)
                    {
                        saveModel.MainData.Remove(keyPro.Name.FirstLetterToUpper());
                        saveModel.MainData.Remove(keyPro.Name.FirstLetterToLower());
                    }
                }
                TEntity entity = saveModel.MainData.DicToModel<TEntity>();
                if (saveModel.DetailData == null || saveModel.DetailData.Count == 0)
                {
                    BaseDal.AddData(entity);
                    return WebResponseContent.Instance.OK();
                }

                if (typeof(TEntity).GetNavigatePro() == null)
                {
                    return WebResponseContent.Instance.Error("未配置导航属性");
                }

                Type detailType = typeof(TEntity).GetDetailType();
                MethodInfo? methodInfo = GetType().GetMethod(nameof(AddDataIncludesDetail));
                methodInfo = methodInfo?.MakeGenericMethod(new Type[] { detailType });
                object? obj = methodInfo?.Invoke(this, new object[] { entity, detailType, saveModel.DetailData });
                return obj as WebResponseContent;
            }
            catch (Exception ex)
            {
                return WebResponseContent.Instance.Error(ex.Message);
            }
        }

        public WebResponseContent AddDataIncludesDetail<TDetail>(TEntity entity, Type detailType, List<Dictionary<string, object>> detailDics) where TDetail : class, new()
        {
            WebResponseContent content = new WebResponseContent();
            try
            {
                string name = typeof(TEntity).GetMainIdByDetail();
                string reslut = detailType.ValidateDicInEntity(detailDics, true, new string[] { name });
                if (reslut != string.Empty)
                    return WebResponseContent.Instance.Error(reslut);

                List<TDetail> list = detailDics.DicToIEnumerable<TDetail>();

                ((SqlSugarClient)BaseDal.Db).BeginTran();

                int id = BaseDal.Db.Insertable(entity).ExecuteReturnIdentity();

                for (int i = 0; i < list.Count; i++)
                {
                    TDetail detail = list[i];
                    typeof(TDetail).SetDetailId(detail, id, name);
                }

                BaseDal.Db.Insertable(list).ExecuteCommand();

                ((SqlSugarClient)BaseDal.Db).CommitTran();

                content = WebResponseContent.Instance.OK();
            }
            catch (Exception ex)
            {
                ((SqlSugarClient)BaseDal.Db).RollbackTran();
                content = WebResponseContent.Instance.Error(ex.Message);
            }
            return content;
        }

        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="entity">单个实体</param>
        /// <returns></returns>
        public virtual WebResponseContent UpdateData(TEntity entity)
        {
            try
            {
                return BaseDal.UpdateData(entity) ? WebResponseContent.Instance.OK() : WebResponseContent.Instance.Error();
            }
            catch (Exception ex)
            {
                return WebResponseContent.Instance.Error(ex.Message);
            }
        }

        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <returns></returns>
        public virtual WebResponseContent UpdateData(List<TEntity> entities)
        {
            try
            {
                return BaseDal.UpdateData(entities) ? WebResponseContent.Instance.OK() : WebResponseContent.Instance.Error();
            }
            catch (Exception ex)
            {
                return WebResponseContent.Instance.Error(ex.Message);
            }
        }

        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="saveModel"></param>
        /// <returns></returns>
        public virtual WebResponseContent UpdateData(SaveModel saveModel)
        {
            try
            {
                List<string>? list = UpdateIgnoreColOnExecute?.Invoke(saveModel);
                if (saveModel == null || saveModel.MainData == null || saveModel.MainData.Count == 0)//判断参数是否传入
                {
                    return WebResponseContent.Instance.Error("传参错误,参数不能为空");
                }
                string validResult = typeof(TEntity).ValidateDicInEntity(saveModel.MainData, false, TProperties, list?.ToArray());

                if (!string.IsNullOrEmpty(validResult))
                {
                    return WebResponseContent.Instance.Error(validResult);
                }

                PropertyInfo keyPro = typeof(TEntity).GetKeyProperty();
                if (keyPro == null)
                {
                    return WebResponseContent.Instance.Error("请先设置主键");
                }

                TEntity entity = saveModel.MainData.DicToModel<TEntity>();

                if (saveModel.DetailData == null || saveModel.DetailData.Count == 0)
                {
                    //if (list != null)
                    //    listCol = listCol.Where(x => !list.Contains(x)).ToList();
                    bool result = BaseDal.UpdateData(entity);
                    return WebResponseContent.Instance.OK();
                }

                if (typeof(TEntity).GetNavigatePro() == null)
                {
                    return WebResponseContent.Instance.Error("未配置导航属性");
                }

                Type detailType = typeof(TEntity).GetDetailType();
                MethodInfo? methodInfo = GetType().GetMethod(nameof(UpdateDataInculdesDetail));
                methodInfo = methodInfo?.MakeGenericMethod(new Type[] { detailType });
                object? obj = methodInfo?.Invoke(this, new object[] { entity, detailType, saveModel.DetailData, saveModel.DelKeys });
                return obj as WebResponseContent;
            }
            catch (Exception ex)
            {
                return WebResponseContent.Instance.Error(ex.Message);
            }
        }

        public WebResponseContent UpdateDataInculdesDetail<TDetail>(TEntity entity, Type detailType, List<Dictionary<string, object>> detailDics, List<object> delKeys) where TDetail : class, new()
        {
            WebResponseContent content = new WebResponseContent();
            try
            {
                string name = typeof(TEntity).GetMainIdByDetail();
                string reslut = detailType.ValidateDicInEntity(detailDics, true, new string[] { name });
                if (reslut != string.Empty)
                    return WebResponseContent.Instance.Error(reslut);

                List<TDetail> list = detailDics.DicToIEnumerable<TDetail>();

                List<object> dynamicDelKeys = new List<object>();
                if (delKeys != null)
                {
                    for (int i = 0; i < delKeys.Count; i++)
                    {
                        dynamicDelKeys.Add(delKeys[i]);
                    }
                }

                List<TDetail> updateRows = new List<TDetail>();
                List<TDetail> addRows = new List<TDetail>();

                for (int i = 0; i < list.Count; i++)
                {
                    object detailId = typeof(TDetail).GetPropertyValue(list[i], typeof(TDetail).GetKeyName());
                    if (detailId != null)
                    {
                        if (detailId.ToString() != "0")
                        {
                            updateRows.Add(list[i]);
                        }
                        else
                        {
                            addRows.Add(list[i]);
                        }
                    }
                }

                object mainId = typeof(TEntity).GetPropertyValue(entity, typeof(TEntity).GetKeyName());
                if (mainId != null)
                {
                    ((SqlSugarClient)BaseDal.Db).BeginTran();

                    if (dynamicDelKeys.Count > 0)
                        BaseDal.Db.Deleteable<object>().AS(detailType.Name).Where($"{detailType.GetKeyName()} in (@id)", new { id = dynamicDelKeys.ToArray() }).ExecuteCommandHasChange();

                    BaseDal.Db.Updateable(entity).ExecuteCommandHasChange();

                    BaseDal.Db.Updateable(updateRows).ExecuteCommand();

                    for (int i = 0; i < addRows.Count; i++)
                    {
                        TDetail detail = addRows[i];
                        typeof(TDetail).SetDetailId(detail, mainId, name);
                    }

                    BaseDal.Db.Insertable(addRows).ExecuteCommand();

                    ((SqlSugarClient)BaseDal.Db).CommitTran();

                    content = WebResponseContent.Instance.OK();
                }
                else
                {
                    content = WebResponseContent.Instance.Error("未找到主表主键值");
                }

            }
            catch (Exception ex)
            {
                ((SqlSugarClient)BaseDal.Db).RollbackTran();
                content = WebResponseContent.Instance.Error(ex.Message);
            }
            return content;
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="key">主键</param>
        /// <returns></returns>
        public virtual WebResponseContent DeleteData(object key)
        {
            try
            {
                return BaseDal.DeleteDataById(key) ? WebResponseContent.Instance.OK() : WebResponseContent.Instance.Error();
            }
            catch (Exception ex)
            {
                return WebResponseContent.Instance.Error(ex.Message);
            }
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="keys">主键数组</param>
        /// <returns></returns>
        public virtual WebResponseContent DeleteData(object[] keys)
        {
            try
            {
                if (typeof(TEntity).GetNavigatePro() == null)
                    return BaseDal.DeleteDataByIds(keys) ? WebResponseContent.Instance.OK() : WebResponseContent.Instance.Error();
                else
                {
                    if (keys != null)
                    {
                        Type detailType = typeof(TEntity).GetDetailType();
                        string name = typeof(TEntity).GetMainIdByDetail();
                        List<object> dynamicDelKeys = new List<object>();

                        for (int i = 0; i < keys.Length; i++)
                        {
                            dynamicDelKeys.Add(keys[i]);
                        }
                        ((SqlSugarClient)BaseDal.Db).BeginTran();

                        if (dynamicDelKeys.Count > 0)
                            BaseDal.Db.Deleteable<object>().AS(detailType.Name).Where($"{name} in (@id)", new { id = dynamicDelKeys.ToArray() }).ExecuteCommandHasChange();

                        BaseDal.DeleteDataByIds(keys);

                        ((SqlSugarClient)BaseDal.Db).CommitTran();

                        return WebResponseContent.Instance.OK();
                    }
                    else
                    {
                        return WebResponseContent.Instance.Error("参数错误");
                    }
                }
            }
            catch (Exception ex)
            {
                ((SqlSugarClient)BaseDal.Db).RollbackTran();
                return WebResponseContent.Instance.Error(ex.Message);
            }
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="entity">单个实体</param>
        /// <returns></returns>
        public virtual WebResponseContent DeleteData(TEntity entity)
        {
            try
            {
                return BaseDal.DeleteData(entity) ? WebResponseContent.Instance.OK() : WebResponseContent.Instance.Error();
            }
            catch (Exception ex)
            {
                return WebResponseContent.Instance.Error(ex.Message);
            }
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <returns></returns>
        public virtual WebResponseContent DeleteData(List<TEntity> entities)
        {
            try
            {
                return BaseDal.DeleteData(entities) ? WebResponseContent.Instance.OK() : WebResponseContent.Instance.Error();
            }
            catch (Exception ex)
            {
                return WebResponseContent.Instance.Error(ex.Message);
            }
        }

        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="pageData"></param>
        /// <returns></returns>
        public virtual WebResponseContent Export(PageDataOptions options)
        {
            WebResponseContent content = new WebResponseContent();
            try
            {
                Type t = typeof(TEntity);
                ISugarQueryable<TEntity> sugarQueryable = Db.Queryable<TEntity>();
                string savePath = AppDomain.CurrentDomain.BaseDirectory + $"ExcelExport";
                IExporter exporter = new ExcelExporter();
                ValidatePageOptions(options, ref sugarQueryable);
                //获取排序字段
                Dictionary<string, OrderByType> orderbyDic = GetPageDataSort(options, TProperties);

                List<TEntity> entities = sugarQueryable.ToList();

                byte[] data = exporter.ExportAsByteArray(entities).Result;

                string fileName = "";
                SugarTable sugarTable = t.GetCustomAttribute<SugarTable>();
                if (sugarTable != null)
                {
                    fileName = sugarTable.TableDescription + ".xlsx";
                }
                else
                {
                    fileName = nameof(TEntity) + ".xlsx";
                }

                FileHelper.WriteFile(savePath, fileName, data);

                content = WebResponseContent.Instance.OK(data: savePath + "\\" + fileName);
            }
            catch (Exception ex)
            {
                content = WebResponseContent.Instance.Error(ex.Message);
            }
            return content;
        }

        /// <summary>
        /// 导入数据
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public virtual WebResponseContent Import(List<IFormFile> files)
        {
            try
            {
                if (files == null || files.Count == 0)
                    return new WebResponseContent { Status = true, Message = "请选择上传的文件" };
                Microsoft.AspNetCore.Http.IFormFile formFile = files[0];
                string dicPath = AppDomain.CurrentDomain.BaseDirectory + $"ExcelImprot/{DateTime.Now.ToString("yyyMMdd")}/{typeof(TEntity).Name}/";
                if (!Directory.Exists(dicPath)) Directory.CreateDirectory(dicPath);
                string fileName = $"{Guid.NewGuid()}_{formFile.FileName}";
                dicPath = $"{dicPath}{fileName}";
                using (FileStream stream = new FileStream(dicPath, FileMode.Create))
                {
                    formFile.CopyTo(stream);
                }
                ExcelImporter importer = new ExcelImporter();
                ImportResult<TEntity> importResult = importer.Import<TEntity>(dicPath, "").Result;
                if (importResult.HasError)
                {
                    return WebResponseContent.Instance.Error(importResult.TemplateErrors.Serialize());
                }
                BaseDal.AddData(importResult.Data.ToList());
                return WebResponseContent.Instance.OK();
            }
            catch (Exception ex)
            {
                return WebResponseContent.Instance.Error(ex.Message);
            }
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public virtual WebResponseContent Upload(List<IFormFile> files)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 模板下载
        /// </summary>
        /// <returns></returns>
        public virtual WebResponseContent DownLoadTemplate()
        {
            WebResponseContent content = new WebResponseContent();
            Type t = typeof(TEntity);
            IExporter exporter = new ExcelExporter();
            byte[] data = exporter.ExportHeaderAsByteArray(new TEntity()).Result;
            string fileName = "";
            SugarTable sugarTable = t.GetCustomAttribute<SugarTable>();
            if (sugarTable != null)
            {
                fileName = sugarTable.TableDescription + "导入模板.xlsx";
            }
            else
            {
                fileName = nameof(TEntity) + "导入模板.xlsx";
            }
            string savePath = AppDomain.CurrentDomain.BaseDirectory + $"ExcelImprotTemplate";
            FileHelper.WriteFile(savePath, fileName, data);

            content = WebResponseContent.Instance.OK(data: savePath + "\\" + fileName);
            return content;
        }

        public WebResponseContent ExportSeedData()
        {
            WebResponseContent content = new WebResponseContent();
            try
            {
                string seedDataFolder = $"WIDESEAWCS_DB.DBSeed.Json/{typeof(TEntity).Name}.tsv";
                List<TEntity> deviceInfos = BaseDal.QueryData();
                string str = JsonConvert.SerializeObject(deviceInfos, Formatting.Indented);
                List<Dictionary<string, object>> keyValuePairs = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(str);
                FileHelper.WriteFileAndDelOldFile($"{AppDomain.CurrentDomain.BaseDirectory}wwwroot/{seedDataFolder}", str);
                content = WebResponseContent.Instance.OK();
            }
            catch (Exception ex)
            {
                content = WebResponseContent.Instance.Error(ex.Message);
            }
            return content;
        }
    }
}
