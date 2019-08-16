using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oasis
{
    public class OasisBase : MonoBehaviour
    {
       public  virtual void Awake()
        {
            
        }
        public virtual void Start()
        {
            
        }
        public virtual void OnEnable()
        {
            
        }
        public virtual void Reset()
        {
            
        }
        public virtual void Update()
        {
            
        }
        public virtual void FixedUpdate()
        {
            
        }
        public virtual void LateUpdate()
        {
            
        }
        public virtual void OnDestroy()
        {
            
        }
        public virtual void OnDisable()
        {
            
        }
        //当粒子碰到collider时被调用
        public virtual void OnParticleCollision(GameObject other)
        {
            
        }
        //当render在任何相机上都不可见时调用
        public virtual void OnBecameInvisible()
        {
            
        }
        //当render在任何相机上可见时调用
        public virtual void OnBecameVisible()
        {
            
        }
        //当一个新关卡被载入时次函数被调用
        public virtual void OnLevelWasLoaded(int level)
        {
            
        }
        public virtual void OnGUI()
        {
            
        }
        //当完成所有渲染图片后被调用，用来渲染图片后期效果
        public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            
        }
        //当玩家暂停时发送到所有的游戏物体
        public virtual void OnApplicationPause(bool pause)
        {
            
        }
        //当玩家获得或失去焦点时发送给所有游戏物体
        public virtual void OnApplicationFocus(bool focus)
        {
            
        }
        //在应用退出之前发送给所有的游戏物体
        public virtual void OnApplicationQuit()
        {
            
        }
    }
}
