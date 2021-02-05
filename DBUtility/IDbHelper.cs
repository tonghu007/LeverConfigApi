using System;
using System.Collections.Generic;
using System.Data;

namespace Lever.DBUtility
{
    /// <summary>
    /// 支持Oracle、Postgresql、SQLServer、MySql、DB2、Sybase、Sqlite
    /// </summary>
    public interface IDbHelper : IDisposable
    {
        /// <summary>
        /// 添加单个参数
        /// </summary>
        /// <param name="parameterName">参数名</param>
        /// <param name="value">参数值</param>
        void AddInputParameter(string parameterName, object value);

        /// <summary>
        /// 添加多个参数
        /// </summary>
        /// <param name="dic">参数键值对字典</param>
        void AddInputParameter(IDictionary<string, object> dic);

        /// <summary>
        /// 添加输出参数(postgresql过程输出参数不用传入)
        /// </summary>
        /// <param name="parameterName">参数名</param>
        /// <param name="value">输出参数值</param>
        void AddOutputParameter(string parameterName, object value);

        /// <summary>
        /// 批量添加输出参数(postgresql过程输出参数不用传入)
        /// </summary>
        /// <param name="dic">输入参数字典，key为参数名；value为参数值</param>
        void AddOutputParameter(IDictionary<string, object> dic);

        /// <summary>
        /// 取指定输出参数值
        /// </summary>
        /// <param name="parameterName">参数值</param>
        /// <returns>返回参数值</returns>
        object GetParameterValue(string parameterName);

        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dic">参数字典</param>
        /// <returns>返回受影响数据行数</returns>
        long ExecuteNonQuery(string sql, IDictionary<string, object> dic);

        /// <summary>
        /// 执行指定sql语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回受影响行数</returns>
        long ExecuteNonQuery(string sql);

        /// <summary>
        /// 执行sql语句,得到结果集
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回结果集合</returns>
        IList<IDictionary<string, object>> ExecuteQuery(string sql);

        /// <summary>
        /// 执行sql语句,得到结果集
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dic">参数字典</param>
        /// <returns>返回结果集合</returns>
        IList<IDictionary<string, object>> ExecuteQuery(string sql, IDictionary<string, object> dic);

        /// <summary>
        /// 执行sql语句,得到DataTable
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dic">参数字典</param>
        /// <returns>返回DataTable结果</returns>
        DataTable ExecuteQueryDataTable(string sql, IDictionary<string, object> dic);

        /// <summary>
        /// 执行sql语句，得到第一行第一列的值
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dic">参数字典</param>
        /// <returns>返回第一行第一列的值</returns>
        object ExecuteScalar(string sql, IDictionary<string, object> dic);

        /// <summary>
        /// 执行sql语句，得到第一行第一列的值
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回第一行第一列的值</returns>
        object ExecuteScalar(string sql);

        /// <summary>
        /// 查询sql第一行数据，参数通过AddInputParameter添加
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns></returns>
        IDictionary<string, object> SelectRow(string sql);

        /// <summary>
        /// 查询指定表第一行数据
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="dic">条件参数字典</param>
        /// <returns>第一行数据</returns>
        IDictionary<string, object> SelectRow(string tableName, IDictionary<string, object> dic);

        /// <summary>
        /// 查询指定表第一行数据
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="fields">查询字段</param>
        /// <param name="dic">条件参数字典</param>
        /// <returns>第一行数据</returns>
        IDictionary<string, object> SelectRow(string tableName, string[] fields, IDictionary<string, object> dic);

        /// <summary>
        /// 查询指定表第一行数据,根据指定字段排序
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="dic">查询字段</param>
        /// <param name="ascFields">顺序字段</param>
        /// <param name="descFields">倒序字段</param>
        /// <returns>第一行数据</returns>
        IDictionary<string, object> SelectRow(string tableName, IDictionary<string, object> dic, string[] ascFields, string[] descFields);

        /// <summary>
        /// 查询指定表第一行数据,根据指定字段排序
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="fields">查询字段</param>
        /// <param name="dic">查询字段</param>
        /// <param name="ascFields">顺序字段</param>
        /// <param name="descFields">倒序字段</param>
        /// <returns>第一行数据</returns>
        IDictionary<string, object> SelectRow(string tableName, string[] fields, IDictionary<string, object> dic, string[] ascFields, string[] descFields);

