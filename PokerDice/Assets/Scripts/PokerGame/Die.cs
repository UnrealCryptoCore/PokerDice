using UnityEngine;

public class Die : MonoBehaviour
{
    public readonly Vector3Int[][] rotationMatrix = {
        new Vector3Int[] { Vector3Int.zero, new(0, -1, 0), new(0, 0, -1), new(0, 0, 1), new(0, 1, 0), new(0, 2, 0)},
        new Vector3Int[] { new(0, 1, 0), Vector3Int.zero, new(1, 0, 0), new(-1, 0, 0), new(2, 0, 0), new(0, -1, 0)},
        new Vector3Int[] { new(0, 0, 1), new(-1, 0, 0), Vector3Int.zero, new(0, 0, 2), new(1, 0, 0), new(0, 0, -1)},
        new Vector3Int[] { new(0, 0, -1), new(1, 0, 0), new(0, 0, 2), Vector3Int.zero, new(-1, 0, 0), new(0, 0, 1)},
        new Vector3Int[] { new(0, -1, 0), new(2, 0, 0), new(-1, 0, 0), new(1, 0, 0), Vector3Int.zero, new(0, 1,0)},
        new Vector3Int[] { new(0, 2, 0), new(0, 1, 0), new(0, 0, 1), new(0, 0, -1), new(0, -1, 0), Vector3Int.zero},
    };
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private GameObject[] _detectors;
    [SerializeField] private GameObject _renderer;
    [SerializeField] public Color HoverColor;
    [SerializeField] public Color SelectionColor;
    private Vector3 _pos;
    private Quaternion _rot;
    private Vector3 _vel;
    private bool _thrown;
    private Outline _outline;

    private Vector3[] _posRecorder;
    private Quaternion[] _rotRecorder;
    public bool Selected = false;

    public bool Selectable;
    private bool _clicked = false;

    void Awake()
    {
        _outline = gameObject.GetComponent<Outline>();
        _outline.enabled = false;
    }

    void Start()
    {
        _thrown = false;
        SaveState();
    }

    void SaveState()
    {
        _pos = transform.position;
        _rot = transform.rotation;
        _vel = _rigidbody.velocity;
    }

    void Update()
    {
        if (_thrown)
        {
            if (CheckObjectStoppedMoving())
            {
                _thrown = false;
            }
        }
    }

    public bool CheckObjectStoppedMoving()
    {
        return _rigidbody.velocity == Vector3.zero && _rigidbody.angularVelocity == Vector3.zero;
    }

    public int GetNumber()
    {
        int max = 0;
        for (int i = 1; i < _detectors.Length; i++)
        {
            if (_detectors[i].transform.position.y > _detectors[max].transform.position.y)
            {
                max = i;
            }
        }
        return max;
    }

    public void OnMouseEnter()
    {
        if (!Selectable)
        {
            return;
        }
        if (!_outline.enabled)
        {
            _outline.enabled = true;
        }
    }

    public void OnMouseOver()
    {
        if (!Selectable)
        {
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (!_clicked)
            {
                OnMouseClick();
            }
        }
        else
        {
            _clicked = false;
        }
    }

    public void OnMouseClick()
    {
        SetSelected(!Selected);
        GameHandler.Instance.client.Game.UpdateDiceRollButtons();

    }

    public void SetSelected(bool b)
    {
        Selected = b;
        _outline.OutlineColor = Selected ? SelectionColor : HoverColor;
    }

    public void OnMouseExit()
    {
        if (!Selectable)
        {
            return;
        }
        if (!Selected)
        {
            _outline.enabled = false;
        }
    }

    public void ClearOutline()
    {
        _outline.enabled = false;
    }

    public void SetInitialState()
    {
        _thrown = true;
        _renderer.transform.localRotation = Quaternion.Euler(0, 0, 0);
        transform.position = _pos;
        _rigidbody.useGravity = true;
        transform.rotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360), Random.Range(0f, 360f));
        _rigidbody.velocity = new Vector3(Random.Range(3f, 9f), -0.5f, 0);
        SaveState();
    }

    public void RevertToState()
    {
        _thrown = true;
        transform.SetPositionAndRotation(_pos, _rot);
        _rigidbody.useGravity = true;
        _rigidbody.velocity = _vel;
    }

    public void ThrowDice()
    {
    }

    public void RotateTo(int from, int to)
    {
        Vector3Int vec = rotationMatrix[from][to];
        _renderer.transform.Rotate(vec * -90);
        if (_rotRecorder == null)
        {
            return;
        }
    }

    public void InitRecorder(int frames)
    {
        _posRecorder = new Vector3[frames];
        _rotRecorder = new Quaternion[frames];
    }

    public void RecordFrame(int frame)
    {
        _posRecorder[frame] = transform.position;
        _rotRecorder[frame] = transform.rotation;
    }

    public void SetFrame(int frame)
    {
        transform.SetPositionAndRotation(_posRecorder[frame], _rotRecorder[frame]);
    }
}
