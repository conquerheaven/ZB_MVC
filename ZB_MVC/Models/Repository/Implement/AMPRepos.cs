using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZB_MVC.Models.Repository.Interface;
using ZB_MVC.Models.Repository.Entity;

namespace ZB_MVC.Models.Repository.Implement
{
    public class AMPRepos : IAMPRepos
    {
        private DB_MVCDataContext cx = new DB_MVCDataContext();
        public IQueryable<AMPExtEntity> GetAllAMP()
        {
            var list = from amp in cx.AnalogMeasurePoints
                       join si in cx.SchoolInfos on amp.AMP_SchooldID equals si.SI_ID into tempsi
                       from si2 in tempsi.DefaultIfEmpty()
                       join sai in cx.SchoolAreaInfos on amp.AMP_SAreaID equals sai.SAI_ID into tempsai
                       from sai2 in tempsai.DefaultIfEmpty()
                       join bbi in cx.BuildingBriefInfos on amp.AMP_BuildingID equals bbi.BDI_ID into tempbbi
                       from bbi2 in tempbbi.DefaultIfEmpty()
                       join ri in cx.RoomInfos on amp.AMP_RoomID equals ri.RI_ID into tempri
                       from ri2 in tempri.DefaultIfEmpty()
                       join aiinfo in cx.AnalogInfos on amp.AMP_AnalogNo equals aiinfo.AI_No into tempaiinfo
                       from aiinfo2 in tempaiinfo.DefaultIfEmpty()
                       select new AMPExtEntity
                       {
                           PNO = amp.AMP_AnalogNo,
                           SchoolID = amp.AMP_SchooldID,
                           AreaID = amp.AMP_SAreaID,
                           BuildingID = amp.AMP_BuildingID,
                           RoomID = amp.AMP_RoomID,
                           PName = amp.AMP_Name,
                           SName = si2.SI_Name,
                           AName = sai2.SAI_Name,
                           BName = bbi2.BDI_Name,
                           RName = ri2.RI_RoomCode,
                           STime = amp.AMP_Date,
                           Val = amp.AMP_Val,
                           RemVal = amp.AMP_ValRem,
                           Unit = amp.AMP_Unit,
                           PowerType = amp.AMP_PowerType,
                           PowerName = amp.AMP_PowerName,
                           RealFlag = amp.AMP_CptFlag,
                           StatFlag = amp.AMP_Statistic,
                           ParentNo = amp.AMP_ParentNo,
                           RTU_NO = aiinfo2.RTU_No,
                           AI_Serial = aiinfo2.AI_Serial,
                           AI_Base = aiinfo2.AI_Base,
                           AI_Rate = aiinfo2.AI_Rate,
                           Encoding = amp.AMP_Encoding
                       };
            return list;
        }

        public AnalogMeasurePoint GetAMP(int analogNo)
        {
            return cx.AnalogMeasurePoints.Single(x => x.AMP_AnalogNo == analogNo);
        }

        public bool ModifyAMP(AnalogMeasurePoint amp)
        {
            try
            {
                AnalogMeasurePoint oldAMP = cx.AnalogMeasurePoints.Single(x => x.AMP_AnalogNo == amp.AMP_AnalogNo);
                oldAMP.AMP_Name = amp.AMP_Name;
                oldAMP.AMP_CptFlag = amp.AMP_CptFlag;
                oldAMP.AMP_Statistic = amp.AMP_Statistic;
                oldAMP.AMP_SchooldID = amp.AMP_SchooldID;
                oldAMP.AMP_SAreaID = amp.AMP_SAreaID;
                oldAMP.AMP_BuildingID = amp.AMP_BuildingID;
                oldAMP.AMP_RoomID = amp.AMP_RoomID;
                oldAMP.AMP_PowerType = amp.AMP_PowerType;
                oldAMP.AMP_PowerName = amp.AMP_PowerName;
                oldAMP.AMP_ParentNo = amp.AMP_ParentNo;
                oldAMP.AMP_Encoding = amp.AMP_Encoding;
                cx.SubmitChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool IsUsedByObj(int pno)
        {
            var amp = cx.AnalogMeasurePoints.Where(x => x.AMP_AnalogNo == pno).FirstOrDefault();
            if (amp != null)
            {
                if (amp.AMP_SchooldID > 0 || amp.AMP_SAreaID > 0 || amp.AMP_BuildingID > 0 || amp.AMP_RoomID > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public bool DeleteAMP(int pno)
        {
            try
            {
                var amp = cx.AnalogMeasurePoints.SingleOrDefault(x => x.AMP_AnalogNo == pno);
                if (amp != null)
                {
                    if (cx.AnalogMeasurePoints.Where(x => x.AMP_ParentNo == amp.AMP_AnalogNo).Count() > 0)
                    {
                        // 更新父测点为该测点的测点的父测点编号为0
                        cx.ExecuteCommand("update AnalogMeasurePoint set AMP_ParentNo=0 where AMP_ParentNo={0}", amp.AMP_AnalogNo);
                    }
                    cx.AnalogMeasurePoints.DeleteOnSubmit(amp);
                    // 删除该测点的历史值
                    cx.ExecuteCommand("delete from AnalogHistory where AH_AnalogNo={0}", amp.AMP_AnalogNo);
                    cx.SubmitChanges();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}