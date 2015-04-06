using System;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZB_MVC.Models.Repository.Interface;
using ZB_MVC.Models.Repository.Implement;
using ZB_MVC.Controllers.Utils;
using ZB_MVC.Models;
using System.Collections.Generic;

namespace ZB_MVC.Controllers
{
    public class HomeController : Controller
    {
        private IRoomRepos _roomRepos = new RoomRepos();
        private IRtuRepos _rtuRepos = new RtuRepos();
        private IAMPRepos _ampRepos = new AMPRepos();
        private IPowerClassRepos _powerClassRepos = new PowerClassRepos();
        private IAnalogInfoRepos _analogInfoRepos = new AnalogInfoRepos();
        private IAnalogHistoryRepos _analogHistoryRepos = new AnalogHistoryRepos();

        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";

            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult QueryPoint()
        {
            var RTUList = _rtuRepos.GetAll().ToList();
            ViewBag.RTUList = RTUList;
            return View();
        }

        public ActionResult AddRealPoint()
        {
            return View();
        }

        public ActionResult AddVirtualPoint()
        {
            return View();
        }

        /// <summary>
        /// 使用Ajax根据校区ID得到区域
        /// </summary>
        /// <param name="schoolID">校区ID</param>
        /// <returns>Json格式区域集合</returns>        
        public ActionResult GetAreasBySchoolIDAjax(int schoolID)
        {
            if (Request.IsAjaxRequest() && schoolID > 0)
            {
                var areas = _roomRepos.GetAreaBySchoolID(schoolID);
                IList list = new ArrayList();
                foreach (var item in areas)
                {
                    list.Add(new
                    {
                        dataID = item.SAI_ID,
                        dataValue = item.SAI_Name
                    });
                }
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        /// <summary>
        /// 根据区域得到建筑
        /// </summary>
        /// <param name="areaID">区域ID</param>
        /// <returns></returns>
        public ActionResult GetBuildingByAreaAjax(int areaID)
        {
            if (Request.IsAjaxRequest() && areaID > 0)
            {
                var buildings = _roomRepos.GetBuildingByAreaID(areaID);
                IList list = new ArrayList();
                foreach (var item in buildings)
                {
                    list.Add(new
                    {
                        dataID = item.BDI_ID,
                        HJFlag = item.BDI_HJFlag,
                        dataValue = item.BDI_Name
                    });
                }
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        /// <summary>
        /// 使用Ajax得到校区信息
        /// </summary>
        /// <returns>Json格式所有校区信息</returns>
        public ActionResult GetAllShoolAjax()
        {
            if (Request.IsAjaxRequest())
            {
                var schools = _roomRepos.GetAllSchool();
                IList list = new ArrayList();
                foreach (var item in schools)
                {
                    list.Add(new
                    {
                        dataID = item.SI_ID,
                        dataValue = item.SI_Name
                    });
                }
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        /// <summary>
        /// 查询测点
        /// </summary>
        /// <param name="pointID"></param>
        /// <param name="pointName"></param>
        /// <param name="objType"></param>
        /// <param name="objIDs"></param>
        /// <param name="powerType"></param>
        /// <param name="realFlag"></param>
        /// <param name="statFlag"></param>
        /// <param name="currentPage"></param>
        /// <param name="totalPages"></param>
        /// <returns></returns>
        public ActionResult QueryPointAjax(string pointID, string pointName, int? objType, int? objIDs, int? realFlag, int? statFlag, int? RTU_No, int currentPage, int totalPages)
        {
            if (Request.IsAjaxRequest())
            {
                var query = _ampRepos.GetAllAMP();
                if (objType.HasValue && objType != 0)
                {
                    switch (objType)
                    {
                        case 1:
                            query = query.Where(x => x.SchoolID == objIDs);
                            break;
                        case 2:
                            query = query.Where(x => x.AreaID == objIDs);
                            break;
                        case 3:
                            query = query.Where(x => x.BuildingID == objIDs);
                            break;
                        case 4:
                            query = query.Where(x => x.RoomID == objIDs);
                            break;
                        default:
                            break;
                    }
                }
                if (!string.IsNullOrWhiteSpace(pointID))
                {
                    int pointIDInt = 0;
                    Int32.TryParse(pointID, out pointIDInt);
                    query = query.Where(x => x.PNO == pointIDInt);
                }
                if (!string.IsNullOrWhiteSpace(pointName))
                {
                    query = query.Where(x => x.PName.Contains(pointName));
                }
                if (realFlag.HasValue && realFlag != -1)
                {
                    query = query.Where(x => x.RealFlag == realFlag);
                }
                if (statFlag.HasValue && statFlag != -1)
                {
                    query = query.Where(x => x.StatFlag == statFlag);
                }
                if (RTU_No.HasValue && RTU_No != -1)
                {
                    query = query.Where(x => x.RTU_NO == RTU_No);
                }
                Pager pager = null;
                if (totalPages == -1)
                {
                    int totalRows = query.Count();
                    pager = new Pager(1, totalRows);
                }
                else
                {
                    pager = new Pager(currentPage, totalPages, false);
                }
                if (pager.TotalPages > 0)
                {
                    var list = query.Skip(pager.StartRow).Take(pager.PageSize).ToList();
                    var resultData = new
                    {
                        totalPages = pager.TotalPages,
                        data = list
                    };
                    return Json(resultData, JsonRequestBehavior.AllowGet);
                }
                return Json(new { totalPages = pager.TotalPages }, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        public ActionResult ModifyPoint(int? p)
        {
            if (p.HasValue)
            {
                var powerList = _powerClassRepos.GetAll();
                ViewBag.powerList = powerList;
                var RTUList = _rtuRepos.GetAll();
                ViewBag.RTUList = RTUList;
                var obj = _ampRepos.GetAllAMP().Where(x => x.PNO == p.Value).FirstOrDefault();
                if (obj != null)
                {
                    if (obj.IName == "未知对象")
                    {
                        obj.IName = "";
                    }
                    return View(obj);
                }
            }
            return View();
        }

        public ActionResult QueryParentAMP(int? schoolID, int? areaID, int? buildingID, int? roomID, string powerId)
        {
            if (Request.IsAjaxRequest())
            {
                var query = _ampRepos.GetAllAMP();
                if (roomID.HasValue && roomID.Value > 0)
                {
                    query = query.Where(x => x.RealFlag == 0 && x.StatFlag == 1 && x.PowerType == powerId && x.BuildingID == buildingID.Value && x.RoomID == 0);
                }
                else if (buildingID.HasValue && buildingID.Value > 0)
                {
                    query = query.Where(x => x.RealFlag == 0 && x.StatFlag == 1 && x.PowerType == powerId && x.AreaID == areaID.Value && x.BuildingID == 0);
                }
                else if (areaID.HasValue && areaID.Value > 0)
                {
                    query = query.Where(x => x.RealFlag == 0 && x.StatFlag == 1 && x.PowerType == powerId && x.SchoolID == schoolID.Value && x.AreaID == 0);
                }
                else
                {
                    return Json(new { totalPages = 0 }, JsonRequestBehavior.AllowGet);
                }
                var list = query.ToList();
                var resultData = new
                {
                    totalPages = list.Count,
                    data = list
                };
                return Json(resultData, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        public ActionResult QueryBoundAMP(int? schoolID, int? areaID, int? buildingID, int? roomID, string powerId)
        {
            if (Request.IsAjaxRequest())
            {
                var query = _ampRepos.GetAllAMP();
                if (schoolID.HasValue && areaID.HasValue && buildingID.HasValue && roomID.HasValue)
                {
                    query = query.Where(x => x.PowerType == powerId && x.SchoolID == schoolID.Value && x.AreaID == areaID.Value && x.BuildingID == buildingID.Value && x.RoomID == roomID.Value);
                }
                else
                {
                    return Json(new { totalPages = 0 }, JsonRequestBehavior.AllowGet);
                }
                var list = query.ToList();
                var resultData = new
                {
                    totalPages = list.Count,
                    data = list
                };
                return Json(resultData, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        public ActionResult ModifyPointInfo(int? pointID, string pointName, int? schoolID, int? areaID, int? buildingID, int? roomID, string powerType, string powerName, int? realFlag, int? statFlag, int? parentPointId, int? RTU_No, int? AI_Serial, double? AI_Rate, double? AI_Base, string encoding)
        {
            if (pointID.HasValue && realFlag.HasValue && statFlag.HasValue)
            {
                if (parentPointId == null || parentPointId.Value < 0)
                {
                    parentPointId = 0;
                }
                //for (int num = 200539; num <= 200547; num++) {
                //    _ampRepos.UpdateValueOfParentPoint(num);
                //}
                AnalogMeasurePoint oldAMP = _ampRepos.GetAMP(pointID.Value);
                int oldParentID = oldAMP.AMP_ParentNo.HasValue ? oldAMP.AMP_ParentNo.Value : 0;
                AnalogMeasurePoint amp = new AnalogMeasurePoint();
                amp.AMP_AnalogNo = pointID.Value;
                amp.AMP_Name = pointName;
                amp.AMP_CptFlag = Convert.ToByte(realFlag.Value);
                amp.AMP_Statistic = Convert.ToByte(statFlag.Value);
                amp.AMP_PowerType = powerType;
                amp.AMP_PowerName = powerName;
                amp.AMP_ParentNo = parentPointId;
                amp.AMP_SchooldID = schoolID.HasValue ? schoolID.Value : 0;
                amp.AMP_SAreaID = areaID.HasValue ? areaID.Value : 0;
                amp.AMP_BuildingID = buildingID.HasValue ? buildingID.Value : 0;
                amp.AMP_RoomID = roomID.HasValue ? roomID.Value : 0;
                amp.AMP_Encoding = encoding;
                bool aiflag = true;
                if (RTU_No.HasValue || AI_Serial.HasValue || AI_Rate.HasValue || AI_Base.HasValue)
                {
                    AnalogInfo ai = new AnalogInfo();
                    ai.AI_No = pointID.Value;
                    ai.AI_Name = pointName;
                    ai.RTU_No = RTU_No.HasValue ? Convert.ToInt16(RTU_No) : Convert.ToInt16(0);
                    ai.AI_Serial = AI_Serial.HasValue ? Convert.ToInt32(AI_Serial) : Convert.ToInt32(0);
                    ai.AI_Rate = AI_Rate.HasValue ? AI_Rate.Value : 1;
                    ai.AI_Base = AI_Base.HasValue ? AI_Base.Value : 0;
                    aiflag = _analogInfoRepos.ModifyAI(ai);
                }

                bool ampflag = _ampRepos.ModifyAMP(amp);
                ViewBag.flag = aiflag & ampflag;
            }
            return View();
        }

        public ActionResult QueryAMPIsUsedAjax(int? pointID)
        {
            if (Request.IsAjaxRequest() && pointID.HasValue)
            {
                bool flag = _ampRepos.IsUsedByObj(pointID.Value);
                return Json(flag, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        public ActionResult DeleteAMPAjax(int? pointID)
        {
            if (Request.IsAjaxRequest() && pointID.HasValue)
            {
                bool flag = _ampRepos.DeleteAMP(pointID.Value);
                return Json(flag, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        public ActionResult QueryValRange(int analogId, DateTime inputDateTime)
        {
            if (Request.IsAjaxRequest())
            {
                IDictionary<string, string> dic = _analogHistoryRepos.GetTwoEndpointVal(analogId, inputDateTime);
                return Json(dic, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        public ActionResult AddEnergyHistoryAjax(int analogNo, DateTime time, double value)
        {
            if (Request.IsAjaxRequest())
            {
                if (_analogHistoryRepos.AddEnergyHistory(analogNo, time, value))
                {
                    var resultData = new
                    {
                        ifSucceed = true
                    };
                    return Json(resultData, JsonRequestBehavior.AllowGet);
                }
                return Json(new { ifSucceed = false }, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        public ActionResult QueryHistoryVal(int? analogNo , string analogName)
        {
            if (analogNo != null && analogName != null)
            {
                ViewData["analogNo"] = analogNo;
                ViewData["analogName"] = analogName;
            }
            return View();
        }

        public ActionResult QueryAllEnergyHistoryAjax(int analogNo, DateTime startTime, DateTime endTime)
        {
            if (Request.IsAjaxRequest())
            {
                IList historyValList = _analogHistoryRepos.GetEnergyHistory(analogNo, startTime, endTime).ToList();
                var resultData = new
                {
                    data = historyValList
                };
                return Json(resultData, JsonRequestBehavior.AllowGet);
            }
            return null;
        }
    }
}
