using Lassalle.WPF.Flow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JCHVRF.MyPipingBLL.NextGen
{
    public static class AddFlowExtension
    {
        public static List<Node> Nodes(this AddFlow addflow, bool returnReversed = false)
        {
            List<Node> listOfNodes = addflow.Items.OfType<Node>().ToList();
            if (returnReversed)
            {
                listOfNodes.Reverse();
            }
            return listOfNodes;
        }

        public static float FloatConverter(object TypeValue)
        {
            float returnValue = 0;
            float.TryParse(Convert.ToString(TypeValue), out returnValue);
            return returnValue;
        }


    }
}
