/*
 * clh 20111223
 * State: Working
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace JCHVRF.Model
{
    /// <summary>
    /// 更改了ModelName的命名规则后的机组应用该 OptionB，区分OptionA
    /// add on 20120424 clh for Phase2
    /// </summary>
    [Serializable]
    public class OptionB : ModelBase
    {
        public OptionB(string modelFull)
        {
            this._modelFull = modelFull;
        }
        private string _modelFull;
        //#region Auxiliary Electric Heater 电辅热
        //private bool _AEH0;
        ///// <summary>
        ///// Auxiliary Electric Heater -0
        ///// </summary>
        //public bool AEH0
        //{
        //    get { return _AEH0; }
        //    set { this.SetValue(ref _AEH0, value); }
        //}

        //private bool _AEH2;
        ///// <summary>
        ///// Auxiliary Electric Heater -2
        ///// </summary>
        //public bool AEH2
        //{
        //    get { return _AEH2; }
        //    set { this.SetValue(ref _AEH2, value); }
        //}

        //private bool _AEH4;
        ///// <summary>
        ///// Auxiliary Electric Heater -4
        ///// </summary>
        //public bool AEH4
        //{
        //    get { return _AEH4; }
        //    set { this.SetValue(ref _AEH4, value); }
        //}

        //// add on 20130912 clh 新需求：增加 Option 价格属性
        //private double _AEH0_Price;
        ///// <summary>
        ///// 0号电加热价钱
        ///// </summary>
        //public double AEH0_Price
        //{
        //    get { return _AEH0_Price; }
        //    set { this.SetValue(ref _AEH0_Price, value); }
        //}

        //private double _AEH2_Price;
        ///// <summary>
        ///// 2号电加热价钱
        ///// </summary>
        //public double AEH2_Price
        //{
        //    get { return _AEH2_Price; }
        //    set { this.SetValue(ref _AEH2_Price, value); }
        //}

        //private double _AEH4_Price;
        ///// <summary>
        ///// 4号电加热价钱
        ///// </summary>
        //public double AEH4_Price
        //{
        //    get { return _AEH4_Price; }
        //    set { this.SetValue(ref _AEH4_Price, value); }
        //}

        //private string _defaultAEH;
        ///// <summary>
        ///// 默认电加热标记
        ///// </summary>
        //public string DefaultAEH
        //{
        //    get { return _defaultAEH; }
        //    set { this.SetValue(ref _defaultAEH, value); }
        //}

        //private string _selectedAEH;
        ///// <summary>
        ///// 选择的电加热选配项
        ///// </summary>
        //public string SelectedAEH
        //{
        //    get { return _selectedAEH; }
        //    set {
        //        if (string.IsNullOrEmpty(value))
        //            _selectedAEH = _defaultAEH;
        //        else
        //            _selectedAEH, value);
        //    }
        //}
        //#endregion

        //#region Drainage Pump and Expansion Device 水泵/节流装置（预留）
        //private bool _DPEDN;
        ///// <summary>
        ///// Drainage Pump and Expansion Device -N
        ///// </summary>
        //public bool DPEDN
        //{
        //    get { return _DPEDN; }
        //    set { this.SetValue(ref _DPEDN, value); }
        //}

        //private bool _DPEDP;
        ///// <summary>
        ///// Drainage Pump and Expansion Device -P
        ///// </summary>
        //public bool DPEDP
        //{
        //    get { return _DPEDP; }
        //    set { this.SetValue(ref _DPEDP, value); }
        //}

        //private bool _DPEDQ;
        ///// <summary>
        ///// Drainage Pump and Expansion Device -Q
        ///// </summary>
        //public bool DPEDQ
        //{
        //    get { return _DPEDQ; }
        //    set { this.SetValue(ref _DPEDQ, value); }
        //}

        //private bool _DPEDR;
        ///// <summary>
        ///// Drainage Pump and Expansion Device -R
        ///// </summary>
        //public bool DPEDR
        //{
        //    get { return _DPEDR; }
        //    set { this.SetValue(ref _DPEDR, value); }
        //}

        //// add on 20130912 clh 新需求：增加 Option 价格属性
        //private double _DPEDN_Price;

        //public double DPEDN_Price
        //{
        //    get { return _DPEDN_Price; }
        //    set { this.SetValue(ref _DPEDN_Price, value); }
        //}

        //private double _DPEDP_Price;

        //public double DPEDP_Price
        //{
        //    get { return _DPEDP_Price; }
        //    set { this.SetValue(ref _DPEDP_Price, value); }
        //}

        //private double _DPEDQ_Price;

        //public double DPEDQ_Price
        //{
        //    get { return _DPEDQ_Price; }
        //    set { this.SetValue(ref _DPEDQ_Price, value); }
        //}

        //private double _DPEDR_Price;

        //public double DPEDR_Price
        //{
        //    get { return _DPEDR_Price; }
        //    set { this.SetValue(ref _DPEDR_Price, value); }
        //}

        //private string _defaultDPED;
        ///// <summary>
        ///// 默认的水泵、节流装置
        ///// </summary>
        //public string DefaultDPED
        //{
        //    get {
        //        return _defaultDPED;
        //    }
        //    set { this.SetValue(ref _defaultDPED, value); }
        //}

        //private string _selectedDPED;
        ///// <summary>
        ///// 选择的水泵、节流装置
        ///// </summary>
        //public string SelectedDPED
        //{
        //    get {
        //        if (string.IsNullOrEmpty(_selectedDPED))
        //            _selectedDPED = _defaultDPED;
        //        return _selectedDPED;
        //    }
        //    set { this.SetValue(ref _selectedDPED, value); }
        //}

        //#endregion

        //#region Non-Standard Controller and Filter 控制器 && 过滤 （&&非标）
        ////private bool _CFR;
        /////// <summary>
        /////// Non-Standard Controller and Filter - R
        /////// </summary>
        ////public bool CFR
        ////{
        ////    get { return _CFR; }
        ////    set { this.SetValue(ref _CFR, value); }
        ////}

        ////private bool _CFW;
        /////// <summary>
        /////// Non-Standard Controller and Filter - W
        /////// </summary>
        ////public bool CFW
        ////{
        ////    get { return _CFW; }
        ////    set { this.SetValue(ref _CFW, value); }
        ////}

        //private bool _CFU;
        ///// <summary>
        ///// Non-Standard Controller and Filter - U
        ///// 增加一个遥控器（YDCC？ 接收器）
        ///// </summary>
        //public bool CFU
        //{
        //    get { return _CFU; }
        //    set { this.SetValue(ref _CFU, value); }
        //}

        //private bool _CF_F;
        ///// <summary>
        ///// Non-Standard Controller and Filter - /F
        ///// 增加过滤网（YDCF）
        ///// </summary>
        //public bool CF_F
        //{
        //    get { return _CF_F; }
        //    set { this.SetValue(ref _CF_F, value); }
        //}

        //private double _CFU_Price;
        ///// <summary>
        ///// 增加一个遥控器的价格
        ///// </summary>
        //public double CFU_Price
        //{
        //    get { return _CFU_Price; }
        //    set { this.SetValue(ref _CFU_Price, value); }
        //}

        //private double _CF_F_Price;
        ///// <summary>
        ///// 过滤网价格
        ///// </summary>
        //public double CF_F_Price
        //{
        //    get { return _CF_F_Price; }
        //    set { this.SetValue(ref _CF_F_Price, value); }
        //}

        //private string _defaultCF;
        ///// <summary>
        ///// 默认的控制器 && 过滤 （&&非标）
        ///// </summary>
        //public string DefaultCF
        //{
        //    get {
        //        //_defaultCF = "";
        //        //if (!string.IsNullOrEmpty(_modelFull))
        //        //{
        //        //    if (!_modelFull.StartsWith("YDCF"))
        //        //    {
        //        //        if (_modelFull.Length > 15)
        //        //            _defaultCF = _modelFull.Substring(_modelFull.Length - 1, 1);  // { R|W|U|F }
        //        //        else if (_modelFull.Length > 14)
        //        //        {
        //        //            // 获取第 15 位
        //        //            string s = _modelFull.Substring(14, 1);
        //        //            if (!Global.IsNumber(s))
        //        //                _defaultCF = s;
        //        //        }
        //        //    }
        //        //    else
        //        //    {
        //        //        if (_modelFull.Length > 16)
        //        //            _defaultCF = _modelFull.Substring(_modelFull.Length - 1, 1);  // { R|W|U|F }
        //        //        else if (_modelFull.Length > 15)
        //        //        {
        //        //            // 获取第 16 位
        //        //            string s = _modelFull.Substring(15, 1);
        //        //            if (!Global.IsNumber(s))
        //        //                _defaultCF = s;
        //        //        }
        //        //    }
        //        //}
        //        return _defaultCF;
        //    }
        //    set { this.SetValue(ref _defaultCF, value); }
        //}

        //private string _selectedCF;
        ///// <summary>
        ///// 选择的水泵、节流装置
        ///// </summary>
        //public string SelectedCF
        //{
        //    get
        //    {
        //        if (string.IsNullOrEmpty(_selectedCF))
        //            _selectedCF = _defaultCF;
        //        return _selectedCF;
        //    }
        //    set { this.SetValue(ref _selectedCF, value); }
        //}

        //#endregion

        /// update on 20141205 clh

        private int _optionID_AEH;

        public int OptionID_AEH
        {
            get { return _optionID_AEH; }
            set { this.SetValue(ref _optionID_AEH, value); }
        }

        private int _optionID_DPED;

        public int OptionID_DPED
        {
            get { return _optionID_DPED; }
            set { this.SetValue(ref _optionID_DPED, value); }
        }

        private int _optionID_CF;

        public int OptionID_CF
        {
            get { return _optionID_CF; }
            set { this.SetValue(ref _optionID_CF, value); }
        }

        private int _optionID_UP;

        public int OptionID_UP
        {
            get { return _optionID_UP; }
            set { this.SetValue(ref _optionID_UP, value); }
        }

        private int _optionID_TIO;

        public int OptionID_TIO
        {
            get { return _optionID_TIO; }
            set { this.SetValue(ref _optionID_TIO, value); }
        }

        private int _optionID_Power;

        public int OptionID_Power
        {
            get { return _optionID_Power; }
            set { this.SetValue(ref _optionID_Power, value); }
        }

        private int _optionID_Insulation;

        public int OptionID_Insulation
        {
            get { return _optionID_Insulation; }
            set { this.SetValue(ref _optionID_Insulation, value); }
        }

        private ExtraOption _optionAEH;
        /// <summary>
        /// Auxiliary Electric Heater 电辅热
        /// </summary>
        public ExtraOption OptionAEH
        {
            get {
            //    if (_optionID_AEH > 0 && _optionAEH == null)
            //        _optionAEH = AccessVRFDB.GetExtraOption(_optionID_AEH);
                return _optionAEH;
            }
        }

        private ExtraOption _optionDPED;
        /// <summary>
        /// Drainage Pump and Expansion Device 水泵/节流装置（预留）
        /// </summary>
        public ExtraOption OptionDPED
        {
            get {
            //    if (_optionID_DPED > 0 && _optionDPED==null)
            //        _optionDPED = AccessVRFDB.GetExtraOption(_optionID_DPED);
                return _optionDPED;
            }
        }

        private ExtraOption _optionCF;
        /// <summary>
        /// Non-Standard Controller and Filter 控制器 && 过滤 （&&非标）
        /// </summary>
        public ExtraOption OptionCF
        {
            get {
                //if (_optionID_CF > 0 && _optionCF == null)
                //    _optionCF = AccessVRFDB.GetExtraOption(_optionID_CF);
                return _optionCF;
            }
        }

        private ExtraOption _optionUP;
        /// <summary>
        /// Unit Panel 面板选项
        /// </summary>
        public ExtraOption OptionUP
        {
            get {
                //if (_optionID_UP > 0 && _optionUP == null)
                //    _optionUP = AccessVRFDB.GetExtraOption(_optionID_UP);
                return _optionUP;
            }

        }

        private ExtraOption _optionTIO;
        /// <summary>
        /// TIO2
        /// </summary>
        public ExtraOption OptionTIO
        {
            get {
                //if (_optionID_TIO > 0 && _optionTIO == null)
                //    _optionTIO = AccessVRFDB.GetExtraOption(_optionID_TIO);
                return _optionTIO;
            }
        }

        private ExtraOption _optionPower;
        /// <summary>
        /// Add on 20141121 clh,新增的Option类型（电源类型）
        /// </summary>
        public ExtraOption OptionPower
        {
            get {
                //if (_optionID_Power > 0 && _optionPower == null)
                //    _optionPower = AccessVRFDB.GetExtraOption(_optionID_Power);
                return _optionPower;
            }
        }

        private ExtraOption _optionInsulation;
        /// <summary>
        /// Add on 20141121 clh,新增的Option类型（水盘带保温棉，0|1）
        /// </summary>
        public ExtraOption OptionInsulation
        {
            get {
                //if (_optionID_Insulation > 0 && _optionInsulation == null)
                //    _optionInsulation = AccessVRFDB.GetExtraOption(_optionID_Insulation);
                return _optionInsulation;
            }
        }


        /// 获取选择 Option 后，新的 ModelName 属性
        /// <summary>
        /// 获取选择 Option 后，新的 ModelName 属性
        /// </summary>
        /// <returns></returns>
        public string GetNewModelWithOptionB()
        {
            string ret = _modelFull;
            int index = 8;// Indoor Unit 
            if (ret.StartsWith("YDCF"))
            {
                index = 9;
            }

            if (_optionAEH != null)
            {
                ret = ret.Remove(index, 1);
                ret = ret.Insert(index, _optionAEH.SelectedValue);
            }

            index++; //处理下一个Option标记位
            if (_optionDPED != null)
            {
                ret = ret.Remove(index, 1);
                ret = ret.Insert(index, _optionDPED.SelectedValue);
            }

            if (_optionCF != null)
            {
                string flag = _optionCF.SelectedValue;

                if (_optionCF.SelectedValue == "F")
                    flag = "/" + _optionCF.SelectedValue;
                else if (_optionCF.SelectedValue == "0")
                    flag = "";

                ret += flag;
            }

            return ret;
        }

        /// 重置当前已选的 Option
        /// <summary>
        /// 重置当前已选的 Option, 将所有已选项改为默认值
        /// </summary>
        public void Reset()
        {
            if (_optionAEH != null)
                _optionAEH.SelectedValue = _optionAEH.DefaultValue;

            if (_optionDPED != null)
                _optionDPED.SelectedValue = _optionDPED.DefaultValue;

            if (_optionCF != null)
                _optionCF.SelectedValue = _optionCF.DefaultValue;

            if (_optionUP != null)
                _optionUP.SelectedValue = _optionUP.DefaultValue;

            if (_optionTIO != null)
                _optionTIO.SelectedValue = _optionTIO.DefaultValue;

            if (_optionPower != null)
                _optionPower.SelectedValue = _optionPower.DefaultValue;

            if (_optionInsulation != null)
                _optionInsulation.SelectedValue = _optionInsulation.DefaultValue;
        }

        /// 是否选择了 Option 项目
        /// <summary>
        /// 是否选择了 Option 项目
        /// </summary>
        /// <returns></returns>
        public bool HasOption()
        {
            if (_optionAEH != null && _optionAEH.HasOption())
                return true;

            if (_optionDPED != null && _optionDPED.HasOption())
                return true;

            if (_optionCF != null && _optionCF.HasOption())
                return true;

            if (_optionUP != null && _optionUP.HasOption())
                return true;

            if (_optionTIO != null && _optionTIO.HasOption())
                return true;

            if (_optionPower != null && _optionPower.HasOption())
                return true;

            if (_optionInsulation != null && _optionInsulation.HasOption())
                return true;

            return false;
        }

    }

    /// Report中 Option 描述, Report 中应用
    /// <summary>
    /// Report中 Option 描述, Report 中应用
    /// </summary>
    public class OptionDescription
    {
        public static DataTable _data;
        public static string GetDescription(string opType, string opValue)
        {
            string ret = "";
            //if (_data == null)
            //{
            //    AccessVRFDB access = new AccessVRFDB();
            //    _data = access.GetOptionDescriptionList();
            //}
            //if (_data != null)
            //{
            //    foreach (DataRow dr in _data.Rows)
            //    {
            //        if (dr["OptionType"].ToString() == opType
            //            && dr["OptionValue"].ToString() == opValue)
            //        {
            //            if (Util.IsChinese())
            //                ret = dr["Description_Zh"].ToString();
            //            else
            //                ret = dr["Description_En"].ToString();
            //        }
            //    }
            //}
            return ret;
        }
    }

    // 注销
    //internal class OptionValue {
    //    public const string AEH0 = "0";
    //    public const string AEH2 = "2";
    //    public const string AEH4 = "4";

    //    public const string DPEDN = "N";
    //    public const string DPEDP = "P";
    //    public const string DPEDQ = "Q";
    //    public const string DPEDR = "R";

    //    public const string CFU = "U";
    //    public const string CF_F = "F";


    //    // 采用新的Option处理方式后（ExtraOption类），以下代码可以注销了，20141209 clh
    //    public const string UPN = "0";
    //    public const string UPW = "1";
    //    public const string UPWO = "2";

    //    public const string TON = "0";
    //    public const string TOA = "1";
    //}

    //public enum OptionType {
    //    //AEH, DPED, CF, 
    //    //UP, TIO, POWER50A, POWER50B, POWER60A, POWER60B, POWER5060A, POWER5060B, INSULATION 
    //}

    /// Add on 20141121---更改Option的处理方式，按照不同的Option类型区分不同的Option对象
    /// <summary>
    /// Add on 20141121---更改Option的处理方式，按照不同的Option类型区分不同的Option对象
    /// ---配合数据表OptionRule与OptionDescription（对应的中英文显示）一起使用
    /// ---暂时仅供PowerSupply使用（后期看情况将其余Option合并）
    /// </summary>
    [Serializable]
    public class ExtraOption : ModelBase
    {
        public ExtraOption(int id, int modelIndex, string optionType, int optionQty)
        {
            this._id = id;
            this._modelIndex = modelIndex;
            this._optionType = optionType;
            this._optionQty = optionQty;
        }

        private int _id;
        /// <summary>
        /// 唯一编码，表示Option的规则编号
        /// </summary>
        public int Id
        {
            get { return _id; }
        }

        private int _modelIndex;
        /// <summary>
        /// 在ModelName中的位置，值为-1则忽略
        /// </summary>
        public int ModelIndex
        {
            get { return _modelIndex; }
        }

        private string _optionType;
        /// <summary>
        /// 枚举类型 OptionType
        /// </summary>
        public string OptionType
        {
            get { return _optionType; }
        }

        private int _optionQty;
        /// <summary>
        /// 当前类型下的Option数量
        /// </summary>
        public int OptionQty
        {
            get { return _optionQty; }
        }

        private string _defaultValue;
        /// <summary>
        /// 当前Option类型的默认值，对应数据表中的StdValue
        /// </summary>
        public string DefaultValue
        {
            get { return _defaultValue; }
            set { this.SetValue(ref _defaultValue, value); }
        }

        private string _selectedValue;
        /// <summary>
        /// 当前Option类型用户选择的值，默认等于DefaultValue
        /// </summary>
        public string SelectedValue
        {
            get { return _selectedValue; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    this.SetValue(ref _selectedValue, _defaultValue);
                else
                    this.SetValue(ref _selectedValue, value);

            }
        }

        public List<OptionElement> OptionList;

        /// 指定项的描述（中|英）
        /// <summary>
        /// 指定项的描述（中|英）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string GetDescription(string value)
        {
            return OptionDescription.GetDescription(_optionType, value);
        }

        /// 选中项的描述（中|英）
        /// <summary>
        /// 选中项的描述（中|英）
        /// </summary>
        /// <returns></returns>
        public string GetDescription()
        {
            return OptionDescription.GetDescription(_optionType, SelectedValue);
        }

        /// 当前的Option选项是否改变（与默认值不一致）
        /// <summary>
        /// 当前的Option选项是否改变（与默认值不一致）
        /// </summary>
        /// <returns></returns>
        public bool HasOption()
        {
            return _selectedValue != _defaultValue && OptionList != null;
        }

        /// 获取当前选取的Option选项的价格
        /// <summary>
        /// 获取当前选取的Option选项的价格
        /// </summary>
        /// <returns></returns>
        public double GetPrice()
        {
            if (HasOption())
            {
                foreach (OptionElement oe in this.OptionList)
                {
                    if (oe.Value == _selectedValue)
                        return oe.Price;
                }
            }
            return 0;
        }
    }

    /// 一个Option对象的一个子选项
    /// <summary>
    /// 一个Option对象的一个子选项
    /// </summary>
    [Serializable]
    public class OptionElement
    {
        private string _value;
        /// <summary>
        /// 值
        /// </summary>
        public string Value
        {
            get { return _value; }
        }

        private bool _selectable;
        /// <summary>
        /// 是否可选
        /// </summary>
        public bool Selectable
        {
            get { return _selectable; }
        }

        private double _price;
        /// <summary>
        /// 选配项价格
        /// </summary>
        public double Price
        {
            get { return _price; }
        }

        public OptionElement(string value, bool selectable, double price)
        {
            _value = value;
            _selectable = selectable;
            _price = price;
        }
    }

    /// 室外机的Option选项
    /// <summary>
    /// 室外机的Option选项
    /// </summary>
    [Serializable]
    public class OptionOutdoor : ModelBase
    {
        private ExtraOption _optionPower;
        /// Add on 20141121 clh,新增的Option类型（电源类型）
        /// <summary>
        /// Add on 20141121 clh,新增的Option类型（电源类型）
        /// </summary>
        public ExtraOption OptionPower
        {
            get { return _optionPower; }
            set { this.SetValue(ref _optionPower, value); }
        }

        /// 重置当前已选的 Option
        /// <summary>
        /// 重置当前已选的 Option, 将所有已选项改为默认值
        /// </summary>
        public void Reset()
        {
            if (_optionPower != null)
                _optionPower.SelectedValue = _optionPower.DefaultValue;
        }

        /// 是否选择了 Option 项目
        /// <summary>
        /// 是否选择了 Option 项目
        /// </summary>
        /// <returns></returns>
        public bool HasOption()
        {
            if (_optionPower != null && _optionPower.HasOption())
                return true;
            return false;
        }

    }


    ///// <summary>
    ///// 尚未使用
    ///// </summary>
    //public class OptionYDCC
    //{
    //    private ExtraOption _optionPower;
    //    /// Add on 20141121 clh,新增的Option类型（电源类型）
    //    /// <summary>
    //    /// Add on 20141121 clh,新增的Option类型（电源类型）
    //    /// </summary>
    //    public ExtraOption OptionPower
    //    {
    //        get { return _optionPower; }
    //        set { this.SetValue(ref _optionPower, value); }
    //    }

    //    private ExtraOption _optionInsulation;
    //    /// <summary>
    //    /// 保温棉
    //    /// </summary>
    //    public ExtraOption OptionInsulation
    //    {
    //        get { return _optionInsulation; }
    //        set { this.SetValue(ref _optionInsulation, value); }
    //    }

    //    /// 重置当前已选的 Option
    //    /// <summary>
    //    /// 重置当前已选的 Option, 将所有已选项改为默认值
    //    /// </summary>
    //    public void Reset()
    //    {
    //        if (_optionPower != null)
    //            _optionPower.SelectedValue = _optionPower.DefaultValue;
    //        if (_optionInsulation != null)
    //            _optionInsulation.SelectedValue = _optionInsulation.DefaultValue;
    //    }

    //    /// 是否选择了 Option 项目
    //    /// <summary>
    //    /// 是否选择了 Option 项目
    //    /// </summary>
    //    /// <returns></returns>
    //    public bool HasOption()
    //    {
    //        if (_optionPower != null && _optionPower.SelectedValue != _optionPower.DefaultValue)
    //            return true;
    //        if (_optionInsulation != null && _optionInsulation.SelectedValue != _optionInsulation.DefaultValue)
    //            return true;
    //        return false;
    //    }

    //}

}
