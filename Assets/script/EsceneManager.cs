using UnityEngine;
using UnityEngine.SceneManagement;

public class EsceneManager : MonoBehaviour
{
    public void escena1()
    {
        SceneManager.LoadScene(1);
    }
    public void escena0()
    {
        SceneManager.LoadScene(0);
    }
    public void escena2()
    {
        SceneManager.LoadScene(2);
    }
}
