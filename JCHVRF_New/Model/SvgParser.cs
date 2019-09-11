using System.Windows;
using System.Xml;

namespace JCHVRF_New.Model
{
    /// <summary>
    /// Class to Parser the SVG file and get Svg Data
    /// </summary>
    public class SvgParser
    {
        private readonly string svgFilePath;
        public SvgParser(string svgPath)
        {
            svgFilePath = svgPath;
        }

        public SvgData GetSvgData()
        {
            var doc = new XmlDocument();
            doc.Load(svgFilePath);

            XmlNode root = doc.DocumentElement;
            if (root == null || root.Attributes == null)
            {
                return null;
            }

            var nodeList = doc.GetElementsByTagName("g");
            var xmlNode = nodeList[0].FirstChild;
            if (xmlNode == null || xmlNode.Attributes == null)
            {
                return null;
            }

            int width = 0;
            int height = 0;
            int.TryParse(root.Attributes["width"].Value, out width);
            int.TryParse(root.Attributes["height"].Value, out height);

            var geometry = xmlNode.Attributes["d"].Value;
            var colorCode = xmlNode.Attributes["fill"].Value;

            return new SvgData(new Size(width, height), geometry, colorCode);
        }
        public class SvgData
        {
            public string GeometryPath { get; }
            public Size SvgSize { get; }
            public string ColorText { get; }

            public SvgData(Size size, string path, string color)
            {
                GeometryPath = path;
                SvgSize = size;
                ColorText = color;
            }
        }
    }
}