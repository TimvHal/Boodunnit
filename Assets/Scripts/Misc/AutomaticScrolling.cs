using UnityEngine;
using UnityEngine.UI;

public class AutomaticScrolling : MonoBehaviour
{

    public ScrollRect ScrollRect;
    public RectTransform Content;
    public float ScrollSpeed;
    private float _fastScrollSpeed;
    private float _actualScrollSpeed;

    // Start is called before the first frame update
    void Start()
    {
        _actualScrollSpeed = ScrollSpeed;
        _fastScrollSpeed = ScrollSpeed * 2;
    }

    // Update is called once per frame
    void Update()
    {
        MoveContent();
        if (Input.GetKey(KeyCode.Space))
        {
            ScrollSpeed = _fastScrollSpeed;
        } else
        {
            ScrollSpeed = _actualScrollSpeed;
        }
    }

    private void MoveContent()
    {
        Vector2 contentPosition = ScrollRect.content.position;
        Vector2 newPosition = new Vector2(contentPosition.x, contentPosition.y + ScrollSpeed);
        ScrollRect.content.position = newPosition;
    }
}
