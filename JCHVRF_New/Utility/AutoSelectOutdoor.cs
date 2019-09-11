using JCHVRF.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextGenModel = JCHVRF.Model.NextGen;
using NextGenBLL = JCHVRF.MyPipingBLL.NextGen;
using JCHVRF.Model;
using JCHVRF_New.Model;

namespace JCHVRF_New.Utility
{
   public class AutoSelectOutdoor
    {
        public AutoSelectODUModel ReselectOutdoor(NextGenModel.SystemVRF CurrVRF, List<RoomIndoor> RoomIndoorList)
        {           
            List<string> MSGList = new List<string>();
            List<string> ERRList = new List<string>();
            SelectOutdoorResult result;
            try
            {
                var IndoorListForCurrentSystem = RoomIndoorList.Where(ri => ri.SystemID == (((JCHVRF.Model.SystemBase)CurrVRF).Id)).ToList();
                if (!Validation.IsIndoorIsValidForODUSeries(CurrVRF))
                {
                    result = SelectOutdoorResult.NotMatch;
                }
                else if (IndoorListForCurrentSystem != null && IndoorListForCurrentSystem.Count > 0)
                {
                    if (CurrVRF.IsAuto == true)
                    {
                        if (ProjectBLL.IsSupportedUniversalSelection(JCHVRF.Model.Project.GetProjectInstance))
                        {
                            result = Global.DoSelectUniversalODUFirst(CurrVRF, IndoorListForCurrentSystem, JCHVRF.Model.Project.GetProjectInstance, CurrVRF.Series, out ERRList, out MSGList);
                        }
                        else
                        {
                            result = Global.DoSelectOutdoorODUFirst(CurrVRF, IndoorListForCurrentSystem, JCHVRF.Model.Project.GetProjectInstance, out ERRList, out MSGList);
                        }
                    }
                    else
                    {
                        if (ProjectBLL.IsSupportedUniversalSelection(JCHVRF.Model.Project.GetProjectInstance))
                        {
                            result = Global.DoSelectUniversalOduManual(CurrVRF, IndoorListForCurrentSystem, JCHVRF.Model.Project.GetProjectInstance, CurrVRF.Series, out ERRList);
                        }
                        else
                        {
                            result = Global.DoSelectOutdoorManual(CurrVRF, IndoorListForCurrentSystem, JCHVRF.Model.Project.GetProjectInstance, out ERRList);
                        }
                    }
                    //if (result == SelectOutdoorResult.OK)
                    //{
                    //    int SysIndex = JCHVRF.Model.Project.CurrentProject.SystemListNextGen.FindIndex(sys => sys.Id == CurrVRF.Id);
                    //    if (SysIndex != -1)
                    //        JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen[SysIndex] = CurrVRF;
                    //}
                }
                else
                    result = SelectOutdoorResult.NotMatch;
            }
            catch(Exception ex) { result = SelectOutdoorResult.NotMatch; }
            return new AutoSelectODUModel{
                ERRList = ERRList,
                MSGList= MSGList,
                SelectionResult= result
            };
        }
    }
}