        /// <summary>
        /// 判断表中是否存在指定条件的数据
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="dic">条件字典</param>
        /// <returns>true:存在;false：不存在</returns>
        bool Exists(string tableName, IDictionary<string, object> dic);

        /// <summary>
        /// 判断指定sql是否返回数据
        /// </summary>
        /// <param name="sql">表名</param>
        /// <returns>true:存在;false：不存在</returns>
        bool Exists(string sql);

        /// <summary>
        /// 查询指定表，得到DataTable数据
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns>返回DataTable结果</returns>
        DataTable SelectTable(string tableName);

        /// <summary>
        /// 查询指定表，得到DataTable数据
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="fields">查询字段名数组</param>
        /// <returns>返回DataTable结果</returns>
        DataTable SelectTable(string tableName, string[] fields);

        /// <summary>
        /// 查询指定表，得到DataTable数据
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="dic">条件参数字典</param>
        /// <returns>返回DataTable结果</returns>
        DataTable SelectTable(string tableName, IDictionary<string, object> dic);

        /// <summary>
        /// 查询指定表，得到DataTable数据
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="fields">查询字段名数组</param>
        /// <param name="dic">条件参数字典</param>
        /// <returns>返回DataTable结果</returns>
        DataTable SelectTable(string tableName, string[] fields, IDictionary<string, object> dic);

        /// <summary>
        /// 查询指定表，得到DataTable数据
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="dic">条件参数字典</param>
        /// <param name="ascFields">正序字典</param>
        /// <param name="descFields">倒序字段</param>
        /// <param name="limit">取得的记录条数</param>
        /// <param name="offset">起始记录数</param>
        /// <returns>返回DataTable结果</returns>
        DataTable SelectTable(string tableName, IDictionary<string, object> dic, string[] ascFields, string[] descFields,
            long limit, long offset);

        /// <summary>
        /// 查询指定表，得到DataTable数据
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="dic">条件参数字典</param>
        /// <param name="ascFields">正序字典</param>
        /// <param name="descFields">倒序字段</param>
        /// <param name="limit">取得的记录条数</param>
        /// <returns>返回DataTable结果</returns>
        DataTable SelectTable(string tableName, IDictionary<string, object> dic, string[] ascFields, string[] descFields,
            long limit);

        /// <summary>
        /// 查询指定表，得到DataTable数据
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="dic">条件参数字典</param>
        /// <param name="ascFields">正序字典</param>
        /// <param name="descFields">倒序字段</param>
        /// <returns>返回DataTable结果</returns>
        DataTable SelectTable(string tableName, IDictionary<string, object> dic, string[] ascFields, string[] descFields);

        /// <summary>
        /// 查询指定表，得到DataTable数据
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="fields">查询字段名数组</param>
        /// <param name="dic">条件参数字典</param>
        /// <param name="ascFields">正序字典</param>
        /// <param name="descFields">倒序字段</param>
        /// <returns>返回DataTable结果</returns>
        DataTable SelectTable(string tableName, string[] fields, IDictionary<string, object> dic, string[] ascFields, string[] descFields);

        /// <summary>
        /// 查询指定表，得到DataTable数据
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="fields">查询字段名数组</param>
        /// <param name="dic">条件参数字典</param>
        /// <param name="ascFields">正序字典</param>
        /// <param name="descFields">倒序字段</param>
        /// <param name="limit">取得的记录条数</param>
        /// <param name="offset">起始记录数</param>
        /// <returns>返回DataTable结果</returns>
        DataTable SelectTable(string tableName, string[] fields, IDictionary<string, object> dic, string[] ascFields, string[] descFields,
            long limit, long offset);

        /// <summary>
        /// 查询指定表，得到DataTable数据
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="fields">查询字段名数组</param>
        /// <param name="dic">条件参数字典</param>
        /// <param name="ascFields">正序字典</param>
        /// <param name="descFields">倒序字段</param>
        /// <param name="limit">取得的记录条数</param>
        /// <returns>返回DataTable结果</returns>
        DataTable SelectTable(string tableName, string[] fields, IDictionary<string, object> dic, string[] ascFields,
            string[] descFields, long limit);

