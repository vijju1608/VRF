using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JCHVRF.Model.New
{
    [Serializable]
    public class Project : ModelBase
    {
        public Project()
        {
            this.objProjectInfo = new ProjectInfo();
            this.objProjectBlobInfo = new ProjectBlobInfo();
            this.objClient = new Client();
            this.objDesignCondition = new DesignCondition();
            this.objFloor = new Floor();

        }

        public ProjectInfo objProjectInfo;
        public ProjectBlobInfo objProjectBlobInfo;
        public Client objClient;
        public DesignCondition objDesignCondition;
        public Floor objFloor;


    }
}
