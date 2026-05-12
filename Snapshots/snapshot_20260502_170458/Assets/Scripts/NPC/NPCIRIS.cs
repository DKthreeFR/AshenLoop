using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NPCIRIS : MonoBehaviour
{

  public CharacterInputAdapter characterInputAdapter;
  

  void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("Player"))
    {
      UIManager.Instance.OpenPanel(UIPanelId.NPCInteractPanel);
      characterInputAdapter = other.GetComponent<CharacterInputAdapter>();
      characterInputAdapter.InteractPressed+=HandleInteract;

    }
  }
  void OnTriggerStay(Collider other)
  {
    if (other.CompareTag("Player")&&characterInputAdapter != null){

    
  }
  }
  void OnTriggerExit(Collider other)
  {
    if (other.CompareTag("Player"))
    {
      UIManager.Instance.ClosePanel(UIPanelId.NPCInteractPanel);
      characterInputAdapter.InteractPressed-=HandleInteract;
    }
    
  }
  void HandleInteract()
  {
    Debug.Log("玩家按下交互键");  
  }

}
