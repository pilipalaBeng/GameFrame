using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Oasis
{
    [DisallowMultipleComponent]
    public class SceneCtrl : MonoBehaviour
    {
        private SceneCtrl()
        {
            //防止它人滥用实例化，导致报错
        }
    }
}
