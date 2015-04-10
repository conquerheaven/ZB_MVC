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

        public bool AddAMP(AnalogMeasurePoint amp)
        {
            try
            {
                cx.AnalogMeasurePoints.InsertOnSubmit(amp);
                cx.SubmitChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public int GetAMPMaxNo()
        {
            return cx.AnalogMeasurePoints.Select(x => x.AMP_AnalogNo).Max();
        }

        /// <summary>
        /// 刷新历史数据
        /// </summary>
        /// <param name="analogNo"></param>
        /// <param name="startTime"></param>
        /// <returns></returns>
        public string UpdateValueOfParentPoint(int analogNo, DateTime startTime)
        {
            //Console.WriteLine("进入函数");
            DateTime t1 = DateTime.Now;

            if (cx.Connection.State != System.Data.ConnectionState.Open) cx.Connection.Open();
            cx.Transaction = cx.Connection.BeginTransaction(System.Data.IsolationLevel.Serializable);
            try
            {
                if (analogNo == 0) return "非虚拟点";
                var point = cx.AnalogMeasurePoints.Single(x => x.AMP_AnalogNo == analogNo);
                int childPointsCount = cx.AnalogMeasurePoints.Where(x => x.AMP_ParentNo == analogNo).Count();
                if (point.AMP_CptFlag == 0 && childPointsCount > 0)
                {
                    //Console.WriteLine("开始查询");
                    IQueryable<AnalogHistory> oldValue = cx.AnalogHistories.Where(x => x.AH_AnalogNo == analogNo);
                    cx.AnalogHistories.DeleteAllOnSubmit(oldValue);
                    cx.SubmitChanges();
                    //Console.WriteLine("寻找儿子节点");
                    DateTime t2 = DateTime.Now;
                    //////////////////////将儿子节点的history值保存到二维list中///////
                    var son = (from son_point in cx.AnalogMeasurePoints
                               where son_point.AMP_ParentNo == analogNo
                               select new
                               {
                                   AMP_analogNo = son_point.AMP_AnalogNo
                               }).ToList();
                    List<List<AnalogHistory>> AH_array = new List<List<AnalogHistory>>();
                    for (int i = 0; i < son.Count; i++)
                    {
                        var son_history = (from sh in cx.AnalogHistories
                                           where sh.AH_AnalogNo == son[i].AMP_analogNo
                                           orderby sh.AH_Time
                                           select sh).ToList();
                        if (son_history.Count > 0) AH_array.Add(son_history);///犯过的错误！没有判0会导致求各个子节点最小时间点的最大值时，下标会越界
                    }
                    /////////////////////////////////////////////////////////////////////
                    DateTime t3 = DateTime.Now;
                    //Console.WriteLine("求各个子节点最小时间点的最大值");
                    //////////求各个子节点最小时间点的最大值////////////////////////////
                    DateTime timeCritical = AH_array[0][0].AH_Time;
                    for (int i = 1; i < AH_array.Count; i++)
                    {
                        if (timeCritical < AH_array[i][0].AH_Time) timeCritical = AH_array[i][0].AH_Time;
                    }
                    ////////////////////////////////////////////////////////////////////

                    DateTime InsertedTime = timeCritical;
                    timeCritical = timeCritical.AddHours(1);
                    //Console.WriteLine("进入循环");
                    //int cnt = 0;
                    DateTime t4 = DateTime.Now;
                    List<DateTime> t = new List<DateTime>();
                    while (timeCritical < DateTime.Now)
                    {
                        var InsertTime = InsertedTime;
                        var InsertValue = 0.0;

                        ///////////////遍历每个节点////////////
                        for (int i = 0; i < AH_array.Count; i++)
                        {
                            //////////////二分查找到该节点第一个小于等于timeCritical的时间点////
                            int l = 0, r = AH_array[i].Count;
                            while (l < r)
                            {
                                int mid = (l + r) / 2;
                                if (AH_array[i][mid].AH_Time > timeCritical) r = mid;
                                else l = mid + 1;
                            }
                            int ans = l - 1;/////二分出来的时间点在下标为l-1的类中
                            ////////////////////////////////////////////////////////////////////

                            if (InsertTime < AH_array[i][ans].AH_Time) InsertTime = AH_array[i][ans].AH_Time;
                            InsertValue += AH_array[i][ans].AH_Value;
                        }
                        ///////////////////////////////////////

                        //////////若插入时间大于已插入时间，则插入数据///////////
                        if (InsertTime > InsertedTime)
                        {
                            AnalogHistory newAHItem = new AnalogHistory
                            {
                                AH_AnalogNo = analogNo,
                                AH_Time = InsertTime,
                                AH_Value = InsertValue
                            };
                            cx.AnalogHistories.InsertOnSubmit(newAHItem);
                        }
                        ////////////////////////////////////////////////////////

                        InsertedTime = InsertTime;
                        timeCritical = timeCritical.AddHours(1);
                        t.Add(timeCritical);
                        //if(cnt > 5)break;
                        //cnt++;
                    }
                    DateTime t5 = DateTime.Now;
                    cx.SubmitChanges();
                    cx.Transaction.Commit();
                    DateTime t6 = DateTime.Now;
                    //return "ll";
                    //return t2 +" " + t3;

                    return t1 + " " + t2 + " " + t3 + " " + t4 + " " + t5 + " " + t6 + " " + t[0] + " " + t[1] + " " + t[2] + " " + t[3] + " " + t[4] + "刷新成功~";
                }
                cx.SubmitChanges();
                cx.Transaction.Commit();
                return "无子节点";
            }
            catch
            {
                cx.Transaction.Rollback();
                cx.Connection.Close();
                return "回滚成功~";
            }
        }
    }
}