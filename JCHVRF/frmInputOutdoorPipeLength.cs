using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

//using Lassalle.Flow;
using JCHVRF.Model;
using JCHVRF.BLL;
using JCBase.UI;
using JCHVRF.VRFMessage;
using JCBase.Utility;

namespace JCHVRF
{
    public partial class frmInputOutdoorPipeLength : JCBase.UI.JCForm
    {
        private SystemVRF curSystemItem;
        private MyNodeOut _nodeOut;
        private string ut_length;
        private double[] _pipeLengthes;
        bool useDoublePipeB = false;

        public frmInputOutdoorPipeLength(SystemVRF sysItem, string region)
        {
            InitializeComponent();
            curSystemItem = sysItem;
            _nodeOut = sysItem.MyPipingNodeOut;

            //特殊处理，当以下系列时，不使用两个b管，使用g管
            if (region=="EU_W" || region=="EU_S" || region=="EU_E" || 
                sysItem.OutdoorItem.Series == "Commercial VRF HP, HNCQ" || 
                sysItem.OutdoorItem.Series == "Commercial VRF HP, HNBQ" || 
                sysItem.OutdoorItem.Series == "Commercial VRF HP, JVOHQ")
            {
                useDoublePipeB = false;
            }
        }

        private void frmInputOutdoorPipeLength_Load(object sender, EventArgs e)
        {
            JCSetLanguage();
            JCCallValidationManager = true;
            ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;

            jclblUnitLengthB.Text = ut_length;
            jclblUnitLengthC.Text = ut_length;
            jclblUnitLengthD.Text = ut_length;
            jclblUnitLengthE.Text = ut_length;
            jclblUnitLengthF.Text = ut_length;
            if (!useDoublePipeB)
            {
                jclblUnitLengthG.Text = ut_length;
            }

            BindToControl();
        }