        /// <summary>
        /// 查询指定表数据，得到数据集合
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns>结果集合</returns>
        IList<IDictionary<string, object>> Select(string tableName);

        /// <summary>
        /// 查询指定表数据，得到数据集合
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="dic">条件参数字典</param>
        /// <returns>结果集合</returns>
        IList<IDictionary<string, object>> Select(string tableName, IDictionary<string, object> dic);

        /// <summary>
        /// 查询指定表数据，得到数据集合
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="dic">条件参数字典</param>
        /// <param name="ascFields">正序字典</param>
        /// <param name="descFields">倒序字段</param>
        /// <param name="limit">取得的记录条数</param>
        /// <param name="offset">起始记录数</param>
        /// <returns>返回结果集</returns>
        IList<IDictionary<string, object>> Select(string tableName, IDictionary<string, object> dic, string[] ascFields,
            string[] descFields, long limit, long offset);

        /// <summary>
        /// 查询指定表数据，得到数据集合
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="dic">条件参数字典</param>
        /// <param name="ascFields">正序字典</param>
        /// <param name="descFields">倒序字段</param>
        /// <param name="limit">取得的记录条数</param>
        /// <returns>返回结果集</returns>
        IList<IDictionary<string, object>> Select(string tableName, IDictionary<string, object> dic, string[] ascFields,
            string[] descFields, long limit);

        /// <summary>
        /// 查询指定表数据，得到数据集合
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="dic">条件参数字典</param>
        /// <param name="ascFields">正序字典</param>
        /// <param name="descFields">倒序字段</param>
        /// <returns>返回结果集合</returns>
        IList<IDictionary<string, object>> Select(string tableName, IDictionary<string, object> dic, string[] ascFields,
            string[] descFields);

        /// <summary>
        /// 查询指定表数据，得到数据集合
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="fields">查询字段名数组</param>
        /// <returns>结果集合</returns>
        IList<IDictionary<string, object>> Select(string tableName, string[] fields);

        /// <summary>
        /// 查询指定表数据，得到数据集合
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="fields">查询字段名数组</param>
        /// <param name="dic">条件参数字典</param>
        /// <returns>结果集合</returns>
        IList<IDictionary<string, object>> Select(string tableName, string[] fields, IDictionary<string, object> dic);

        /// <summary>
        /// 查询指定表数据，得到数据集合
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="fields">查询字段名数组</param>
        /// <param name="dic">条件参数字典</param>
        /// <param name="ascFields">正序字典</param>
        /// <param name="descFields">倒序字段</param>
        /// <param name="limit">取得的记录条数</param>
        /// <param name="offset">起始记录数</param>
        /// <returns>返回结果集</returns>
        IList<IDictionary<string, object>> Select(string tableName, string[] fields, IDictionary<string, object> dic, string[] ascFields,
            string[] descFields, long limit, long offset);

        /// <summary>
        /// 查询指定表数据，得到数据集合
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="fields">查询字段名数组</param>
        /// <param name="dic">条件参数字典</param>
        /// <param name="ascFields">正序字典</param>
        /// <param name="descFields">倒序字段</param>
        /// <param name="limit">取得的记录条数</param>
        /// <returns>返回结果集</returns>
        IList<IDictionary<string, object>> Select(string tableName, string[] fields, IDictionary<string, object> dic,
            string[] ascFields, string[] descFields, long limit);

        /// <summary>
        /// 查询指定表数据，得到数据集合
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="fields">查询字段名数组</param>
        /// <param name="dic">条件参数字典</param>
        /// <param name="ascFields">正序字典</param>
        /// <param name="descFields">倒序字段</param>
        /// <returns>返回结果集合</returns>
        IList<IDictionary<string, object>> Select(string tableName, string[] fields, IDictionary<string, object> dic, string[] ascFields,
            string[] descFields);

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="dic">插入数据字典</param>
        /// <returns>返回受影响数据行数</returns>
        long Insert(string tableName, IDictionary<string, object> dic);

