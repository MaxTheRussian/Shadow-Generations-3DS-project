using UnityEngine;
using UnityEngine.N3DS;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScene : MonoBehaviour {
    public RectTransform Cursor;
    public Image Background;
    public Sprite Gens;
    public Sprite Shad;

    public void LoadLevel(int i)
    {
        SceneManager.LoadScene(i);
    }

    public void LerpImages(bool isShad)
    {
        Background.sprite = !isShad ? Gens : Shad;
        Cursor.localPosition = !isShad ? new Vector2(-96.2f, 38.4f) : new Vector2(-96.2f, -48f);
    }

    void Update()
    {
        if (GamePad.GetButtonTrigger(N3dsButton.Y))
            LoadLevel(2);
        else if (GamePad.GetButtonTrigger(N3dsButton.B))
            LoadLevel(1);
        else if (GamePad.GetButtonTrigger(N3dsButton.L) && GamePad.GetButtonHold(N3dsButton.X))
            LoadLevel(3);
    }
}