        private void BindToControl()
        {
            if (_nodeOut == null || _nodeOut.UnitCount < 2) return;

            int pipeCount = _nodeOut.UnitCount - 2 + _nodeOut.UnitCount;
            _pipeLengthes = new double[pipeCount];
            if (_nodeOut.PipeLengthes != null)
            {
                for (int i = 0; i < pipeCount && i < _nodeOut.PipeLengthes.Length; i++)
                {
                    _pipeLengthes[i] = _nodeOut.PipeLengthes[i];
                }
            }

            Point ptPicBox = pictureBox1.Location;
            //图片左上角坐标
            Point ptPic = new Point(ptPicBox.X + 6, ptPicBox.Y + 6);

            //显示图片 + 标注定位
            if (_nodeOut.UnitCount == 2)
            {
                //ODU_3.png (38,65)(105,65)
                pictureBox1.ImageLocation = MyConfig.PipingNodeImageDirectory + "ODU_03.png";

                jclblMarkerB1.Location = new Point(ptPic.X + 38 + 35, ptPic.Y + 65);
                jclblMarkerC.Location = new Point(ptPic.X + 105 + 40, ptPic.Y + 65);
            } 
            else if (_nodeOut.UnitCount == 3)
            {
                //ODU_6.png (55,96)(35,60)(95,60)(157,60)
                pictureBox1.ImageLocation = MyConfig.PipingNodeImageDirectory + "ODU_06.png";

                jclblMarkerB1.Location = new Point(ptPic.X + 55, ptPic.Y + 96);
                jclblMarkerC.Location = new Point(ptPic.X + 35 + 30, ptPic.Y + 60);
                jclblMarkerD.Location = new Point(ptPic.X + 95 + 30, ptPic.Y + 60);
                jclblMarkerE.Location = new Point(ptPic.X + 157 + 30, ptPic.Y + 60);
            }
            else if (_nodeOut.UnitCount == 4)
            {
                //ODU_10.png (70,96)(20,52)(84,52)(142,52)(203,52)(263,52)
                pictureBox1.ImageLocation = MyConfig.PipingNodeImageDirectory + "ODU_10.png";
                if (!useDoublePipeB)
                {
                    jclblMarkerC.Location = new Point(ptPic.X + 70 + 30, ptPic.Y + 96);
                    jclblMarkerB1.Location = new Point(ptPic.X + 20 + 10, ptPic.Y + 52 + 15);
                    jclblMarkerD.Location = new Point(ptPic.X + 84 - 20, ptPic.Y + 52);
                    jclblMarkerE.Location = new Point(ptPic.X + 142 - 20, ptPic.Y + 52);
                    jclblMarkerF.Location = new Point(ptPic.X + 203 - 20, ptPic.Y + 52);
                    jclblMarkerG.Location = new Point(ptPic.X + 263 - 20, ptPic.Y + 52);
                }
                else
                {
                    jclblMarkerB1.Location = new Point(ptPic.X + 70 + 30, ptPic.Y + 96);
                    jclblMarkerG.Location = new Point(ptPic.X + 20 + 10, ptPic.Y + 52 + 15);
                    jclblMarkerG.Text = "b";
                    jclblMarkerC.Location = new Point(ptPic.X + 84 - 20, ptPic.Y + 52);
                    jclblMarkerD.Location = new Point(ptPic.X + 142 - 20, ptPic.Y + 52);
                    jclblMarkerE.Location = new Point(ptPic.X + 203 - 20, ptPic.Y + 52);
                    jclblMarkerF.Location = new Point(ptPic.X + 263 - 20, ptPic.Y + 52);
                }
            }

            //显示标注和文本框
            if (_nodeOut.UnitCount >= 2)
            {
                jclblMarkerB1.Visible = true;
                jclblMarkerC.Visible = true;

                jclblLengthB.Visible = true;
                jctxtLengthB.Visible = true;
                jclblUnitLengthB.Visible = true;
                jctxtLengthB.RequireValidation = true;

                jclblLengthC.Visible = true;
                jctxtLengthC.Visible = true;
                jclblUnitLengthC.Visible = true;
                jctxtLengthC.RequireValidation = true;
            }
            if (_nodeOut.UnitCount >= 3)
            {
                jclblMarkerD.Visible = true;
                jclblMarkerE.Visible = true;

                jclblLengthD.Visible = true;
                jctxtLengthD.Visible = true;
                jclblUnitLengthD.Visible = true;
                jctxtLengthD.RequireValidation = true;

                jclblLengthE.Visible = true;
                jctxtLengthE.Visible = true;
                jclblUnitLengthE.Visible = true;
                jctxtLengthE.RequireValidation = true;

                //2个piping connection kit之间的管长最大为0.5m
                jctxtLengthB.JCMaxValue = (float)Unit.ConvertToControl(0.5, UnitType.LENGTH_M, ut_length);
            }
            if (_nodeOut.UnitCount >= 4)
            {
                jclblMarkerG.Visible = true;
                jclblMarkerF.Visible = true;

                jclblLengthF.Visible = true;
                jctxtLengthF.Visible = true;
                jclblUnitLengthF.Visible = true;
                jctxtLengthF.RequireValidation = true;
                if (!useDoublePipeB)
                {
                    jclblLengthG.Visible = true;
                    jctxtLengthG.Visible = true;
                    jclblUnitLengthG.Visible = true;
                    jctxtLengthG.RequireValidation = true;
                }
            }

            //文本框赋值
            int index = 0;
            if (_nodeOut.UnitCount >= 2)
            {
                if (_nodeOut.UnitCount >= 4 && !useDoublePipeB)
                {
                    jctxtLengthC.Text = Unit.ConvertToControl(_pipeLengthes[index++], UnitType.LENGTH_M, ut_length).ToString("n2");
                    jctxtLengthB.Text = Unit.ConvertToControl(_pipeLengthes[index++], UnitType.LENGTH_M, ut_length).ToString("n2");
                }
                else
                {
                    jctxtLengthB.Text = Unit.ConvertToControl(_pipeLengthes[index++], UnitType.LENGTH_M, ut_length).ToString("n2");
                    if (_nodeOut.UnitCount >= 4 && useDoublePipeB)
                    {
                        //4个机组有2个b
                        index++;
                    }
                    jctxtLengthC.Text = Unit.ConvertToControl(_pipeLengthes[index++], UnitType.LENGTH_M, ut_length).ToString("n2");
                }
            }
            if (_nodeOut.UnitCount >= 3)
            {
                jctxtLengthD.Text = Unit.ConvertToControl(_pipeLengthes[index++], UnitType.LENGTH_M, ut_length).ToString("n2");
                jctxtLengthE.Text = Unit.ConvertToControl(_pipeLengthes[index++], UnitType.LENGTH_M, ut_length).ToString("n2");
            }
            if (_nodeOut.UnitCount >= 4)
            {
                jctxtLengthF.Text = Unit.ConvertToControl(_pipeLengthes[index++], UnitType.LENGTH_M, ut_length).ToString("n2");
                if (!useDoublePipeB)
                {
                    jctxtLengthG.Text = Unit.ConvertToControl(_pipeLengthes[index++], UnitType.LENGTH_M, ut_length).ToString("n2");
                }
            }
        }

