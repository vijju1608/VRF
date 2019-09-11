using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JCHVRF.Model
{
    public class PipingHeaderBranch : PipingBranchKit
    {
        private int _maxBranches;
        /// <summary>
        /// 最大分支数量
        /// </summary>
        public int MaxBranches
        {
            get { return _maxBranches; }
            set { this.SetValue(ref _maxBranches, value); }
        }

    }
}
