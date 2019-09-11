using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//using CADImport;
//using System.Threading;
//using CADImport.FaceModule;
//using System.Drawing;
//using CADImport.CADImportForms;
//using System.Windows.Forms;
//using CADImport.RasterImage;
//using System.IO;

namespace JCHVRF.MyPipingBLL
{
    public abstract class ApplicationConstants
    {
        public static readonly string defaultstr;
        public static readonly string languagepath;
        public static readonly string loadfilestr;
        public static readonly string msgBoxDebugCaption;
        public static readonly string sepstr;
        public static readonly string notsupportedstr;
        public static readonly string notsupportedextstr;
        public static readonly string allchahgeswillbecanceled;
        public static readonly string appnamestr;
        public static readonly string sepstr2;
        public static readonly string jpgextstr;
        public static readonly string bmpextstr;
        public static readonly string tiffextstr;
        public static readonly string gifextstr;
        public static readonly string emfextstr;
        public static readonly string wmfextstr;
        public static readonly string pngextstr;
        public static readonly string dxfextstr;
        public static readonly string lngextstr;
        public static readonly string languagestr;
        public static readonly string languageIDstr;
        public static readonly string backcolorstr;
        public static readonly string blackstr;
        public static readonly string showentitystr;
        public static readonly string drawmodestr;
        public static readonly string truestr;
        public static readonly string shxpathcnstr;
        public static readonly string installstr;
        public static readonly string sepstr3;
        public static readonly string typelcstr;
        public static readonly string floatingstr;
        public static readonly string hoststr;
        public static readonly string portstr;
        public static readonly string appconst;
        public static readonly string blackstr2;
        public static readonly string whitestr;
        public static readonly string registerstr;
        public static readonly string headstr1;
        public static readonly string headstr2;
        public static readonly string headstr3;
        public static readonly string headstr4;
        public static readonly string namestr;
        public static readonly string visiblestr;
        public static readonly string freezestr;
        public static readonly string colorstr;

        static ApplicationConstants()
        {
            defaultstr = "Default";
            languagepath = "LanguagePath";
            loadfilestr = "Loading file...";
            msgBoxDebugCaption = "Debug application message";
            sepstr = " - ";
            notsupportedstr = "not supported";
            notsupportedextstr = "Not supported in current version!";
            allchahgeswillbecanceled = "All changes will be canceled.";
            appnamestr = "CADImportNet Demo";
            sepstr2 = " : ";
            jpgextstr = ".JPG";
            bmpextstr = ".BMP";
            tiffextstr = ".TIFF";
            gifextstr = ".GIF";
            emfextstr = ".EMF";
            wmfextstr = ".WMF";
            pngextstr = ".PNG";
            dxfextstr = ".DXF";
            lngextstr = ".lng";
            languagestr = "Language";
            languageIDstr = "LanguageID";
            backcolorstr = "BackgroundColor";
            blackstr = "BLACK";
            showentitystr = "ShowEntity";
            drawmodestr = "drawMode";
            truestr = "TRUE";
            shxpathcnstr = "SHXPathCount";
            installstr = "Install";
            sepstr3 = ";";
            typelcstr = "Type_license";
            floatingstr = "floating";
            hoststr = "Host";
            portstr = "Port";
            appconst = "Application";
            blackstr2 = "Black";
            whitestr = "White";
            registerstr = "register";
            headstr1 = "Head1";
            headstr2 = "Head2";
            headstr3 = "Head3";
            headstr4 = "Head4";
            namestr = "Name";
            visiblestr = "Visible";
            freezestr = "Freeze";
            colorstr = "Color";
        }
    }

}
