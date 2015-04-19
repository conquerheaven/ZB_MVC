using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZB_MVC.Models.Repository.Interface;
using ZB_MVC.Models.Repository.Entity;

namespace ZB_MVC.Models.Repository.Implement
{
    public class AnalogHistoryRepos : IAnalogHistoryRepos
    {
        DB_MVCDataContext cx = new DB_MVCDataContext();
        public IDictionary<string, string> GetTwoEndpointVal(int analogNo, DateTime inputDateTime)
        {
            if (cx.AnalogHistories.Where(x => x.AH_AnalogNo == analogNo && x.AH_Time == inputDateTime).Count() > 0)
            {
                return new Dictionary<string, string>();
            }
            IDictionary<string, string> dic = new Dictionary<string, string>();
            var lessObj = cx.AnalogHistories.Where(x => x.AH_AnalogNo == analogNo && x.AH_Time < inputDateTime).OrderByDescending(x => x.AH_Time).FirstOrDefault();
            if (lessObj != null)
            {
                dic["min"] = lessObj.AH_Value.ToString("f1");
            }
            else
            {
                dic["min"] = "";
            }
            var greatObj = cx.AnalogHistories.Where(x => x.AH_AnalogNo == analogNo && x.AH_Time > inputDateTime).OrderBy(x => x.AH_Time).FirstOrDefault();
            if (greatObj != null)
            {
                dic["max"] = greatObj.AH_Value.ToString("f1");
            }
            else
            {
                dic["max"] = "";
            }
            return dic;
        }

        /// <summary>
        /// 添加新值
        /// </summary>
        /// <param name="analogNo"></param>
        /// <param name="time"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Boolean AddEnergyHistory(int analogNo, DateTime time, double value)
        {
            if (cx.Connection.State != System.Data.ConnectionState.Open) cx.Connection.Open();
            cx.Transaction = cx.Connection.BeginTransaction(System.Data.IsolationLevel.Serializable);
            try
            {
                var nextTimeVal = cx.AnalogHistories.Where(x => x.AH_AnalogNo == analogNo && x.AH_Time > time).OrderBy(x => x.AH_Time).Select(x => x.AH_Value).FirstOrDefault();
                var preTimeVal = cx.AnalogHistories.Where(x => x.AH_AnalogNo == analogNo && x.AH_Time < time).OrderByDescending(x => x.AH_Time).Select(x => x.AH_Value).FirstOrDefault();
                if (preTimeVal > 0 && value <= preTimeVal)
                {
                    cx.Transaction.Commit();
                    return false;
                }
                if (nextTimeVal > 0 && value >= nextTimeVal)
                {
                    cx.Transaction.Commit();
                    return false;
                }
                var newItem = new AnalogHistory
                {
                    AH_AnalogNo = analogNo,
                    AH_Time = time,
                    AH_Value = value
                };
                cx.AnalogHistories.InsertOnSubmit(newItem);
                cx.SubmitChanges();
                cx.Transaction.Commit();
                return true;
            }
            catch (Exception)
            {
                cx.Transaction.Rollback();
                return false;
            }

        }

        public IQueryable<EnergyEntity> GetEnergyHistory(int analogNo, DateTime startTime, DateTime endTime)
        {
            var list = from ah in cx.AnalogHistories
                       where ah.AH_AnalogNo == analogNo && ah.AH_Time >= startTime && ah.AH_Time < endTime
                       select new EnergyEntity
                       {
                           PNO = ah.AH_AnalogNo,
                           STime = ah.AH_Time,
                           RealTime = ah.AH_Time,
                           Val = ah.AH_Value
                       };
            return list;
        }

        /// <summary>
        /// 批量修改
        /// </summary>
        /// <param name="analogNo"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Boolean ModifyByTimePeriod(int analogNo, DateTime startTime, DateTime endTime, double value)
        {
            if (cx.Connection.State != System.Data.ConnectionState.Open) cx.Connection.Open();
            cx.Transaction = cx.Connection.BeginTransaction(System.Data.IsolationLevel.Serializable);
            try
            {
                IQueryable<AnalogHistory> allAIInTimePeriod = cx.AnalogHistories.Where(x => x.AH_AnalogNo == analogNo && x.AH_Time >= startTime && x.AH_Time <= endTime);
                if (allAIInTimePeriod.Count() == 0) return false;
                foreach (var item in allAIInTimePeriod)
                {
                    item.AH_Value += value;
                }
                cx.SubmitChanges();
                cx.Transaction.Commit();
                cx.Connection.Close();
                return true;
            }
            catch
            {
                cx.Transaction.Rollback();
                cx.Connection.Close();
                return false;
            }
        }

