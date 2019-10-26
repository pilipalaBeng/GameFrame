using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Oasis
{
    [DisallowMultipleComponent]
    public class UIMessageCtrl : OasisBase
    {
        private UIMessageCtrl()
        {
            //防止它人滥用实例化，导致报错
        }

        public enum UISaveType
        {
            JumpSceneSave,//跳转场景保存
            JumpSceneDestory,//跳转场景销毁
        }
        [SerializeField]
        private UISaveType uiSaveType = UISaveType.JumpSceneDestory;
        public UISaveType UiSaveType
        {
            get
            {
                return uiSaveType;
            }
        }
        public enum UIShowType
        {
            Active,//通过active控制显示隐藏
            Layer,//通过layer控制显示隐藏
        }
        [SerializeField]
        private static UIShowType uiShowType = UIShowType.Layer;
        public static UIShowType UiShowType
        {
            get
            {
                return uiShowType;
            }
        }

        private GameObject self;
        public GameObject Self
        {
            get
            {
                if (self == null)
                {
                    self = this.gameObject;
                }
                return self;
            }
        }

        public int ShowLayer
        {
            get
            {
                if (showLayer == null)
                {
                    showLayer = 14;
                }
                return (int)showLayer;
            }
        }

        public int CloseLayer
        {
            get
            {
                if (closeLayer == null)
                {
                    closeLayer = 15;
                }
                return (int)closeLayer;
            }
        }
        private int? showLayer = 14;
        private int? closeLayer = 15;
        public UnityAction<GameObject> onShowAction;
        public UnityAction<GameObject> onCloseAction;

        public virtual void Show(params object[] args)
        {
            onShowAction?.Invoke(this.gameObject);
            switch (UiShowType)
            {
                case UIShowType.Active:
                    Self.SetActive(true);
                    break;
                case UIShowType.Layer:
                    Self.layer = ShowLayer;
                    break;
            }
            ResetUI();
        }
        public virtual void ResetUI()
        {

        }
        public virtual void Close()
        {
            onCloseAction?.Invoke(this.gameObject);
            switch (UiShowType)
            {
                case UIShowType.Active:
                    Self.SetActive(false);
                    break;
                case UIShowType.Layer:
                    Self.layer = CloseLayer;
                    break;
            }
        }
    }
}