        /// <summary>
        /// 不存在uniqueFields字段值的数据就执行插入
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="dic">插入数据字典</param>
        /// <param name="uniqueFields">验证数据是否存在的字段</param>
        /// <returns>返回受影响数据行数</returns>
        long InsertNotExists(String tableName, IDictionary<string, object> dic, String[] uniqueFields);

        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="keys">条件字段</param>
        /// <param name="dic">修改数据字典</param>
        /// <returns>返回受影响数据行数</returns>
        long Update(string tableName, string[] keys, IDictionary<string, object> dic);

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="dic">条件参数字典</param>
        /// <returns>返回受影响数据行数</returns>
        long Delete(string tableName, IDictionary<string, object> dic);

        /// <summary>
        /// 数据记录条数
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="dic">条件参数字典</param>
        /// <returns>返回记录数</returns>
        long Count(string tableName, IDictionary<string, object> dic);

        /// <summary>
        /// 得到指定表记录数
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        long Count(string tableName);

        /// <summary>
        /// 取当前序列值,postgresql、oracle、DB2需给定sequence名,sybase和sqlserver给定表名；sqlite插入数据后立即获取
        /// </summary>
        /// <param name="name">序列名称（postgresql、oracle、DB2）；sybase、sqlserver和mysql给定表名</param>
        /// <returns>序列值</returns>
        long GetCurrentSequence(string name = "");

        /// <summary>
        /// 取下一序列值，sybase和sqlserver给定表名；mysql多连接同时操作可能有误；sqlite可能有误
        /// </summary>
        /// <param name="name">postgresql、oracle、DB2序列名称；mysql、sybase和sqlserver给定表名</param>
        /// <returns>下一序列值</returns>
        long GetNextSequence(string name);

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="procedureName">过程名</param>
        void ProcedureNonQuery(string procedureName);

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="procedureName">过程名</param>
        /// <param name="dic">输入参数键值对</param>
        void ProcedureNonQuery(string procedureName, IDictionary<string, object> dic);

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="procedureName">过程名</param>
        /// <param name="dic">输入参数键值对</param>
        /// <param name="outputParams">输出参数名数组(postgresql过程输出参数不用传入)</param>
        void ProcedureNonQuery(string procedureName, IDictionary<string, object> dic, IDictionary<string, object> outputParams);

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="procedureName">过程名</param>
        /// <returns>返回游标值即字典集合</returns>
        IList<IList<IDictionary<string, object>>> ProcedureList(string procedureName);

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="procedureName">过程名</param>
        /// <param name="dic">输入参数键值对</param>
        /// <returns>返回游标值即字典集合</returns>
        IList<IList<IDictionary<string, object>>> ProcedureList(string procedureName, IDictionary<string, object> dic);

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="procedureName">过程名</param>
        /// <param name="dic">输入参数键值对</param>
        /// <param name="outputParams">输出参数名数组(postgresql过程输出参数不用传入)</param>
        /// <returns>返回游标值即字典集合</returns>
        IList<IList<IDictionary<string, object>>> ProcedureList(string procedureName, IDictionary<string, object> dic, IDictionary<string, object> outputParams);

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="procedureName">过程名</param>
        /// <returns>返回游标值即DataTable集合</returns>
        IList<DataTable> ProcedureDataTable(string procedureName);

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="procedureName">过程名</param>
        /// <param name="dic">输入参数键值对</param>
        /// <returns>返回游标值即DataTable集合</returns>
        IList<DataTable> ProcedureDataTable(string procedureName, IDictionary<string, object> dic);

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="procedureName">过程名</param>
        /// <param name="dic">输入参数键值对</param>
        /// <param name="outputParams">输出参数名数组(postgresql过程输出参数不用传入)</param>
        /// <returns>返回游标值即DataTable集合</returns>
        IList<DataTable> ProcedureDataTable(string procedureName, IDictionary<string, object> dic, IDictionary<string, object> outputParams);

        /// <summary>
        /// 事务提交
        /// </summary>
        void Commit();

        /// <summary>
        /// 事务回滚
        /// </summary>
        void Rollback();

        /// <summary>
        /// 启动事务
        /// </summary>
        void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    }
}