        private void BindToSource()
        {
            if (_nodeOut == null) return;
            //从文本框取值
            int index = 0;
            if (_nodeOut.UnitCount >= 2)
            {
                if (_nodeOut.UnitCount >= 4 && !useDoublePipeB)
                {
                    _pipeLengthes[index++] = Unit.ConvertToSource(Convert.ToDouble(jctxtLengthC.Text), UnitType.LENGTH_M, ut_length);
                    _pipeLengthes[index++] = Unit.ConvertToSource(Convert.ToDouble(jctxtLengthB.Text), UnitType.LENGTH_M, ut_length);
                }
                else
                {
                    _pipeLengthes[index++] = Unit.ConvertToSource(Convert.ToDouble(jctxtLengthB.Text), UnitType.LENGTH_M, ut_length);
                    if (_nodeOut.UnitCount >= 4 && useDoublePipeB)
                    {
                        _pipeLengthes[index++] = Unit.ConvertToSource(Convert.ToDouble(jctxtLengthB.Text), UnitType.LENGTH_M, ut_length);
                    }
                    _pipeLengthes[index++] = Unit.ConvertToSource(Convert.ToDouble(jctxtLengthC.Text), UnitType.LENGTH_M, ut_length);
                }
            }
            if (_nodeOut.UnitCount >= 3)
            {
                _pipeLengthes[index++] = Unit.ConvertToSource(Convert.ToDouble(jctxtLengthD.Text), UnitType.LENGTH_M, ut_length);
                _pipeLengthes[index++] = Unit.ConvertToSource(Convert.ToDouble(jctxtLengthE.Text), UnitType.LENGTH_M, ut_length);
            }
            if (_nodeOut.UnitCount >= 4)
            {
                _pipeLengthes[index++] = Unit.ConvertToSource(Convert.ToDouble(jctxtLengthF.Text), UnitType.LENGTH_M, ut_length);
                if (!useDoublePipeB)
                {
                    _pipeLengthes[index++] = Unit.ConvertToSource(Convert.ToDouble(jctxtLengthG.Text), UnitType.LENGTH_M, ut_length);
                }
            }
        }