        public int AICountByTimePeriod(int analogNo, DateTime startTime, DateTime endTime)
        {
            int aiCount = cx.AnalogHistories.Where(x => x.AH_AnalogNo == analogNo && x.AH_Time > startTime && x.AH_Time < endTime).Count();
            return aiCount;
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="analogNo"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public Boolean DeleteByTimePeriod(int analogNo, DateTime startTime, DateTime endTime)
        {
            if (cx.Connection.State != System.Data.ConnectionState.Open) cx.Connection.Open();
            cx.Transaction = cx.Connection.BeginTransaction(System.Data.IsolationLevel.Serializable);
            try
            {
                IQueryable<AnalogHistory> allAIInTimePeriod = cx.AnalogHistories.Where(x => x.AH_AnalogNo == analogNo && x.AH_Time > startTime && x.AH_Time < endTime);
                if (allAIInTimePeriod.Count() == 0) return false;
                cx.AnalogHistories.DeleteAllOnSubmit(allAIInTimePeriod);
                cx.SubmitChanges();
                cx.Transaction.Commit();
                cx.Connection.Close();
                return true;
            }
            catch
            {
                cx.Transaction.Rollback();
                cx.Connection.Close();
                return false;
            }
        }

        public IDictionary<string, string> GetTwoEndpointValAlt(int analogNo, DateTime inputDateTime)
        {
            IDictionary<string, string> dic = new Dictionary<string, string>();
            var lessObj = cx.AnalogHistories.Where(x => x.AH_AnalogNo == analogNo && x.AH_Time < inputDateTime).OrderByDescending(x => x.AH_Time).FirstOrDefault();
            if (lessObj != null)
            {
                dic["min"] = lessObj.AH_Value.ToString("f1");
            }
            else
            {
                dic["min"] = "";
            }
            var greatObj = cx.AnalogHistories.Where(x => x.AH_AnalogNo == analogNo && x.AH_Time > inputDateTime).OrderBy(x => x.AH_Time).FirstOrDefault();
            if (greatObj != null)
            {
                dic["max"] = greatObj.AH_Value.ToString("f1");
            }
            else
            {
                dic["max"] = "";
            }

            return dic;
        }

        /// <summary>
        /// 单点修改
        /// </summary>
        /// <param name="analogNo"></param>
        /// <param name="time"></param>
        /// <param name="modifyVal"></param>
        /// <returns></returns>
        public bool Modify(int analogNo, DateTime time, double modifyVal)
        {
            if (cx.Connection.State != System.Data.ConnectionState.Open) cx.Connection.Open();
            cx.Transaction = cx.Connection.BeginTransaction(System.Data.IsolationLevel.Serializable);
            try
            {
                var item = cx.AnalogHistories.Where(x => x.AH_AnalogNo == analogNo && x.AH_Time == time).SingleOrDefault();
                if (item != null)
                {
                    item.AH_Value = modifyVal;
                    cx.SubmitChanges();
                    cx.Transaction.Commit();
                    cx.Connection.Close();
                    return true;
                }
                else
                {
                    cx.Transaction.Commit();
                    cx.Connection.Close();
                    return false;
                }
            }
            catch
            {
                cx.Transaction.Rollback();
                cx.Connection.Close();
                return false;
            }
        }

        /// <summary>
        /// 单点删除
        /// </summary>
        /// <param name="analogNo"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool Delete(int analogNo, DateTime time)
        {
            if (cx.Connection.State != System.Data.ConnectionState.Open) cx.Connection.Open();
            cx.Transaction = cx.Connection.BeginTransaction(System.Data.IsolationLevel.Serializable);
            try
            {
                var item = cx.AnalogHistories.Where(x => x.AH_AnalogNo == analogNo && x.AH_Time == time).SingleOrDefault();
                if (item != null)
                {
                    cx.AnalogHistories.DeleteOnSubmit(item);
                    cx.SubmitChanges();
                    cx.Transaction.Commit();
                    cx.Connection.Close();
                    return true;
                }
                else
                {
                    cx.Transaction.Commit();
                    cx.Connection.Close();
                    return false;
                }
            }
            catch
            {
                cx.Transaction.Rollback();
                cx.Connection.Close();
                return false;
            }
        }
    }
}