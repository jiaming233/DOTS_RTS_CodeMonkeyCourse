using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DOTSEventsManager.Instance.OnHQDead += DOTSEventManager_OnHQDead;
        Hide();
    }

    private void DOTSEventManager_OnHQDead(object sender, System.EventArgs e)
    {
        Show();
        Time.timeScale = 0f;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
