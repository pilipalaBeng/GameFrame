using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oasis
{
    [DisallowMultipleComponent]
    public class LocalPlayer : MonoBehaviour
    {
        private LocalPlayer()
        {
            //防止它人滥用实例化，导致报错
        }
        private string _name;

        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
            }
        }

        private int _level;

        public int Level
        {
            get
            {
                return _level;
            }

            set
            {
                _level = value;
            }
        }
    }
}
