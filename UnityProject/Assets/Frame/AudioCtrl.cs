using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oasis
{
    [DisallowMultipleComponent]
    public class AudioCtrl : MonoBehaviour
    {
        private AudioCtrl()
        {
            //防止它人滥用实例化，导致报错
        }
    }
}