using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oasis
{
    public enum GameState
    {
        Two_Dimension,
        There_Dimension,
    }
    [DisallowMultipleComponent]
    public class MasterControl : OasisBase
    {
        private static MasterControl _instance;

        public static MasterControl Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<MasterControl>();
                }
                return _instance;
            }
        }
        //玩家数据管理 玩家的各种数据
        [SerializeField]
        public LocalPlayer localPlayer;
        //游戏数据管理 持久化数据
        [SerializeField]
        public GameDataCtrl gameDataCtrl;
        //UI数据管理
        [SerializeField]
        public UIMessageCtrl uiMessageCtrl;
        //资源数据管理
        [SerializeField]
        public ResourceCtrl resourceCtrl;
        //音频数据管理
        [SerializeField]
        public AudioCtrl audioCtrl;
        //模型数据管理
        [SerializeField]
        public ModelCtrl modelCtrl;
        //场景数据管理
        [SerializeField]
        public SceneCtrl sceneCtrl;
        //游戏类型控制管理 多游戏框架管理
        [SerializeField]
        public GameStateCtrl gameStateCtrl;
        //网络连接控制管理
        [SerializeField]
        public  NetworkCtrl networkCtrl;
        public override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this.gameObject);

            Init();
        }

        private void Init()
        {
            resourceCtrl = this.GetComponent<ResourceCtrl>();
            localPlayer = this.GetComponent<LocalPlayer>();
            gameDataCtrl = this.GetComponent<GameDataCtrl>();
            uiMessageCtrl = this.GetComponent<UIMessageCtrl>();
            audioCtrl = this.GetComponent<AudioCtrl>();
            modelCtrl = this.GetComponent<ModelCtrl>();
            sceneCtrl = this.GetComponent<SceneCtrl>();
            gameStateCtrl = this.GetComponent<GameStateCtrl>();
        }
    }
}