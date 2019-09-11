using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using JCHVRF.Model.NextGen;
using Lassalle.WPF.Flow;

namespace JCHVRF_New.Common.Helpers
{
    public static class JCHNodeExtensions
    {
        public static bool IsOutdoorUnit(this JCHNode node)
        {
            return node.ImageData != null && node.ImageData.equipmentType != null &&
                   node.ImageData.equipmentType.Equals("Outdoor");
        }

        public static bool IsIndoorUnit(this JCHNode node)
        {
            return node.ImageData != null && node.ImageData.equipmentType != null &&
                   node.ImageData.equipmentType.Equals("Indoor");
        }

        public static bool IsYpNode(this JCHNode node)
        {
            return node is JCHVRF.Model.NextGen.MyNodeYP;
        }

        public static string GetUniqueName(this JCHNode node)
        {
            return node != null && node.ImageData != null ? node.ImageData.UniqName : string.Empty;
        }

        public static void RotateYpNode(this MyNodeYP ypNode, double angle, AddFlow addflow)
        {
            if (ypNode == null) { return; }
            
            if(ypNode.Tooltip.ToString().Equals(JCHVRF.Model.NodeType.YP.ToString())){return;}

            var pinCount = ypNode.PinsLayout.Count;

            var is8HeaderBranch=pinCount==9?true:false;

            ypNode.PinsLayout.Clear();

            if (angle == -90)
            {
                var oldLinks = new List<Link>(ypNode.Links);
                RemoveOldLinks(oldLinks, addflow);

                var oldCenterLocation = new Point(ypNode.Location.X + ypNode.Size.Width / 2, ypNode.Location.Y + ypNode.Size.Height / 2);
                
                ypNode.Size =is8HeaderBranch? new Size(30, 120):new Size(24,45);

                if (is8HeaderBranch)
                {
                    ypNode.Geometry =
                        Geometry.Parse(
                            "M8.75 2.17H4.09v2.52h4.66v2.17H4.09v2.52h4.66v2.17H4.09v2.52h4.66v2.17H4.09v2.52h4.66v2.17H4.09v2.52h4.66v2.17H4.09v2.52h4.66v2.17H4.09v2.52h4.66V35h-6.7V0h6.7zm-6.7 16.7H0v-2.74h2.05z");

                    var q = 100 / 8;
                    ypNode.PinsLayout.Add(new Point(0, 50));
                    ypNode.PinsLayout.Add(new Point(100, q - 10));
                    ypNode.PinsLayout.Add(new Point(100, (2 * q) - 9));
                    ypNode.PinsLayout.Add(new Point(100, (3 * q) - 8));
                    ypNode.PinsLayout.Add(new Point(100, (4 * q) - 6));
                    ypNode.PinsLayout.Add(new Point(100, (5 * q) - 4));
                    ypNode.PinsLayout.Add(new Point(100, (6 * q) - 3));
                    ypNode.PinsLayout.Add(new Point(100, 7 * q));
                    ypNode.PinsLayout.Add(new Point(100, 8 * q));
                }
                else
                {
                    ypNode.Geometry =
                        Geometry.Parse(
                            "M13.33 25.08H3.15v-9.89H0v-4.07h3.15V.08h10.18v3.49H6.74v3.48h6.59v4.07H6.88v3.49h6.45v3.49H6.74v3.49h6.59v3.49z");
                    var q = 100 / 4;
                    ypNode.PinsLayout.Add(new Point(0, 45));
                    ypNode.PinsLayout.Add(new Point( 100,q - 15));
                    ypNode.PinsLayout.Add(new Point( 100,(2 * q) - 15));
                    ypNode.PinsLayout.Add(new Point( 100,(3 * q) - 10));
                    ypNode.PinsLayout.Add(new Point(100,(4 * q) - 10));

                }

                var newLocation = new Point(oldCenterLocation.X - ypNode.Size.Width / 2,
                    oldCenterLocation.Y - ypNode.Size.Height / 2);
                ypNode.Location = newLocation;
                
                AddNewLinks(ypNode, oldLinks, addflow,pinCount);
            }
            else if (angle == 0)
            {
                var oldLinks = new List<Link>(ypNode.Links);
                RemoveOldLinks(oldLinks, addflow);

                var oldCenterLocation = new Point(ypNode.Location.X + ypNode.Size.Width / 2, ypNode.Location.Y + ypNode.Size.Height / 2);

                ypNode.Size =is8HeaderBranch? new Size(120, 30):new Size(45,24);
                if (is8HeaderBranch)
                {
                    ypNode.Geometry =
                        Geometry.Parse(
                            "M56.274 15V7.013H51.96V15h-3.725V7.013h-4.314V15H40.2V7.013h-4.314V15h-3.725V7.013h-4.314V15h-3.725V7.013H19.8V15h-3.726V7.013H11.76V15H8.035V7.013H3.721V15H0V3.507h60V15zM27.647 3.507V0h4.706v3.507z");
                    var q = 100 / 8;
                    ypNode.PinsLayout.Add(new Point(50, 0));
                    ypNode.PinsLayout.Add(new Point(q - 10, 100));
                    ypNode.PinsLayout.Add(new Point((2 * q) - 9, 100));
                    ypNode.PinsLayout.Add(new Point((3 * q) - 8, 100));
                    ypNode.PinsLayout.Add(new Point((4 * q) - 6, 100));
                    ypNode.PinsLayout.Add(new Point((5 * q) - 4, 100));
                    ypNode.PinsLayout.Add(new Point((6 * q) - 3, 100));
                    ypNode.PinsLayout.Add(new Point(7 * q, 100));
                    ypNode.PinsLayout.Add(new Point(8 * q, 100));
                }
                else
                {

                    ypNode.Geometry =
                        Geometry.Parse(
                            "M-.138 24V5.677h17.791V0h7.325v5.677h19.884V24h-6.279V12.13h-6.28V24h-7.326V12.387h-6.279V24h-6.279V12.13h-6.28V24H-.138z");
                    var q = 100 / 4;
                    ypNode.PinsLayout.Add(new Point(45, 0));
                    ypNode.PinsLayout.Add(new Point(q - 15, 100));
                    ypNode.PinsLayout.Add(new Point((2 * q) - 15, 100));
                    ypNode.PinsLayout.Add(new Point((3 * q) - 10, 100));
                    ypNode.PinsLayout.Add(new Point((4 * q) - 10, 100));

                }
               
                var newLocation = new Point(oldCenterLocation.X - ypNode.Size.Width / 2,
                    oldCenterLocation.Y - ypNode.Size.Height / 2);
                ypNode.Location = newLocation;
                
                AddNewLinks(ypNode, oldLinks, addflow,pinCount);
            }

        }

        private static void AddNewLinks(MyNodeYP ypNode, List<Link> oldLinks, AddFlow addflow,int pinCount)
        {
            if (oldLinks.Count > 0)
            {
                foreach (var oldLink in oldLinks)
                {
                    var org = oldLink.Org == ypNode ? oldLink.Org : oldLink.Dst;
                    var dst = oldLink.Org == ypNode ? oldLink.Dst : oldLink.Org;

                    var orgIndex = oldLink.Org == ypNode ? oldLink.PinOrgIndex : oldLink.PinDstIndex;
                    var dstIndex = oldLink.Org == ypNode ? oldLink.PinDstIndex : oldLink.PinOrgIndex;

                    Link newLink = new Link(org, dst, orgIndex == 0 ? 0 : pinCount - orgIndex, dstIndex, "", addflow);

                    addflow.AddLink(newLink);
                }
            }
        }

        private static void RemoveOldLinks(List<Link> oldLinks, AddFlow addflow)
        {
            var linkCount = oldLinks.Count;
            for (int i = 0; i < linkCount; i++)
            {
                addflow.RemoveLink(oldLinks[i]);
            }
        }
    }
}