using Lever.DBUtility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lever.Dal
{
    public class LeverXinDal : BaseDal
    {
        public LeverXinDal(MultipleDbContext dbContext) : base(dbContext)
        { }

        public IDictionary<string, object> GetEnterprisePage(int pageIndex, int pageSize, IDictionary<string, object> searchKeys)
        {
            StringBuilder whereSql = new StringBuilder();
            IDictionary<string, object> searchParams = new Dictionary<string, object>();
            foreach (var pair in searchKeys)
            {
                if (whereSql.Length > 0)
                    whereSql.Append(" and ");
                whereSql.Append(pair.Key + " like @" + pair.Key);
                searchParams[pair.Key] = "%" + pair.Value + "%";
            }
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            db.AddInputParameter(searchParams);
            string sql = "select count(1) as v_count from Leverxin.s_donation_enterprise where 1=1 " + (whereSql.Length > 0 ? " and " + whereSql : "");
            long count = (long)db.ExecuteScalar(sql);
            if (pageSize <= 0)
                pageSize = 10;
            int maxPage = (int)Math.Ceiling((decimal)count / pageSize);
            if (pageIndex > maxPage)
                pageIndex = maxPage;
            if (pageIndex <= 0)
                pageIndex = 1;
            db.AddInputParameter(searchParams);
            db.AddInputParameter("Limit", pageSize);
            db.AddInputParameter("Offset", (pageIndex - 1) * pageSize);
            var list = db.ExecuteQuery(@"SELECT t.*,case when t.Status=1 then '启用' else '停用' end status_text FROM Leverxin.s_donation_enterprise t WHERE 1 = 1 " + (whereSql.Length > 0 ? " and " + whereSql : "") + " limit @Limit offset @Offset");
            return new Dictionary<string, object> { { "TotalRows", count }, { "TotalPage", maxPage }, { "PageSize", pageSize }, { "PageIndex", pageIndex }, { "List", list } };
        }

        public void DeleteEnterprise(ulong enterpriseId)
        {
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            db.AddInputParameter("id", enterpriseId);
            db.ExecuteNonQuery("Delete From Leverxin.s_donation_enterprise where id=@id");
        }

        public IDictionary<string, object> GetEnterprise(ulong enterpriseId)
        {
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            db.AddInputParameter("id", enterpriseId);
            return db.SelectRow("select * from Leverxin.s_donation_enterprise where id=@id");
        }

        public IDictionary<string, object> InsertEnterprise(IDictionary<string, object> parameters)
        {
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            long id = DbHelper.NewLongId();
            parameters["id"] = id;
            db.Insert("Leverxin.s_donation_enterprise", parameters);
            return parameters;
        }

        public IDictionary<string, object> UpdateEnterprise(IDictionary<string, object> parameters)
        {
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            db.Update("Leverxin.s_donation_enterprise", new string[] { "id" }, parameters);
            return parameters;
        }

        public IDictionary<string, object> GetProjectPage(int pageIndex, int pageSize, IDictionary<string, object> searchKeys)
        {
            StringBuilder whereSql = new StringBuilder();
            IDictionary<string, object> searchParams = new Dictionary<string, object>();
            foreach (var pair in searchKeys)
            {
                if (whereSql.Length > 0)
                    whereSql.Append(" and ");
                whereSql.Append(pair.Key + " like @" + pair.Key);
                searchParams[pair.Key] = "%" + pair.Value + "%";
            }
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            db.AddInputParameter(searchParams);
            string sql = "select count(1) as v_count from Leverxin.s_donative_project where 1=1 " + (whereSql.Length > 0 ? " and " + whereSql : "");
            long count = (long)db.ExecuteScalar(sql);
            if (pageSize <= 0)
                pageSize = 10;
            int maxPage = (int)Math.Ceiling((decimal)count / pageSize);
            if (pageIndex > maxPage)
                pageIndex = maxPage;
            if (pageIndex <= 0)
                pageIndex = 1;
            db.AddInputParameter(searchParams);
            db.AddInputParameter("Limit", pageSize);
            db.AddInputParameter("Offset", (pageIndex - 1) * pageSize);
            var list = db.ExecuteQuery(@"SELECT t.*,case when t.Status=1 then '激活' else '未激活' end status_text FROM Leverxin.s_donative_project t WHERE 1 = 1 " + (whereSql.Length > 0 ? " and " + whereSql : "") + " limit @Limit offset @Offset");
            return new Dictionary<string, object> { { "TotalRows", count }, { "TotalPage", maxPage }, { "PageSize", pageSize }, { "PageIndex", pageIndex }, { "List", list } };
        }

        public void DeleteProject(ulong projectId)
        {
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            db.AddInputParameter("id", projectId);
            db.ExecuteNonQuery("Delete From Leverxin.s_donative_project where id=@id");
        }

        public IDictionary<string, object> GetProject(ulong projectId)
        {
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            db.AddInputParameter("id", projectId);
            return db.SelectRow("select * from Leverxin.s_donative_project where id=@id");
        }

        public IDictionary<string, object> InsertProject(IDictionary<string, object> parameters)
        {
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            long id = DbHelper.NewLongId();
            parameters["id"] = id;
            db.Insert("Leverxin.s_donative_project", parameters);
            return parameters;
        }

        public IDictionary<string, object> UpdateProject(IDictionary<string, object> parameters)
        {
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            db.Update("Leverxin.s_donative_project", new string[] { "id" }, parameters);
            return parameters;
        }

        public void UpdateProjectStatus(int status)
        {
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            db.AddInputParameter("status", status);
            db.ExecuteNonQuery("update Leverxin.s_donative_project set status=@status");
        }


        public IDictionary<string, object> GetPracticeModePage(ulong parentId, int pageIndex, int pageSize, IDictionary<string, object> searchKeys)
        {
            StringBuilder whereSql = new StringBuilder();
            IDictionary<string, object> searchParams = new Dictionary<string, object>();
            foreach (var pair in searchKeys)
            {
                if (whereSql.Length > 0)
                    whereSql.Append(" and ");
                whereSql.Append(pair.Key + " like @" + pair.Key);
                searchParams[pair.Key] = "%" + pair.Value + "%";
            }
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            db.AddInputParameter(searchParams);
            db.AddInputParameter("parent_id", parentId);
            string sql = "select count(1) as v_count from Leverxin.s_practice_mode where parent_id=@parent_id " + (whereSql.Length > 0 ? " and " + whereSql : "");
            long count = (long)db.ExecuteScalar(sql);
            if (pageSize <= 0)
                pageSize = 10;
            int maxPage = (int)Math.Ceiling((decimal)count / pageSize);
            if (pageIndex > maxPage)
                pageIndex = maxPage;
            if (pageIndex <= 0)
                pageIndex = 1;
            db.AddInputParameter(searchParams);
            db.AddInputParameter("Limit", pageSize);
            db.AddInputParameter("Offset", (pageIndex - 1) * pageSize);
            db.AddInputParameter("parent_id", parentId);
            var list = db.ExecuteQuery(@"SELECT t.* FROM Leverxin.s_practice_mode t where parent_id=@parent_id " + (whereSql.Length > 0 ? " and " + whereSql : "") + " limit @Limit offset @Offset");
            return new Dictionary<string, object> { { "TotalRows", count }, { "TotalPage", maxPage }, { "PageSize", pageSize }, { "PageIndex", pageIndex }, { "List", list } };
        }

        public void DeletePracticeMode(ulong practiceModeId)
        {
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            db.AddInputParameter("id", practiceModeId);
            db.ExecuteNonQuery("Delete From Leverxin.s_practice_mode where id=@id");
        }

        public IDictionary<string, object> GetPracticeMode(ulong practiceModeId)
        {
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            db.AddInputParameter("id", practiceModeId);
            return db.SelectRow("select * from Leverxin.s_practice_mode where id=@id");
        }

        public IDictionary<string, object> InsertPracticeMode(IDictionary<string, object> parameters)
        {
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            long id = DbHelper.NewLongId();
            parameters["id"] = id;
            db.Insert("Leverxin.s_practice_mode", parameters);
            return parameters;
        }

        public IDictionary<string, object> UpdatePracticeMode(IDictionary<string, object> parameters)
        {
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            db.Update("Leverxin.s_practice_mode", new string[] { "id" }, parameters);
            return parameters;
        }

        public IList<IDictionary<string, object>> GetChildrenPracticeModeList(ulong practiceModeId)
        {
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            db.AddInputParameter("parent_id", practiceModeId);
            return db.ExecuteQuery("select * from Leverxin.s_practice_mode where parent_id=@parent_id");
        }

        public IDictionary<string, object> GetGuideMusicPage(int pageIndex, int pageSize, IDictionary<string, object> searchKeys)
        {
            StringBuilder whereSql = new StringBuilder();
            IDictionary<string, object> searchParams = new Dictionary<string, object>();
            foreach (var pair in searchKeys)
            {
                if (whereSql.Length > 0)
                    whereSql.Append(" and ");
                whereSql.Append(pair.Key + " like @" + pair.Key);
                searchParams[pair.Key] = "%" + pair.Value + "%";
            }
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            db.AddInputParameter(searchParams);
            string sql = "select count(1) as v_count from Leverxin.s_guide_music where 1=1 " + (whereSql.Length > 0 ? " and " + whereSql : "");
            long count = (long)db.ExecuteScalar(sql);
            if (pageSize <= 0)
                pageSize = 10;
            int maxPage = (int)Math.Ceiling((decimal)count / pageSize);
            if (pageIndex > maxPage)
                pageIndex = maxPage;
            if (pageIndex <= 0)
                pageIndex = 1;
            db.AddInputParameter(searchParams);
            db.AddInputParameter("Limit", pageSize);
            db.AddInputParameter("Offset", (pageIndex - 1) * pageSize);
            var list = db.ExecuteQuery(@"SELECT 
  t.*,
  CASE
    WHEN t.kind = 1 
    THEN '开始音乐' 
    WHEN t.kind = 2 
    THEN '结束音乐' 
    ELSE '背景音乐' 
  END kind_text,
  pm.practice_mode AS practice_mode 
FROM
  Leverxin.s_guide_music t 
  LEFT JOIN (SELECT GROUP_CONCAT(m.practice_name) AS practice_mode,p.music_id FROM Leverxin.s_practice_music p LEFT JOIN Leverxin.s_practice_mode m ON p.practice_mode_id=m.id GROUP BY p.music_id) pm 
    ON t.id = pm.music_id 
WHERE 1 = 1 " + (whereSql.Length > 0 ? " and " + whereSql : "") + " limit @Limit offset @Offset");
            return new Dictionary<string, object> { { "TotalRows", count }, { "TotalPage", maxPage }, { "PageSize", pageSize }, { "PageIndex", pageIndex }, { "List", list } };
        }

        public void DeleteGuideMusic(ulong guideMusicId)
        {
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            db.AddInputParameter("id", guideMusicId);
            db.ExecuteNonQuery("Delete From Leverxin.s_guide_music where id=@id");
        }

        public IDictionary<string, object> GetGuideMusic(ulong guideMusicId)
        {
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            db.AddInputParameter("id", guideMusicId);
            return db.SelectRow(@"select t.*,pm.practice_mode_id,pm.practice_mode from Leverxin.s_guide_music t left join (SELECT GROUP_CONCAT(p.practice_mode_id) AS practice_mode_id,GROUP_CONCAT(m.practice_name) AS practice_mode,p.music_id FROM Leverxin.s_practice_music p LEFT JOIN Leverxin.s_practice_mode m ON p.practice_mode_id=m.id GROUP BY p.music_id) pm 
    ON t.id = pm.music_id  where id=@id");
        }

        public IDictionary<string, object> InsertGuideMusic(IDictionary<string, object> parameters)
        {
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            long id = DbHelper.NewLongId();
            parameters["id"] = id;
            db.Insert("Leverxin.s_guide_music", parameters);
            return parameters;
        }

        public IDictionary<string, object> UpdateGuideMusic(IDictionary<string, object> parameters)
        {
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            db.Update("Leverxin.s_guide_music", new string[] { "id" }, parameters);
            return parameters;
        }

        public IDictionary<string, object> GetProjectEnterprise(ulong projectEnterpriseId)
        {
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            db.AddInputParameter("id", projectEnterpriseId);
            return db.SelectRow("select t.*,e.enterprise_name,p.project_name from Leverxin.s_project_enterprise t left join Leverxin.s_donation_enterprise e on t.enterprise_id=e.id left join Leverxin.s_donative_project p on t.project_id=p.id where t.id=@id");
        }

        public IList<IDictionary<string, object>> GetAllEnterprise(ulong projectId,ulong projectEnterpriseId)
        {
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            db.AddInputParameter("project_id", projectId);
            db.AddInputParameter("id", projectEnterpriseId);
            return db.ExecuteQuery("SELECT t.* FROM Leverxin.s_donation_enterprise t WHERE NOT EXISTS(SELECT 1 FROM Leverxin.s_project_enterprise a WHERE a.project_id =@project_id AND a.enterprise_id=t.id and a.id<>@id) and t.status=1");
        }

        public IDictionary<string, object> InsertProjectEnterprise(IDictionary<string, object> parameters)
        {
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            long id = DbHelper.NewLongId();
            parameters["id"] = id;
            db.Insert("Leverxin.s_project_enterprise", parameters);
            return parameters;
        }

        public IDictionary<string, object> UpdateProjectEnterprise(IDictionary<string, object> parameters)
        {
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            db.Update("Leverxin.s_project_enterprise", new string[] { "id" }, parameters);
            return parameters;
        }

        public IDictionary<string, object> GetProjectEnterprisePage(ulong projectId, int pageIndex, int pageSize, IDictionary<string, object> searchKeys)
        {
            StringBuilder whereSql = new StringBuilder();
            IDictionary<string, object> searchParams = new Dictionary<string, object>();
            foreach (var pair in searchKeys)
            {
                if (whereSql.Length > 0)
                    whereSql.Append(" and ");
                whereSql.Append(pair.Key + " like @" + pair.Key);
                searchParams[pair.Key] = "%" + pair.Value + "%";
            }
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            db.AddInputParameter(searchParams);
            db.AddInputParameter("project_id", projectId);
            string sql = "select count(1) as v_count from Leverxin.s_project_enterprise where project_id=@project_id " + (whereSql.Length > 0 ? " and " + whereSql : "");
            long count = (long)db.ExecuteScalar(sql);
            if (pageSize <= 0)
                pageSize = 10;
            int maxPage = (int)Math.Ceiling((decimal)count / pageSize);
            if (pageIndex > maxPage)
                pageIndex = maxPage;
            if (pageIndex <= 0)
                pageIndex = 1;
            db.AddInputParameter(searchParams);
            db.AddInputParameter("Limit", pageSize);
            db.AddInputParameter("Offset", (pageIndex - 1) * pageSize);
            db.AddInputParameter("project_id", projectId);
            var list = db.ExecuteQuery(@"SELECT e.enterprise_name,e.logo_url,e.status,t.* FROM Leverxin.s_project_enterprise t left join Leverxin.s_donation_enterprise e on t.enterprise_id=e.id where project_id=@project_id " + (whereSql.Length > 0 ? " and " + whereSql : "") + " limit @Limit offset @Offset");
            return new Dictionary<string, object> { { "TotalRows", count }, { "TotalPage", maxPage }, { "PageSize", pageSize }, { "PageIndex", pageIndex }, { "List", list } };
        }

        public void DeleteProjectEnterprise(ulong projectEnterpriseId)
        {
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            db.AddInputParameter("id", projectEnterpriseId);
            db.ExecuteNonQuery("Delete From Leverxin.s_project_enterprise where id=@id");
        }

        public void DeletePracticeMusic(ulong musicId) {
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            db.AddInputParameter("music_id", musicId);
            db.ExecuteNonQuery("Delete From Leverxin.s_practice_music where music_id=@music_id");
        }

        public IDictionary<string, object> InsertPracticeMusic(IDictionary<string, object> parameters)
        {
            IDbHelper db = this._dbContext.Use("Leverxin_manage");
            long id = DbHelper.NewLongId();
            parameters["id"] = id;
            db.Insert("Leverxin.s_practice_music", parameters);
            return parameters;
        }
    }
}
