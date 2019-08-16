using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Oasis
{
    [DisallowMultipleComponent]
    public class NetworkCtrl : OasisBase
    {
       private NetworkCtrl()
        {
            //防止它人滥用实例化，导致报错
        }
    }
}