        /// <summary>
        /// 管长校验
        /// </summary>
        /// <returns></returns>
        private bool ValidateOthers()
        {
            // 多台室外机组成机组时，Connection kit之间的管长(b)不能小于0.5m add on 20170720 by Shen Junjie
            double betweenConnectionKits_Min = 0.5;
            for (int i = 0; i < _pipeLengthes.Length; i++)
            {
                double len = _pipeLengthes[i];
                //所有管长必须>0
                if (len <= 0)
                {
                    JCMsg.ShowErrorOK(Msg.PIPING_LINK_LENGTH);
                    return false;
                }
                //connection kit前面的管长不能小于0.5m add on 2018/8/3 by Shen Junjie
                if (len < betweenConnectionKits_Min)
                {
                    if ((_nodeOut.UnitCount == 3 && i < 1) || (_nodeOut.UnitCount == 4 && i < 2))
                    {
                        string msg = Unit.ConvertToControl(betweenConnectionKits_Min, UnitType.LENGTH_M, ut_length).ToString("n1") + ut_length;
                        JCMsg.ShowErrorOK(Msg.PIPING_BETWEEN_CONNECTION_KITS_MIN_LENGTH("b", msg));
                        return false;
                    }
                }
            }

            //第一Piping connection kit 与每一个室外机之间的管长不能大于10m
            double firstConnectionKitToODU_Max = curSystemItem.MaxFirstConnectionKitToEachODU;
            string firstConnectionKitToODU_Msg = Unit.ConvertToControl(firstConnectionKitToODU_Max, UnitType.LENGTH_M, ut_length).ToString("n2") + ut_length;
            if (_nodeOut.UnitCount == 2)
            {
                if (_pipeLengthes[0] > firstConnectionKitToODU_Max)
                {
                    JCMsg.ShowErrorOK(Msg.PIPING_FIRST_CONNECTION_KIT_TO_ODU_MAX_LENGTH("b", firstConnectionKitToODU_Msg));
                    return false;
                }
                else if (_pipeLengthes[1] > firstConnectionKitToODU_Max)
                {
                    JCMsg.ShowErrorOK(Msg.PIPING_FIRST_CONNECTION_KIT_TO_ODU_MAX_LENGTH("c", firstConnectionKitToODU_Msg));
                    return false;
                }
            }
            else if (_nodeOut.UnitCount == 3)
            {
                if (_pipeLengthes[1] > firstConnectionKitToODU_Max)
                {
                    JCMsg.ShowErrorOK(Msg.PIPING_FIRST_CONNECTION_KIT_TO_ODU_MAX_LENGTH("c", firstConnectionKitToODU_Msg));
                    return false;
                }
                else if (_pipeLengthes[0] + _pipeLengthes[2] > firstConnectionKitToODU_Max)
                {
                    JCMsg.ShowErrorOK(Msg.PIPING_FIRST_CONNECTION_KIT_TO_ODU_MAX_LENGTH("b+d", firstConnectionKitToODU_Msg));
                    return false;
                }
                else if (_pipeLengthes[0] + _pipeLengthes[3] > firstConnectionKitToODU_Max)
                {
                    JCMsg.ShowErrorOK(Msg.PIPING_FIRST_CONNECTION_KIT_TO_ODU_MAX_LENGTH("b+e", firstConnectionKitToODU_Msg));
                    return false;
                }
            }
            else if (_nodeOut.UnitCount == 4)
            {
                if (!useDoublePipeB)
                {
                    if (_pipeLengthes[1] + _pipeLengthes[2] > firstConnectionKitToODU_Max)
                    {
                        JCMsg.ShowErrorOK(Msg.PIPING_FIRST_CONNECTION_KIT_TO_ODU_MAX_LENGTH("b+d", firstConnectionKitToODU_Msg));
                        return false;
                    }
                    else if (_pipeLengthes[1] + _pipeLengthes[3] > firstConnectionKitToODU_Max)
                    {
                        JCMsg.ShowErrorOK(Msg.PIPING_FIRST_CONNECTION_KIT_TO_ODU_MAX_LENGTH("b+e", firstConnectionKitToODU_Msg));
                        return false;
                    }
                    else if (_pipeLengthes[0] + _pipeLengthes[4] > firstConnectionKitToODU_Max)
                    {
                        JCMsg.ShowErrorOK(Msg.PIPING_FIRST_CONNECTION_KIT_TO_ODU_MAX_LENGTH("c+f", firstConnectionKitToODU_Msg));
                        return false;
                    }
                    else if (_pipeLengthes[0] + _pipeLengthes[5] > firstConnectionKitToODU_Max)
                    {
                        JCMsg.ShowErrorOK(Msg.PIPING_FIRST_CONNECTION_KIT_TO_ODU_MAX_LENGTH("c+g", firstConnectionKitToODU_Msg));
                        return false;
                    }
                }
                else
                {
                    if (_pipeLengthes[0] + _pipeLengthes[2] > firstConnectionKitToODU_Max)
                    {
                        JCMsg.ShowErrorOK(Msg.PIPING_FIRST_CONNECTION_KIT_TO_ODU_MAX_LENGTH("b+c", firstConnectionKitToODU_Msg));
                        return false;
                    }
                    else if (_pipeLengthes[0] + _pipeLengthes[3] > firstConnectionKitToODU_Max)
                    {
                        JCMsg.ShowErrorOK(Msg.PIPING_FIRST_CONNECTION_KIT_TO_ODU_MAX_LENGTH("b+d", firstConnectionKitToODU_Msg));
                        return false;
                    }
                    else if (_pipeLengthes[1] + _pipeLengthes[4] > firstConnectionKitToODU_Max)
                    {
                        JCMsg.ShowErrorOK(Msg.PIPING_FIRST_CONNECTION_KIT_TO_ODU_MAX_LENGTH("b+e", firstConnectionKitToODU_Msg));
                        return false;
                    }
                    else if (_pipeLengthes[1] + _pipeLengthes[5] > firstConnectionKitToODU_Max)
                    {
                        JCMsg.ShowErrorOK(Msg.PIPING_FIRST_CONNECTION_KIT_TO_ODU_MAX_LENGTH("b+f", firstConnectionKitToODU_Msg));
                        return false;
                    }
                }
            }

            return true;
        }

