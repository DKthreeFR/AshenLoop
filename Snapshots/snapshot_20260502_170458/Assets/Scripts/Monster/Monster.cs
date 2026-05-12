using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Tasks.Actions;
using UnityEngine.UI;
using System;
using ParadoxNotion.Design;
using NodeCanvas.Framework;
using UnityEngine.Video;
public class Monster : MonoBehaviour
{
  [UnityEngine.Header("手动填入的怪物ID")]
  public int monsterId;

  [UnityEngine.Header("怪物数据 根据匹配的ID自动读取")]
  public MonsterData monsterData;

  [UnityEngine.Header("行为树所有者")]
 	public BehaviourTreeOwner owner; 
  public BehaviourTree monsterTree;

  [UnityEngine.Header("血条")]
  public Image hpValueBar;
  public Canvas monsterCanvas;

  [UnityEngine.Header("玩家对象 动态创建后动态获取")]
  public GameObject player;


  void Awake()
  {
    monsterCanvas.gameObject.SetActive(false);
  }
  void OnEnable()
  {
        player = GameObject.FindGameObjectWithTag("Player");
    owner.blackboard.SetVariableValue("Player",player);

    //怪物每次从对象池里获取时都要重新获取怪物数据
    monsterData = GameManager.Instance.monsterDataList.monsters.Find(m=>m.monsterID == monsterId);
    Debug.Log("怪物名字是:" + monsterData.monsterName);
    //将行为树里与玩家有关的内容 把玩家放到黑板里供他们使用 行为树里现在判断玩家距离 和 是否可看见玩家 用的是黑板上的玩家变量
    //所以我们要给他赋值
    gameObject.transform.position = GameManager.Instance.monsterSpawnPoint.position;
    //配合对象池每次重新生成时要刷新怪物血条为最大血量
    MonsterResetHp();
  }
  void Start()
  {


      
  }

  public void SendEvent(string eventName){
    owner.SendEvent(eventName);
  }
  public void SendEvent<T>(string eventName, T value, GameObject sender = null)
  {
    if(sender == null)
    {
      sender =gameObject;
    }
    owner.SendEvent<T>( eventName, value, sender );
    //如果是玩家攻击怪物事件 则更新怪物血条
    if(eventName == "PlayerAttackMonster")
    {
      MonsterHpUpdate(Convert.ToSingle(value),gameObject);
    }
  }
  void MonsterResetHp()
  {
    monsterData.monsterHP = monsterData.monsterMaxHP;
    hpValueBar.fillAmount = monsterData.monsterHP / monsterData.monsterMaxHP;
  }
  void MonsterHpUpdate(float playerAtk,GameObject gameObject)
  {
    if(monsterCanvas.gameObject.activeSelf == false)
    {
      monsterCanvas.gameObject.SetActive(true);
    }
    Debug.Log("玩家实际伤害" + (playerAtk-monsterData.monsterDEF));
    monsterData.monsterHP -= playerAtk-monsterData.monsterDEF;
    if(monsterData.monsterHP <= 0)
    {
      monsterData.monsterHP = 0;
      //如果声明归零则怪物死亡
      MonsterDie();
    }
    hpValueBar.fillAmount = monsterData.monsterHP / monsterData.monsterMaxHP;
  }
  void MonsterDie()
  {
    owner.SendEvent("MonsterDie");
  }
  //死亡后销毁怪物 由行为树来调用 后续应该改成对象池的方式（暂时不使用 直接在行为树里销毁了）
  public void MonsterDestory()
  {
    GameManager.Instance.monsterPool.Release(gameObject);
  }
}
