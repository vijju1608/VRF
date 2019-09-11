using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using JCHVRF.Model;

namespace JCHVRF.DAL
{
    public interface IPipingDAL
    {
        // 组合室外机内部Y型分支管
        PipingBranchKit GetConnectionKit(Outdoor outItem);

        // Y型第一分支管
        PipingBranchKit GetFirstBranchKit(string factoryCode, string type, string unitType, double capacity, string sizeUP, bool isMal, int IDUNum, string region);

        // Y型其他分支管
        PipingBranchKit GetTertiaryBranchKit(string factoryCode, string type, string unitType, double capacity, string sizeUP, bool isMal, int IDUNum, string region);
        
        // 梳型分支管，指定最大连接数
        // 20170513 增加ODUUnitType参数 by Yunxiao Lin
        PipingHeaderBranch GetHeaderBranch(string type, double capacity, string sizeUP, int maxBranches, string ODUUnitType, string region);

        // CH box
        // 增加Series参数, 20170302 by Yunxiao Lin
        PipingChangeOverKit GetChangeOverKit(string factoryCode, double capacity, string sizeUP, int IUCount, string Series, string region);


        // 获取室内机连接管管径尺寸
        string[] GetPipeSizeIDU_SizeUP(string regionCode,string factoryCode, double capacity, string model_Hitachi,string SizeUp);

        // 获取室外机内部连接管管径尺寸
        string[] GetPipeSizeODU(string factoryCode, string type, string unitType, double capacity);

        // 获取管长修正系数
        //decimal GetPipingLengthFactor(string factoryCode, string unitType, string model_Hitachi, string condition, int highDiff, int eqLength);
        //获取管长修正系数增加PipeType参数 20161102 by Yunxiao Lin
        double GetPipingLengthFactor(string factoryCode, string unitType, string model_Hitachi, string condition, double highDiff, double eqLength, string PipeType);

        // 获取室外机节点对象，包含组合室外机内部的分支管和管径型号
        NodeElement_Piping GetPipingNodeOutElement(string model, bool isHitachi, string pipeType, string SizeUp);

        NodeElement_Piping GetPipingNodeIndElement(string unitType, bool isHitachi);

        string GetPipeSize_Inch(string orgPipeSize);


    }
}
