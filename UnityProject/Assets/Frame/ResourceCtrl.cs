using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oasis
{
    [DisallowMultipleComponent]
    public class ResourceCtrl : MonoBehaviour
    {
        private ResourceCtrl()
        {
            //防止它人滥用实例化，导致报错
        }
        public const string LOAD_SPRITE_PATH = "";
        public const string LOAD_GAMEOBJECT_PATH = "";
        public const string LOAD_ASSET_PATH = "";

        Sprite _spr = null;
        public Sprite LoadSprite(string str)
        {
            return _spr;
        }

        GameObject _go = null;
        public GameObject LoadGameObject(string str)
        {
            return _go;
        }

        public void LoadData<T>()
        {

        }

        public void Load(string str)
        {

        }
    }
}