        private void jcbtnOK_Click(object sender, EventArgs e)
        {
            if (JCValidateForm())
            {
                if (ValidateLength())
                {
                    BindToSource();
                    if (ValidateOthers())
                    {
                        _nodeOut.PipeLengthes = _pipeLengthes;
                        DialogResult = DialogResult.OK;
                    }
                }
            }
        }

        private void jcbtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// 校验输入长度 add by axj 2018 03 12
        /// 规则 LA <= LB <= LC <= 10m
        /// </summary>
        /// <returns></returns>
        private bool ValidateLength()
        {
            bool chk = true;
            string series=curSystemItem.Series;
            //Add FSNC7B/5B by Yunxiao Lin 20190107
            if (series.Contains("FSNS") || series.Contains("FSNP") || series.Contains("FSXNS") || series.Contains("FSXNP") || series.Contains("JTOH-BS1") || series.Contains("JTOR-BS1") || series.Contains("FSNC7B") || series.Contains("FSNC5B"))
            {
                if (_nodeOut.UnitCount == 2)
                {
                    double b = Unit.ConvertToSource(Convert.ToDouble(jctxtLengthB.Text), UnitType.LENGTH_M, ut_length);
                    double c = Unit.ConvertToSource(Convert.ToDouble(jctxtLengthC.Text), UnitType.LENGTH_M, ut_length);
                    if (!(b <= c && c <= 10))
                    {
                        JCMsg.ShowErrorOK("b ≤ c ≤ " + Unit.ConvertToControl(10d, UnitType.LENGTH_M, ut_length).ToString("n1") + ut_length);
                        chk = false;
                    }
                }
                else if (_nodeOut.UnitCount == 3)
                {
                    double b = Unit.ConvertToSource(Convert.ToDouble(jctxtLengthB.Text), UnitType.LENGTH_M, ut_length);
                    double c = Unit.ConvertToSource(Convert.ToDouble(jctxtLengthC.Text), UnitType.LENGTH_M, ut_length);
                    double d = Unit.ConvertToSource(Convert.ToDouble(jctxtLengthD.Text), UnitType.LENGTH_M, ut_length);
                    double e = Unit.ConvertToSource(Convert.ToDouble(jctxtLengthE.Text), UnitType.LENGTH_M, ut_length);
                    if (!(c <= (b + d) && (b + d) <= (b + e) && (b + e) <= 10))
                    {
                        JCMsg.ShowErrorOK("c ≤ b+d ≤ b+e ≤" + Unit.ConvertToControl(10d, UnitType.LENGTH_M, ut_length).ToString("n1") + ut_length);
                        chk = false;
                    }
                }
                else if (_nodeOut.UnitCount == 4)
                {
                    double b = Unit.ConvertToSource(Convert.ToDouble(jctxtLengthB.Text), UnitType.LENGTH_M, ut_length);
                    double c = Unit.ConvertToSource(Convert.ToDouble(jctxtLengthC.Text), UnitType.LENGTH_M, ut_length);
                    double d = Unit.ConvertToSource(Convert.ToDouble(jctxtLengthD.Text), UnitType.LENGTH_M, ut_length);
                    double e = Unit.ConvertToSource(Convert.ToDouble(jctxtLengthE.Text), UnitType.LENGTH_M, ut_length);
                    double f = Unit.ConvertToSource(Convert.ToDouble(jctxtLengthF.Text), UnitType.LENGTH_M, ut_length);
                    if (!useDoublePipeB)
                    {
                        double g = Unit.ConvertToSource(Convert.ToDouble(jctxtLengthG.Text), UnitType.LENGTH_M, ut_length);
                        if (!((b + d) <= (b + e) && (b + e) <= (c + f) && (c + f) <= (c + g) && (c + g) <= 10))
                        {
                            JCMsg.ShowErrorOK("b+d ≤ b+e ≤ c+f ≤ c+g ≤" + Unit.ConvertToControl(10d, UnitType.LENGTH_M, ut_length).ToString("n1") + ut_length);
                            chk = false;
                        }
                    }
                    else
                    {
                        if (!((b + c) <= (b + d) && (b + d) <= (b + e) && (b + e) <= (b + f) && (b + f) <= 10))
                        {
                            JCMsg.ShowErrorOK("b+c ≤ b+d ≤ b+e ≤ b+f ≤" + Unit.ConvertToControl(10d, UnitType.LENGTH_M, ut_length).ToString("n1") + ut_length);
                            chk = false;
                        }
                    }
                }
            }
            return chk;
        }
    }
}
