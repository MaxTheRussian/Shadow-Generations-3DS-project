using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingSpinAnimation : MonoBehaviour {

    Transform camera;
    public Sprite[] RingSprites;
    SpriteRenderer RingRenderer;
    byte ringSprite;
    bool animDir;
    float spriteChangePause = .05f;
    // Use this for initialization
    void Start()
    {
        RingRenderer = GetComponent<SpriteRenderer>();
        camera = Camera.main.transform;
        StartCoroutine(RingAnim());
    }

    void Update()
    {
        transform.rotation = Quaternion.LookRotation(camera.position - transform.position);
    }

    IEnumerator RingAnim()
    {
        WaitForSeconds wait = new WaitForSeconds(spriteChangePause);

        while (true)
        {
            yield return wait;

            if (animDir)
                ringSprite--;
            else
                ringSprite++;

            if (ringSprite == RingSprites.Length - 1)
                animDir = true;
            else if (ringSprite == 0)
                animDir = false;

            RingRenderer.sprite = RingSprites[ringSprite];
            RingRenderer.flipX = animDir;
        }
    }

}
