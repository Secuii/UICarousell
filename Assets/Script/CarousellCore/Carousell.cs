using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Carousell : MonoBehaviour
{
    [Header("UI for example")]
    [SerializeField] private Image sprite = null;
    [SerializeField] private Text Name = null;
    [SerializeField] private Text Attack = null;
    [SerializeField] private Text Speed = null;

    [Header("Items")]

    [Tooltip("Check to rotate carousell automatically")]
    [SerializeField] private bool AutomaticRotation = false;

    [Tooltip("Check to current selected item with movement")]
    [SerializeField] private bool SetItemWithMovement = false;

    private RectTransform[] CarousellItemRectTransform = null;
    private Vector2[] CarousellItemPositions = null;    
    private Vector2[] CarousellItemSize = null;         
    private Image[] CarousellItemImage = null;          
    private Canvas[] CarousellItemCanvas = null;        

    private Color[] CarousellItemColor = null;
    private int[] CarousellIteSortingLayer = null;

    private bool ChangingCharacter = true;
    private int NextCarousellPos = 0;
    private int NextCarousellItem = 0;
    private float Angle = 0f;
    private float timer = 0f;

    [Header("Inverse Carousell Rotation")]
    [Tooltip("Check it before RUN THE GAME")]
    [SerializeField] private bool invertRotation = false;

    [Header("Heigh")]
    [SerializeField] private float CarousellMinX = -80f;
    [SerializeField] float CarousellMaxX = -80f;

    [Header("width")]
    [SerializeField] private float CarousellMinY = -200f;
    [SerializeField] float CarousellMaxY = 0f;

    [Header("Items Size")]
    [SerializeField] private float CarousellMinSize = 10f;
    [SerializeField] private float CarousellMaxSize = 200f;

    [Header("Item Color")]
    [SerializeField] private Color CarousellFarestColor = new Color();
    [SerializeField] private Color CarousellNearestColor = new Color();

    private readonly float CarousellLayerMin = 100f;
    private readonly float CarousellLayerMax = 200f;

    [Header("Rotation Speed")]
    [SerializeField] private float CarousellSpeed = 10f;
    [SerializeField] private float ButtonPressDelay = 0f;

    [Header("Events")]
    public UnityEvent OnCarousellRotate;

    private void Start()
    {
        CarousellItemRectTransform = new RectTransform[transform.childCount];
        CarousellItemPositions = new Vector2[transform.childCount];
        CarousellItemImage = new Image[transform.childCount];
        CarousellItemCanvas = new Canvas[transform.childCount];
        CarousellItemSize = new Vector2[transform.childCount];
        CarousellItemColor = new Color[transform.childCount];
        CarousellIteSortingLayer = new int[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            CarousellItemRectTransform[i] = transform.GetChild(i).GetComponent<RectTransform>();
            CarousellItemImage[i] = CarousellItemRectTransform[i].GetComponent<Image>();
            CarousellItemCanvas[i] = CarousellItemRectTransform[i].GetComponent<Canvas>();
        }

        if (invertRotation)
        {
            Angle = (360 * Mathf.Deg2Rad) / transform.childCount;
        }
        else
        {
            Angle = (-360 * Mathf.Deg2Rad) / transform.childCount;
        }
        CheckCarousellItem();

        ChangingCharacter = false;
        StartCoroutine(nameof(CarousellRotationDelay));
    }

    private void Update()
    {
        //JoystickInput();
        //JoystickInputRepeating();
        CarousellRotation();
    }

    public void CarousellRotation()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            double childCountLerp = Mathf.InverseLerp(transform.childCount, 0, i);
            float truncateLerp = (float)Math.Round(childCountLerp * 100f) / 100f;

            float ItemPosY;
            float ItemPosX;
            float SizeX;
            float SizeY;
            int ItemSortingLayer;
            Color ColorA;

            if (i <= transform.childCount / 2)
            {
                ColorA = Color.Lerp(CarousellFarestColor, CarousellNearestColor, truncateLerp);
                ItemSortingLayer = Mathf.RoundToInt(Mathf.Lerp(CarousellLayerMin, CarousellLayerMax, truncateLerp));

                ItemPosX = Mathf.Lerp(CarousellMaxX, CarousellMinX, truncateLerp);
                ItemPosY = Mathf.Lerp(CarousellMaxY, CarousellMinY, truncateLerp);

                SizeX = Mathf.Lerp(CarousellMinSize, CarousellMaxSize, truncateLerp);
                SizeY = Mathf.Lerp(CarousellMinSize, CarousellMaxSize, truncateLerp);
            }
            else
            {
                ColorA = Vector4.Lerp(CarousellNearestColor, CarousellFarestColor, truncateLerp);
                ItemSortingLayer = Mathf.RoundToInt(Mathf.Lerp(CarousellLayerMax, CarousellLayerMin, truncateLerp));

                ItemPosX = Mathf.Lerp(CarousellMinX, CarousellMaxX, truncateLerp);
                ItemPosY = Mathf.Lerp(CarousellMinY, CarousellMaxY, truncateLerp);

                SizeX = Mathf.Lerp(CarousellMaxSize, CarousellMinSize, truncateLerp);
                SizeY = Mathf.Lerp(CarousellMaxSize, CarousellMinSize, truncateLerp);
            }

            CarousellIteSortingLayer[i] = ItemSortingLayer;
            CarousellItemPositions[i] = new Vector2(ItemPosX * Mathf.Cos(Angle * i),
                                                    ItemPosY * Mathf.Sin(Angle * i));
            CarousellItemColor[i] = ColorA;
            CarousellItemSize[i] = new Vector2(SizeX, SizeY);

            CarousellItemRectTransform[i].anchoredPosition = Vector2.Lerp(CarousellItemRectTransform[i].anchoredPosition,
                                                             CarousellItemPositions[NextPositionValue(i)],
                                                             CarousellSpeed / 100);

            CarousellItemCanvas[i].sortingOrder = CarousellIteSortingLayer[NextPositionValue(i)];
            CarousellItemImage[i].color = CarousellItemColor[NextPositionValue(i)];
            CarousellItemRectTransform[i].sizeDelta = CarousellItemSize[NextPositionValue(i)];
        }
    }

    private int NextPositionValue(int actualPos)
    {
        return (actualPos + NextCarousellPos) % transform.childCount;
    }

    private IEnumerator CarousellRotationDelay()
    {
        yield return new WaitForSeconds(ButtonPressDelay);
        ChangingCharacter = true;
    }

    private void JoystickInput()
    {
        if (Input.GetAxis("Horizontal") == -1)
        {
            ChangeCarousellPositionValue(1, -1);
        }
        if (Input.GetAxis("Horizontal") == 1)
        {
            ChangeCarousellPositionValue(-1, 1);
        }
    }

    private void JoystickInputRepeating()
    {
        if (AutomaticRotation)
        {
            if (Input.GetAxis("Horizontal") == -1)
            {
                timer += Time.deltaTime;
                if (timer >= ButtonPressDelay)
                {
                    ChangeCarousellPositionValue(1, -1);
                    timer = 0f;
                }
            }
            else if (Input.GetAxis("Horizontal") == 1)
            {
                timer += Time.deltaTime;
                if (timer >= ButtonPressDelay)
                {
                    ChangeCarousellPositionValue(-1, 1);
                    timer = 0f;
                }
            }
            else
            {
                timer = 0;
            }
        }
    }

    public void OnLeftButton()
    {
        ChangeCarousellPositionValue(1, -1);
    }

    public void OnRightButton()
    {
        ChangeCarousellPositionValue(-1, 1);
    }

    private void ChangeCarousellPositionValue(int CarousellPos, int ItemPos)
    {
        if (ChangingCharacter)
        {
            NextCarousellPos += CarousellPos;
            NextCarousellItem += ItemPos;

            OnCarousellRotate.Invoke();
            if (NextCarousellPos < 0)
            {
                NextCarousellPos = transform.childCount - 1;
            }
            if (NextCarousellItem < 0)
            {
                NextCarousellItem = transform.childCount - 1;
            }
            ChangingCharacter = false;
            StartCoroutine(nameof(CarousellRotationDelay));
        }
        if (SetItemWithMovement)
        {
            CheckCarousellItem();
        }
    }

    public void CheckCarousellItem()
    {
        EventSystem.current.SetSelectedGameObject(CarousellItemRectTransform[NextCarousellItem % transform.childCount].gameObject);

        sprite.sprite = CarousellItemRectTransform[NextCarousellItem % transform.childCount].GetComponent<Children>().presset.sprite;
        Name.text = CarousellItemRectTransform[NextCarousellItem % transform.childCount].name;
        Attack.text = CarousellItemRectTransform[NextCarousellItem % transform.childCount].GetComponent<Children>().presset.Attack;
        Speed.text = CarousellItemRectTransform[NextCarousellItem % transform.childCount].GetComponent<Children>().presset.Speed;
    }
}