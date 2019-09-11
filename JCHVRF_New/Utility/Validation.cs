using JCHVRF.BLL;
using System;
using System.Data;
using System.Linq;
using Langauge = JCHVRF_New.LanguageData.LanguageViewModel;

namespace JCHVRF_New.Utility
{
    public static class Validation
    {
        public static bool IsValidatedSystemVRF(JCHVRF.Model.Project project, JCHVRF.Model.NextGen.SystemVRF CurrentSystem, out string ErrMsg)
        {
            bool Result = true;
            ErrMsg = "";
            var IndoorListForCurrentSystem = project.RoomIndoorList.Where(ri => ri.SystemID == (((JCHVRF.Model.SystemBase)CurrentSystem).Id)).ToList();

            if (IndoorListForCurrentSystem == null || IndoorListForCurrentSystem.Count == 0)
            {
                Result = false;
                ErrMsg = Langauge.Current.GetMessage("ALERT_INDOOR");
            }
            else if (CurrentSystem == null)
            {
                Result = false;
                ErrMsg = string.Format(JCHVRF.Const.ValidationMessage.AtleastOneODU);
            }
            else
            {
                if (CurrentSystem.SelOutdoorType == null)
                {
                    Result = false;
                    ErrMsg = string.Format(Langauge.Current.GetMessage("ERROR_PROPERTY") + CurrentSystem.Name);
                }
                else if (CurrentSystem.Power == null || CurrentSystem.Power.Trim() == "")
                {
                    Result = false;
                    ErrMsg = string.Format(Langauge.Current.GetMessage("ERROR_POWER_PROPERTY") + CurrentSystem.Name);
                }
                else if (CurrentSystem.MaxRatio == 0)
                {
                    Result = false;
                    ErrMsg = string.Format(Langauge.Current.GetMessage("ERROR_MAXRATIO_PROPERTY") + CurrentSystem.Name);
                }
            }
            foreach (var indoor in IndoorListForCurrentSystem)
            {
                if (indoor.IndoorItem == null || indoor.IndoorItem.Type == null || indoor.IndoorItem.ModelFull == null)
                {
                    Result = false;
                    ErrMsg = string.Format(Langauge.Current.GetMessage("ERROR_INDOOR_PROPERTY") + indoor.IndoorName);
                    break;
                }
            }

            return Result;

        }

        public static bool ValidateAddFlow(Lassalle.WPF.Flow.AddFlow addflow, JCHVRF.Model.NextGen.SystemVRF CurrentSystem)
        {           
            var Nodes = Enumerable.OfType<JCHVRF.Model.NextGen.JCHNode>(addflow.Items).ToList();
            foreach(var node in Nodes)
            {
                if(node is JCHVRF.Model.NextGen.MyNodeYP )
                {
                    if (((JCHVRF.Model.NextGen.MyNodeYP)node).ChildCount != 2)
                        return false;                    
                }
                if (node is JCHVRF.Model.NextGen.MyNodeOut)
                {
                    var MyNodeOut = (JCHVRF.Model.NextGen.MyNodeOut)node;
                    if (MyNodeOut.Links.Count == 0)
                        return false;
                }
                if (node is JCHVRF.Model.NextGen.MyNodeCH)
                {
                    if((((JCHVRF.Model.NextGen.MyNodeCH)node).ChildNode is JCHVRF.Model.NextGen.MyNodeIn ))
                    {
                        var MyNodeIn = (JCHVRF.Model.NextGen.MyNodeIn)((JCHVRF.Model.NextGen.MyNodeCH)node).ChildNode;
                        if (MyNodeIn.MyInLinks == null || MyNodeIn.MyInLinks.Count == 0 || MyNodeIn.Links.Count==0)
                            return false;                        
                    }                        
                }
            }           
            return true;
        }

