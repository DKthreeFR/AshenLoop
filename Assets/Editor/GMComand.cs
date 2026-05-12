using System.IO;
using UnityEditor;
public class GMComand : Editor
{
  [MenuItem("TestTools/playerAddHp")]
  public static void PlayerAddHp()
  {
    GameManager.Instance.playerModel.AddHp(10);
  }
  [MenuItem("TestTools/playerMinusHp")]
  public static void PlayerMinusHp()
  {
    GameManager.Instance.playerModel.AddHp(-10);
  }
  [MenuItem("TestTools/playerAddMoney")]
  public static void PlayerAddMoney()
  {
    GameManager.Instance.playerModel.AddMoney(10);
  }
  [MenuItem("TestTools/playerMinusMoney")]
  public static void PlayerMinusMoney()
  {
    GameManager.Instance.playerModel.AddMoney(-10);
  }
  [MenuItem("TestTools/AddTask2")]
  public static void AddTask2()
  {
    TaskManager.Instance.AcceptTask("2");
  }
  [MenuItem("TestTools/删除任务存档")]
  public static void DeleteTaskSave()
  {
    //指定路径删除
    File.Delete(TaskSaveSystem.SavePath);
  }
}