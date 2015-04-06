using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZB_MVC.Models.Repository.Interface;

namespace ZB_MVC.Models.Repository.Implement
{
    public class RoomRepos : IRoomRepos
    {
        private DB_MVCDataContext cx = new DB_MVCDataContext();

        public IQueryable<SchoolInfo> GetAllSchool()
        {
            var q = from si in cx.SchoolInfos
                    select si;
            return q;
        }

        public IQueryable<SchoolAreaInfo> GetAreaBySchoolID(int schoolID)
        {
            var q = from sa in cx.SchoolAreaInfos
                    where sa.SI_ID == schoolID
                    select sa;
            return q;
        }

        public IQueryable<BuildingBriefInfo> GetBuildingByAreaID(int areaID)
        {
            var q = from bi in cx.BuildingBriefInfos
                    where bi.SAI_ID == areaID
                    select bi;
            return q;
        }
    }
}