        public static bool IsIndoorIsValidForODUSeries(JCHVRF.Model.NextGen.SystemVRF CurrentSys)
        {
            var listRISelected = JCHVRF.Model.Project.GetProjectInstance.RoomIndoorList.Where(ri => ri.SystemID == (((JCHVRF.Model.SystemBase)CurrentSys).Id)).ToList();

            if (ProjectBLL.IsSupportedUniversalSelection(JCHVRF.Model.Project.GetProjectInstance))
            {
                MyProductTypeBLL productTypeBll = new MyProductTypeBLL();
                DataTable typeDt = productTypeBll.GetIduTypeBySeries(JCHVRF.Model.Project.GetProjectInstance.BrandCode, JCHVRF.Model.Project.GetProjectInstance.SubRegionCode, CurrentSys.Series);
                if (typeDt != null && typeDt.Rows.Count > 0)
                {
                    foreach (var idu in listRISelected)
                    {
                        if (idu.IndoorItem != null)
                        {
                            if (!typeDt.AsEnumerable().Any(row => row["IDU_UnitType"].ToString() == idu.IndoorItem.Type))
                                return false;
                        }
                        else
                            return false;
                    }
                }

            }
            else
            {
                IndoorBLL bll = new IndoorBLL(JCHVRF.Model.Project.GetProjectInstance.SubRegionCode, JCHVRF.Model.Project.GetProjectInstance.BrandCode);
                string colName = "UnitType";
                DataTable typeDt = bll.GetIndoorFacCodeList(CurrentSys.ProductType);
                foreach (DataRow dr in typeDt.Rows)
                {
                    if (System.Convert.ToInt32(dr["FactoryCount"].ToString()) > 1)
                    {
                        dr[colName] = AlterUnitTypeBasedOnFactoryCode(dr["FactoryCode"].ToString(), Convert.ToString(dr[colName]));
                    }
                }
                var dv = new DataView(typeDt);
                if (CurrentSys.ProductType == "Comm. Tier 2, HP")
                {

                    if (CurrentSys.Series == "Commercial VRF HP, FSN6Q" || CurrentSys.Series == "Commercial VRF HP, JVOH-Q")
                    {
                        dv.RowFilter = "UnitType not in ('High Static Ducted-HAPE','Medium Static Ducted-HAPE','Low Static Ducted-HAPE','High Static Ducted-SMZ','Medium Static Ducted-SMZ','Four Way Cassette-SMZ')";
                    }
                    else
                    {
                        dv.RowFilter = "UnitType <>'Four Way Cassette-HAPQ'";
                    }
                }
                typeDt = dv.Table;
                if (typeDt != null && typeDt.Rows.Count > 0)
                {
                    foreach (var idu in listRISelected)
                    {
                        if (idu.IndoorItem != null)
                        {
                            var factoryCode = idu.IndoorItem.GetFactoryCode();
                            var type = AlterUnitTypeBasedOnFactoryCode(factoryCode, idu.IndoorItem.Type);
                            

                            if (!typeDt.AsEnumerable().Any(row => (row["UnitType"].ToString() == idu.IndoorItem.Type || 
                                    (System.Convert.ToInt32(row["FactoryCount"].ToString()) > 1 && row["UnitType"].ToString() == type))))
                                return false;
                        }
                        else
                            return false;
                    }
                }
            }
            return true;

        }

        private static string AlterUnitTypeBasedOnFactoryCode(string factoryCode, string iduType)
        {
            var type = iduType;
            switch (factoryCode.ToString())
            {
                case "G":
                    type += "-GZF";
                    break;
                case "E":
                    type += "-HAPE";
                    break;
                case "Q":
                    type += "-HAPQ";
                    break;
                case "B":
                    type += "-HAPB";
                    break;
                case "I":
                    type += "-HHLI";
                    break;
                case "M":
                    type += "-HAPM";
                    break;
                case "S":
                    type += "-SMZ";
                    break;
            }
            return type;
        }
    }
}
