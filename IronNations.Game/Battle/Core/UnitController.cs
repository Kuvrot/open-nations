using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Engine;
using System.Printing;

namespace IronNations.Battle.Core
{
    public class UnitController : SyncScript
    {

        public int idUnit = 0; //unique identifier for the unit

        private TransformComponent target;

        public override void Start()
        {
            // Initialization of the script.
        }

        public override void Update()
        {
            // Do stuff every new frame
        }
    }
}
