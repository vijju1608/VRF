//********************************************************************
// 文件名: Accessory.cs
// 描述: 定义 VRF 项目中的附件类，室内机的可选附件
// 作者: clh
// 创建时间: 2016-2-3
// 修改历史: 
//********************************************************************

using JCBase.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace JCHVRF.Model
{
    [Serializable]
    public class Accessory : ModelBase
    {
        string _factoryCode;
        /// <summary>
        /// 工厂代码, 此处的代码与室内机的工厂代码不全一致
        /// </summary>
        public string FactoryCode
        {
            get { return _factoryCode; }
            set { this.SetValue(ref _factoryCode, value); }
        }

        string _brandCode;
        /// <summary>
        /// 品牌代码
        /// </summary>
        public string BrandCode
        {
            get { return _brandCode; }
            set { this.SetValue(ref _brandCode, value); }
        }

        string _unitType;
        /// <summary>
        /// 室内机产品类型
        /// </summary>
        public string UnitType
        {
            get { return _unitType; }
            set { this.SetValue(ref _unitType, value); }
        }

        double _minCapacity;
        /// <summary>
        /// 最小制冷量数值 kW
        /// </summary>
        public double MinCapacity
        {
            get { return _minCapacity; }
            set { this.SetValue(ref _minCapacity, value); }
        }

        double _maxCapacity;
        /// <summary>
        /// 最大制冷量数值 kW
        /// </summary>
        public double MaxCapacity
        {
            get { return _maxCapacity; }
            set { this.SetValue(ref _maxCapacity, value); }
        }

        string _type;
        /// <summary>
        /// 附件类型
        /// </summary>
        public string Type
        {
            get { return _type; }
            set { this.SetValue(ref _type, value); }
        }

        string _model_York;
        /// <summary>
        /// 附件型号名称(York)
        /// </summary>
        public string Model_York
        {
            get { return _model_York; }
            set { this.SetValue(ref _model_York, value); }
        }

        string _model_Hitachi;
        /// <summary>
        /// 附件型号名称(Hitachi)
        /// </summary>
        public string Model_Hitachi
        {
            get { return _model_Hitachi; }
            set { this.SetValue(ref _model_Hitachi, value); }
        }

        int _maxNumber;
        /// <summary>
        /// 最大可选数量
        /// </summary>
        public int MaxNumber
        {
            get { return _maxNumber; }
            set { this.SetValue(ref _maxNumber, value); }
        }

        bool _isDefault;
        /// <summary>
        /// 是否默认自带
        /// </summary>
        public bool IsDefault
        {
            get { return _isDefault; }
            set { this.SetValue(ref _isDefault, value); }
        }


        bool _isShared;
        /// <summary>
        /// Remote Control Switch 是否共享 on 2017-11-16 by xyj
        /// </summary>
        public bool IsShared
        {
            get { return _isShared; }
            set { this.SetValue(ref _isShared, value); }
        }

        //Adding New properties for JCHVRFNew

        #region fields
        //private string _selectedType;
        private string _selectedModelName;
        private static bool flag = false;
        private string _model;
        private string _description;
        private List<string> _modelName;
        //private int _maxCount;
        private int _count;
        private bool _isSelect;
        private bool _isApplyToSimilarUnit;
        private ObservableCollection<Accessory> _listAccessory;
        private string _selectedIDUModel;
        private static Dictionary<string, int> accType = new Dictionary<string, int>()
        { {"Half-size Remote Control Switch",0 },
            {"Remote Control Switch",0},
            { "Wireless Remote Control Switch",0 },
            { "Receiver Kit for Wireless Control",0} };
        string[] typeName1 =
        {
            "Wireless Remote Control Switch",
            //"T-Tube Connecting Kit", 
            //"Deodorant Air Filter", 
            "Long-life Filter Kit",
            "Long-life Filter Kit", 
            //"Antibacterial Long Life Air Filter",
            "Antibacterial Long-life Filter",
            "Anti-bacterial Air Filter",
            "Kit for Deodorant Filter (Deodorant Filter)",
            "Antibacterial Long-life Filter",
            "Antibacterial Air Filter"
        };
        string[] typeName2 =
        {
            "Receiver Kit for Wireless Control",//Wireless Receiver Kit
            //"Fresh Air Intake Kit", 
            //"Filter Box",
            "Filter Box",
            "Filter Box",
            //"Filter Box",
            "Kit for Deodorant Filter (Filter Box)",
            "Filter Box",
            "Kit for Deodorant Filter (Filter Box)",
            "Filter Box",
            "Filter Box"

        };
        string[] unitType =
        {
            "",
            //"Four Way Cassette",
            //"Four Way Cassette",
            "Medium Static Ducted",
            "High Static Ducted",
            //"Two Way Cassette",
            "Four Way Cassette",
            "Four Way Cassette",
            "Four Way Cassette",
            "Two Way Cassette",
            "Ceiling Suspended (NA)"
        };

        #endregion

        #region properties
        //public string SelectedType
        //{
        //    get { return _selectedType; }
        //    set { this.SetValue(ref _selectedType, value); }
        //}

        public string SelectedModelName
        {
            get { return _selectedModelName; }
            set { this.SetValue(ref _selectedModelName, value); }
        }


        //public string Type
        //{
        //    get { return _type; }
        //    set { this.SetValue(ref _type, value); }
        //}

        public string Model
        {
            get { return _model; }
            set { this.SetValue(ref _model, value); }
        }

        public List<string> ModelName
        {
            get
            {
                if (_modelName == null)
                    _modelName = new List<string>();
                return _modelName;
            }
            set { this.SetValue(ref _modelName, value); }
        }

        public string Description
        {
            get { return _description; }
            set { this.SetValue(ref _description, value); }
        }

        //public int MaxCount
        //{
        //    get { return _maxCount; }
        //    set
        //    {
        //        this.SetValue(ref _maxCount, value);

        //    }
        //}

        public int Count
        {
            get
            {

                return _count;
            }
            set
            {
                if (value > MaxNumber)
                {
                    //JCMsg.ShowWarningOK("Count can not be greater then maxnumber");
                    return;
                }

                if (value < 0)
                {
                   // JCMsg.ShowWarningOK("Count can not be below zero");
                    return;
                }

                this.SetValue(ref _count, value);

                //CheckAccessryCount();

            }
        }


        public bool IsSelect
        {
            get { return _isSelect; }
            set
            {
                this.SetValue(ref _isSelect, value);

                if (!IsSelect && !IsApplyToSimilarUnit)
                    Count = 0;
                else
                    Count = Count==0?1:Count;

                 //CheckAccessryCount();

            }
        }

        public bool IsApplyToSimilarUnit
        {
            get { return _isApplyToSimilarUnit; }
            set
            {
                this.SetValue(ref _isApplyToSimilarUnit, value);
                //if (!IsSelect && IsApplyToSimilarUnit)
                //    Count = 0;
                //if (IsApplyToSimilarUnit)
                //    IsSelect = true;
            }
        }
      
        #endregion properties

        #region methods



        public void CheckAccessryCount()
        {
            if (MaxNumber < _count && MaxNumber != 0)
            {
                JCMsg.ShowWarningOK("Count can not be greater then maxnumber ");
                _count = MaxNumber;
            }
            else if (_count < 0)
            {
                JCMsg.ShowWarningOK("Count can not be below zero!");
                _count = 0;
            }
            else if (accType.ContainsKey(Type))
            {

                if ((!flag && Type == "Wireless Remote Control Switch") || (!flag && Type == "Receiver Kit for Wireless Control")
                    || (Type == "Wireless Remote Control Switch" && Count == 0 && flag) || (Type == "Receiver Kit for Wireless Control" && Count == 0 && flag))
                {
                    accType["Wireless Remote Control Switch"] = Count;
                    accType["Receiver Kit for Wireless Control"] = Count;
                    if (Count == 0)
                        flag = false;
                    else
                        flag = true;
                }
                else
                {
                    accType[Type] = Count;

                    //flag = false;
                }

                if (accType["Half-size Remote Control Switch"] + accType["Remote Control Switch"] + accType["Wireless Remote Control Switch"] + accType["Receiver Kit for Wireless Control"] > 2)
                {
                    JCMsg.ShowWarningOK("Number exceeds limitation");
                    _count = Count - 1;
                    if (Type=="Wireless Remote Control Switch" || Type=="Receiver Kit for Wireless Control")
                    {
                        accType["Wireless Remote Control Switch"] = 0;
                        accType["Receiver Kit for Wireless Control"] = 0;
                        flag = false;
                    }
                    else
                        accType[Type] = Count-1;
                }
            }
        }
        #endregion
    }
}

