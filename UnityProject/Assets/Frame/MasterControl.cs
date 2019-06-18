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
        private LocalPlayer localPlayer;
        //游戏数据管理 持久化数据
        private GameDataCtrl gameDataCtrl;
        //UI数据管理
        private UIMessageCtrl uiMessageCtrl;
        //资源数据管理
        private ResourceCtrl resourceCtrl;
        //音频数据管理
        private AudioCtrl audioCtrl;
        //模型数据管理
        private ModelCtrl modelCtrl;
        //场景数据管理
        private SceneCtrl sceneCtrl;
        //游戏类型控制管理 多游戏框架管理
        private GameStateCtrl gameStateCtrl;

        private void Awake()
        {
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